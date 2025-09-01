using System;
using System.Collections.Generic;
using _Utilities;
using Cysharp.Threading.Tasks;
using MyBox;
using System.Linq;
using UnityEngine;

namespace GameCore.Tutorial.Steps
{
    [CreateAssetMenu(fileName = "SaveTutorialCheckPoint",
        menuName = "ScriptableObjects/Tutorial/Steps/Save Tutorial Check Point",
        order = 0)]
    public class SaveTutorialCheckPoint : TutorialStep
    {
        [SerializeField] private List<SaveCheckPointData> tutorialCheckPointDatas;
        private TutorialCheckPoint _tutorialCheckPoint;

        public override UniTask ProcessStep()
        {
            _tutorialCheckPoint = SaveLoadHelper.TryLoadPersistentData<TutorialCheckPoint>();
            _tutorialCheckPoint.HasCheckPoint = true;
            _tutorialCheckPoint.TutorialCheckPointDatas = tutorialCheckPointDatas.Select(data => new TutorialCheckPointData
            {
                type = data.type,
                Position = data.Position.ToString(),
                StartStepName = data.StartStepName,
                InitialSteps = data.InitialSteps
            }).ToList();

            SaveLoadHelper.SaveData(_tutorialCheckPoint);

            return UniTask.CompletedTask;
        }
    }

    [Serializable]

    public struct SaveCheckPointData
    {
        public CheckPointType type;
        public Vector3 Position;
        public string StartStepName;
        public List<InitialSteps> InitialSteps;
    }
}