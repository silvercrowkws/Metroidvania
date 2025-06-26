using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
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

    private void Start()
    {
        
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

#if UNITY_EDITOR


#endif
}
