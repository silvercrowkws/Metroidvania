using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    [field: SerializeField] public ItemDataSO itemData { get; private set; }

    BoxCollider2D col;

    // 👇 추가: 아이템이 주워질 수 있는지 여부 플래그
    private bool canBePickedUp = false;

    // 👇 추가: 주워질 수 있게 되기까지의 딜레이 시간 (예: 0.5초)
    private const float PICKUP_DELAY = 0.5f;
    private void Awake()
    {
        col = GetComponent<BoxCollider2D>();

        // 아이템이 생성될 때 바로 주워지지 않도록 딜레이 코루틴 시작
        StartCoroutine(EnablePickupAfterDelay());
    }

    // 👇 추가: 딜레이 후 줍기 활성화 코루틴
    private IEnumerator EnablePickupAfterDelay()
    {
        // PICKUP_DELAY만큼 기다립니다.
        yield return new WaitForSeconds(PICKUP_DELAY);
        // 딜레이가 끝나면 주워질 수 있도록 설정
        canBePickedUp = true;
    }

    //디버그용 코드
    /*private void OnMouseDown()
    {
        if (Inventory.Instance != null)
        {
            Inventory.Instance.AddItem(this);
            //Debug.Log("아이템 클릭");
        }
    }*/

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // canBePickedUp이 true일 때만 줍기 허용
        if (canBePickedUp && collision.CompareTag("Player"))
        {
            Debug.Log("아이템이 플레이어와 충돌");
            Inventory.Instance.AddItem(this);
        }
    }

    /*private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("플레이어와 충돌");
            Inventory.Instance.AddItem(this);
        }
    }*/

#if UNITY_EDITOR

    //itemData설정시 자동으로 스프라이트등이 바뀌도록
    private void OnValidate()
    {
        if (itemData == null)
        {
            GetComponent<SpriteRenderer>().sprite = null;
        }
        else
        {
            GetComponent<SpriteRenderer>().sprite = itemData.ItemSprite;
            transform.name = itemData.ItemName;
        }
    }
#endif
}