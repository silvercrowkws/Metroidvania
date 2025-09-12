using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 직렬화가 가능한 인벤토리 슬롯 데이터용 클래스
[System.Serializable]
public class ItemSlotData
{
    public string itemName;
    public int count;
}

// 게임 데이터를 저장하는데 사용되고, 게임 씬에 존재하는 객체(카메라, 캐릭터...)가 아니기 때문에 MonoBehaviour 상속하지 않는다!
[System.Serializable]   // 직렬화를 하겠다는 어트리뷰트(Attribute)
// 직렬화는 객체나 데이터를 연속적인 형식(예: JSON, XML, 바이너리 등)으로 변환하는 과정

/// <summary>
/// 게임 데이터를 저장해주는 클래스
/// </summary>
public class PlayerData
{
    public string name;

    /// <summary>
    /// 플레이어의 레벨
    /// </summary>
    public int level;

    /// <summary>
    /// 플레이어의 경험치
    /// </summary>
    public float xp;

    /// <summary>
    /// 플레이어의 레벨업에 필요한 경험치
    /// </summary>
    public float maxXp;

    /// <summary>
    /// 힘 관련 스탯
    /// </summary>
    public int str;

    /// <summary>
    /// 민첩 관련 스탯
    /// </summary>
    public int dex;

    /// <summary>
    /// 체력 관련 스탯
    /// </summary>
    public int hp;

    //public string[] items;

    /// <summary>
    /// 인벤토리 아이템 리스트
    /// </summary>
    public List<ItemSlotData> inventoryItems;
}