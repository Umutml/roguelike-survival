#if UNITY_IOS
using System.Runtime.InteropServices;
using UnityEngine;

namespace _Utilities
{
    public static class IOSKeychainHelper
    {
        [DllImport("__Internal")]
        private static extern void SaveToKeychain(string key, string value);

        [DllImport("__Internal")]
        private static extern string LoadFromKeychain(string key);

        public static void SaveKey(string key, string value)
        {
            SaveToKeychain(key, value);
        }

        public static string LoadKey(string key)
        {
            return LoadFromKeychain(key);
        }
    }
}
#endif
