using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public GameObject slotPrefab;
    public Transform slotParent;

    public void RefreshUI(List<ItemSlot> slots)
    {
        foreach (Transform child in slotParent)
            Destroy(child.gameObject);

        foreach (var slot in slots)
        {
            var item = ItemDatabase.GetItem(slot.itemId);
            if (item == null) continue;

            GameObject go = Instantiate(slotPrefab, slotParent);
            var icon = go.transform.Find("Icon").GetComponent<UnityEngine.UI.Image>();
            var countText = go.transform.Find("CountText").GetComponent<UnityEngine.UI.Text>();

            icon.sprite = item.icon;
            countText.text = slot.count.ToString();
        }
    }
}
