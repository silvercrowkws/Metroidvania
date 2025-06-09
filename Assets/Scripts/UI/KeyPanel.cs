using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeyPanel : MonoBehaviour
{
    Image[] keyImages;

    GameManager gameManager;
    Player player;

    private void Awake()
    {
        int childCount = 3;
        keyImages = new Image[childCount];      // 이미지 배열 초기화

        for (int i = 0; i < childCount; i++)
        {
            keyImages[i] = transform.GetChild(i).GetComponent<Image>();
            SetAlpha(keyImages[i], 0f); // 시작 시 모두 투명하게
        }
    }

    private void OnEnable()
    {
        gameManager = GameManager.Instance;
        player = gameManager.Player;
        player.onKeyCountChanged += SetKeyImage;

    }

    private void OnDisable()
    {
        player.onKeyCountChanged -= SetKeyImage;
    }

    /// <summary>
    /// 키 개수만큼 이미지를 활성화(알파값 1)로 변경
    /// </summary>
    public void SetKeyImage(int keyCount)
    {
        for (int i = 0; i < keyImages.Length; i++)
        {
            SetAlpha(keyImages[i], i < keyCount ? 1f : 0f);
        }
    }

    /// <summary>
    /// 알파값을 투명하게 하는 함수
    /// </summary>
    /// <param name="img"></param>
    /// <param name="alpha"></param>
    private void SetAlpha(Image img, float alpha)
    {
        Color c = img.color;
        c.a = alpha;
        img.color = c;
    }
}
