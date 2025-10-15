using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Test_14_GameDifficulty : TestBase
{
    Player_Test player_test;

    private void Start()
    {
        player_test = GameManager.Instance.Player_Test;
    }

#if UNITY_EDITOR

    protected override void OnTest1(InputAction.CallbackContext context)
    {
        Debug.Log("플레이어의 레벨업에 필요한 경험치");
        Debug.Log(player_test.MaxXP);
    }

    protected override void OnTest2(InputAction.CallbackContext context)
    {
        Debug.Log("플레이어의 현재 경험치");
        Debug.Log(player_test.XP);
    }

    protected override void OnTest3(InputAction.CallbackContext context)
    {
        player_test.XP += 50;
    }

#endif
}
