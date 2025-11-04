using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FireFloor : MonoBehaviour
{
    // 장판 지속시간은 약 5초 정도
    // 만약 플레이어가 충돌하면 지속적으로 데미지를 준다

    // 미사일 회전 5초, 발사 전까지 1초에
    // 나이트메어는 보스가 장판 사라지고 미사일 생성하고
    // 헬은 장판 생성과 동시에 미사일 생성하면 되겠네.

    /// <summary>
    /// 이 오브젝트의 지속 시간
    /// </summary>
    float duration = 0;

    /// <summary>
    /// 플레이어
    /// </summary>
    Player_Test player_test;

    /// <summary>
    /// 보스 몬스터 베이스
    /// </summary>
    BossMonsterBase bossMonsterBase;

    /// <summary>
    /// 장판 데미지
    /// </summary>
    float damage = 10;

    /// <summary>
    /// 데미지 적용이 가능한지 확인하는 bool 변수
    /// </summary>
    bool canDamage = true;

    private void Awake()
    {
        
    }

    private void Start()
    {
        player_test = GameManager.Instance.Player_Test;

        bossMonsterBase = FindAnyObjectByType<BossMonsterBase>();

        // 보스 타입에 따라 데미지와 지속시간 결정
        switch (bossMonsterBase.bossType)
        {
            case BossType.NightmareBoss:
                damage = 10f;
                duration = 5;
                break;
            case BossType.HellBoss:
                damage = 20f;
                duration = 5;
                break;
            default:
                damage = 1;
                duration = 1;
                break;
        }

        StartCoroutine(LifeDuration());

        // 만약 헬 보스면 미사일 바로 생성하라고 알림
        if (bossMonsterBase.bossType == BossType.HellBoss)
        {

        }
    }

    private void OnDestroy()
    {
        // 만약 나이트메어 보스면 지금 생성하라고 알림
        if (bossMonsterBase.bossType == BossType.NightmareBoss)
        {

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

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (canDamage)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                player_test.OnPlayerApplyDamage(damage);
                canDamage = false;
                StartCoroutine(CanDamageCoroutine());
            }
        }
    }

    /// <summary>
    /// 데미지 넣을 수 있는지 결정하는 bool변수
    /// </summary>
    /// <returns></returns>
    IEnumerator CanDamageCoroutine()
    {
        yield return new WaitForSeconds(0.5f);
        canDamage = true;
    }
}
