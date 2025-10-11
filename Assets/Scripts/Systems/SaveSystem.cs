using UnityEngine;
using System.IO;

namespace Game.Systems
{
    [System.Serializable]
    public class SaveData
    {
        public int selectedClass;
        public int playerLevel;
        public int playerHealth;
        public string[] inventoryItemIds;
        public int[] inventoryCounts;
    }

    public static class SaveSystem
    {
        private static string Path => System.IO.Path.Combine(Application.persistentDataPath, "save.json");

        public static void Save(SaveData data)
        {
            var json = JsonUtility.ToJson(data);
            File.WriteAllText(Path, json);
        }

        public static SaveData Load()
        {
            if (!File.Exists(Path)) return null;
            var json = File.ReadAllText(Path);
            return JsonUtility.FromJson<SaveData>(json);
        }

        public static void Delete()
        {
            if (File.Exists(Path)) File.Delete(Path);
        }
    }
}
