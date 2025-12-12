using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Laser : MonoBehaviour
{
    /// <summary>
    /// 레이저로 주는 데미지
    /// </summary>
    float laserDamage = 0;

    /// <summary>
    /// 보스 몬스터 베이스
    /// </summary>
    BossMonsterBase bossMonsterBase;

    /// <summary>
    /// 플레이어
    /// </summary>
    Player_Test player_test;

    /// <summary>
    /// 데미지 적용 가능한지 확인용
    /// </summary>
    bool canApplyDamage = true;

    SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        canApplyDamage = true;

        bossMonsterBase = GetComponentInParent<BossMonsterBase>();
        bossMonsterBase.onBossDie += OnBossDie;
        player_test = GameManager.Instance.Player_Test;

        CalculateLaserDamage();
    }

    private void OnBossDie()
    {
        // 혹시 맞을 지도 모르니 레이저 데미지 0으로 변경
        canApplyDamage = false;
        laserDamage = 0;

        // 레이저 바로 사라지지 않고 알파값 줄어들다가 없어지는 것으로 수정
        StartCoroutine(FadeOutLaser());
    }

    /// <summary>
    /// 레이저를 알파값 감소시키며 사라지게 하는 코루틴
    /// </summary>
    IEnumerator FadeOutLaser()
    {
        float duration = 0.8f;   // 페이드 아웃 시간
        float elapsed = 0f;

        Color color = spriteRenderer.color;

        /*// 사라지는 동안 충돌 비활성화 (선택사항)
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;*/

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            color.a = Mathf.Lerp(1f, 0f, t);
            spriteRenderer.color = color;

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 완전히 투명한 상태로
        color.a = 0f;
        spriteRenderer.color = color;

        Destroy(gameObject);
    }

    /// <summary>
    /// 보스에 따라 레이저의 데미지를 결정하는 함수
    /// </summary>
    private void CalculateLaserDamage()
    {
        switch (bossMonsterBase.bossType)
        {
            case BossType.EasyBoss:
                laserDamage = 20f;
                break;
            case BossType.NormalBoss:
                laserDamage = 30f;
                break;
            case BossType.HardBoss:
                laserDamage = 50f;
                break;
            case BossType.NightmareBoss:
                laserDamage = 80f;
                break;
            case BossType.HellBoss:
                laserDamage = 9999f;    // 즉사 처리용
                break;
            default:
                laserDamage = 10f;
                break;
        }
    }

    private void ApplyLaserDamage()
    {
        if (canApplyDamage)
        {
            player_test.OnPlayerApplyDamage(laserDamage);
            StartCoroutine(DamageCooldown());
        }
    }

    private IEnumerator DamageCooldown()
    {
        canApplyDamage = false;
        yield return new WaitForSeconds(1f);
        canApplyDamage = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            // 만약 레이저와 플레이어가 충돌했으면
            Debug.Log("플레이어와 레이저가 충돌 감지");
            ApplyLaserDamage();
        }
    }

    private void OnDestroy()
    {
        bossMonsterBase.onBossDie -= OnBossDie;
    }
}
