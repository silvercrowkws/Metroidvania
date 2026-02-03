using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ExitButton : MonoBehaviour
{
    ExitActions inputActions;

    Button tButton;
    Button fButton;

    private void Awake()
    {
        inputActions = new ExitActions();
        tButton = transform.GetChild(0).GetComponent<Button>();
        tButton.onClick.AddListener(TButton);        

        fButton = transform.GetChild(1).GetComponent<Button>();
        fButton.onClick.AddListener(FButton);

        tButton.gameObject.SetActive(false);
        fButton.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        inputActions.Actions.Enable();
        inputActions.Actions.Exit.performed += OnExit;
    }

    private void OnDisable()
    {
        inputActions.Actions.Exit.performed -= OnExit;
        inputActions.Actions.Disable();
    }

    private void OnExit(InputAction.CallbackContext context)
    {
        tButton.gameObject.SetActive(true);
        fButton.gameObject.SetActive(true);
    }

    private void TButton()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
    }

    private void FButton()
    {
        tButton.gameObject.SetActive(false);
        fButton.gameObject.SetActive(false);
    }
}
