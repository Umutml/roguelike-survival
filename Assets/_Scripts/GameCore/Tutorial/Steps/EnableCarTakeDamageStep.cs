using Cysharp.Threading.Tasks;
using GameCore.Tutorial;
using UnityEngine;
using VContainer;

[CreateAssetMenu(fileName = "EnableCarTakeDamageStep",
    menuName = "ScriptableObjects/Tutorial/Steps/EnableCarTakeDamageStep",
    order = 0)]
public class EnableCarTakeDamageStep : TutorialStep
{
    [SerializeField] private bool enable;
    
    private CarManager _carManager;

    public override UniTask ProcessStep()
    {
        _carManager = Resolver.Resolve<CarManager>();
        _carManager.IsBridgeDrive = enable;

        return UniTask.CompletedTask;
    }
}
