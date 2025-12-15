using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PressAnyKey : MonoBehaviour
{
    Button pressAnyKeyButton;

    GameManager gameManager;

    public Action<int> onSceneChangeAnyKey;

    private void Awake()
    {
        pressAnyKeyButton = GetComponent<Button>();
    }

    private void Start()
    {
        gameManager = GameManager.Instance;
        pressAnyKeyButton.onClick.AddListener(PressButton);
    }

    /// <summary>
    /// 화면 클릭으로 씬을 넘어가는 함수
    /// </summary>
    private void PressButton()
    {
        // 1번째 로비씬으로 이동
        onSceneChangeAnyKey?.Invoke(1);
    }
}
