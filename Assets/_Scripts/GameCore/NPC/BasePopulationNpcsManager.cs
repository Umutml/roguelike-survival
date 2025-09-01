using System.Collections.Generic;
using GameCore.Player;
using GameCore.Tutorial;
using UnityEngine;
using VContainer;

namespace _Scripts.GameCore.NPC
{
    public class BasePopulationNpcsManager : MonoBehaviour
    {
        [SerializeField] private List<GameObject> disabledObject;

        public void SetEnableAllChildren(bool enable)
        {
            foreach (var child in disabledObject)
            {
                child.gameObject.SetActive(enable);
            }
        }

        [Inject]
        public void Init(TutorialSequenceController tutorialSequenceController, PlayerController playerController)
        {
            if (tutorialSequenceController == null) return;
            if (playerController == null) return;

            playerController.SetBasePopulationNpcsManager(this);
            SetEnableAllChildren(true);
        }
    }
}