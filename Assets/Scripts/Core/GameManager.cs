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

public enum GameDifficulty
{
    Easy = 0,
    Normal,
    Hard,
    Nightmare,
    Hell,
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
    /// 현재 게임 난이도
    /// </summary>
    [Tooltip("난이도에 따라 Easy : 4:9², Normal : 5:11², Hard : 6:13², Nightmare : 7:15², Hell : 8:17²")]
    public GameDifficulty gameDifficulty = GameDifficulty.Easy;

    /// <summary>
    /// 현재 게임 난이도 변경시 알리는 프로퍼티
    /// </summary>
    public GameDifficulty GameDifficulty
    {
        get => gameDifficulty;
        set
        {
            if (gameDifficulty != value)
            {
                gameDifficulty = value;
                switch (gameDifficulty)
                {
                    case GameDifficulty.Easy:
                        Debug.Log("이지 난이도");
                        break;
                    case GameDifficulty.Normal:
                        Debug.Log("노말 난이도");
                        break;
                    case GameDifficulty.Hard:
                        Debug.Log("하드 난이도");
                        break;
                    case GameDifficulty.Nightmare:
                        Debug.Log("나이트메어 난이도");
                        break;
                    case GameDifficulty.Hell:
                        Debug.Log("헬 난이도");
                        break;
                }
            }
        }
    }

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
                player_test.onPlayerLevelUP += OnPlayerLevelUP;

                // 경험치 변경 시 저장
                player_test.onPlayerXPChange += OnPlayerXPChange;
                player_test.onKeyCountChanged += OnKeyCountChanged;
            }
            return player_test;
        }
    }

    /// <summary>
    /// 경험치 변경 시 저장
    /// </summary>
    /// <param name="xp"></param>
    private void OnPlayerXPChange(float xp)
    {
        if (isDataRecovered)
        {
            OnDataSave();
        }
    }

    /// <summary>
    /// 레벨업 시 저장
    /// </summary>
    /// <param name="level"></param>
    private void OnPlayerLevelUP(int level)
    {
        Debug.Log("플레이어의 레벨업 확인");
        Debug.Log($"플레이어의 현재 레벨: {level}");

        if (isDataRecovered)
        {
            OnDataSave();
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

    // Json 데이터 연동 부분 끝 ------------------------------------------------------------

    // 카메라 진동 부분 ------------------------------------------------------------

    /// <summary>
    /// 버츄얼 카메라 흔드는 클래스
    /// </summary>
    CameraShakeController cameraShakeController;

    /// <summary>
    /// 진동 시간
    /// </summary>
    public float shakeTime = 3f;

    /// <summary>
    /// 진동 세기
    /// </summary>
    public float shakePower = 5f;

    /// <summary>
    /// 진동 속도
    /// </summary>
    public float shakeSpeed = 2f;

    /// <summary>
    /// 어디선가 문이 열렸다! 패널
    /// </summary>
    public GameObject DoorOpenNotification;

    // 카메라 진동 부분 끝 ------------------------------------------------------------


    private void Start()
    {
        tempPlayerData = new PlayerData();

        // 불러온 데이터를 tempPlayerData로 복사
        PlayerData loaded = loadManager.LoadData();
        tempPlayerData = loaded;
        loadPlayerData = loaded;     // 필요하다면 같이 저장

        // InventoryViewer의 인스턴스를 찾기
        inventoryViewer = FindObjectOfType<InventoryViewer>();
        
        // InventoryViewer의 슬롯 순서 변경 이벤트에 OnDataSave 함수를 연결
        if (inventoryViewer != null)
        {
            inventoryViewer.OnInventoryOrderChanged += OnDataSave;
        }

        // Inventory의 이벤트에 OnDataSave 함수를 구독
        if (Inventory.Instance != null)
        {
            // 새로운 종류의 아이템이 추가되었을 때 OnDataSave 실행
            Inventory.Instance.OnNewItemAdded += (itemData) => OnDataSave();

            // 기존 아이템의 개수가 변경되었을 때 OnDataSave 실행
            Inventory.Instance.OnItemChanged += (itemData, count) => OnDataSave();
        }

        cameraShakeController = FindAnyObjectByType<CameraShakeController>();
        cameraShakeController.onShakeFinished += OnShakeFinished;

        DoorOpenNotification.gameObject.SetActive(false);
    }

    private void OnEnable()
    {        
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        
        // GameManager 오브젝트가 비활성화되거나 파괴될 때, 연결했던 이벤트를 해제
        // 이렇게 하지 않으면 메모리 누수가 발생할 수 있음
        if (Inventory.Instance != null)
        {
            Inventory.Instance.OnNewItemAdded -= (itemData) => OnDataSave();
            Inventory.Instance.OnItemChanged -= (itemData, count) => OnDataSave();
        }
        
        // InventoryViewer의 이벤트 구독도 해제
        if (inventoryViewer != null)
        {
            inventoryViewer.OnInventoryOrderChanged -= OnDataSave;
        }
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
        Debug.Log($"level : {loadPlayerData.level}");
        Debug.Log($"xp : {loadPlayerData.xp}");
        Debug.Log($"maxXP : {loadPlayerData.maxXp}");
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

    private bool isDataRecovered = false;

    /// <summary>
    /// 데이터를 저장하는 함수
    /// </summary>
    private void OnDataSave()
    {
        Debug.Log("게임매니저에서 인벤토리의 변경 감지됨. 데이터 저장 실행.");


        /*// 인벤토리 인스턴스가 준비되지 않았으면 저장하지 않음
        if (Inventory.Instance == null)
        {
            Debug.LogWarning("인벤토리 인스턴스가 아직 준비되지 않았으므로 저장을 건너뜁니다.");
            return;
        }

        // 인벤토리 컨테이너가 비어있으면 저장하지 않음
        var container = Inventory.Instance.GetItemContainer();
        if (container == null || container.Count == 0)
        {
            Debug.LogWarning("인벤토리 데이터가 비어있으므로 저장을 건너뜁니다.");
            return;
        }*/


        // PlayerData 객체와 인벤토리 리스트를 새로 생성
        tempPlayerData.inventoryItems = new List<ItemSlotData>();

        if (inventoryViewer != null && Inventory.Instance != null)
        {
            /*// InventoryViewer에서 UI 순서대로 정렬된 슬롯들을 가져옴
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
            }*/

            foreach (var kvp in Inventory.Instance.GetItemContainer())
            {
                if (kvp.Value > 0) // 0개 이상만 저장
                {
                    tempPlayerData.inventoryItems.Add(new ItemSlotData
                    {
                        itemName = kvp.Key.ItemName,
                        count = kvp.Value
                    });
                }
            }
        }


        // 1. 현재 플레이어 스탯을 tempPlayerData에 저장
        if (Player_Test != null)
        {
            tempPlayerData.name = Player_Test.PlayerName;
            tempPlayerData.level = Player_Test.Level;
            tempPlayerData.xp = Player_Test.XP;
            tempPlayerData.maxXp = Player_Test.MaxXP;
            tempPlayerData.str = Player_Test.Strength;
            tempPlayerData.dex = Player_Test.Dexterity;
            tempPlayerData.hp = Player_Test.Health;
        }

        // 순서가 반영된 리스트가 담긴 PlayerData를 저장
        saveManager.SaveData(tempPlayerData);

        //OnDataLoad();       // 데이터 로드
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

        if (Player_Test != null && loadPlayerData != null)
        {
            Player_Test.PlayerName = loadPlayerData.name;
            Player_Test.Level = loadPlayerData.level;
            Player_Test.MaxXP = loadPlayerData.maxXp;   // 프로퍼티로 할당
            Player_Test.XP = loadPlayerData.xp;
            Player_Test.Strength = loadPlayerData.str;
            Player_Test.Dexterity = loadPlayerData.dex;
            Player_Test.Health = loadPlayerData.hp;
        }

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

        isDataRecovered = true; // 복원 완료 플래그 ON
    }

    /// <summary>
    /// 플레이어의 열쇠 개수가 3개면 타임스케일 조절 및 화면을 흔드는 함수
    /// </summary>
    /// <param name="keyCount"></param>
    private void OnKeyCountChanged(int keyCount)
    {
        if(keyCount == 3)
        {
            Time.timeScale = 0;     // 타임 스케일 0
            cameraShakeController.StartShake(shakeTime, shakePower, shakeSpeed);
        }
    }

    /// <summary>
    /// 카메라 진동이 끝나서 실행되는 함수
    /// </summary>
    private void OnShakeFinished()
    {
        StartCoroutine(DoorNotification());
    }

    /// <summary>
    /// 문이 열렸다 패널 코루틴
    /// </summary>
    /// <returns></returns>
    IEnumerator DoorNotification()
    {
        DoorOpenNotification.gameObject.SetActive(true);

        //yield return new WaitForSeconds(1);
        yield return new WaitForSecondsRealtime(1.5f);     // 실제 시간으로 1.5초 기다리고

        Time.timeScale = 1;     // 타임 스케일 1
        DoorOpenNotification.gameObject.SetActive(false);
    }


#if UNITY_EDITOR


#endif
}
