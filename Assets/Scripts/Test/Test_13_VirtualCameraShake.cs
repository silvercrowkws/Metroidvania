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


    private void Start()
    {
        cameraShakeController = FindAnyObjectByType<CameraShakeController>();
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

#endif
}
