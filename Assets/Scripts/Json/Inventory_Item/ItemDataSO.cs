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

/// <summary>
/// 아이템의 증가량(체력, 배부름 등등..)
/// </summary>
public enum RecoveryType
{
    None = 0,
    HP,
    Fullness,
}

[System.Serializable]
public class RecoveryInfo
{
    public RecoveryType type;
    public int amount;
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
    /// 여러 회복값을 리스트로 관리
    /// </summary>
    [SerializeField] public List<RecoveryInfo> recoveryList = new List<RecoveryInfo>();

    /// <summary>
    /// 아이템의 구매 가격
    /// </summary>
    /// [Tooltip("체력 회복 10당 2골드, 배부름 5당 1골드로 계산")]
    [field: SerializeField] public int PurchasePrice { get; private set; }

    /// <summary>
    /// 아이템의 판매 가격
    /// </summary>
    [field: SerializeField] public int SelliPrice { get; private set; }

    /// <summary>
    /// 아이템의 프리팹
    /// </summary>
    [field: SerializeField] public GameObject ItemPrefab { get; protected set; }


    /// <summary>
    /// 구매 가격의 50%를 반올림하여 판매 가격 자동 계산
    /// </summary>
    private void OnValidate()
    {
        PurchasePrice = Mathf.Max(0, PurchasePrice);
        //SelliPrice = Mathf.RoundToInt(PurchasePrice * 0.5f); => 반올림에서 올림으로 수정
        SelliPrice = Mathf.CeilToInt(PurchasePrice * 0.5f);
    }
}
