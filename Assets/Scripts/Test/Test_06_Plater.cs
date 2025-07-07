using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Test_06_Plater : TestBase
{
#if UNITY_EDITOR
    GameManager gameManager;

    /// <summary>
    /// 플레이어
    /// </summary>
    Player player;

    Player_Test player_test;

    SceneManager sceneManager;

    HeartPanel heartPanel;

    private void Start()
    {
        gameManager = GameManager.Instance;
        player = gameManager.Player;
        if(player == null)
        {
            player_test = gameManager.Player_Test;
        }

        heartPanel = FindAnyObjectByType<HeartPanel>();
    }

    protected override void OnTest1(InputAction.CallbackContext context)
    {
        if(player != null)
        {
            player.HP -= 2.5f;
            heartPanel.UpdateHearts(player.HP);
        }
        else if(player_test != null)
        {
            player_test.HP -= 2.5f;
            heartPanel.UpdateHearts(player_test.HP);
        }
    }

    protected override void OnTest2(InputAction.CallbackContext context)
    {
        if (player != null)
        {
            player.HP -= 5f;
            heartPanel.UpdateHearts(player.HP);
        }
        else if (player_test != null)
        {
            player_test.HP -= 5f;
            heartPanel.UpdateHearts(player_test.HP);
        }
    }

    protected override void OnTest3(InputAction.CallbackContext context)
    {
        if (player != null)
        {
            player.HP -= 10f;
            heartPanel.UpdateHearts(player.HP);
        }
        else if (player_test != null)
        {
            player_test.HP -= 10f;
            heartPanel.UpdateHearts(player_test.HP);
        }
    }

    protected override void OnTest4(InputAction.CallbackContext context)
    {
        if (player != null)
        {
            player.HP += 2.5f;
            heartPanel.UpdateHearts(player.HP);
        }
        else if (player_test != null)
        {
            player_test.HP += 2.5f;
            heartPanel.UpdateHearts(player_test.HP);
        }
    }

    protected override void OnTest5(InputAction.CallbackContext context)
    {
        if (player != null)
        {
            player.HP += 5f;
            heartPanel.UpdateHearts(player.HP);
        }
        else if (player_test != null)
        {
            player_test.HP += 5f;
            heartPanel.UpdateHearts(player_test.HP);
        }
    }

    protected override void OnTest6(InputAction.CallbackContext context)
    {
        if (player != null)
        {
            player.HP += 10f;
            heartPanel.UpdateHearts(player.HP);
        }
        else if (player_test != null)
        {
            player_test.HP += 10f;
            heartPanel.UpdateHearts(player_test.HP);
        }
    }
#endif 
}
