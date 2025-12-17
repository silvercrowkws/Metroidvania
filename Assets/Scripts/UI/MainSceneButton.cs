using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainSceneButton : MonoBehaviour
{
    /// <summary>
    /// 시작 버튼
    /// </summary>
    Button button;

    GameManager gameManager;

    public Action<int> onSceneChangeButton;

    Player_Test player_test;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void Start()
    {
        gameManager = GameManager.Instance;
        player_test = gameManager.Player_Test;
        player_test.ResetMotionAndPosition();

        Debug.Log("게임매니저 찾음");
        button.onClick.AddListener(GameStart);
    }

    private void GameStart()
    {
        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        Debug.Log("버튼 누름");


        // 2번 미궁 탐색 씬으로 이동
        onSceneChangeButton?.Invoke(2);
    }
}
