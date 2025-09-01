using _Scripts.GameCore.Vibration.Constants;
using Lofelt.NiceVibrations;
using UnityEngine;

public class VibrationManager : MonoBehaviour
{
    #region Serializable Fields

    [SerializeField] private VibrationResources vibrationResources;
    [SerializeField] private bool isVibrationActive;

    #endregion


    #region Fields

    private VibrationModel _vibrationModel;
    private const float MaxTime = 0.2f;
    private float _currentTime;

    #endregion


    #region Public Methods

    public void TriggerVibration(VibrationEnums.VibrationEventType vibrationEventType)
    {
        if (!isVibrationActive) return;
        
        if (PlayerPrefs.GetString("Vibration") == "Off") return;
        
        _vibrationModel = vibrationResources.GetVibrationModel(vibrationEventType);
        HapticPatterns.PlayPreset(_vibrationModel.HapticPattern);
    }
    
    
    public void TriggerVibrationCarDrift(bool isDrifting)
    {
        if (!isVibrationActive) return;

        if (isDrifting)
        {
            if (_currentTime <= 0)
            {
                TriggerVibration(VibrationEnums.VibrationEventType.Drift);
                _currentTime = MaxTime;
            }
            else
            {
                _currentTime -= Time.deltaTime;
            }
        }
    }

    #endregion
}
