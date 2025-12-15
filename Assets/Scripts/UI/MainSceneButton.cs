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

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(GameStart);
    }

    private void Start()
    {
        gameManager = GameManager.Instance;
    }

    private void GameStart()
    {
        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        onSceneChangeButton?.Invoke(1);
    }
}
