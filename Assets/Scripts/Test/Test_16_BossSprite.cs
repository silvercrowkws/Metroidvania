using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Test_16_BossSprite : TestBase
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
        player_test.HP -= 10;
    }
}
