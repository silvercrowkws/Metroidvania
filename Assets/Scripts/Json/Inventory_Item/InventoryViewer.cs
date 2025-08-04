using System;
using UnityEngine;
using UnityEngine.UI;

public class InventoryViewer : MonoBehaviour
{
    [SerializeField] private Slot slotPrefab;
    [SerializeField] private Transform slotParent;
    [SerializeField] private Image changeSlotUI;

    private bool isChanging;
    private Slot currentChangeSlot;

    /// <summary>
    /// 현재 포인터가 가리키고있는 슬롯
    /// </summary>
    private Slot pointerUpSlot;

    private void Start()
    {
        //이벤트 구독
        Inventory.Instance.OnNewItemAdded += HandleItemAdded;
    }

    private void HandleItemAdded(ItemDataSO obj)
    {
        Instantiate(slotPrefab, slotParent).Init(obj);
    }

    //Slot설정
    public void SetPointerSlot(Slot slot)
    {
        pointerUpSlot = slot;
    }

    private void Update()
    {
        if (Input.GetMouseButtonUp(1) && isChanging)
        {
            if (pointerUpSlot != null)
            {
                SwapSlot(pointerUpSlot);
            }

            changeSlotUI.gameObject.SetActive(false);
            currentChangeSlot = null;
            isChanging = false;
        }

        if (isChanging)
        {
            changeSlotUI.transform.position = Input.mousePosition;
        }
    }

    public void StartChange(Slot slot)
    {
        isChanging = true;
        changeSlotUI.gameObject.SetActive(true);
        changeSlotUI.sprite = slot.currentSaveItem.ItemSprite;
        currentChangeSlot = slot;
    }

    public void SwapSlot(Slot slot)
    {
        //같은 슬롯끼리 바꾸려고 한다면 못하도록
        if (slot == currentChangeSlot) return;

        //두 아이템을 바꿔주기
        var oldData = slot.currentSaveItem;

        slot.Init(currentChangeSlot.currentSaveItem);
        currentChangeSlot.Init(oldData);

        currentChangeSlot = null;
    }
}