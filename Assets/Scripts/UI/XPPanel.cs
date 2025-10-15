using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class XPPanel : MonoBehaviour
{
    Slider xpSlider;

    TextMeshProUGUI currentXPText;
    TextMeshProUGUI maxXPText;
    TextMeshProUGUI percentText;

    Player_Test player_test;

    private void Awake()
    {
        Transform child = transform.GetChild(0);
        xpSlider = child.GetComponent<Slider>();

        // 슬라이더의 범위를 0과 1 사이로 고정
        xpSlider.minValue = 0;
        xpSlider.maxValue = 1;

        child = transform.GetChild(1);
        currentXPText = child.GetChild(0).GetComponent<TextMeshProUGUI>();
        maxXPText = child.GetChild(2).GetComponent<TextMeshProUGUI>();

        percentText = child.GetChild(3).GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        player_test = GameManager.Instance.Player_Test;
        player_test.onPlayerXPChange += OnPlayerXPChange;
        player_test.onPlayerMaxXPChange += OnPlayerMaxXPChange;
    }

    /// <summary>
    /// 플레이어의 현재 경험치가 변경되면 실행될 함수
    /// </summary>
    /// <param name="xp"></param>
    private void OnPlayerXPChange(float xp)
    {
        currentXPText.text = xp.ToString("F0"); // 소수점 없이 깔끔하게 표시
        UpdateXPUI();
    }

    /// <summary>
    /// 플레이어의 최대 경험치가 변경되면 실행될 함수
    /// </summary>
    /// <param name="maxXP"></param>
    private void OnPlayerMaxXPChange(float maxXP)
    {
        maxXPText.text = maxXP.ToString("F0"); // 소수점 없이 깔끔하게 표시
        UpdateXPUI();
    }

    /// <summary>
    /// 현재 경험치와 최대 경험치를 기반으로 슬라이더 값을 업데이트하는 함수
    /// </summary>
    private void UpdateXPUI()
    {
        float xpValue = player_test.XP / player_test.MaxXP;

        float perXP = xpValue * 100;
        percentText.text = $"[{perXP.ToString("F2")}%]";

        if (player_test.MaxXP > 0)
        {
            // 현재 XP / 최대 XP 로 백분율을 계산하여 슬라이더 값에 적용
            xpSlider.value = xpValue;
        }
        else
        {
            xpSlider.value = 0;
        }
    }
}
