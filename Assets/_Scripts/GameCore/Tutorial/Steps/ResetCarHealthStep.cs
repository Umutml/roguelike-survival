using Cysharp.Threading.Tasks;
using GameCore.Tutorial;
using UnityEngine;
using VContainer;

[CreateAssetMenu(fileName = "ResetCarHealthStep",
    menuName = "ScriptableObjects/Tutorial/Steps/ResetCarHealthStep",
    order = 0)]
public class ResetCarHealthStep : TutorialStep
{
    private CarManager _carManager;

    public override UniTask ProcessStep()
    {
        _carManager = Resolver.Resolve<CarManager>();
        _carManager.OnResetCarHealth?.Invoke();
        return UniTask.CompletedTask;
    }
}
