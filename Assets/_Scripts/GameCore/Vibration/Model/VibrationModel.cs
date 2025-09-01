using _Scripts.GameCore.Vibration.Constants;
using Lofelt.NiceVibrations;
using UnityEngine;

[System.Serializable]
public struct VibrationModel
{
   [SerializeField] private VibrationEnums.VibrationEventType vibrationEventType;
   [SerializeField] private HapticPatterns.PresetType hapticPattern;
   
   
   public readonly VibrationEnums.VibrationEventType VibrationEventType => vibrationEventType;
   public readonly HapticPatterns.PresetType HapticPattern => hapticPattern;
}
