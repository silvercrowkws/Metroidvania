using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuantityPanel : MonoBehaviour
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
    /// 버릴 아이템 수량
    /// </summary>
    public int quantityItemCount;

    /// <summary>
    /// 확인 버튼
    /// </summary>
    Button confirmButton;

    /// <summary>
    /// 취소 버튼
    /// </summary>
    Button cancelButton;

    /// <summary>
    /// 확인 버튼 클릭으로 아이템을 버리라고 알리는 델리게이트
    /// </summary>
    public Action onThrowingItem;

    private void Awake()
    {
        Transform child = transform.GetChild(1);

        inputField = child.GetComponent<TMP_InputField>();

        quantityText = inputField.transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>();

        child = transform.GetChild(2);
        confirmButton = child.GetComponent <Button>();      // 확인 버튼
        confirmButton.onClick.AddListener(Confirm);

        child = transform.GetChild(3);
        cancelButton = child.GetComponent <Button>();       // 취소 버튼
        cancelButton.onClick.AddListener(Cancel);
    }

    private void OnEnable()
    {
        // 프레임 끝나고 실행해야 정상적으로 포커스됨
        StartCoroutine(SetFocus());
    }

    public void Show(int maxCount)
    {
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

    /// <summary>
    /// 아이템 버리기 확인 버튼
    /// </summary>
    /*private void Confirm()
    {
        Debug.Log("아이템 버리기");
        onThrowingItem?.Invoke();
        this.gameObject.SetActive(false);
    }*/

    private Slot targetSlot;

    public void SetTargetSlot(Slot slot)
    {
        targetSlot = slot;
    }

    private void Confirm()
    {
        Debug.Log("아이템 버리기");
        if (targetSlot != null)
        {
            int.TryParse(inputField.text, out quantityItemCount);
            Inventory.Instance.RemoveItem(targetSlot.currentSaveItem, UnityEngine.Random.insideUnitCircle, quantityItemCount);
        }
        this.gameObject.SetActive(false);
    }

    /// <summary>
    /// 아이템 버리기 취소 버튼
    /// </summary>
    private void Cancel()
    {
        Debug.Log("아이템 버리기 취소");
        this.gameObject.SetActive(false);
    }
}
