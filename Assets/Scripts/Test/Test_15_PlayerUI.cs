using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Test_15_PlayerUI : TestBase
{
    GameManager gameManager;

    Player_Test player_test;

    private void Start()
    {
        gameManager = GameManager.Instance;
        player_test = gameManager.Player_Test;
    }

    protected override void OnTest1(InputAction.CallbackContext context)
    {
        player_test.transform.position = new Vector3(0, -9.05f, 0);
        gameManager.OnSceneChange(2);
    }

    protected override void OnTest2(InputAction.CallbackContext context)
    {
        player_test.Fullness += 10;
    }

    protected override void OnTest3(InputAction.CallbackContext context)
    {
        player_test.Fullness -= 10;
    }
}
