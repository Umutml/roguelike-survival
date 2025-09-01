using GameCore.PopupSystem;
using UnityEngine;

public class ClaimRewardPopup : Popup
{
    #region Serializable Fields

    [SerializeField] private ClaimRewardSegment claimRewardSegmentPrefab;

    #endregion


    #region Fields

    private ClaimRewardSegment _segmentInstance;

    #endregion


    #region Public Methods

    public override void OnOpenPopup()
    {
        CreateSegments();
    }

    #endregion


    #region Private Methods

    private void CreateSegments()
    {
        
    }

    #endregion
}
