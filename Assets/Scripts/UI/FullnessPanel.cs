using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FullnessPanel : MonoBehaviour
{
    Slider fullnessSlider;

    Image fillImage;

    TextMeshProUGUI currentFullnessText;
    //TextMeshProUGUI maxFullnessText;
    TextMeshProUGUI percentText;

    Player_Test player_test;

    // 색상 코드 상수 정의
    private readonly Color Color_Buff = new Color(0.5647f, 0.9333f, 0.5647f); // #90EE90 (밝은 초록)
    private readonly Color Color_Normal = new Color(1.0000f, 0.7647f, 0.0000f); // #FFC300 (금색/노란색)
    private readonly Color Color_Warning = new Color(1.0000f, 0.5490f, 0.0000f); // #FF8C00 (진한 주황)
    private readonly Color Color_Danger = new Color(0.8627f, 0.0784f, 0.2353f); // #DC143C (진홍색/빨간색)
    private readonly Color Color_Critical = new Color(0.5451f, 0.0314f, 0.0314f); // #8B0000 (검붉은색)

    // 이전 색상을 추적하여 불필요한 색상 변경을 방지
    private Color lastAppliedColor = Color.white;

    private void Awake()
    {
        Transform child = transform.GetChild(0);
        fullnessSlider = child.GetComponent<Slider>();

        // 슬라이더의 범위를 0과 1 사이로 고정
        fullnessSlider.minValue = 0;
        fullnessSlider.maxValue = 1;

        Transform fillArea = fullnessSlider.transform.Find("Fill Area");
        if (fillArea != null)
        {
            Transform fill = fillArea.Find("Fill");
            if (fill != null)
            {
                fillImage = fill.GetComponent<Image>();
            }
        }

        child = transform.GetChild(1);
        currentFullnessText = child.GetChild(0).GetComponent<TextMeshProUGUI>();
        //maxFullnessText = child.GetChild(2).GetComponent<TextMeshProUGUI>();

        percentText = child.GetChild(3).GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        player_test = GameManager.Instance.Player_Test;
        player_test.onPlayerFullnessChange += OnPlayerFullnessChange;
        UpdateXPUI(); // 초기 UI 설정
    }

    /// <summary>
    /// 플레이어의 현재 배부름이 변경되면 실행될 함수
    /// </summary>
    /// <param name="xp"></param>
    private void OnPlayerFullnessChange(float xp)
    {
        //currentFullnessText.text = xp.ToString("F0"); // 소수점 없이 깔끔하게 표시
        UpdateXPUI();
    }

    /// <summary>
    /// 현재 배부름과 최대 배부름을 기반으로 슬라이더 값을 업데이트하는 함수
    /// </summary>
    private void UpdateXPUI()
    {
        /*float fullnessValue = player_test.Fullness / player_test.maxFullness;
        float fullXP = fullnessValue * 100;
        percentText.text = $"[{fullXP.ToString("F2")}%]";*/

        float fullnessRatio = player_test.Fullness / player_test.maxFullness; // 0.0 ~ 1.0 비율
        float fullnessPercent = fullnessRatio * 100f; // 0.0% ~ 100.0%
        percentText.text = $"[{fullnessPercent.ToString("F2")}%]";

        if (player_test.maxFullness > 0)
        {
            // 1. 슬라이더 값 업데이트 (0.0 ~ 1.0)
            fullnessSlider.value = fullnessRatio;

            // 2. 색상 변경 로직 적용
            Color targetColor;

            if (fullnessPercent >= 70f)
            {
                targetColor = Color_Buff;       // 100% ~ 70% : 밝은 초록색 (#90EE90)
            }
            else if (fullnessPercent >= 51f)
            {
                targetColor = Color_Normal;     // 69% ~ 51% : 노란색 (#FFC300)
            }
            else if (fullnessPercent >= 31f)
            {
                targetColor = Color_Warning;    // 50% ~ 31% : 주황색 (#FF8C00)
            }
            else if (fullnessPercent >= 11f)
            {
                targetColor = Color_Danger;     // 30% ~ 11% : 빨간색 (#DC143C)
            }
            else
            {
                targetColor = Color_Critical;   // 10% ~ 0% : 검붉은색 (#8B0000)
            }

            // 색상이 변경되어야 할 때만 업데이트하여 성능 최적화
            if (fillImage != null && targetColor != lastAppliedColor)
            {
                fillImage.color = targetColor;
                lastAppliedColor = targetColor;
            }
        }
        else
        {
            fullnessSlider.value = 0;
            if (fillImage != null)
            {
                fillImage.color = Color_Critical; // 0% 일때는 최저 색상으로 고정
            }
        }
    }
}
