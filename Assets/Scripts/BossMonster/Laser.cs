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

    private void Awake()
    {
        
    }

    private void Start()
    {
        bossMonsterBase = GetComponentInParent<BossMonsterBase>();
        bossMonsterBase.onBossDie += OnBossDie;
        player_test = GameManager.Instance.Player_Test;

        CalculateLaserDamage();
    }

    private void OnBossDie()
    {
        // 혹시 맞을 지도 모르니 레이저 데미지 0으로 변경
        laserDamage = 0;
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
