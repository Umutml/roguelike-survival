using UI.Game.Architectural;
using UnityEngine;

public class ClaimRewardSegment : Content
{
    #region Consts

    private const string Icon = "Icon";
    private const string Amount = "AmountText";

    #endregion


    #region Public Methods

    public void SetSegment(Sprite icon, int amount)
    {
        SetImage(Icon, icon);
        SetText(Amount, $"x{amount}");
    }

    #endregion
}
