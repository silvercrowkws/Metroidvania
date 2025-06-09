using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Test_01_Sprite : TestBase
{
#if UNITY_EDITOR
    GameManager gameManager;

    /// <summary>
    /// 플레이어
    /// </summary>
    Player player;

    private void Start()
    {
        gameManager = GameManager.Instance;
        player = gameManager.Player;
    }    

    protected override void OnTest1(InputAction.CallbackContext context)
    {
        Debug.Log("아이들 상태");
        //player.triggerNumber = 0;
    }

    protected override void OnTest2(InputAction.CallbackContext context)
    {
        Debug.Log("점프 상태");
        //player.triggerNumber = 1;
    }

    protected override void OnTest3(InputAction.CallbackContext context)
    {
        Debug.Log("걷기 상태");
        //player.triggerNumber = 2;
    }

    protected override void OnTest4(InputAction.CallbackContext context)
    {
        Debug.Log("기어갈 준비");
        //player.triggerNumber = 3;
    }

    protected override void OnTest5(InputAction.CallbackContext context)
    {
        Debug.Log("기어가기");
        //player.triggerNumber = 4;
    }

    protected override void OnTest6(InputAction.CallbackContext context)
    {
        Debug.Log("기어가기 끝");
        //player.triggerNumber = 5;
    }
#endif
}
