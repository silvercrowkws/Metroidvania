using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Test_14_GameDifficulty : TestBase
{
#if UNITY_EDITOR

    [Header("UI 복원을 위해 ItemDatabaseSO 에셋을 연결해주세요.")]
    public ItemDatabaseSO itemDatabase;

    private InventoryData inventoryData = new InventoryData();

    // 2. InventoryViewer의 인스턴스를 찾습니다.
    InventoryViewer inventoryViewer;

    /// <summary>
    /// 게임 매니저
    /// </summary>
    GameManager gameManager;

    /// <summary>
    /// 플레이어
    /// </summary>
    Player player;

    Player_Test player_test;

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
        player_test = gameManager.Player_Test;

        tempPlayerData = new PlayerData();
        saveManager = GameManager.Instance.SaveManager;
        loadManager = GameManager.Instance.LoadManager;

        // 자동으로 저장된 데이터를 불러옴
        //loadPlayerData = loadManager.LoadData();

        /*// 불러온 데이터를 tempPlayerData로 복사
        PlayerData loaded = loadManager.LoadData();
        tempPlayerData = loaded;
        loadPlayerData = loaded;     // 필요하다면 같이 저장*/

        // 파일에서 불러온 데이터는 loadPlayerData에만 저장
        loadPlayerData = loadManager.LoadData();

        // InventoryViewer의 인스턴스를 찾습니다.
        inventoryViewer = FindObjectOfType<InventoryViewer>();
    }

    void PrintData(PlayerData loadPlayerData)
    {
        Debug.Log($"name : {loadPlayerData.name}");
        Debug.Log($"level : {loadPlayerData.level}");
        Debug.Log($"xp : {loadPlayerData.xp}");
        Debug.Log($"maxXP : {loadPlayerData.maxXp}");
        Debug.Log($"statePoint : {loadPlayerData.statePoint}");
        Debug.Log($"str : {loadPlayerData.str}");
        Debug.Log($"dex : {loadPlayerData.dex}");
        Debug.Log($"hp : {loadPlayerData.hp}");

        if (loadPlayerData.inventoryItems != null && loadPlayerData.inventoryItems.Count > 0)
        {
            foreach (var item in loadPlayerData.inventoryItems)
            {
                Debug.Log($"itemName : {item.itemName}, count : {item.count}");
            }
        }
        else
        {
            Debug.Log("인벤토리 아이템이 없습니다.");
        }

        /*if (loadPlayerData.inventorySlots != null)
        {
            foreach (ItemSlot slot in loadPlayerData.inventorySlots)
            {
                Debug.Log($"itemId : {slot.itemId}, count : {slot.count}");
            }
        }
        else
        {
            Debug.Log("인벤토리 슬롯이 비어있습니다.");
        }*/
    }

    protected override void OnTest1(InputAction.CallbackContext context)
    {
        player_test.hasAllKeys = true;
        Debug.Log("플레이어의 레벨업에 필요한 경험치");
        Debug.Log(player_test.MaxXP);
    }

    protected override void OnTest2(InputAction.CallbackContext context)
    {
        Debug.Log("플레이어의 현재 경험치");
        Debug.Log(player_test.XP);
    }

    protected override void OnTest3(InputAction.CallbackContext context)
    {
        player_test.XP += 50;
    }

    protected override void OnTest4(InputAction.CallbackContext context)
    {
        tempPlayerData.name = "Player_Test_1";
        tempPlayerData.level = 1;
        tempPlayerData.xp = 0;
        tempPlayerData.maxXp = 100;
        tempPlayerData.statePoint = 1;
        tempPlayerData.str = 1;
        tempPlayerData.dex = 1;
        tempPlayerData.hp = 1;
    }

    protected override void OnTest6(InputAction.CallbackContext context)
    {
        Debug.Log("데이터 저장");

        // 1. PlayerData 객체와 인벤토리 리스트를 새로 생성합니다.
        tempPlayerData.inventoryItems = new List<ItemSlotData>();

        if (inventoryViewer != null && Inventory.Instance != null)
        {
            // 2. InventoryViewer의 인스턴스를 찾습니다. Start에서 찾도록 수정
            //InventoryViewer inventoryViewer = FindObjectOfType<InventoryViewer>();

            // 3. InventoryViewer에서 UI 순서대로 정렬된 슬롯들을 가져옵니다.
            Slot[] slots = inventoryViewer.GetSlots();

            // 4. 이 슬롯 배열을 순서대로 순회합니다.
            foreach (Slot slot in slots)
            {
                // 5. 각 슬롯의 아이템 정보와 개수를 가져와 리스트에 추가합니다.
                tempPlayerData.inventoryItems.Add(new ItemSlotData
                {
                    itemName = slot.currentSaveItem.ItemName,
                    // 아이템 개수는 여전히 Inventory의 데이터에 있으므로, 여기서 가져옵니다.
                    count = Inventory.Instance.GetItemCount(slot.currentSaveItem)
                });
            }
        }

        // 6. 순서가 반영된 리스트가 담긴 PlayerData를 저장합니다.
        saveManager.SaveData(tempPlayerData);
    }

    protected override void OnTest7(InputAction.CallbackContext context)
    {
        Debug.Log("데이터 로드(메모리로)");
        loadPlayerData = loadManager.LoadData();    // 데이터 로드
    }

    protected override void OnTest8(InputAction.CallbackContext context)
    {
        Debug.Log("--- 불러온 데이터 출력 ---");
        PrintData(loadPlayerData);                              // 불러온 데이터 출력
    }

    protected override void OnTest9(InputAction.CallbackContext context)
    {
        Debug.Log("<<<<< 불러온 데이터로 인벤토리 UI 복원 시작 >>>>>");

        // 1. 필수 요소들이 준비되었는지 확인
        if (loadPlayerData == null)
        {
            Debug.LogError("로드된 데이터(loadPlayerData)가 없습니다! OnTest7을 먼저 실행하세요.");
            return;
        }
        if (Inventory.Instance == null)
        {
            Debug.LogError("Inventory 인스턴스를 찾을 수 없습니다.");
            return;
        }
        if (itemDatabase == null)
        {
            Debug.LogError("ItemDatabaseSO가 연결되지 않았습니다! Inspector 창에서 연결해주세요.");
            return;
        }


        Debug.Log("플레이어 스탯 적용 중...");

        // 불러온 데이터로 Player 객체의 상태를 갱신합니다.
        // (Player 클래스에 이름, 레벨, 스탯 등을 설정할 수 있는 프로퍼티나 함수가 있다고 가정합니다.)
        player_test.PlayerName = loadPlayerData.name; // 예시: Player 클래스에 PlayerName 프로퍼티가 있다고 가정
        player_test.Level = loadPlayerData.level;
        player_test.XP = loadPlayerData.xp;
        player_test.MaxXP = loadPlayerData.maxXp;
        player_test.StatePoint = loadPlayerData.statePoint;
        player_test.Strength = loadPlayerData.str;
        player_test.Dexterity = loadPlayerData.dex;
        player_test.Health = loadPlayerData.hp;

        Debug.Log($"'{loadPlayerData.name}' (Lv.{loadPlayerData.level}) 데이터 적용 완료!");



        // 2. 기존 인벤토리 UI와 데이터를 모두 비웁니다.
        Inventory.Instance.Clear();

        // 3. 불러온 데이터 목록(loadPlayerData.inventoryItems)을 하나씩 확인합니다.
        foreach (var itemSlotData in loadPlayerData.inventoryItems)
        {
            // 4. 저장된 아이템 이름으로 데이터베이스에서 실제 아이템(ItemDataSO)을 찾습니다.
            ItemDataSO itemData = itemDatabase.GetItemByName(itemSlotData.itemName);
            if (itemData != null)
            {
                // 5. 찾은 아이템과 개수로 인벤토리를 채웁니다. 이 과정에서 UI가 자동으로 업데이트됩니다.
                Inventory.Instance.LoadItem(itemData, itemSlotData.count);
            }
            else
            {
                Debug.LogWarning($"아이템 '{itemSlotData.itemName}'을 데이터베이스에서 찾을 수 없습니다.");
            }
        }




        Debug.Log("<<<<< 인벤토리 UI 복원 완료 >>>>>");
    }

#endif
}
