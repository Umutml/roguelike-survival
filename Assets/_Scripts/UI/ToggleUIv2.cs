using UI.Game.Architectural;

public class ToggleUIv2 : Content
{
    #region Fields

    private const string CLOSE_LINE = "CloseLine";
    private string _currentState;

    #endregion
    
    
    #region Properties

    public string CurrentState
    {
        get => _currentState;
        set
        {
            _currentState = value;
            SetToggle();
        }
    }

    #endregion
    
    
    #region Public Methods

    public void SetStartState(string state)
    {
        _currentState = state;
        SetToggle();
    }    

    #endregion


    #region Private Methods

    private void SetToggle()
    {
        SetGameObject(CLOSE_LINE, _currentState.Equals("Off"));
    }

    #endregion
}
