using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Item/Database")]
public class ItemDatabaseSO : ScriptableObject
{
    // 이 스크립터블 오브젝트는 저장된 이름을 실제 아이템 데이터와 연결하는 역할


    public List<ItemDataSO> allItems;

    // 이름으로 ItemDataSO를 찾는 메서드
    public ItemDataSO GetItemByName(string name)
    {
        // allItems 리스트에서 이름이 일치하는 첫 번째 아이템을 반환합니다.
        return allItems.FirstOrDefault(item => item.ItemName == name);
    }
}