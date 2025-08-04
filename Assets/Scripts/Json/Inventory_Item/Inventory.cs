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

    public void RemoveItem(ItemDataSO item, Vector2 pos)
    {
        //아이템이 존재한다면?
        if (itemContainer.ContainsKey(item))
        {
            //생성 해주기
            Instantiate(item.ItemPrefab, pos, Quaternion.identity);

            //카운트 1 빼주기
            itemContainer[item]--;

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