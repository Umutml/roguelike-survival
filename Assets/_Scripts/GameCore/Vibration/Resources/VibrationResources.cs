using UnityEngine;
using System.Collections.Generic;
using _Scripts.GameCore.Vibration.Constants;
using System.Linq;

[CreateAssetMenu(fileName = "VibrationResources", menuName = "ScriptableObjects/VibrationResources", order = 0)]
public class VibrationResources : ScriptableObject
{
    [SerializeField] private List<VibrationModel> vibrationModels = new ();
    
    
    public VibrationModel GetVibrationModel(VibrationEnums.VibrationEventType targetEvent)
    {
        return vibrationModels.FirstOrDefault(vibrationModel => vibrationModel.VibrationEventType.Equals(targetEvent));
    }
}
