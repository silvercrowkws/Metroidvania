using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Notification : MonoBehaviour
{
    /// <summary>
    /// 정보 텍스트
    /// </summary>
    TextMeshProUGUI notiText;

    /// <summary>
    /// 캔버스 그룹
    /// </summary>
    CanvasGroup canvasGroup;

    /// <summary>
    /// 게임 매니저
    /// </summary>
    GameManager gameManager;

    private void Awake()
    {
        notiText = transform.GetChild(0).GetComponent<TextMeshProUGUI>();

        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        gameManager = GameManager.Instance;
        gameManager.onNotificationActive += OnNotificationActive;
        gameManager.onNotificationText += OnNotificationText;

        // 시작 시 안보이게
        canvasGroup.alpha = 0f;
    }

    /// <summary>
    /// 게임 매니저의 델리게이트를 받아 캔버스 그룹을 조절하는 함수
    /// </summary>
    /// <param name="active"></param>
    private void OnNotificationActive(bool active)
    {
        if (active)
        {
            // 활성화 부분
            canvasGroup.alpha = 1.0f;
        }
        else
        {
            // 비활성화 부분
            canvasGroup.alpha = 0f;
        }
    }

    /// <summary>
    /// 게임 매니저의 델리게이트를 받아 텍스트를 조절하는 함수
    /// </summary>
    /// <param name="obj"></param>
    private void OnNotificationText(string obj)
    {
        notiText.text = obj.ToString();
    }
}
