using System.IO;
using UnityEngine;

public static class SaveManager
{
    private static readonly string SavePath = Path.Combine(Application.persistentDataPath, "save.json");

    public static void Save(SaveData data)
    {
        File.WriteAllText(SavePath, JsonUtility.ToJson(data, true));
    }

    public static SaveData Load()
    {
        if (!File.Exists(SavePath))
            return new SaveData();

        return JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath));
    }

    public static void DeleteSave()
    {
        if (File.Exists(SavePath))
            File.Delete(SavePath);
    }
}
