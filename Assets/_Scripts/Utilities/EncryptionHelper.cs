using System;
using System.IO;
using System.Security.Cryptography;
using _Scripts.Utilities;
using UnityEngine;

namespace _Utilities
{
    public static class EncryptionHelper
    {
        private static readonly byte[] Key;
        private static readonly byte[] Iv;

        private const string EncryptionKey = "EncryptionKey";
        private const string EncryptionIv = "EncryptionIv";

        static EncryptionHelper()
        {
            (Key, Iv) = LoadOrGenerateKeyAndIv();
        }

        public static string Encrypt(string plainText)
        {
            try
            {
                using var aes = Aes.Create();
                aes.Key = Key;
                aes.IV = Iv;

                using var memoryStream = new MemoryStream();
                using (var cryptoStream = new CryptoStream(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                using (var writer = new StreamWriter(cryptoStream))
                {
                    writer.Write(plainText);
                }

                return Convert.ToBase64String(memoryStream.ToArray());
            }
            catch (Exception ex)
            {
                LoggerNS.Log($"Encryption failed: {ex.Message}");
                return string.Empty;
            }
        }

        public static string Decrypt(string cipherText)
        {
            try
            {
                using var aes = Aes.Create();
                aes.Key = Key;
                aes.IV = Iv;

                using var memoryStream = new MemoryStream(Convert.FromBase64String(cipherText));
                using var cryptoStream = new CryptoStream(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Read);
                using var reader = new StreamReader(cryptoStream);
                return reader.ReadToEnd();
            }
            catch (Exception ex)
            {
                LoggerNS.Log($"Decryption failed: {ex.Message}");
                return string.Empty;
            }
        }

        private static (byte[] key, byte[] iv) LoadOrGenerateKeyAndIv()
        {
            string key = null, iv = null;

            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
#if UNITY_IOS
                key = IOSKeychainHelper.LoadKey("EncryptionKey");
                iv = IOSKeychainHelper.LoadKey("EncryptionIv");
#endif
            }
            else if (Application.platform == RuntimePlatform.Android)
            {
#if UNITY_ANDROID
                key = AndroidKeystoreHelper.LoadKey();
                iv = AndroidKeystoreHelper.LoadIv();
#endif
            }

            if (key != null && iv != null)
            {
                LoggerNS.Log($"Loaded Key: {key} and IV: {iv}");
                return (Convert.FromBase64String(key), Convert.FromBase64String(iv));
            }

            using var aes = Aes.Create();
            aes.GenerateKey();
            aes.GenerateIV();

            var generatedKey = Convert.ToBase64String(aes.Key);
            var generatedIv = Convert.ToBase64String(aes.IV);

            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
#if UNITY_IOS
                IOSKeychainHelper.SaveKey("EncryptionKey", generatedKey);
                IOSKeychainHelper.SaveKey("EncryptionIv", generatedIv);
#endif
                LoggerNS.Log($"KeyChain: Generated Key: {generatedKey} and IV: {generatedIv}");
            }
            else if (Application.platform == RuntimePlatform.Android)
            {
#if UNITY_ANDROID
                AndroidKeystoreHelper.SaveKey(generatedKey);
                AndroidKeystoreHelper.SaveIv(generatedIv);
#endif

                LoggerNS.Log($"KeyStore: Generated Key: {generatedKey} and IV: {generatedIv}");
            }

            return (aes.Key, aes.IV);
        }
    }
}