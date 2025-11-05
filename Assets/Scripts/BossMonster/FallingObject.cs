using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingObject : MonoBehaviour
{
    /// <summary>
    /// 낙하 오브젝트로 주는 데미지
    /// </summary>
    float fallingObjectDamage = 10;

    /// <summary>
    /// 보스 몬스터 베이스
    /// </summary>
    BossMonsterBase bossMonsterBase;

    /// <summary>
    /// 플레이어
    /// </summary>
    Player_Test player_test;

    /// <summary>
    /// 이 오브젝트의 지속 시간
    /// </summary>
    float duration = 3;

    private void Start()
    {
        bossMonsterBase = FindAnyObjectByType<BossMonsterBase>();
        player_test = GameManager.Instance.Player_Test;

        StartCoroutine(LifeDuration());
    }

    private void ApplyFallingObjectDamage()
    {
        player_test.OnPlayerApplyDamage(fallingObjectDamage);

        // 만약 몬스터 타입이 하드, 나이트메어, 헬 이면 기절 추가
        if(bossMonsterBase.bossType == BossType.HardBoss || bossMonsterBase.bossType == BossType.NightmareBoss || bossMonsterBase.bossType == BossType.HellBoss)
        {
            // 플레이어의 움직임 잠시동안 정지 부분
            player_test.StunPlayer();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // 만약 낙하 오브젝트와 플레이어가 충돌했으면
            Debug.Log("플레이어와 낙하오브젝트가 충돌 감지");
            ApplyFallingObjectDamage();
        }
    }

    /// <summary>
    /// 이 오브젝트의 파괴까지 걸리는 시간
    /// </summary>
    /// <returns></returns>
    IEnumerator LifeDuration()
    {
        // 지속시간까지 기다리고
        yield return new WaitForSeconds(duration);

        // 이 오브젝트 파괴
        Destroy(this.gameObject);
    }
}
