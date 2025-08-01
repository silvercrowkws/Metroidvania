using System.IO;
using UnityEngine;

public static class InventorySaveSystem
{
    private static string path = Path.Combine(Application.persistentDataPath, "inventory.json");

    public static void Save(Inventory inventory)
    {
        string json = JsonUtility.ToJson(inventory, true);
        File.WriteAllText(path, json);
        Debug.Log("Inventory saved to " + path);
    }

    public static Inventory Load()
    {
        if (!File.Exists(path))
        {
            Debug.Log("No saved inventory found, returning new.");
            return new Inventory();
        }

        string json = File.ReadAllText(path);
        return JsonUtility.FromJson<Inventory>(json);
    }
}
