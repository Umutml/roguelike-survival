using System;
using _Scripts.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.Interfaces
{
    public interface IGeneralOnClickManager
    {
        public void RegisterButton(Button button, Action action, ButtonEvent buttonEvent = ButtonEvent.None, string soundKey = null);

        public void RegisterButton<T>(Button button, Action<T> action, T param,
            ButtonEvent buttonEvent = ButtonEvent.None, string soundKey = null);
        public void UnregisterButton(Button button);
    }
}
