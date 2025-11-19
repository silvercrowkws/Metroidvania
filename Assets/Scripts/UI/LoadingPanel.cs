using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//using UnityEngine.UIElements;

public class LoadingPanel : MonoBehaviour
{
    /// <summary>
    /// 게임 매니저
    /// </summary>
    GameManager gameManager;

    /// <summary>
    /// 캔버스 그룹
    /// </summary>
    CanvasGroup canvasGroup;

    /// <summary>
    /// 로딩바 슬라이더
    /// </summary>
    Slider loadingSlider;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();

        canvasGroup.alpha = 0;                  // 알파값 0
        canvasGroup.interactable = true;        // 상호작용 가능
        canvasGroup.blocksRaycasts = false;     // 레이케스트 가능

        Transform child = transform.GetChild(1);
        loadingSlider = child.GetComponent<Slider>();
    }

    private void Start()
    {
        gameManager = GameManager.Instance;
        gameManager.onPanelActive += OnPanelActive;
        gameManager.onLoadingBar += OnLoadingBar;
    }

    /// <summary>
    /// 게임 매니저의 요청으로 로딩 패널을 조절하는 함수
    /// </summary>
    /// <param name="active"></param>
    private void OnPanelActive(bool active)
    {
        if(active)
        {
            canvasGroup.alpha = 1;                  // 알파값 1로
            canvasGroup.interactable = false;       // 상호작용 불가능
            canvasGroup.blocksRaycasts = true;      // 레이케스트 차단
        }
        else
        {
            canvasGroup.alpha = 0;                  // 알파값 0
            canvasGroup.interactable = true;        // 상호작용 가능
            canvasGroup.blocksRaycasts = false;     // 레이케스트 가능
        }
    }

    private void OnLoadingBar(float sliderValue)
    {
        loadingSlider.value = sliderValue;
    }
}
