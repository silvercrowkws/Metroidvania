using System;
using System.Collections.Generic;
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

    /// <summary>
    /// 판매 수량 개수 입력하는 패널
    /// </summary>
    QuantityPanel quantityPanel;

    /// <summary>
    /// 드래그 중인 아이템 이미지를 표시할 GameObject
    /// </summary>
    public static GameObject dragItemIcon;

    /// <summary>
    /// 버튼들 상위 오브젝트
    /// </summary>
    GameObject buttons;

    /// <summary>
    /// 사용 버튼
    /// </summary>
    Button useButton;

    /// <summary>
    /// 취소 버튼
    /// </summary>
    Button cancelButton;

    /// <summary>
    /// 현재 활성화된 슬롯의 버튼을 추적할 static 변수
    /// </summary>
    private static Slot activeSlotWithButtons = null;

    /// <summary>
    /// 다른 곳 클릭 시 알림 받을 static 델리게이트
    /// </summary>
    public static Action OnAnyUIClickedOutside;

    /// <summary>
    /// 인벤토리 패널
    /// </summary>
    InventoryPanel inventoryPanel;

    Player_Test player_test;

    private void Awake()
    {
        itemCountText = GetComponentInChildren<TMP_Text>();
        itemImage = transform.Find("ItemImage").GetComponent<Image>();
        inventoryViewer = FindObjectOfType<InventoryViewer>();

        buttons = transform.GetChild(2).gameObject;
        buttons.gameObject.SetActive(false);

        OnAnyUIClickedOutside += HideButtons;

        useButton = buttons.transform.GetChild(0).GetComponent<Button>();
        useButton.onClick.AddListener(OnUseButton);

        cancelButton = buttons.transform.GetChild(1).GetComponent<Button>();
        cancelButton.onClick.AddListener(OnCancelButton);
    }

    private void Start()
    {
        // 비활성화된 오브젝트도 찾기 위해 이렇게 함
        quantityPanel = FindAnyObjectByType<QuantityPanel>(FindObjectsInactive.Include);

        if (quantityPanel != null )
        {
            quantityPanel.gameObject.SetActive(false);
        }
        else
        {
            Debug.Log("quantityPanel을 못찾은건가");
        }

        inventoryPanel = FindAnyObjectByType<InventoryPanel>();
        inventoryPanel.onButtonsActiveFalse += OnButtonsActiveFalse;

        player_test = GameManager.Instance.Player_Test;
    }

    private void OnDestroy()
    {
        OnAnyUIClickedOutside -= HideButtons;

        if (inventoryPanel != null)
            inventoryPanel.onButtonsActiveFalse -= OnButtonsActiveFalse;
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
                //이벤트 구독을 먼저 해제 -> Inventory.Clear() 루프의 다음 호출 때 이 슬롯에 접근하지 않기 위해
                if (Inventory.Instance != null)
                {
                    Inventory.Instance.OnItemChanged -= HandleItemAdded;
                }

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

    /*private void OnDestroy()
    {
        if (Inventory.Instance != null)
        {
            //이벤트 구독 해제
            Inventory.Instance.OnItemChanged -= HandleItemAdded;
        }
    }*/

    // 이 오브젝트위에서 마우스를 눌렀다면?
    // IPointerDownHandler에 정의되어있음
    public void OnPointerDown(PointerEventData eventData)
    {
        // 슬롯이 클릭되면, 우선 모든 슬롯의 버튼 닫기
        OnAnyUIClickedOutside?.Invoke();

        // 좌클릭이라면?
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            // 랜덤한좌표에 아이템 버리기 => 아이템 버리기가 아니라 선택지로 사용 or 버리기 표시
            //Inventory.Instance.RemoveItem(currentSaveItem, UnityEngine.Random.insideUnitCircle);

            int itemCount = Inventory.Instance.GetItemCount(currentSaveItem);
            Debug.Log($"좌클릭한 아이템의 수량: {itemCount}");

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

            // 우클릭한 아이템의 이름 출력
            Debug.Log($"슬롯에서 아이템 우클릭: **{currentSaveItem.ItemName}**"); // currentSaveItem의 ItemName에 접근

            // 소모품인 경우에만 사용/취소 버튼 활성화시키록 변경
            if (currentSaveItem.Type == ItemType.Consumable)
            {
                // 여기서 비활성화된 버튼들 활성화 시키고
                buttons.gameObject.SetActive(true);
                activeSlotWithButtons = this;
                // 사용, 취소 이런 버튼들
            }

            /*// 여기서 비활성화된 버튼들 활성화 시키고
            buttons.gameObject.SetActive(true);
            activeSlotWithButtons = this;
            // 사용, 취소 이런 버튼들*/
        }
    }

    private void HideButtons()
    {
        if (buttons != null && buttons.activeSelf)
        {
            buttons.SetActive(false);
            if (activeSlotWithButtons == this)
                activeSlotWithButtons = null;
        }
    }

    /// <summary>
    /// 드래그 중일 때
    /// </summary>
    /// <param name="eventData"></param>
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

            /*// 마우스가 떼어진 곳 아래에 UI가 있다면
            // EventSystem.IsPointerOverGameObject()는 UI 요소 위에 마우스가 있는지 확인하는 메서드입니다.
            if (eventData.pointerCurrentRaycast.gameObject != null)
            {
                // Raycast가 감지한 오브젝트가 InventoryPanel 태그를 가지고 있는지 확인합니다.
                // 최상위 부모를 확인하는 것이 더 안전하지만, 간단하게는 Raycast된 오브젝트를 바로 확인해도 됩니다.
                GameObject hitObject = eventData.pointerCurrentRaycast.gameObject;

                // hitObject가 InventoryPanel 태그를 가지고 있지 않다면
                if (!hitObject.CompareTag("InventoryPanel"))
                {
                    Debug.Log("인벤토리 패널 밖으로 아이템을 버립니다.");
                    // 아이템 버리기 로직 실행
                    //Inventory.Instance.RemoveItem(currentSaveItem, UnityEngine.Random.insideUnitCircle);

                    ThrowingItem();
                }
                else
                {
                    Debug.Log("아이템을 인벤토리 패널 위에 놓았습니다.");
                    // 여기에 인벤토리 내의 다른 슬롯으로 옮기는 로직 추가
                }
            }
            else // UI가 없는 빈 공간에 놓았을 때
            {
                Debug.Log("UI가 없는 곳에 아이템을 버립니다.");
                // 아이템 버리기 로직 실행
                //Inventory.Instance.RemoveItem(currentSaveItem, UnityEngine.Random.insideUnitCircle);
                ThrowingItem();
            }*/


            // 마우스가 InventoryPanel 위에 있다면
            if (IsPointerOverInventoryPanel(eventData))
            {
                Debug.Log("아이템을 인벤토리 패널 위에 놓았습니다.");

                inventoryViewer.StartChange(this);
            }
            else // InventoryPanel이 아닌 다른 곳에 놓았을 때
            {
                Debug.Log("인벤토리 패널 밖으로 아이템을 버립니다.");
                ShowQuantityPanel();

                //ThrowingItem();
            }
        }
    }

    /// <summary>
    /// 마우스 포인터가 겹쳐 있는 UI 요소들 중
    /// 'InventoryPanel' 태그를 가진 오브젝트가 있는지 확인하는 함수
    /// </summary>
    /// <param name="eventData"> PointerEventData (마우스 이벤트 데이터)</param>
    /// <returns> 마우스가 InventoryPanel 위에 있다면 true를 반환</returns>
    private bool IsPointerOverInventoryPanel(PointerEventData eventData)
    {
        // Raycast 결과를 담을 리스트
        List<RaycastResult> results = new List<RaycastResult>();

        // 현재 EventSystem을 가져옵니다.
        EventSystem.current.RaycastAll(eventData, results);

        // 결과를 순회하면서 InventoryPanel 태그를 가진 오브젝트를 찾고
        foreach (RaycastResult result in results)
        {
            // 겹쳐있는 UI 중 하나라도 InventoryPanel 태그를 가지고 있다면 true 반환
            if (result.gameObject.CompareTag("InventoryPanel"))
            {
                return true;
            }
        }

        // 모든 UI를 검사했지만 InventoryPanel 태그를 가진 오브젝트가 없으면 false 반환
        return false;
    }

    /*/// <summary>
    /// 아이템 버리기를 시도하는 함수
    /// </summary>
    private void ThrowingItem()
    {
        Inventory.Instance.RemoveItem(currentSaveItem, UnityEngine.Random.insideUnitCircle, quantityPanel.quantityItemCount);
    }*/

    /// <summary>
    /// 포인터가 가리키는 슬롯 설정
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        inventoryViewer.SetPointerSlot(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        inventoryViewer.SetPointerSlot(null);
    }

    
    private void ShowQuantityPanel()
    {
        quantityPanel.SetTargetSlot(this); // 반드시 현재 슬롯을 패널에 전달
        quantityPanel.gameObject.SetActive(true);
        int maxCount = Inventory.Instance.GetItemCount(currentSaveItem);
        quantityPanel.Show(maxCount);
    }

    private void OnButtonsActiveFalse()
    {
        buttons.gameObject.SetActive(false);
    }

    /// <summary>
    /// 아이템 사용 버튼
    /// </summary>
    private void OnUseButton()
    {
        // 사용 버튼을 누르면 아이템의 개수를 1개 감소시키고,
        // 아이템의 효과 발동
        // 하라고 델리게이트?

        // 1. 아이템의 개수를 1개 감소
        Inventory.Instance.UseItem(currentSaveItem);

        // 2. 아이템의 효과 발동 (주석 처리된 델리게이트 부분, 추후 구현)
        switch (currentSaveItem.ItemName)
        {
            case "Health Potion":
                Debug.Log("힐 포션 아이템 발동");
                // 플레이어의 체력 일정량 회복
                player_test = GameManager.Instance.Player_Test;
                player_test.HP += 50;
                break;
            case "Bread":
                Debug.Log("빵 아이템 발동");
                player_test = GameManager.Instance.Player_Test;
                player_test.HP += 10;
                break;
        }

        // 사용 후 버튼 숨기기
        buttons.gameObject.SetActive(false);

        //Debug.Log($"슬롯에서 {currentSaveItem.ItemName} 사용 시도.");
    }

    /// <summary>
    /// 아이템 취소 버튼
    /// </summary>
    private void OnCancelButton()
    {
        buttons.gameObject.SetActive(false);
    }
}