using System;
using _Scripts.Utilities;
using GameCore.Player;
using VContainer;

public class CarRecoverNpcController : AreaBaseNpc
{
    private CarManager _carManager;
    private PlayerController _playerController;

    protected override void Awake()
    {
        base.Awake();
        SetState(TutorialSequenceController.IsTutorialCompleted);
    }

    private void OnEnable() => TutorialSequenceController.TutorialCompleted += SetTutorialCompleted;
    private void OnDestroy() => TutorialSequenceController.TutorialCompleted -= SetTutorialCompleted;

    public override void Execute(bool isActive)
    {
        if (_playerController.PlayerMovementMode.Equals(PlayerMovementMode.Drive)) { return; }

        base.Execute(isActive);
    }

    protected override async void OnCompleteTimer()
    {
        try
        {
            if (_carManager == null)
            {
                LoggerNS.LogWarning("CarManager is null.");
                return;
            }

            await _carManager.Recover();
        }
        catch (Exception e)
        {
            LoggerNS.LogError("CarRecoverNpcController: OnCompleteTimer() - " + e);
        }
    }

    [Inject]
    private void Initialize(PlayerController playerController, CarManager carManager)
    {
        _playerController = playerController;
        _carManager = carManager;
    }

    private void SetTutorialCompleted() => SetState(TutorialSequenceController.IsTutorialCompleted);

    public void SetState(bool isActive)
    {
        SetActivateNpcObjects(isActive);
    }
}
