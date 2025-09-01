#if UNITY_ANDROID
using System;
using _Scripts.Utilities;
using UnityEngine;

namespace _Utilities
{
    public static class AndroidKeystoreHelper
    {
        private const string KeyAlias = "MyEncryptionKeyAlias";
        private const string IvPrefKey = "MyEncryptionIv";
        private static AndroidJavaObject _securePreferences;

        static AndroidKeystoreHelper()
        {
            try
            {
                using var playerActivity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                using var context = playerActivity.GetStatic<AndroidJavaObject>("currentActivity");

                using var helperClass = new AndroidJavaClass("com.nosurrender.preferences.SecurePreferencesHelper");
                _securePreferences =
                    new AndroidJavaObject("com.nosurrender.preferences.SecurePreferencesHelper", context);
            }
            catch (Exception e)
            {
                LoggerNS.LogError($"Failed to initialize SecurePreferencesHelper: {e.Message}");
            }
        }

        public static void SaveKey(string key)
        {
            try
            {
                _securePreferences?.Call("saveKey", KeyAlias, key);
            }
            catch (Exception e)
            {
                LoggerNS.LogError($"Failed to save key: {e.Message}");
            }
        }

        public static string LoadKey()
        {
            try
            {
                return _securePreferences?.Call<string>("loadKey", KeyAlias);
            }
            catch (Exception e)
            {
                LoggerNS.LogError($"Failed to load key: {e.Message}");
                return null;
            }
        }

        public static void DeleteKey()
        {
            try
            {
                _securePreferences?.Call("deleteKey", KeyAlias);
            }
            catch (Exception e)
            {
                LoggerNS.LogError($"Failed to delete key: {e.Message}");
            }
        }

        public static void SaveIv(string iv)
        {
            try
            {
                _securePreferences?.Call("saveKey", IvPrefKey, iv);
            }
            catch (Exception e)
            {
                LoggerNS.LogError($"Failed to save IV: {e.Message}");
            }
        }

        public static string LoadIv()
        {
            try
            {
                return _securePreferences?.Call<string>("loadKey", IvPrefKey);
            }
            catch (Exception e)
            {
                LoggerNS.LogError($"Failed to load IV: {e.Message}");
                return null;
            }
        }

        public static void DeleteIv()
        {
            try
            {
                _securePreferences?.Call("deleteKey", IvPrefKey);
            }
            catch (Exception e)
            {
                LoggerNS.LogError($"Failed to delete IV: {e.Message}");
            }
        }
    }
}
#endif
