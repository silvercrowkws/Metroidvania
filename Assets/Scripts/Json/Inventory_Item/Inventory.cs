using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    /// <summary>
    /// 아이템을 저장할 딕셔너리
    /// </summary>
    private Dictionary<ItemDataSO, int> itemContainer = new();

    public static Inventory Instance { get; private set; }

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


    private void Awake()
    {
        Instance = this;
    }

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
        itemContainer.Add(itemData, count);

        // 2. InventoryViewer가 새 Slot UI를 생성하도록 이벤트를 호출합니다.
        OnNewItemAdded?.Invoke(itemData);

        // 3. 생성된 Slot이 개수 텍스트를 올바르게 설정하도록 이벤트를 호출합니다.
        OnItemChanged?.Invoke(itemData, count);
    }

    public void AddItem(Item item)
    {
        //이미 같은종류의 아이템이 존재한다면?
        if (itemContainer.ContainsKey(item.itemData))
        {
            //MaxStackCount보다 덜 가지고있다면?
            if (item.itemData.MaxStackCount > itemContainer[item.itemData])
            {
                //카운트 1 증가
                itemContainer[item.itemData]++;

                //이벤트 실행
                OnItemChanged?.Invoke(item.itemData, itemContainer[item.itemData]);

                //아이템 제거
                Destroy(item.gameObject);
            }
        }
        //같은 종류의 아이템을 가지고 있지 않다면
        else
        {
            //아이템 추가해주기
            itemContainer.Add(item.itemData, 1);

            //이벤트 실행
            OnNewItemAdded?.Invoke(item.itemData);

            //아이템 제거
            Destroy(item.gameObject);
        }
    }

    public void RemoveItem(ItemDataSO item, Vector2 pos, int removeCount = 1)
    {
        // 아이템이 존재한다면?
        if (itemContainer.ContainsKey(item))
        {
            // 실제로 버릴 개수만큼 반복해서 아이템 생성
            for (int i = 0; i < removeCount; i++)
            {
                Instantiate(item.ItemPrefab, pos, Quaternion.identity);
            }

            //생성 해주기
            //Instantiate(item.ItemPrefab, pos, Quaternion.identity);

            //카운트 1 빼주기
            //itemContainer[item]--;
            itemContainer[item] -= removeCount;

            //이벤트 실행
            OnItemChanged?.Invoke(item, itemContainer[item]);

            //0이하로 떨어졌다면?
            if (itemContainer[item] <= 0)
            {
                //item 제거
                itemContainer.Remove(item);
            }
        }
    }

    public int GetItemCount(ItemDataSO data)
    {
        if (itemContainer.TryGetValue(data, out var count))
        {
            //있다면 카운트를 반환
            return count;
        }
        //아이템이 없다면 -1반환
        return -1;
    }

    private void OnDestroy()
    {
        Instance = null;
    }
}