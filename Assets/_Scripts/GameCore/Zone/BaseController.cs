using GameCore.Player;
using UnityEngine;
using VContainer;

public class BaseController : MonoBehaviour
{
    [SerializeField] private Transform centerOfBase;
    [SerializeField] private Transform centerOfGarage;
    private PlayerController _playerController;
    public Transform CenterOfBase => centerOfBase;
    public Transform CenterOfGarage => centerOfGarage;
    
    [Inject]
    public void Init(PlayerController playerController)
    {
        if(playerController == null) return;
        
        _playerController = playerController;
        
        _playerController.CenterOfBase = centerOfBase;
        
        _playerController.CenterOfGarage = centerOfGarage;
    }
}
