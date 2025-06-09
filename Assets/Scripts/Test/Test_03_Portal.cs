using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Test_03_Portal : TestBase
{
#if UNITY_EDITOR
    GameManager gameManager;

    /// <summary>
    /// 플레이어
    /// </summary>
    Player player;

    SceneManager sceneManager;

    private void Start()
    {
        gameManager = GameManager.Instance;
        player = gameManager.Player;
    }

    protected override void OnTest1(InputAction.CallbackContext context)
    {
        SceneManager.LoadScene(1);
    }

    protected override void OnTest2(InputAction.CallbackContext context)
    {
        SceneManager.LoadScene(0);
    }

    protected override void OnTest3(InputAction.CallbackContext context)
    {
        
    }

    protected override void OnTest4(InputAction.CallbackContext context)
    {
        
    }

    protected override void OnTest5(InputAction.CallbackContext context)
    {
        
    }

    protected override void OnTest6(InputAction.CallbackContext context)
    {
        
    }
#endif
}
