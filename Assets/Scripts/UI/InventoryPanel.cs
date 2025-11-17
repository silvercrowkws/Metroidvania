using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryPanel : Singleton<InventoryPanel>
{
    private CanvasGroup canvasGroup;

    //private bool isVisible = true;

    Player player;
    Player_Test playerTest;

    public Action onButtonsActiveFalse;

    private void Awake()
    {
        var others = FindObjectsOfType<InventoryPanel>();
        if (others.Length > 1)
        {
            // 이미 다른 인스턴스가 존재하면 자신을 파괴하고 초기화 중단
            Destroy(gameObject);
            return;
        }

        // 씬 전환 시 이 게임오브젝트가 파괴되지 않도록 설정
        DontDestroyOnLoad(gameObject);

        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        player = GameManager.Instance.Player;
        if(player == null)
        {
            playerTest = GameManager.Instance.Player_Test;
            playerTest.onInventoryToggle += OnInventoryToggle;
        }
        else
        {
            //playerTransform.onInventoryToggle += OnInventoryToggle;
        }

        SetInventoryAlpha(0);        // 초기 알파값 설정
    }

    private void OnInventoryToggle(bool isVisible)
    {
        if (isVisible)
        {
            SetInventoryAlpha(1f);
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        else
        {
            SetInventoryAlpha(0f);
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            onButtonsActiveFalse?.Invoke();
        }
    }

    private void SetInventoryAlpha(float alpha)
    {
        canvasGroup.alpha = alpha;
    }
}
