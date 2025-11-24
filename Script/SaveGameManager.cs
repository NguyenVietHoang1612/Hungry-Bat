using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace CandyProject
{
    [Serializable]
    public class ProgressData
    {
        public List<LevelProgress> progressList;
    }

    [Serializable]
    public class SettingsData
    {
        public float volumeMusic = 1f;
        public float volumeSFX = 1f;
    }


    [Serializable]
    public class ResourceData
    {
        public float gold = 200;
        public float health;
        public Dictionary<GemData, int> boosters;
    }

    public static class SaveSystem
    {
        private const string SecretKey = "CandyCrushSecretKey2025";

        private const string ProgressFileName = "save_progress.dat";
        private const string SettingsFileName = "save_settings.dat";
        private const string ResourceFileName = "save_settings.dat";


        private static string ProgressPath => Path.Combine(Application.persistentDataPath, ProgressFileName);
        private static string SettingsPath => Path.Combine(Application.persistentDataPath, SettingsFileName);
        private static string ResourcePath => Path.Combine(Application.persistentDataPath, ResourceFileName);


        // ==================== SAVE ====================
        public static void SaveProgress(List<LevelProgress> levelProgressList)
        {
            try
            {
                ProgressData saveData = new ProgressData { progressList = levelProgressList };

                string json = JsonUtility.ToJson(saveData);
                string encrypted = Encrypt(json, SecretKey);

                File.WriteAllText(ProgressPath, encrypted);
                Debug.Log($"Game progress saved at: {ProgressPath}");
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to save progress: " + e.Message);
            }
        }

        public static void SaveSettings(SettingsData settingsData)
        {
            try
            {
                string json = JsonUtility.ToJson(settingsData);
                string encrypted = Encrypt(json, SecretKey);

                File.WriteAllText(SettingsPath, encrypted);
                Debug.Log($"Game VolumnSound saved at: {SettingsPath}");
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to save Setting: " + e.Message);
            }
        }

        public static void SaveResource(ResourceData resourceData)
        {
            try
            {
                ResourceData resource = new ResourceData {gold = resourceData.gold, health = resourceData.health, boosters = resourceData.boosters };

                string json = JsonUtility.ToJson(resource);
                string encrypted = Encrypt(json, SecretKey);
                File.WriteAllText(ResourcePath, encrypted);
                Debug.Log($"Game VolumnSound saved at: {ResourcePath}");
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to save resource: " + e.Message);
            }
        }

        

        // ==================== LOAD ====================
        public static List<LevelProgress> LoadProgress(List<LevelProgress> defaultList)
        {
            try
            {
                if (!File.Exists(ProgressPath))
                {
                    Debug.Log("Save file not found. Using default data.");
                    return defaultList;
                }

                string encrypted = File.ReadAllText(ProgressPath);
                string decrypted = Decrypt(encrypted, SecretKey);
                ProgressData saveData = JsonUtility.FromJson<ProgressData>(decrypted);

                if (saveData == null || saveData.progressList == null)
                {
                    Debug.LogWarning("Save file empty or corrupted. Returning default data.");
                    return defaultList;
                }

                // Merge dữ liệu cũ vào danh sách mặc định
                foreach (var saved in saveData.progressList)
                {
                    // Tìm trong danh sách mặc định level tương ứng
                    var match = defaultList.Find(p =>
                        p.levelData != null &&
                        saved.levelData != null &&
                        p.levelData.levelIndex == saved.levelData.levelIndex
                    );

                    if (match != null)
                    {
                        match.bestScore = saved.bestScore;
                        match.starLevel = saved.starLevel;
                        match.isUnlocked = saved.isUnlocked;
                    }
                }

                Debug.Log($"Game progress merged successfully. Total Levels: {defaultList.Count}");
                return defaultList;
            }
            catch (Exception e)
            {
                Debug.LogWarning("Error loading save file, reset progress: " + e.Message);
                return defaultList;
            }
        }

        public static SettingsData LoadSetings()
        {
            try
            {
                if (!File.Exists(SettingsPath))
                {
                    Debug.Log("Save file not found. Using default data.");
                    return null;
                }
                string encrypted = File.ReadAllText(SettingsPath);
                string decrypted = Decrypt(encrypted, SecretKey);
                SettingsData settingsData = JsonUtility.FromJson<SettingsData>(decrypted);

                Debug.Log($"Game Load settings successfully");
                return settingsData;
            }
            catch (Exception e)
            {
                Debug.LogWarning("Error loading save file, reset progress: " + e.Message);
                return null;
            }

        }

        public static ResourceData LoadResource()
        {
            try
            {
                if (!File.Exists(ResourcePath))
                {
                    Debug.Log("Save file not found. Using default data.");
                    return null;
                }

                string encrypted = File.ReadAllText(ResourcePath);
                string decrypted = Decrypt(encrypted, SecretKey);
                ResourceData resourceData = JsonUtility.FromJson<ResourceData>(decrypted);
                Debug.Log($"Game Load resource successfully");
                return resourceData;
            }
            catch (Exception e)
            {
                Debug.LogWarning("Error loading save file, reset progress: " + e.Message);
                return null;
            }
        }

        // ==================== DELETE SAVE ====================
        public static void DeleteProgressSave()
        {
            if (File.Exists(ProgressPath))
            {
                File.Delete(ProgressPath);
                Debug.Log("Save file deleted.");
            }
        }

        public static void DeleteSettingSave()
        {
            if (File.Exists(SettingsPath))
            {
                File.Delete(SettingsPath);
                Debug.Log("Save file deleted.");
            }
        }

        public static void DeleteResourceSave()
        {
            if (File.Exists(ResourcePath))
            {
                File.Delete(ResourcePath);
                Debug.Log("Save file deleted.");
            }
        }

        // ==================== ENCRYPT / DECRYPT ====================
        private static string Encrypt(string plainText, string key)
        {
            byte[] data = Encoding.UTF8.GetBytes(plainText);
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] encrypted = new byte[data.Length];

            for (int i = 0; i < data.Length; i++)
                encrypted[i] = (byte)(data[i] ^ keyBytes[i % keyBytes.Length]);

            return Convert.ToBase64String(encrypted);
        }

        private static string Decrypt(string encryptedText, string key)
        {
            byte[] data = Convert.FromBase64String(encryptedText);
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] decrypted = new byte[data.Length];

            for (int i = 0; i < data.Length; i++)
                decrypted[i] = (byte)(data[i] ^ keyBytes[i % keyBytes.Length]);

            return Encoding.UTF8.GetString(decrypted);
        }
    }
}
