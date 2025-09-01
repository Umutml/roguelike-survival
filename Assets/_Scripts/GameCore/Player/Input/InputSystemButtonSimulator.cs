using UnityEngine.InputSystem.OnScreen;

namespace GameCore.Player.Input
{
    public class InputSystemButtonSimulator : OnScreenButton
    {
        #region Public Methods

        public void SendButtonUp()
        {
            SendValueToControl(0.0f);
        }

        public void SendButtonDown()
        {
            SendValueToControl(1.0f);
        }

        #endregion
    }
}
