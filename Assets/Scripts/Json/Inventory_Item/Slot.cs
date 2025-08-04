using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class Slot : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    private TMP_Text itemCountText;
    private Image itemImage;
    private InventoryViewer inventoryViewer;
    public ItemDataSO currentSaveItem { get; private set; }

    private void Awake()
    {
        itemCountText = GetComponentInChildren<TMP_Text>();
        itemImage = transform.Find("ItemImage").GetComponent<Image>();
        inventoryViewer = FindObjectOfType<InventoryViewer>();
    }

    public void Init(ItemDataSO item)
    {
        currentSaveItem = item;
        itemCountText.text = Inventory.Instance.GetItemCount(item).ToString();
        itemImage.sprite = item.ItemSprite;

        //이벤트 구독
        Inventory.Instance.OnItemChanged += HandleItemAdded;
    }

    private void HandleItemAdded(ItemDataSO item, int count)
    {
        if (currentSaveItem == item)
        {
            //카운트가 0이라면?
            if (count == 0)
            {
                //슬롯 없에기
                Destroy(gameObject);
            }
            //아니라면?
            else
            {
                //텍스트 갱신
                itemCountText.text = count.ToString();
            }

        }
    }

    private void OnDestroy()
    {
        if (Inventory.Instance != null)
        {
            //이벤트 구독 해제
            Inventory.Instance.OnItemChanged -= HandleItemAdded;
        }
    }

    //이 오브젝트위에서 마우스를 눌렀다면?
    //IPointerDownHandler에 정의되어있음
    public void OnPointerDown(PointerEventData eventData)
    {
        //좌클릭이라면?
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            //랜덤한좌표에 아이템 버리기
            Inventory.Instance.RemoveItem(currentSaveItem, UnityEngine.Random.insideUnitCircle);
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            inventoryViewer.StartChange(this);
        }
    }

    //포인터가 가리키는 슬롯 설정
    public void OnPointerEnter(PointerEventData eventData)
    {
        inventoryViewer.SetPointerSlot(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        inventoryViewer.SetPointerSlot(null);
    }
}