using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeartPanel : MonoBehaviour
{
    /// <summary>
    /// 최대 HP
    /// </summary>
    public float maxHP;

    /// <summary>
    /// 하트 한 칸당 HP
    /// </summary>
    public int hpPerHeart = 10;

    /// <summary>
    /// 하트 한 칸당 Fill 개수(1/4씩)
    /// </summary>
    public int fillPerHeart = 4;

    /// <summary>
    /// 하트 프리팹
    /// </summary>
    public GameObject heartPrefab;

    /// <summary>
    /// 하트들을 담을 부모 오브젝트
    /// </summary>
    Transform heartParent;

    public List<Image> heartFills = new List<Image>();

    Player player;

    Player_Test player_Test;

    private void Awake()
    {
        heartParent = this.transform;
    }

    private void Start()
    {
        GameManager gameManager = GameManager.Instance;
        player = gameManager.Player;
        if(player == null)
        {
            player_Test = gameManager.Player_Test;
            maxHP = player_Test.maxHP;
        }
        else
        {
            maxHP = player.maxHP;
        }

        InitHearts();
    }

    /// <summary>
    /// 하트 UI 초기화
    /// </summary>
    private void InitHearts()
    {
        //float heartCount = maxHP / hpPerHeart;
        int heartCount = Mathf.CeilToInt(maxHP / hpPerHeart);
        for (int i = 0; i < heartCount; i++)
        {
            GameObject heart = Instantiate(heartPrefab, heartParent);
            heart.name = $"Heart_{i}";

            // HeartFill 찾기
            Image heartFill = heart.transform.GetChild(0).GetComponent<Image>();
            if (heartFill != null)
            {
                heartFills.Add(heartFill);
            }
        }
    }

    /// <summary>
    /// HP 변경 시 하트 Fill 업데이트
    /// </summary>
    /// <param name="currentHP"></param>
    public void UpdateHearts(float currentHP)
    {
        int heartCount = heartFills.Count;
        float hpLeft = currentHP;

        for (int i = 0; i < heartCount; i++)
        {
            float heartValue = Mathf.Clamp(hpLeft, 0, hpPerHeart);
            float fillAmount = heartValue / hpPerHeart; // 0~1
            heartFills[i].fillAmount = fillAmount;
            hpLeft -= hpPerHeart;
        }
    }
}