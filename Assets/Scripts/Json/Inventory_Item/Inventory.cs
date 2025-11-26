using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : Singleton<Inventory>
{
    /// <summary>
    /// 아이템을 저장할 딕셔너리
    /// </summary>
    private Dictionary<ItemDataSO, int> itemContainer = new();

    // 싱글톤으로 변경되면서 없어짐
    //public static Inventory Instance { get; private set; }

    /// <summary>
    /// 아이템이 바뀌었을때의 이벤트
    /// </summary>
    //public Action<ItemDataSO, int> OnItemChanged;
    public event Action<ItemDataSO, int> OnItemChanged;

    /// <summary>
    /// 새로운 아이템이 추가되었을때 이벤트
    /// </summary>
    //public Action<ItemDataSO> OnNewItemAdded;
    public event Action<ItemDataSO> OnNewItemAdded;

    /// <summary>
    /// 아이템을 던지는 힘
    /// </summary>
    public float throwPower;


    /*private void Awake()
    {
        Instance = this;

        *//*var others = FindObjectsOfType<Inventory>();
        if (others.Length > 1)
        {
            // 이미 다른 인스턴스가 존재하면 자신을 파괴하고 초기화 중단
            Destroy(gameObject);
            return;
        }

        // 씬 전환 시 이 게임오브젝트가 파괴되지 않도록 설정
        DontDestroyOnLoad(gameObject);*//*
    }*/

    /// <summary>
    /// 외부에서 인벤토리 데이터를 읽기 위한 메서드 (for Saving)
    /// </summary>
    public Dictionary<ItemDataSO, int> GetItemContainer()
    {
        return itemContainer;
    }

    /// <summary>
    /// 인벤토리의 모든 아이템 데이터와 UI를 제거하는 메서드 (for Loading)
    /// </summary>
    public void Clear()
    {
        // Slot UI가 스스로를 파괴하도록 이벤트를 발생시킵니다.
        var itemsToRemove = new List<ItemDataSO>(itemContainer.Keys);
        foreach (var item in itemsToRemove)
        {
            // OnItemChanged 이벤트를 count = 0으로 호출하면, 해당 슬롯이 이 신호를 받고 스스로를 파괴합니다.
            OnItemChanged?.Invoke(item, 0);
        }

        // 실제 데이터가 담긴 딕셔너리를 비웁니다.
        itemContainer.Clear();
    }

    /// <summary>
    /// 불러온 데이터를 기반으로 인벤토리에 아이템을 추가하고 UI를 업데이트하는 메서드 (for Loading)
    /// </summary>
    public void LoadItem(ItemDataSO itemData, int count)
    {
        // 1. 데이터 추가
        //itemContainer.Add(itemData, count);

        // 이미 있으면 덮어쓰기, 없으면 추가로 수정
        if (itemContainer.ContainsKey(itemData))
        {
            itemContainer[itemData] = count;
        }
        else
        {
            itemContainer.Add(itemData, count);
        }


        // 2. InventoryViewer가 새 Slot UI를 생성하도록 이벤트를 호출합니다.
        OnNewItemAdded?.Invoke(itemData);

        // 3. 생성된 Slot이 개수 텍스트를 올바르게 설정하도록 이벤트를 호출합니다.
        OnItemChanged?.Invoke(itemData, count);
    }

    public void AddItem(Item item)
    {
        // 이미 같은종류의 아이템이 존재한다면?
        if (itemContainer.ContainsKey(item.itemData))
        {
            // MaxStackCount보다 덜 가지고있다면?
            if (item.itemData.MaxStackCount > itemContainer[item.itemData])
            {
                // 카운트 1 증가
                itemContainer[item.itemData]++;

                // 이벤트 실행
                OnItemChanged?.Invoke(item.itemData, itemContainer[item.itemData]);

                // 아이템 제거
                Destroy(item.gameObject);
            }
        }
        // 같은 종류의 아이템을 가지고 있지 않다면
        else
        {
            // 아이템 추가해주기
            itemContainer.Add(item.itemData, 1);

            // 이벤트 실행
            OnNewItemAdded?.Invoke(item.itemData);

            // 아이템 제거
            Destroy(item.gameObject);
        }
    }

    public void RemoveItem(ItemDataSO item, Vector2 pos, int removeCount = 1)
    {
        // 아이템이 존재한다면?
        if (itemContainer.ContainsKey(item))
        {
            // 아이템을 생성할 기준 위치 (플레이어 위치)
            Vector2 playerPos;
            float dirX;

            if (GameManager.Instance.Player != null)
            {
                playerPos = GameManager.Instance.Player.transform.position;
                dirX = GameManager.Instance.Player.transform.localScale.x;
            }
            else if (GameManager.Instance.Player_Test != null)
            {
                playerPos = GameManager.Instance.Player_Test.transform.position;
                dirX = GameManager.Instance.Player_Test.transform.localScale.x;
            }
            else
            {
                // 플레이어 인스턴스가 없을 경우
                playerPos = Vector2.zero;
                dirX = 1f;
            }

            // 👇 수정: 아이템이 플레이어의 중심 위치(playerPos)에서 Y값 1만큼 위로 생성되도록 설정
            Vector2 dropPos = playerPos + Vector2.up * 1f;

            // 실제로 버릴 개수만큼 반복해서 아이템 생성
            for (int i = 0; i < removeCount; i++)
            {
                var dropped = Instantiate(item.ItemPrefab, dropPos, Quaternion.identity); // dropPos(플레이어 중심) 사용

                // 휙 던지기: Rigidbody2D가 있으면 힘을 준다
                var rb = dropped.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    // 플레이어 방향(localScale.x) 기준으로 던지기
                    Vector2 throwDir = new Vector2(dirX, 1).normalized; // 위로 살짝 던지기

                    // 👇 수정된 부분: 최소 던지기 힘을 2.0f로 대폭 상향
                    float randomthrowPower = UnityEngine.Random.Range(2.0f, 4.0f);

                    // throwPower를 사용하여 강력하게 던집니다.
                    rb.AddForce(throwDir * (throwPower + randomthrowPower), ForceMode2D.Impulse);
                }
            }




            /*// 실제로 버릴 개수만큼 반복해서 아이템 생성
            for (int i = 0; i < removeCount; i++)
            {
                //Instantiate(item.ItemPrefab, pos, Quaternion.identity);
                var dropped = Instantiate(item.ItemPrefab, pos, Quaternion.identity);

                // 휙 던지기: Rigidbody2D가 있으면 힘을 준다
                var rb = dropped.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    // 플레이어 방향(localScale.x) 기준으로 던지기
                    float dirX = (GameManager.Instance.Player != null)
                        ? GameManager.Instance.Player.transform.localScale.x
                        : (GameManager.Instance.Player_Test != null ? GameManager.Instance.Player_Test.transform.localScale.x : 1f);

                    Vector2 throwDir = new Vector2(dirX, 1).normalized; // 위로 살짝 던지기

                    float randomthrowPower = UnityEngine.Random.Range(0, 0.5f);

                    rb.AddForce(throwDir * (throwPower + randomthrowPower), ForceMode2D.Impulse);
                }
            }*/

            // 카운트 1 빼주기
            //itemContainer[item]--;
            itemContainer[item] -= removeCount;

            // 이벤트 실행
            OnItemChanged?.Invoke(item, itemContainer[item]);

            // 0이하로 떨어졌다면?
            if (itemContainer[item] <= 0)
            {
                // item 제거
                itemContainer.Remove(item);
            }
        }
    }

    public int GetItemCount(ItemDataSO data)
    {
        if (itemContainer.TryGetValue(data, out var count))
        {
            // 있다면 카운트를 반환
            return count;
        }
        // 아이템이 없다면 -1반환
        return -1;
    }

    /// <summary>
    /// 아이템을 1개 사용하고 인벤토리에서 개수를 감소시키는 메서드
    /// </summary>
    public void UseItem(ItemDataSO item)
    {
        // 아이템이 존재하고 개수가 1개 이상이라면
        if (itemContainer.TryGetValue(item, out int currentCount) && currentCount > 0)
        {
            // 카운트 1 감소
            itemContainer[item]--;

            // 이벤트 실행 (Slot UI 업데이트)
            OnItemChanged?.Invoke(item, itemContainer[item]);

            // 0이하로 떨어졌다면 딕셔너리에서 제거 (OnItemChanged 이벤트가 Slot을 파괴함)
            if (itemContainer[item] <= 0)
            {
                itemContainer.Remove(item);
            }

            // 여기에 아이템의 '효과 발동' 로직이 들어갈 수 있습니다.
            Debug.Log($"아이템 {item.ItemName} 사용됨. 남은 개수: {itemContainer.GetValueOrDefault(item)}");
        }
    }

    // 마찬가지로 싱글톤에서 처리
    /*private void OnDestroy()
    {
        Instance = null;
    }*/
}