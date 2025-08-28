using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;



/// <summary>
/// 게임상태
/// </summary>
public enum GameState
{
    Main = 0,                   // 
    SelectCharacter,            // 
    SelectCard,                 // 
    GameComplete,               // 
}

[Serializable]
public class InventoryData
{
    public List<Item> inventoryList = new List<Item>();
}

public class GameManager : Singleton<GameManager>
{
    /// <summary>
    /// 현재 게임상태
    /// </summary>
    public GameState gameState = GameState.Main;

    /// <summary>
    /// 현재 게임상태 변경시 알리는 프로퍼티
    /// </summary>
    public GameState GameState
    {
        get => gameState;
        set
        {
            if (gameState != value)
            {
                gameState = value;
                switch (gameState)
                {
                    case GameState.Main:
                        Debug.Log("메인 상태");
                        break;
                    case GameState.SelectCharacter:
                        Debug.Log("캐릭터 선택 상태");
                        onSelectCharacter?.Invoke();
                        break;
                    case GameState.SelectCard:
                        Debug.Log("카드 선택 상태");
                        onSelectCard?.Invoke();
                        break;
                    case GameState.GameComplete:
                        Debug.Log("게임 완료 상태");
                        onGameComplete?.Invoke();
                        break;
                }
            }
        }
    }


    // 게임상태 델리게이트
    public Action onSelectCharacter;
    public Action onSelectCard;
    public Action onGameComplete;

    /// <summary>
    /// 플레이어
    /// </summary>
    Player player;

    public Player Player
    {
        get
        {
            if (player == null)
                player = FindAnyObjectByType<Player>();
            return player;
        }
    }

    /// <summary>
    /// 플레이어_2
    /// </summary>
    Player_Test player_test;

    public Player_Test Player_Test
    {
        get
        {
            if(player_test == null)
            {
                player_test = FindAnyObjectByType<Player_Test>();
            }
            return player_test;
        }
    }

    /*SaveManager saveManager = new SaveManager();

    public SaveManager SaveManager => saveManager;
    *//*public SaveManager SaveManager        // 이걸 줄여서 가능
    {
        get
        {
            return saveManager;
        }
    }*//*

    LoadManager loadManager = new LoadManager();

    public LoadManager LoadManager => loadManager;*/

    /// <summary>
    /// 턴 매니저
    /// </summary>
    //TurnManager turnManager;






    // Json 데이터 연동 부분 ------------------------------------------------------------

    [Header("UI 복원을 위해 ItemDatabaseSO 에셋을 연결해주세요.")]
    public ItemDatabaseSO itemDatabase;

    // 데이터 관리 기능
    private InventoryData inventoryData = new InventoryData();

    // 2. InventoryViewer의 인스턴스를 찾습니다.
    InventoryViewer inventoryViewer;

    /// <summary>
    /// 세이브 매니저
    /// </summary>
    SaveManager saveManager = new SaveManager();

    public SaveManager SaveManager => saveManager;
    /*public SaveManager SaveManager        // 이걸 줄여서 가능
    {
        get
        {
            return saveManager;
        }
    }*/

    /// <summary>
    /// 로드 매니저
    /// </summary>
    LoadManager loadManager = new LoadManager();

    public LoadManager LoadManager => loadManager;

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
        tempPlayerData = new PlayerData();

        // 불러온 데이터를 tempPlayerData로 복사
        PlayerData loaded = loadManager.LoadData();
        tempPlayerData = loaded;
        loadPlayerData = loaded;     // 필요하다면 같이 저장

        // InventoryViewer의 인스턴스를 찾기
        inventoryViewer = FindObjectOfType<InventoryViewer>();
    }

    private void OnEnable()
    {        
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    protected override void OnInitialize()
    {
        base.OnInitialize();
        player = FindAnyObjectByType<Player>();        

        //turnManager = FindAnyObjectByType<TurnManager>();
        //turnManager.OnInitialize2();
    }

    private void Update()
    {

    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode arg1)
    {
        switch(scene.buildIndex)
        {
            case 0:
                Debug.Log("메인 씬");
                gameState = GameState.Main;
                break;
            case 1:
                Debug.Log("캐릭터 선택 씬");
                gameState = GameState.SelectCharacter;
                break;
            case 2:
                Debug.Log("카드 선택 씬");
                gameState = GameState.SelectCard;
                break;
            case 3:
                Debug.Log("전투 완료 씬");
                gameState = GameState.GameComplete;
                break;
        }

        StartCoroutine(DataRecoverCoroutine());
    }

    /// <summary>
    /// 데이터를 복구시키는 코루틴
    /// </summary>
    /// <returns></returns>
    IEnumerator DataRecoverCoroutine()
    {
        yield return new WaitForSeconds(0.1f);
        OnDataRecover();
    }

    /*/// <summary>
    /// 게임 종료 버튼으로 게임을 종료시키는 함수
    /// </summary>
    private void OnQuit()
    {
        Application.Quit();

        // 에디터에서 실행 시 종료 테스트 (에디터에서는 실제로 종료되지 않음)
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #endif
    }*/

    /// <summary>
    /// 데이터를 출력하는 함수
    /// </summary>
    /// <param name="loadPlayerData"></param>
    void PrintData(PlayerData loadPlayerData)
    {
        Debug.Log($"name : {loadPlayerData.name}");
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
    }

    /// <summary>
    /// 데이터를 저장하는 함수
    /// </summary>
    private void OnDataSave()
    {
        Debug.Log("게임 매니저에서 데이터 저장");

        // PlayerData 객체와 인벤토리 리스트를 새로 생성
        tempPlayerData.inventoryItems = new List<ItemSlotData>();

        if (inventoryViewer != null && Inventory.Instance != null)
        {
            // InventoryViewer에서 UI 순서대로 정렬된 슬롯들을 가져옴
            Slot[] slots = inventoryViewer.GetSlots();

            // 이 슬롯 배열을 순서대로 순회
            foreach (Slot slot in slots)
            {
                // 각 슬롯의 아이템 정보와 개수를 가져와 리스트에 추가
                tempPlayerData.inventoryItems.Add(new ItemSlotData
                {
                    itemName = slot.currentSaveItem.ItemName,

                    // 아이템 개수는 여전히 Inventory의 데이터에 있으므로, 여기서 가져옴
                    count = Inventory.Instance.GetItemCount(slot.currentSaveItem)
                });
            }
        }

        // 순서가 반영된 리스트가 담긴 PlayerData를 저장
        saveManager.SaveData(tempPlayerData);
    }

    /// <summary>
    /// 데이터를 로드하는 함수
    /// </summary>
    private void OnDataLoad()
    {
        Debug.Log("게임 매니저에서 데이터 로드(메모리로)");
        loadPlayerData = loadManager.LoadData();    // 데이터 로드
    }

    /// <summary>
    /// 데이터를 출력하는 함수
    /// </summary>
    private void OnDataPrint()
    {
        Debug.Log("--- 불러온 데이터 출력 ---");
        PrintData(loadPlayerData);                              // 불러온 데이터 출력
    }

    /// <summary>
    /// 데이터를 복구하는 함수(인벤토리 UI)
    /// </summary>
    private void OnDataRecover()
    {
        Debug.Log("<<<<< 불러온 데이터로 인벤토리 UI 복원 시작 >>>>>");

        // 1. 필수 요소들이 준비되었는지 확인
        if (loadPlayerData == null) { Debug.LogError("로드된 데이터(loadPlayerData)가 없습니다! OnTest7을 먼저 실행하세요."); return; }
        if (Inventory.Instance == null) { Debug.LogError("Inventory 인스턴스를 찾을 수 없습니다."); return; }
        if (itemDatabase == null) { Debug.LogError("ItemDatabaseSO가 연결되지 않았습니다! Inspector 창에서 연결해주세요."); return; }

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


#if UNITY_EDITOR


#endif
}
