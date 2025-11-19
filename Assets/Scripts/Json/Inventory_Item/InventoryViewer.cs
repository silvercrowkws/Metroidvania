using System;
using UnityEngine;
using UnityEngine.UI;

public class InventoryViewer : Singleton<InventoryViewer>
{
    [SerializeField] private Slot slotPrefab;
    [SerializeField] private Transform slotParent;
    [SerializeField] private Image changeSlotUI;

    private bool isChanging;
    private Slot currentChangeSlot;

    /// <summary>
    /// 인벤토리 슬롯의 순서가 바뀌었을 때 게임매니저에 알리는 델리게이트
    /// </summary>
    public Action OnInventoryOrderChanged;

    /// <summary>
    /// 현재 포인터가 가리키고있는 슬롯
    /// </summary>
    private Slot pointerUpSlot;

    private void Awake()
    {
        var others = FindObjectsOfType<InventoryViewer>();
        if (others.Length > 1)
        {
            // 이미 다른 인스턴스가 존재하면 자신을 파괴하고 초기화 중단
            Destroy(gameObject);
            return;
        }

        // 씬 전환 시 이 게임오브젝트가 파괴되지 않도록 설정
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // "InventoryPanel" 태그를 가진 오브젝트를 찾아 부모로 설정
        GameObject panelObject = GameObject.FindGameObjectWithTag("InventoryPanel");

        if (panelObject != null)
        {
            // InventoryPanel 오브젝트의 Transform을 slotParent로 설정
            slotParent = panelObject.transform;
            Debug.Log("슬롯 부모(slotParent)를 InventoryPanel 태그를 가진 오브젝트로 성공적으로 설정했습니다.");
        }
        else
        {
            Debug.LogError("씬에서 'InventoryPanel' 태그를 가진 오브젝트를 찾을 수 없습니다! 슬롯 생성이 불가능합니다.");
        }

        //이벤트 구독
        Inventory.Instance.OnNewItemAdded += HandleItemAdded;
    }

    private void HandleItemAdded(ItemDataSO obj)
    {
        Instantiate(slotPrefab, slotParent).Init(obj);
    }

    // Slot설정
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
        
        // 슬롯 순서가 변경되었음을 델리게이트로 알림
        OnInventoryOrderChanged?.Invoke();        
    }

    /// <summary>
    /// 현재 UI에 있는 모든 슬롯들을 순서대로 가져옵니다.
    /// </summary>
    /// <returns>UI 순서대로 정렬된 슬롯의 배열</returns>
    public Slot[] GetSlots()
    {
        // slotParent의 자식으로 있는 모든 Slot 컴포넌트를 순서대로 가져옵니다.
        return slotParent.GetComponentsInChildren<Slot>();
    }
}