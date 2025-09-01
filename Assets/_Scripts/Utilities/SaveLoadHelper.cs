using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using _Scripts.Utilities;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace _Utilities
{
    public static class SaveLoadHelper
    {
        private static List<string> _savedFiles = new();

        public static void SaveData<T>(T data, string customName = "")
        {
            try
            {
                if (customName == "") customName = typeof(T).Name;
                var filePath = GetFilePath(customName);

                var json = JsonConvert.SerializeObject(data, Formatting.Indented);

                if (!IsValidJson(json))
                {
                    LoggerNS.LogError("Error saving data: Invalid JSON format.");
                    return;
                }

#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
                var encryptedData = EncryptionHelper.Encrypt(json);
                encryptedData = string.IsNullOrEmpty(encryptedData) ? json : encryptedData;
                File.WriteAllText(filePath, encryptedData);
#else
                File.WriteAllText(filePath, json);
#endif

                if (!_savedFiles.Contains(filePath))
                {
                    _savedFiles.Add(filePath);
                }
            }
            catch (JsonSerializationException ex)
            {
                LoggerNS.LogError("Error saving data: JSON serialization error - " + ex.Message);
            }
            catch (Exception ex)
            {
                LoggerNS.LogError("Error saving data: " + ex.Message);
            }
        }

        private static T LoadData<T>(string customName = "") where T : new()
        {
            try
            {
                if (customName == "") customName = typeof(T).Name;
                var filePath = GetFilePath(customName);

                if (!IsDataExists(customName))
                {
                    throw new FileNotFoundException($"File not found: {filePath}");
                }

                var fileData = File.ReadAllText(filePath);

#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
                var json = EncryptionHelper.Decrypt(fileData);
                if (string.IsNullOrEmpty(json))
                {
                    json = IsValidJson(fileData) ? fileData : JsonConvert.SerializeObject(new T(), Formatting.Indented);
                }
#else
                var json = fileData;
#endif

                if (IsValidJson(json))
                {
                    return JsonConvert.DeserializeObject<T>(json);
                }
                else
                {
                    LoggerNS.LogError("Error loading data: Invalid JSON format.");
                }

                return default;
            }
            catch (Exception ex)
            {
                LoggerNS.LogError("Error loading data: " + ex.Message);
                return default;
            }
        }

        public static T TryLoadPersistentData<T>(string customName = "") where T : new()
        {
            var filePath = GetFilePath(string.IsNullOrEmpty(customName) ? typeof(T).Name : customName);

            if (FilePathExists(filePath))
            {
                try
                {
                    var data = LoadData<T>(customName);
                    if (data != null)
                    {
                        return data;
                    }
                }
                catch (Exception ex)
                {
                    LoggerNS.LogError(
                        $"Handled: Error loading data for {typeof(T).Name}: {ex.Message}. Creating new data.");
                }
            }

            var newData = new T();
            SaveData(newData);
            LoggerNS.Log($"Data for {typeof(T).Name} not found. New data created and saved.");
            return newData;
        }

        public static T TryLoadRuntimeData<T>(string customName = "") where T : new()
        {
            if (IsRuntimeDataAvailable<T>(customName))
            {
                try
                {
                    var data = LoadData<T>(customName);
                    if (data != null)
                    {
                        return data;
                    }
                }
                catch (Exception ex)
                {
                    LoggerNS.LogError(
                        $"Handled: Error loading data for {typeof(T).Name}: {ex.Message}. Creating new data.");
                }
            }

            var newData = new T();
            SaveData(newData);
            LoggerNS.Log($"Data for {typeof(T).Name} not found. New data created and saved.");
            return newData;
        }

        public static void UpdateData<T>(Action<T> updateAction) where T : new()
        {
            if (!IsDataExists(typeof(T).Name))
            {
                return;
            }

            var loadedData = LoadData<T>();
            updateAction(loadedData);
            SaveData(loadedData);
        }

        public static void DeleteData(string customName)
        {
            try
            {
                var filePath = GetFilePath(customName);

                if (!FilePathExists(filePath))
                {
                    return;
                }

                DeleteFileAtPath(filePath);
            }
            catch (Exception e)
            {
                LoggerNS.LogError($"Error deleting data for custom name {customName}: {e.Message}");
                throw;
            }
        }
        
        public static void DeleteAllSavedData()
        {
            try
            {
                var directoryPath = Application.persistentDataPath;
                var files = Directory.GetFiles(directoryPath);

                if (files.Length == 0)
                {
                    LoggerNS.Log("No files to delete in Application.persistentDataPath.");
                    return;
                }

                foreach (var file in files)
                {
                    File.Delete(file);
                }

                _savedFiles.Clear();
                LoggerNS.Log("All saved data in Application.persistentDataPath has been deleted.");
            }
            catch (Exception ex)
            {
                LoggerNS.LogError("Error deleting all saved data: " + ex.Message);
            }
        }

        public static void DeleteData<T>()
        {
            try
            {
                var filePath = GetFilePath(typeof(T).Name);

                if (!FilePathExists(filePath))
                {
                    return;
                }

                DeleteFileAtPath(filePath);
            }
            catch (Exception e)
            {
                LoggerNS.LogError($"Error deleting data for type {typeof(T)}: {e.Message}");
                throw;
            }
        }

        public static void DeleteFileAtPath(string filePath)
        {
            try
            {
                if (!FilePathExists(filePath))
                {
                    return;
                }

                if (_savedFiles.Contains(filePath))
                {
                    _savedFiles.Remove(filePath);
                }

                File.Delete(filePath);
            }
            catch (Exception e)
            {
                LoggerNS.LogError($"Error deleting data at {filePath}: {e.Message}");
                throw;
            }
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Tools/Delete All Cached Data")]
#endif
        public static void DeleteAllData()
        {
            _savedFiles.Clear();
        }

        private static bool FilePathExists(string filePath) =>
            !string.IsNullOrEmpty(filePath) && File.Exists(filePath);

        private static bool IsRuntimeDataAvailable<T>(string customName)
        {
            var filePath = string.IsNullOrEmpty(customName) ? typeof(T).Name : customName;
            return IsDataExists(filePath) &&
                   _savedFiles.Contains(GetFilePath(filePath));
        }

        public static bool IsDataExists(string name)
        {
            try
            {
                return File.Exists(GetFilePath(name));
            }
            catch (Exception ex)
            {
                LoggerNS.LogError($"Warning: An error occurred while checking if data exists for '{name}'. {ex.Message}");
                return false;
            }
        }

        private static bool IsValidJson(string jsonString)
        {
            try
            {
                JToken.Parse(jsonString);
                return true;
            }
            catch (JsonReaderException)
            {
                return false;
            }
        }

        private static string GetFilePath(string customName)
        {
            var fileName = customName + ".json";
            return Path.Combine(Application.persistentDataPath, fileName);
        }
    }
}