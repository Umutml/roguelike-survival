using Cysharp.Threading.Tasks;
using GameCore.Tutorial;
using UnityEngine;
using VContainer;


[CreateAssetMenu(fileName = "EnableBadgeStep",
    menuName = "ScriptableObjects/Tutorial/Steps/EnableBadgeStep",
    order = 0)]
public class EnableBadgeStep : TutorialStep
{
    [SerializeField] private bool enable;
    
    private BadgeManager _badgeManager;

    public override UniTask ProcessStep()
    {
        _badgeManager = Resolver.Resolve<BadgeManager>();
        _badgeManager.OnEnableBadges?.Invoke(enable);
        PlayerPrefs.SetInt("IsCarUpgrade", 1);
        return UniTask.CompletedTask;
    }
}

