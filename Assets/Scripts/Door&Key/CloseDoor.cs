using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloseDoor : MonoBehaviour
{
    // 신호를 받으면 애니메이션 실행
    Animator animator;

    SpriteRenderer spriteRenderer;

    /// <summary>
    /// 서서히 사라지는데 걸리는 시간
    /// </summary>
    [Header("페이드 아웃 설정")]
    public float fadeDuration = 1.5f;

    /// <summary>
    /// 시작 색상
    /// </summary>
    Color startColor;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        startColor = spriteRenderer.color;

        animator.speed = 0;
    }

    public void OnAnimationStart()
    {
        StartCoroutine(CloseDoorControlCoroutine());
    }

    /// <summary>
    /// 이 오브젝트를 컨트롤 하는 코루틴
    /// </summary>
    /// <returns></returns>
    IEnumerator CloseDoorControlCoroutine()
    {
        yield return new WaitForSeconds(1);

        animator.speed = 1;

        yield return new WaitForSeconds(1);     // 플레이어 연출 끝나고

        // 알파값 조절로 서서히 사라지고 0 이되면
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startColor.a, 0f, timer / fadeDuration);        // 선형 보간으로 알파값 계산

            // 새로운 알파값으로 색상 업데이트
            spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, newAlpha);

            yield return null;      // 다음 프레임까지 대기
        }

        // 혹시 모르니 알파값을 확실히 0으로 설정
        spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, 0f);
        Destroy(this.gameObject);               // 이 오브젝트 파괴
    }
}
