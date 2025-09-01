using _Scripts.GameCore.Scriptables;
using UI.Game.Architectural;
using UI.Game.InGame.SupplyRunPopupConstants;
using UnityEngine;

public class SupplyRunSegment : Content
{
    #region Fields

    private RectTransform _wayTransform;
    private RectTransform _rewardTransform;

    #endregion
    
    
    #region Public Methods

    public async void InitializeSegment(AdRewardData rewardData, Sprite rewardSprite, int index, bool isLast, bool isClaimed)
    {
        _wayTransform = GetGameObject(SupplyRunPopupConstants.Way).GetComponent<RectTransform>();
        _rewardTransform = GetGameObject(SupplyRunPopupConstants.Reward).GetComponent<RectTransform>();

        _wayTransform.localScale = _rewardTransform.localScale = new Vector3(index % 2 == 0 ? 1 : -1, 1, 1);

        SetImage(SupplyRunPopupConstants.Reward, rewardSprite);
        SetImage(SupplyRunPopupConstants.Icon, await rewardData.RewardSprite());
        SetText(SupplyRunPopupConstants.Amount, rewardData.RewardCount.ToString());

        if (isClaimed)
        {
            SetGameObject(SupplyRunPopupConstants.Tick, true);
            SetColor(SupplyRunPopupConstants.Way, SupplyRunPopupConstants.Claimed_Way_Color);
            SetColor(SupplyRunPopupConstants.Amount, SupplyRunPopupConstants.Claimed_Text_Color);
        }

        if (isLast) SetColor(SupplyRunPopupConstants.Way, new(1f, 1f, 1f, 0f));
    }


    public void UpdateSegment(Sprite rewardSprite, bool isLast)
    {
        SetImage(SupplyRunPopupConstants.Reward, rewardSprite);
        SetGameObject(SupplyRunPopupConstants.Tick, true);
        SetColor(SupplyRunPopupConstants.Way, SupplyRunPopupConstants.Claimed_Way_Color);
        SetColor(SupplyRunPopupConstants.Amount, SupplyRunPopupConstants.Claimed_Text_Color);
        if (isLast) SetColor(SupplyRunPopupConstants.Way, new(1f, 1f, 1f, 0f));
    }

    #endregion    
}
