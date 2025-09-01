using _Scripts.Utilities;
using MyBox;
using UnityEngine;

namespace GameCore.Scriptables
{
    [CreateAssetMenu(fileName = "ManagementNpcData", menuName = "ScriptableObjects/ManagementNpcData", order = 0)]
    public class ManagementNpcData : ScriptableObject
    {
        public ManagementStateDetails[] managementStateDetails;

#if UNITY_EDITOR
        [ButtonMethod]
        public void SetRunQueue()
        {
            if (managementStateDetails is not { Length: > 0 })
            {
                LoggerNS.LogWarning("No ManagementStateDetails found to set run queue.");
                return;
            }

            for (var i = 0; i < managementStateDetails.Length; i++)
            {
                var index = i + 1;
                var waveCount = index * 2;
                managementStateDetails[i].waveCount = waveCount;
                managementStateDetails[i].name = $"Run Queue {index}";
                managementStateDetails[i].softCurrency = waveCount * 1000;
            }

            LoggerNS.Log("Run queues have been updated!");
        }
#endif
    }

    [System.Serializable]
    public struct ManagementStateDetails
    {
        public string name;
        public int waveCount;
        public int softCurrency;
    }
}