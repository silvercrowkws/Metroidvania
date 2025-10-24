using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingObject : MonoBehaviour
{
    /// <summary>
    /// 레이저로 주는 데미지
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
    /// 플레이어가 낙하 오브젝트에 맞아 잠시 못 움직인다고 알리는 델리게이트
    /// </summary>
    public Action onFallingObject;

    private void Start()
    {
        bossMonsterBase = GetComponentInParent<BossMonsterBase>();
        player_test = GameManager.Instance.Player_Test;
    }

    private void ApplyFallingObjectDamage()
    {
        player_test.OnPlayerApplyDamage(fallingObjectDamage);

        // 만약 몬스터 타입이 하드, 나이트메어, 헬 이면 기절 추가
        if(bossMonsterBase.bossType == BossType.HardBoss || bossMonsterBase.bossType == BossType.NightmareBoss || bossMonsterBase.bossType == BossType.HellBoss)
        {
            // 플레이어의 움직임 잠시동안 정지 부분
            onFallingObject?.Invoke();
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
}
