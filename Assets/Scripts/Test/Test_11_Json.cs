using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Test_11_Json : TestBase
{
#if UNITY_EDITOR    

    /// <summary>
    /// 게임 매니저
    /// </summary>
    GameManager gameManager;

    /// <summary>
    /// 플레이어
    /// </summary>
    Player player;

    /// <summary>
    /// 세이브 매니저
    /// </summary>
    SaveManager saveManager;

    /// <summary>
    /// 로드 매니저
    /// </summary>
    LoadManager loadManager;

    /// <summary>
    /// 플레이어의 정보(저장용)
    /// </summary>
    PlayerData tempPlayerData;

    /// <summary>
    /// 플레이어의 정보(로드용)
    /// </summary>
    PlayerData loadPlayerData;

    private void Start()
    {
        gameManager = GameManager.Instance;
        player = gameManager.Player;

        tempPlayerData = new PlayerData();
        saveManager = GameManager.Instance.SaveManager;
        loadManager = GameManager.Instance.LoadManager;

        // 자동으로 저장된 데이터를 불러옴
        //loadPlayerData = loadManager.LoadData();

        // 불러온 데이터를 tempPlayerData로 복사
        PlayerData loaded = loadManager.LoadData();
        tempPlayerData = loaded;
        loadPlayerData = loaded;     // 필요하다면 같이 저장
    }

    void PrintData(PlayerData loadPlayerData)
    {
        Debug.Log($"name : {loadPlayerData.name}");
        Debug.Log($"str : {loadPlayerData.str}");
        Debug.Log($"dex : {loadPlayerData.dex}");
        Debug.Log($"hp : {loadPlayerData.hp}");

        if (loadPlayerData.inventorySlots != null)
        {
            foreach (ItemSlot slot in loadPlayerData.inventorySlots)
            {
                Debug.Log($"itemId : {slot.itemId}, count : {slot.count}");
            }
        }
        else
        {
            Debug.Log("인벤토리 슬롯이 비어있습니다.");
        }
    }

    protected override void OnTest1(InputAction.CallbackContext context)
    {
        tempPlayerData.name = "Player_Test_1";
        tempPlayerData.str = 1;
        tempPlayerData.dex = 1;
        tempPlayerData.hp = 1;
        tempPlayerData.inventorySlots = new List<ItemSlot>
        {
        new ItemSlot { itemId = "sword_1", count = 1 },
        new ItemSlot { itemId = "potion_1", count = 1 }
        };
    }

    protected override void OnTest2(InputAction.CallbackContext context)
    {
        tempPlayerData.name = "Player_Test_2";
        tempPlayerData.str = 2;
        tempPlayerData.dex = 2;
        tempPlayerData.hp = 2;
        tempPlayerData.inventorySlots = new List<ItemSlot>
        {
        new ItemSlot { itemId = "sword_2", count = 2 },
        new ItemSlot { itemId = "potion_2", count = 2 }
        };
    }

    protected override void OnTest3(InputAction.CallbackContext context)
    {
        tempPlayerData.name = "Player_Test_3";
        tempPlayerData.str = 3;
        tempPlayerData.dex = 3;
        tempPlayerData.hp = 3;
        tempPlayerData.inventorySlots = new List<ItemSlot>
        {
        new ItemSlot { itemId = "sword_3", count = 3 },
        new ItemSlot { itemId = "potion_3", count = 3 }
        };
    }

    protected override void OnTest4(InputAction.CallbackContext context)
    {

    }

    protected override void OnTest5(InputAction.CallbackContext context)
    {

    }

    protected override void OnTest6(InputAction.CallbackContext context)
    {
        Debug.Log("데이터 저장");
        saveManager.SaveData(tempPlayerData);       // 데이터 저장
    }

    protected override void OnTest7(InputAction.CallbackContext context)
    {
        Debug.Log("데이터 로드");
        loadPlayerData = loadManager.LoadData();    // 데이터 로드
    }

    protected override void OnTest8(InputAction.CallbackContext context)
    {
        PrintData(loadPlayerData);                              // 불러온 데이터 출력
    }

#endif
}
