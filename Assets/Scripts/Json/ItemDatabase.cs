using System.Collections.Generic;
using UnityEngine;

public static class ItemDatabase
{
    private static Dictionary<string, Item> itemDict = new();

    private static bool initialized = false;

    public static void Init()
    {
        if (initialized) return;

        Item[] items = Resources.LoadAll<Item>("Items");  // Resources/Items 폴더의 모든 SO 로드
        foreach (Item item in items)
        {
            if (!itemDict.ContainsKey(item.id))
                itemDict.Add(item.id, item);
        }

        initialized = true;
    }

    public static Item GetItem(string id)
    {
        Init();
        itemDict.TryGetValue(id, out Item item);
        return item;
    }
}
