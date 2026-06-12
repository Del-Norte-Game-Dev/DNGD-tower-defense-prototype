using System;
using System.IO;
using UnityEngine;

public static class SaveSystem
{
    [Serializable]
    public class SaveFile
    {
        public string timestamp;
        public string data;
    }

    private static readonly string SAVE_FOLDER =
        Path.Combine(Application.persistentDataPath, "Saves");

    public static void Init()
    {
        if (!Directory.Exists(SAVE_FOLDER))
        {
            Directory.CreateDirectory(SAVE_FOLDER);
        }
    }

    public static void Save(string saveString)
    {
        Init(); // ensure folder exists

        string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string fileName = $"save_{timestamp}.json";
        string saveFilePath = Path.Combine(SAVE_FOLDER, fileName);

        SaveFile saveFile = new SaveFile
        {
            timestamp = timestamp,
            data = saveString
        };

        string json = JsonUtility.ToJson(saveFile, true);

        try
        {
            File.WriteAllText(saveFilePath, json);
            Debug.Log($"Saved to: {saveFilePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Save failed: {e}");
        }
    }

    public static string LoadMostRecent()
    {
        Init();

        DirectoryInfo dir = new DirectoryInfo(SAVE_FOLDER);
        FileInfo[] files = dir.GetFiles("*.json");

        if (files.Length == 0)
            return null;

        FileInfo mostRecent = null;

        foreach (var file in files)
        {
            if (mostRecent == null || file.LastWriteTime > mostRecent.LastWriteTime)
            {
                mostRecent = file;
            }
        }

        return LoadFromFile(mostRecent.FullName);
    }

    public static string Load(string timestamp)
    {
        Init();

        string fileName = $"save_{timestamp}.json";
        string path = Path.Combine(SAVE_FOLDER, fileName);

        if (!File.Exists(path))
        {
            Debug.LogWarning("Save not found: " + path);
            return null;
        }

        return LoadFromFile(path);
    }

    public static void SaveJson(string fileName, object data)
    {
        if (string.IsNullOrEmpty(fileName) || data == null)
            return;

        Init();
        string path = Path.Combine(SAVE_FOLDER, fileName);
        string json = JsonUtility.ToJson(data, true);

        try
        {
            File.WriteAllText(path, json);
            Debug.Log($"Saved JSON to: {path}");
        }
        catch (Exception e)
        {
            Debug.LogError($"SaveJson failed: {e}");
        }
    }

    public static T LoadJson<T>(string fileName) where T : class
    {
        if (string.IsNullOrEmpty(fileName))
            return null;

        Init();
        string path = Path.Combine(SAVE_FOLDER, fileName);
        if (!File.Exists(path))
        {
            Debug.LogWarning("JSON file not found: " + path);
            return null;
        }

        try
        {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<T>(json);
        }
        catch (Exception e)
        {
            Debug.LogError($"LoadJson failed: {e}");
            return null;
        }
    }

    private static string LoadFromFile(string path)
    {
        try
        {
            string json = File.ReadAllText(path);
            SaveFile saveFile = JsonUtility.FromJson<SaveFile>(json);
            return saveFile.data;
        }
        catch (Exception e)
        {
            Debug.LogError($"Load failed: {e}");
            return null;
        }
    }
}