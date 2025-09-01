using System;
using System.Collections.Generic;
using _Scripts.Interfaces;
using _Scripts.Utilities;
using Managers;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.Managers
{
    public class GeneralOnClickManager : MonoBehaviour, IGeneralOnClickManager
    {
        [SerializeField] private AudioManager audioManager;
        [SerializeField] private AnalyticManager analyticManager;
        
        private Dictionary<Button, Delegate> buttonActions = new Dictionary<Button, Delegate>();
        private Dictionary<Button, string> buttonSounds = new Dictionary<Button, string>();

        public void RegisterButton(Button button, Action action, ButtonEvent buttonEvent = ButtonEvent.None, string soundKey = null)
        {
            if (button == null || action == null)
            {
                Debug.LogWarning("Button cannot be null!");
                return;
            }

            if (buttonActions.ContainsKey(button))
            {
                UnregisterButton(button);
            }

            buttonActions[button] = action;
            buttonSounds[button] = soundKey;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                HandleButtonClick(button, buttonEvent);
            });
        }

        public void RegisterButton<T>(Button button, Action<T> action, T param, ButtonEvent buttonEvent = ButtonEvent.None, string soundKey = null)
        {
            if (button == null || action == null)
            {
                Debug.LogWarning("Button cannot be null!");
                return;
            }

            if (buttonActions.ContainsKey(button))
            {
                UnregisterButton(button);
            }

            buttonActions[button] = (Action)(() => action(param));
            buttonSounds[button] = soundKey;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                HandleButtonClick(button, buttonEvent);
            });
        }

        private void HandleButtonClick(Button button, ButtonEvent buttonEvent)
        {
            if (buttonActions.TryGetValue(button, out Delegate action))
            {
                #region Action

                if (action is Action simpleAction)
                {
                    simpleAction.Invoke();
                }
                else if (action is Action<object> paramAction)
                {
                    paramAction.Invoke(null);
                }

                #endregion

                #region Sfx

                if (buttonSounds.TryGetValue(button, out string soundKey))
                {
                    audioManager.PlayOneShot(!string.IsNullOrEmpty(soundKey) ? soundKey : "UIButtonPress");
                }
                else
                {
                    audioManager.PlayOneShot("UIButtonPress");
                }

                #endregion

                #region Analytics

                analyticManager.SendAnalyticButtonEvent(buttonEvent);

                #endregion
            }
            else
            {
                Debug.LogWarning($"Cannot found any action for Button '{button.name}'!");
            }
        }

        public void UnregisterButton(Button button)
        {
            if (buttonActions.ContainsKey(button))
            {
                buttonActions.Remove(button);
                buttonSounds.Remove(button);
                button.onClick.RemoveAllListeners();
            }
        }
    }
}