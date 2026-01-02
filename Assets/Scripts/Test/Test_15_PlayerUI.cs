using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Test_15_PlayerUI : TestBase
{
    GameManager gameManager;

    Player_Test player_test;

    Door door;

    /// <summary>
    /// 플레이어의 정보(저장용)
    /// </summary>
    PlayerData tempPlayerData;

    private void Start()
    {
        gameManager = GameManager.Instance;
        player_test = gameManager.Player_Test;
        door = FindAnyObjectByType<Door>();
    }

    protected override void OnTest1(InputAction.CallbackContext context)
    {
        player_test.transform.position = new Vector3(0, -9.05f, 0);
        gameManager.OnSceneChange(3);
    }

    protected override void OnTest2(InputAction.CallbackContext context)
    {
        player_test.onKeyCountChanged(3);
        //player_test.hasAllKeys = true;
        door.transform.position = new Vector2(player_test.transform.position.x, player_test.transform.position.y + 0.75f);
        player_test.canEnterDoor = true;
        //player_test.Fullness += 10;
    }

    protected override void OnTest3(InputAction.CallbackContext context)
    {
        player_test.Money += 1;
    }
}
