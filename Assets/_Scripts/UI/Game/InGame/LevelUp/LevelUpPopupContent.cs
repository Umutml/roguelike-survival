using GameCore.Scriptables;
using UI.Game.Architectural;
using UI.Game.InGame.LevelUp.Constants;

public class LevelUpPopupContent : Content
{
    #region Public Methods

    public void SetWaveText(Wave wave)
    {
        SetText(InGameLevelUpPanelConstants.WAVE_VALUE_TEXT, wave.level.ToString());
    }
    
    
    public void SetKillsValueText(int value)
    {
        SetText(InGameLevelUpPanelConstants.KILLS_VALUE_TEXT, value.ToString());
    }

    #endregion
}
