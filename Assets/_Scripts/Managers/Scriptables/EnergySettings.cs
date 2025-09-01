using UnityEngine;

namespace Managers.Scriptables
{
    [CreateAssetMenu(fileName = "EnergySettings", menuName = "ScriptableObjects/EnergySettings")]
    public class EnergySettings : ScriptableObject
    {
        public int MaxEnergy;
        public int EnergyRecoveryTimeSeconds;
    }
}
