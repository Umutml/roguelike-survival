using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using _Scripts.Utilities;
using AdjustSdk;
using Firebase;
using Firebase.Analytics;
using Firebase.Extensions;
using Interfaces;
using UnityEngine;

namespace Managers
{
    public class AnalyticManager : MonoBehaviour, IAnalyticsService
    {
        private const string AppQuitToken = "d7ngjn"; // Adjust token for the app_quit event
        private const string AmplitudeApiKey = "23813daecef4a4cea03a4135e588cb12";
        private Amplitude _amplitudeInstance; // Amplitude instance after initialization
        private FirebaseApp _firebaseApp; // Firebase instance after initialization
        private DateTime _splashScreenOpenedTime;
        public bool IsAmplitudeReady { get; set; }
        [SerializeField] private bool enableLogging = false;

        private async void Awake()
        {
            try
            {
                await InitAnalytics();
                _splashScreenOpenedTime = DateTime.Now;
                await Task.Delay(2000); // Wait for 2 seconds before sending the first_open event
                SendFirstOpenEvent();
            }
            catch (Exception e)
            {
                LoggerNS.LogError($"Error initializing analytics: {e.Message}");
            }
        }

        private void OnApplicationQuit()
        {
            LogSessionDuration();
        }

        public bool IsFirebaseReady { get; set; }

        public void LogEvent<T>(EventParameters<T> eventParameters)
        {
            try
            {
                if (string.IsNullOrEmpty(eventParameters.EventName))
                {
                    if (enableLogging) LoggerNS.Log("Event name is required for logging an event");
                    return; // Event name is must provide for logging an event
                }
                
                if (!string.IsNullOrEmpty(eventParameters.AdjustToken))
                    LogEventWithTokenAdjust(eventParameters.AdjustToken);

                if (!string.IsNullOrEmpty(eventParameters.ParameterName))
                {
                    LogEventWithDynamicParameter(eventParameters.EventName, eventParameters.ParameterName,
                        eventParameters.ParameterValue);
                }
                else if (eventParameters.ParameterValue is Dictionary<string, object> amplitudeDict)
                {
                    if (IsAmplitudeReady) _amplitudeInstance.logEvent(eventParameters.EventName, amplitudeDict);
                }
                else
                {
                    LogSingleStringEvent(eventParameters.EventName);
                }
            }
            catch (Exception e)
            {
                LoggerNS.LogError($"Error logging event: {e.Message}");
            }
        }

        public void LogEventParameterArray(string eventName, Dictionary<string, object> parameters)
        {
            if (IsFirebaseReady)
            {
                var parameter = parameters?
                    .Select(x => new Parameter(x.Key, x.Value?.ToString() ?? string.Empty))
                    .ToArray();
                FirebaseAnalytics.LogEvent(eventName, parameter);
            }

            if (IsAmplitudeReady)
            {
                _amplitudeInstance.logEvent(eventName, parameters);
            }
            if (enableLogging) LoggerNS.Log($"ANALYTIC SEND: Event: {eventName}");
        }

        public void SendAnalyticButtonEvent(ButtonEvent buttonEvent)
        {
            if (buttonEvent is not ButtonEvent.None)
            {
                if (_buttonEvents.TryGetValue(buttonEvent, out var eventParameters))
                {
                    LogEvent(eventParameters);
                }
            }
        }

        public async Task InitAnalytics()
        {
            var firebaseTask = InitFirebase();
            var amplitudeTask = InitAmplitude();
            // Add other analytics SDKs init as necessary. For example: Adjust, Devtodev, etc.
            await Task.WhenAll(firebaseTask, amplitudeTask);
        }

        private void LogEventWithTokenAdjust(string token)
        {
            var adjustEvent = new AdjustEvent(token);
            Adjust.TrackEvent(adjustEvent);
        }

        private void LogSingleStringEvent(string eventName)
        {
            if (IsFirebaseReady) FirebaseAnalytics.LogEvent(eventName);
            if (IsAmplitudeReady) _amplitudeInstance.logEvent(eventName);
            if (enableLogging) LoggerNS.Log($"ANALYTIC SEND: Event: {eventName}");
        }

        private void LogEventWithDynamicParameter<T>(string eventName, string parameterName, T parameterValue)
        {
            // Check parameterValue and cast to the appropriate type to log the event
            switch (parameterValue)
            {
                case string strValue:
                    if (IsFirebaseReady) FirebaseAnalytics.LogEvent(eventName, parameterName, strValue);
                    break;
                case int intValue:
                    if (IsFirebaseReady) FirebaseAnalytics.LogEvent(eventName, parameterName, intValue);
                    break;
                case float floatValue:
                    if (IsFirebaseReady) FirebaseAnalytics.LogEvent(eventName, parameterName, floatValue);
                    break;
                case double doubleValue:
                    if (IsFirebaseReady) FirebaseAnalytics.LogEvent(eventName, parameterName, doubleValue);
                    break;
                case long longValue:
                    if (IsFirebaseReady) FirebaseAnalytics.LogEvent(eventName, parameterName, longValue);
                    break;
                default:
                    throw new ArgumentException($"Unsupported parameter type: {typeof(T)}");
            }
            if (enableLogging) LoggerNS.Log($"ANALYTIC SEND: Event: {eventName} | Parameter: {parameterName} | Value: {parameterValue}");
        }

        private void SendFirstOpenEvent()
        {
            var deviceType = SystemInfo.deviceType.ToString();
            var osVersion = SystemInfo.operatingSystem;
            var country = new RegionInfo(CultureInfo.CurrentCulture.Name).TwoLetterISORegionName;
            // <<< Log the first_open event >>>
            if (IsFirebaseReady || IsAmplitudeReady)
                LogEventParameterArray("first_open_ns", new Dictionary<string, object> { { "device_type", deviceType }, { "os_version", osVersion }, { "country", country } });

            LogEventWithTokenAdjust(AdjustNsEventTokens.FirstOpen);
            SendUniqueFirstOpenEvent();
        }

        private void SendUniqueFirstOpenEvent()
        {
            var isSend = PlayerPrefs.GetInt("first_open_zdsr", 0) == 1; // 1 mean true already sent
            if (!isSend)
            {
                LogEvent(new EventParameters<string> { EventName = "first_open_zdsr" });
                PlayerPrefs.SetInt("first_open_zdsr", 1);
                PlayerPrefs.Save();
            }
        }

        private Task InitFirebase()
        {
            try
            {
                FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
                {
                    var dependencyStatus = task.Result;
                    if (dependencyStatus == DependencyStatus.Available)
                    {
                        _firebaseApp = FirebaseApp.DefaultInstance;
                        IsFirebaseReady = true;
                        FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
                        // When this property is set to true, Crashlytics will report all, uncaught exceptions as fatal events. This is the recommended behavior.
                        // Crashlytics.ReportUncaughtExceptionsAsFatal = true; // Uncomment this line if you want to report all uncaught exceptions as fatal events
                    }
                    else
                    {
                        LoggerNS.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
                        // Firebase Unity SDK is not safe to use here.
                        IsFirebaseReady = false;
                    }
                });
            }
            catch (Exception e)
            {
                LoggerNS.LogError($"Error initializing Firebase: {e.Message}");
                IsFirebaseReady = false;
            }

            return Task.CompletedTask;
        }

        private Task InitAmplitude()
        {
            try
            {
                _amplitudeInstance = Amplitude.getInstance();
                _amplitudeInstance.setServerUrl("https://api2.amplitude.com");
                _amplitudeInstance.logging = true;
                _amplitudeInstance.trackSessionEvents(true);
                _amplitudeInstance.init(AmplitudeApiKey);
                IsAmplitudeReady = true;
            }
            catch (Exception e)
            {
                LoggerNS.LogError($"Error initializing Amplitude: {e.Message}");
                IsAmplitudeReady = false;
            }

            return Task.CompletedTask;
        }

        private void LogSessionDuration()
        {
            var timeSpent = DateTime.Now - _splashScreenOpenedTime;
            LogEvent(new EventParameters<string>
            {
                EventName = "app_quit",
                ParameterName = "session_duration",
                ParameterValue = timeSpent.TotalSeconds.ToString(CultureInfo.InvariantCulture),
                AdjustToken = AppQuitToken
            });
        }
        
        #region Button Events
        
        private Dictionary<ButtonEvent, EventParameters<string>> _buttonEvents = new Dictionary<ButtonEvent, EventParameters<string>>
        {
            {ButtonEvent.PauseButton, new EventParameters<string> {EventName = "settings_open_btn_clk"}},
            {ButtonEvent.SettingsCloseButton, new EventParameters<string> {EventName = "settings_close1_btn_clk"}},
            {ButtonEvent.ResumeButton, new EventParameters<string> {EventName = "settings_close2_btn_clk"}},
            {ButtonEvent.RestartCheckpointButton, new EventParameters<string> {EventName = "settings_restart_btn_clk"}},
            {ButtonEvent.TermsOfUseButton, new EventParameters<string> {EventName = "settings_eula_btn_clk"}},
            {ButtonEvent.PrivacyPolicyButton, new EventParameters<string> {EventName = "settings_pp_btn_clk"}},
            {ButtonEvent.TermOfServicesButton, new EventParameters<string> {EventName = "settings_tos_btn_clk"}},
            {ButtonEvent.CookiePolicyButton, new EventParameters<string> {EventName = "settings_cp_btn_clk"}},
            {ButtonEvent.RefillEnergyButton, new EventParameters<string> {EventName = "energy_open_btn_clk"}},
            {ButtonEvent.RefillEnergyNoButton, new EventParameters<string> {EventName = "energy_close_btn_clk"}},
            {ButtonEvent.RefillEnergyWatchAdButton, new EventParameters<string> {EventName = "energy_watchad_btn_clk"}},
        };

        #endregion
    }
}
