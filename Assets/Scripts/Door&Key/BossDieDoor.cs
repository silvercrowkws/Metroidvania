using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class BossDieDoor : MonoBehaviour
{
    // 신호를 받으면 애니메이션 실행
    Animator animator;

    SpriteRenderer spriteRenderer;

    /// <summary>
    /// 서서히 나타나는데 걸리는 시간
    /// </summary>
    [Header("페이드 인 설정")]
    public float fadeDuration = 1.5f;

    /// <summary>
    /// 시작 색상
    /// </summary>
    Color startColor;   // 알파 0

    /// <summary>
    /// 바뀔 색상
    /// </summary>
    Color targetColor;  // 원래 색 (알파 1)

    /// <summary>
    /// 콜라이더
    /// </summary>
    BoxCollider2D box2D;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        box2D = GetComponent<BoxCollider2D>();

        // 원래 색 저장
        targetColor = spriteRenderer.color;

        // 시작 색은 알파 0
        startColor = new Color(
            targetColor.r,
            targetColor.g,
            targetColor.b,
            0f
        );

        // 시작 시 투명하게 설정
        spriteRenderer.color = startColor;

        animator.speed = 0;

        // 충돌 불가능하도록
        box2D.enabled = false;
    }

    private void Start()
    {
        StartCoroutine(OpenDoorControlCoroutine());
    }

    IEnumerator OpenDoorControlCoroutine()
    {
        float timer = 0f;

        // fadeDuration초 동안 페이드 인
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;

            spriteRenderer.color = Color.Lerp(
                startColor,
                targetColor,
                timer / fadeDuration
            );

            yield return null;
        }

        // 최종 색 보정 (알파 1 보장)
        spriteRenderer.color = targetColor;

        // 페이드 인 완료 후 애니메이션 시작
        animator.speed = 1;

        animator.SetTrigger("Open");

        // 충돌 가능하도록
        box2D.enabled = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 플레이어와 충돌했는지 확인
        Player_Test player_test = collision.GetComponent<Player_Test>();
        if (player_test != null)
        {
            // 플레이어와 충돌
            player_test.canEnterDoor = true;
            Debug.Log("플레이어와 충돌");
        }
    }
}
