using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryPanel : MonoBehaviour
{
    private CanvasGroup canvasGroup;

    //private bool isVisible = true;

    Player player;
    Player_Test playerTest;

    private void Awake()
    {
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
            //player.onInventoryToggle += OnInventoryToggle;
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
        }
    }    

    private void SetInventoryAlpha(float alpha)
    {
        canvasGroup.alpha = alpha;
    }
}
