using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Test_17_LobbyScene : TestBase
{
    /// <summary>
    /// 플레이어의 정보(저장용)
    /// </summary>
    PlayerData tempPlayerData;

    /// <summary>
    /// 게임 매니저
    /// </summary>
    GameManager gameManager;

    /// <summary>
    /// 플레이어
    /// </summary>
    Player_Test player_Test;

    /// <summary>
    /// 세이브 매니저
    /// </summary>
    SaveManager saveManager;

    /// <summary>
    /// 로드 매니저
    /// </summary>
    LoadManager loadManager;

    /// <summary>
    /// 플레이어의 정보(로드용)
    /// </summary>
    PlayerData loadPlayerData;

    private void Start()
    {
        gameManager = GameManager.Instance;
        player_Test = gameManager.Player_Test;

        tempPlayerData = new PlayerData();
        saveManager = GameManager.Instance.SaveManager;
        loadManager = GameManager.Instance.LoadManager;

        // 파일에서 불러온 데이터는 loadPlayerData에만 저장
        loadPlayerData = loadManager.LoadData();
    }

    protected override void OnTest1(InputAction.CallbackContext context)
    {
        // 1. 내가 원하는 데이터를 설정 (tempPlayerData)
        tempPlayerData.name = "Player_Test_1";
        tempPlayerData.level = 1;
        tempPlayerData.xp = 0;
        tempPlayerData.maxXp = 100;
        tempPlayerData.statePoint = 0;
        tempPlayerData.str = 1;
        tempPlayerData.dex = 1;
        tempPlayerData.hp = 1;
        tempPlayerData.money = 200;
        tempPlayerData.inventoryItems = new List<ItemSlotData>(); // 인벤토리 비우기

        // 2. 이 데이터를 파일에 강제로 씀
        saveManager.SaveData(tempPlayerData);

        // 3. 파일에 쓴 데이터를 다시 '로드용 변수'에 담음
        // (GameManager가 이 변수를 기준으로 복구하기 때문)
        // GameManager의 loadPlayerData를 직접 갱신해줘야 합니다.
        // gameManager에 public 접근이 가능하다면 아래처럼 하세요.
        
        // 이 방식이 가장 확실합니다:
        // "현재 파일의 데이터를 읽어서, 실제 게임 속 플레이어 객체에 적용해라"
        gameManager.StartCoroutine("DataRecoverCoroutine"); 

        Debug.Log("데이터를 파일에 저장하고, 플레이어 객체에 복구 요청을 보냈습니다.");
    }

    protected override void OnTest2(InputAction.CallbackContext context)
    {
        player_Test.XP = 0;
        player_Test.Money = 300;
    }
}
