using System.Collections.Generic;

[System.Serializable]
public class Inventory
{
    public List<ItemSlot> slots = new();

    public void AddItem(string itemId, int count = 1)
    {
        var slot = slots.Find(s => s.itemId == itemId);
        if (slot != null)
        {
            slot.count += count;
        }
        else
        {
            slots.Add(new ItemSlot { itemId = itemId, count = count });
        }
    }

    public void RemoveItem(string itemId, int count = 1)
    {
        var slot = slots.Find(s => s.itemId == itemId);
        if (slot == null) return;

        slot.count -= count;
        if (slot.count <= 0)
            slots.Remove(slot);
    }
}
