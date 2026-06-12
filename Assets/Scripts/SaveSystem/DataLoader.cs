using System;
using System.IO;
using UnityEngine;

public static class DataLoader
{
    private static readonly string SAVE_FOLDER =
        Path.Combine(Application.dataPath, "WorldData");

    public static void Init()
    {
        if (!Directory.Exists(SAVE_FOLDER))
        {
            Directory.CreateDirectory(SAVE_FOLDER);
        }
    }

    private static string GetPath(string fileName)
    {
        return Path.Combine(SAVE_FOLDER, fileName);
    }

    //overwrite or create new
    public static void SaveJson(string fileName, object data)
    {
        if (string.IsNullOrEmpty(fileName) || data == null)
            return;

        Init();

        string path = GetPath(fileName);
        string json = JsonUtility.ToJson(data, true);

        try
        {
            File.WriteAllText(path, json); // overwrites if exists, creates if not
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

        string path = GetPath(fileName);

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
}