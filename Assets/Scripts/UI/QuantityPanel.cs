using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;

public class QuantityPanel : MonoBehaviour
{
    // 이 오브젝트는 인스펙터에서 비활성화 하고 시작
    // Slot에서만 찾고 있고, quantityPanel = FindAnyObjectByType<QuantityPanel>(FindObjectsInactive.Include); 로 비활성화 한 상태에서도 찾기 때문에 상관 없음)

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
    /// 버릴 수 있는 최대 수량(아이템 몇개 가지고 있는지)
    /// </summary>
    int maxItemCount;

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

    Player player;

    Player_Test player_test;

    /// <summary>
    /// 벽을 감지하기 위한 레이어 마스크
    /// </summary>
    [SerializeField]
    private LayerMask wallLayer;

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
        player = GameManager.Instance.Player;
        if (player == null)
        {
            //Debug.Log("player는 없고");
            player_test = GameManager.Instance.Player_Test;

            if (player_test == null)
            {
                //Debug.Log("player_test도 없는데?");
            }
            else
            {
                //Debug.Log("player_test는 있는데..");
            }
        }

        // 프레임 끝나고 실행해야 정상적으로 포커스됨
        StartCoroutine(SetFocus());
    }

    /// <summary>
    /// 인풋필드에 최대 개수를 미리 입력해 놓는 함수
    /// </summary>
    /// <param name="maxCount"></param>
    public void Show(int maxCount)
    {
        // 아이템 최대 수량 기록
        maxItemCount = maxCount;

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

    /// <summary>
    /// 아이템을 버릴 방향(플레이어가 보는 위치)
    /// </summary>
    Vector2 throwDirection;

    /// <summary>
    /// 아이템 버리기 확인 버튼
    /// </summary>
    private void Confirm()
    {
        // 인풋 텍스트를 quantityItemCount에 반영
        int.TryParse(inputField.text, out quantityItemCount);

        if (quantityItemCount > maxItemCount)
        {
            Debug.Log("판매하려는 개수가 최대 개수보다 크다");
            return;
        }

        // 아이템을 버릴 위치
        Vector2 dropPosition;

        Debug.Log("아이템 버리기");
        if (targetSlot != null)
        {
            if (player == null)
            {
                if(player_test == null)
                {
                    Debug.Log("아이템 버리려는데 플레이어가 없다.");
                }
                else
                {
                    // 플레이어의 방향(local scale)을 가져와서 아이템을 버릴 위치 계산
                    throwDirection = new Vector2(player_test.transform.localScale.x, 0).normalized;
                }
            }
            else
            {
                // 플레이어의 방향(local scale)을 가져와서 아이템을 버릴 위치 계산
                throwDirection = new Vector2(player.transform.localScale.x, 0).normalized;
            }

            // 아이템을 버릴 위치를 플레이어 위치 + (방향 * 거리)로 설정
            if (player == null)
            {
                dropPosition = (Vector2)player_test.transform.position + throwDirection + new Vector2(0, 1);
            }
            else
            {
                dropPosition = (Vector2)player.transform.position + throwDirection + new Vector2(0, 1);
            }

            // 최종적으로 계산된 dropPosition 위치에 아이템을 버림
            //int.TryParse(inputField.text, out quantityItemCount);
            //Inventory.Instance.RemoveItem(targetSlot.currentSaveItem, UnityEngine.Random.insideUnitCircle, quantityItemCount);
            Inventory.Instance.RemoveItem(targetSlot.currentSaveItem, dropPosition, quantityItemCount);
        }
        else
        {
            Debug.Log("targetSlot이 없는데???");
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
