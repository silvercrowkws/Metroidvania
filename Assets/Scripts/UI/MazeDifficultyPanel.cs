using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MazeDifficultyPanel : MonoBehaviour
{
    /// <summary>
    /// 난이도 텍스트
    /// </summary>
    TextMeshProUGUI diffText;

    public string targetDifficultyName;

    /// <summary>
    /// 진입하기 버튼
    /// </summary>
    Button confirmButton;

    /// <summary>
    /// 물러나기 버튼
    /// </summary>
    Button cancelButton;

    /// <summary>
    /// 게임 매니저
    /// </summary>
    GameManager gameManager;

    Player_Test player_test;

    public Action<int> onSceneChangeButton;

    /// <summary>
    /// 돌아가기 버튼 클릭으로 대사를 변경하는 델리게이트
    /// </summary>
    public Action<bool> onMazeCanceled;

    private void Awake()
    {
        Transform child = transform.GetChild(1);

        diffText = child.GetChild(0).GetComponent<TextMeshProUGUI>();
        
        confirmButton = transform.GetChild(2).GetComponent<Button>();
        confirmButton.onClick.AddListener(Confirm);

        cancelButton = transform.GetChild(3).GetComponent<Button>();
        cancelButton.onClick.AddListener(Cancel);
    }

    private void Start()
    {
        gameManager = GameManager.Instance;
        player_test = gameManager.Player_Test;
        player_test.ResetMotionAndPosition();
    }

    private void OnEnable()
    {
        onMazeCanceled?.Invoke(true);
        diffText.text = targetDifficultyName;
    }

    /// <summary>
    /// 미궁 진입 버튼
    /// </summary>
    private void Confirm()
    {
        // => DialoguePanel에서 처리
        /*switch(targetDifficultyName)
        {
            case "Easy":
                gameManager.GameDifficulty = GameDifficulty.Easy;
                break;
            case "Normal":
                gameManager.GameDifficulty = GameDifficulty.Normal;
                break;
            case "Hard":
                gameManager.GameDifficulty = GameDifficulty.Hard;
                break;
            case "Nightmare":
                gameManager.GameDifficulty = GameDifficulty.Nightmare;
                break;
            case "Hell":
                gameManager.GameDifficulty = GameDifficulty.Hell;
                break;

        }*/
        onSceneChangeButton?.Invoke(2);
    }

    /// <summary>
    /// 취소 버튼
    /// </summary>
    private void Cancel()
    {
        onMazeCanceled?.Invoke(false);
        this.gameObject.SetActive(false);
    }
}
