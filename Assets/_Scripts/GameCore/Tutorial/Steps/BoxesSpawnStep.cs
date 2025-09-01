using System.Collections.Generic;
using _Scripts.Utilities;
using Cathei.LinqGen;
using Cysharp.Threading.Tasks;
using GameCore.Box;
using GameCore.Health;
using GameCore.Spawner;
using Interfaces;
using UnityEngine;
using VContainer;

namespace GameCore.Tutorial.Steps
{
    [CreateAssetMenu(fileName = "BoxesSpawnStep",
        menuName = "ScriptableObjects/Tutorial/Steps/Boxes Spawn Step",
        order = 0)]
    public class BoxesSpawnStep : TutorialStep
    {
        [SerializeField] private List<Vector3> spawnPosition = new();
        [SerializeField] private bool isDisabledPickup;
        [SerializeField] private bool isDisabledDropIncrement;
        [SerializeField] private bool isDisabledDrop;
        [SerializeField] private DropPodType forcedDropPodType = DropPodType.Coin;
        [SerializeField] private bool skipRegistration;

        private List<IDamageable> _boxes = new();
        private BoxManager _boxManager;

        public override async UniTask ProcessStep()
        {
            _boxManager = Resolver.Resolve<BoxManager>();

            foreach (var position in spawnPosition)
            {
                await UniTask.Delay(200);
                var box = await _boxManager.DropBox(position);
                var damageable = box.GetComponent<IDamageable>();
                var dropItem = box.GetComponent<IDropItem>();
                var boxController = box.GetComponent<BoxController>();
                boxController.Config = new BoxController.BoxConfig
                {
                    IsDisabledPickup = isDisabledPickup,
                    IsDisabledDropIncrement = isDisabledDropIncrement,
                    IsDisabledDrop = isDisabledDrop,
                    ForcedDropPodType = forcedDropPodType
                };

                _boxes.Add(damageable);
                if (damageable is null)
                {
                    LoggerNS.LogError("Box is null");
                    return;
                }

                if (skipRegistration)
                {
                    return;
                }

                _boxManager.SubscribedTutorialDamageables.Add((dropItem, damageable));
            }
        }
    }
}