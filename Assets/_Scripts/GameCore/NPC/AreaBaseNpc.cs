using _Scripts.GameCore.NPC;
using GameCore.NPC;
using GameCore.Scriptables;
using Interfaces;
using TMPro;
using UnityEngine;

public class AreaBaseNpc : NpcBase
{
    #region Serializable Fields

    [SerializeField] private AreaNpcType areaNpcType;
    [SerializeField] private TMP_Text areaNameText;
    [SerializeField] private TMP_Text areaUnlockText;
    [SerializeField] private GameObject npcModel;
    [SerializeField] private GameObject lockedObject;
    [SerializeField] private GameObject unlockedObject;

    #endregion


    #region Fields

    private Area _area;

    #endregion


    #region Properties

    public AreaNpcType AreaNpcType => areaNpcType;
    protected GameObject LockedObject => lockedObject;
    protected Area Area => _area;

    #endregion


    #region Public Methods

    public void InitializeArea(Area area)
    {
        _area = area;

        areaNameText.text = area.AreaName;
        areaUnlockText.text = area.AreaName;

        npcModel.SetActive(npcModel != null && _area.IsNpcModel);
        SetActivateNpcObjects(TutorialSequenceController.IsTutorialCompleted
            ? !Area.AfterTutorialLock
            : !Area.InTutorialLock);
        TutorialService.TutorialCompleted += () => SetActivateNpcObjects(!Area.AfterTutorialLock);
        SetNpcModel();
    }


    public void SetActivateNpcObjects(bool value)
    {
        if (lockedObject != null)
        {
            lockedObject.SetActive(!value);
            unlockedObject.SetActive(value);
        }

        if (OutlineSpriteRenderer != null) { OutlineSpriteRenderer.gameObject.SetActive(value); }

        IsLocked = !value;
        Collider.enabled = value;
    }


    protected void SendAreaOpenedEvent()
    {
        IAnalyticsService.LogEvent(new EventParameters<string> {EventName = _area.EventParameter});
    }


    protected override void OnCompleteTimer()
    {
    }

    #endregion


    #region Private Methods

    private void SetNpcModel()
    {
        if (!_area.IsNpcModel) return;

        npcModel.SetActive(_area.IsNpcModel);
        foreach (Transform body in npcModel.transform)
        {
            if (!body.name.Equals(_area.NpcModelKey)) continue;

            body.gameObject.SetActive(true);
            break;
        }
    }

    #endregion
}
