using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace CandyProject
{
    [Serializable]
    public class SaveData
    {
        public List<LevelProgress> progressList;
    }

    public static class SaveSystem
    {
        private const string SaveFileName = "save.dat";
        private const string SecretKey = "CandyCrushSecretKey2025"; // key mã hóa

        private static string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

        // ==================== SAVE ====================
        public static void SaveProgress(List<LevelProgress> levelProgressList)
        {
            try
            {
                SaveData saveData = new SaveData { progressList = levelProgressList };

                string json = JsonUtility.ToJson(saveData);
                string encrypted = Encrypt(json, SecretKey);

                File.WriteAllText(SavePath, encrypted);
                Debug.Log($"Game progress saved at: {SavePath}");
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to save progress: " + e.Message);
            }
        }

        // ==================== LOAD ====================
        public static List<LevelProgress> LoadProgress(List<LevelProgress> defaultList)
        {
            try
            {
                if (!File.Exists(SavePath))
                {
                    Debug.Log("Save file not found. Using default data.");
                    return defaultList;
                }

                string encrypted = File.ReadAllText(SavePath);
                string decrypted = Decrypt(encrypted, SecretKey);
                SaveData saveData = JsonUtility.FromJson<SaveData>(decrypted);

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


        // ==================== DELETE SAVE ====================
        public static void DeleteSave()
        {
            if (File.Exists(SavePath))
            {
                File.Delete(SavePath);
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
