using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    [field: SerializeField] public ItemDataSO itemData { get; private set; }


    BoxCollider2D col;
    private void Awake()
    {
        col = GetComponent<BoxCollider2D>();
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
        if (collision.CompareTag("Player"))
        {
            Debug.Log("플레이어와 충돌");
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