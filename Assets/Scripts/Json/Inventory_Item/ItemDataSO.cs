using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    /// 아이템의 프리팹
    /// </summary>
    [field: SerializeField] public GameObject ItemPrefab { get; protected set; }

}
