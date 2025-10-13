using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Test_13_VirtualCameraShake : TestBase
{
    CameraShakeController cameraShakeController;

    public float shakeTime = 3f;
    public float shakePower = 5f;
    public float shakeSpeed = 2f;

    Player_Test player_test;


    private void Start()
    {
        cameraShakeController = FindAnyObjectByType<CameraShakeController>();

        player_test = GameManager.Instance.Player_Test;
    }

#if UNITY_EDITOR

    protected override void OnTest1(InputAction.CallbackContext context)
    {
        Time.timeScale = 0;
        cameraShakeController.StartShake(shakeTime, shakePower, shakeSpeed);
    }

    protected override void OnTest2(InputAction.CallbackContext context)
    {
        Time.timeScale = 1;
    }

    protected override void OnTest3(InputAction.CallbackContext context)
    {
        player_test.HP += 10;
    }

    protected override void OnTest4(InputAction.CallbackContext context)
    {
        player_test.HP -= 10;
    }

#endif
}
