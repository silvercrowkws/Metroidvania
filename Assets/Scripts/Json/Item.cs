using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    public string id;              // 고유 식별자 (예: "sword_01")
    public string itemName;
    public string description;
    public Sprite icon;
}