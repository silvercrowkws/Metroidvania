using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 아이템의 종류
/// </summary>
public enum ItemType
{
    Consumable,     // 소모품
    General,        // 일반 아이템
    //Material,       // 재료
    //Equipment,      // 장비
    //QuestItem       // 퀘스트 아이템
}

[CreateAssetMenu(menuName = "SO/Item/Data")]
public class ItemDataSO : ScriptableObject
{
    /// <summary>
    /// 아이템의 스프라이트
    /// </summary>
    [field: SerializeField] public Sprite ItemSprite { get; protected set; }

    /// <summary>
    /// 아이템의 이름
    /// </summary>
    [field: SerializeField] public string ItemName { get; protected set; }

    /// <summary>
    /// 몇개까지 겹칠 수 있는지
    /// </summary>
    [field: SerializeField] public int MaxStackCount { get; protected set; }

    /// <summary>
    /// 아이템의 종류(사용 가능 아이템, 일반 아이템 등)
    /// </summary>
    [field: SerializeField] public ItemType Type { get; private set; }

    /// <summary>
    /// 아이템의 프리팹
    /// </summary>
    [field: SerializeField] public GameObject ItemPrefab { get; protected set; }
}
