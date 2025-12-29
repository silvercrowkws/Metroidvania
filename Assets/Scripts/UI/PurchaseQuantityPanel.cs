using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PurchaseQuantityPanel : MonoBehaviour
{
    /// <summary>
    /// 인풋 필드
    /// </summary>
    private TMP_InputField inputField;

    /// <summary>
    /// 입력할 수량 텍스트 매쉬 프로
    /// </summary>
    TextMeshProUGUI quantityText;

    /// <summary>
    /// 선택 아이템 수량
    /// </summary>
    public int quantityItemCount;

    /// <summary>
    /// 아이템의 개당 가격
    /// </summary>
    int itemPrice;

    /// <summary>
    /// 살 수 있는 최대 수량(아이템 몇개 가지고 있는지)
    /// </summary>
    int maxItemCount;

    /// <summary>
    /// 확인 버튼
    /// </summary>
    Button confirmButton;

    /// <summary>
    /// 확인 버튼의 텍스트
    /// </summary>
    TextMeshProUGUI confirmText;

    /// <summary>
    /// 취소 버튼
    /// </summary>
    Button cancelButton;

    int totalCost;

    Player_Test player_test;

    // 1. 선택된 아이템 데이터를 저장할 변수 추가
    private ItemDataSO selectedItemData;

    /// <summary>
    /// 아이템 구매 여부를 알리는 델리게이트
    /// </summary>
    public Action<bool> onItemPurchased;

    private void Awake()
    {
        Transform child = transform.GetChild(3);
        cancelButton = child.GetComponent<Button>();
        cancelButton.onClick.AddListener(Cancel);

        child = transform.GetChild(1);

        inputField = child.GetComponent<TMP_InputField>();

        // 오직 양의 정수(0~9)만 입력 가능하게 설정 (마이너스, 문자 차단)
        inputField.contentType = TMP_InputField.ContentType.IntegerNumber;

        // (선택) 아예 숫자 Validation을 Digit으로 고정
        inputField.characterValidation = TMP_InputField.CharacterValidation.Digit;

        quantityText = inputField.transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>();

        child = transform.GetChild(2);
        confirmButton = child.GetComponent<Button>();      // 확인 버튼
        confirmButton.onClick.AddListener(Confirm);

        confirmText = child.GetComponentInChildren<TextMeshProUGUI>();

        // 인풋필드 값이 바뀔 때마다 실행될 함수 등록
        inputField.onValueChanged.AddListener(OnInputFieldValueChanged);
    }

    private void Start()
    {
        player_test = GameManager.Instance.Player_Test;
    }

    private void OnEnable()
    {
        // 프레임 끝나고 실행해야 정상적으로 포커스됨
        StartCoroutine(SetFocus());
    }

    /// <summary>
    /// 인풋필드에 최대 개수를 미리 입력해 놓는 함수
    /// </summary>
    /// <param name="maxCount"></param>
    public void Show(ItemDataSO data, int maxCount, int price)
    {
        // 전달받은 데이터 저장
        selectedItemData = data;

        // 아이템 최대 수량 기록
        maxItemCount = maxCount;

        // 아이템 가격 기록
        itemPrice = price;

        quantityItemCount = maxCount;
        gameObject.SetActive(true);

        // UI에 즉시 반영
        inputField.text = maxCount.ToString();
    }

    private IEnumerator SetFocus()
    {
        yield return null; // 한 프레임 대기
        inputField.Select();
        inputField.ActivateInputField();
    }

    private void Cancel()
    {
        onItemPurchased?.Invoke(false);
        this.gameObject.SetActive(false);
    }

    /// <summary>
    /// 아이템 구매 버튼
    /// </summary>
    public void Confirm()
    {
        // 1. 인풋 필드에서 현재 입력된 숫자를 가져옴
        if (int.TryParse(inputField.text, out int buyCount))
        {
            // OnInputFieldValueChanged에서 계산된 totalCost를 그대로 사용하거나 여기서 다시 계산
            totalCost = buyCount * itemPrice;

            // 2. 총액이 0 초과이고, 플레이어의 소지금이 총액 이상이면
            if (player_test.Money >= totalCost && buyCount > 0)
            {
                // 돈 차감
                player_test.Money -= totalCost;

                // 인벤토리에 아이템과 개수 추가
                Inventory.Instance.AddItem(selectedItemData, buyCount);

                Debug.Log($"{selectedItemData.ItemName}을 {buyCount}개 구매했습니다.");

                // 3. 구매 완료 후 패널 닫기
                onItemPurchased?.Invoke(true);
                gameObject.SetActive(false);
            }
            else
            {
                Debug.Log("플레이어의 돈이 부족하거나 수량이 0입니다.");
            }
        }
    }

    private void OnInputFieldValueChanged(string value)
    {
        // 입력된 문자열을 정수로 변환 (실패 시 0)
        if (int.TryParse(value, out int currentCount))
        {
            // 최대 수량을 넘지 못하도록 제한 (선택 사항)
            if (currentCount > maxItemCount)
            {
                currentCount = maxItemCount;
                inputField.text = maxItemCount.ToString();
            }

            // 텍스트 업데이트 (예: 1000 G)
            UpdateConfirmButtonText(currentCount);
        }
        else
        {
            confirmText.text = "0 G";
        }
    }

    /// <summary>
    /// 확인 버튼의 텍스트를 업데이트하는 별도 함수
    /// </summary>
    /// <param name="count"></param>
    private void UpdateConfirmButtonText(int count)
    {
        totalCost = count * itemPrice;
        confirmText.text = $"{totalCost} G 구매"; // 원하는 포맷으로 변경 가능
    }
}
