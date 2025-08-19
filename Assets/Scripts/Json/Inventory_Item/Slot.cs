using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class Slot : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler, IDragHandler, IPointerUpHandler
{
    private TMP_Text itemCountText;
    private Image itemImage;
    private InventoryViewer inventoryViewer;
    public ItemDataSO currentSaveItem { get; private set; }

    // 드래그 중인 아이템 이미지를 표시할 GameObject
    public static GameObject dragItemIcon;

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

    // 이 오브젝트위에서 마우스를 눌렀다면?
    // IPointerDownHandler에 정의되어있음
    public void OnPointerDown(PointerEventData eventData)
    {
        // 좌클릭이라면?
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            // 랜덤한좌표에 아이템 버리기 => 아이템 버리기가 아니라 선택지로 사용 or 버리기 표시
            //Inventory.Instance.RemoveItem(currentSaveItem, UnityEngine.Random.insideUnitCircle);

            // 드래그용 아이템 아이콘 생성
            // Canvas에 자식으로 생성
            GameObject canvas = transform.root.gameObject;
            dragItemIcon = new GameObject("DragItemIcon");
            dragItemIcon.transform.SetParent(canvas.transform);

            Image dragImage = dragItemIcon.AddComponent<Image>();
            dragImage.sprite = itemImage.sprite;
            dragImage.raycastTarget = false; // 드래그 아이콘이 다른 UI를 가리지 않도록 설정
            dragImage.rectTransform.sizeDelta = new Vector2(50, 50); // 아이콘 크기 설정

            // 원본 아이콘은 잠시 비활성화하여 드래그 중인 것처럼 보이게 함
            itemImage.enabled = false;

        }

        // 우클릭이라면
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            inventoryViewer.StartChange(this);
            Debug.Log("슬롯에서 아이템 우클릭");
        }
    }



    // 드래그 중일 때
    public void OnDrag(PointerEventData eventData)
    {
        if (dragItemIcon != null)
        {
            // 마우스 포인터의 위치를 드래그 아이콘의 위치로 설정
            dragItemIcon.transform.position = eventData.position;
        }
    }

    // 마우스 클릭이 끝났을 때
    public void OnPointerUp(PointerEventData eventData)
    {
        // 원본 아이콘을 다시 활성화
        itemImage.enabled = true;

        if (dragItemIcon != null)
        {
            Destroy(dragItemIcon);
            dragItemIcon = null;

            // 마우스가 떼어진 곳 아래에 어떤 UI도 없다면
            // EventSystem.IsPointerOverGameObject()는 UI 요소 위에 마우스가 있는지 확인하는 메서드입니다.
            if (eventData.pointerCurrentRaycast.gameObject == null)
            {
                Debug.Log("인벤토리 영역 밖으로 아이템을 버립니다.");
                // 아이템을 버리는 로직 실행 (예: RemoveItem 메서드 호출)
                //Inventory.Instance.RemoveItem(currentSaveItem, 1);
                Inventory.Instance.RemoveItem(currentSaveItem, UnityEngine.Random.insideUnitCircle);
            }
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