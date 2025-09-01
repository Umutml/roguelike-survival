public class AreaNpc : AreaBaseNpc
{
    protected override void OnCompleteTimer()
    {
        if (Area.AfterTutorialLock && TutorialService.IsTutorialCompleted) return;
        
        SendAreaOpenedEvent();
        OpenPopup(Area.OpeningPopupType);
    }
}
