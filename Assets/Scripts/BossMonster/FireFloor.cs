using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum FireFloorSource
{
    Missile,   // 미사일에서 생성됨 → 기존 로직 유지
    Ball       // 공(BounceObject)에서 생성됨 → 커스텀 적용
}

public class FireFloor : MonoBehaviour
{
    // 장판 지속시간은 약 5초 정도
    // 만약 플레이어가 충돌하면 지속적으로 데미지를 준다

    // 미사일 회전 5초, 발사 전까지 1초에
    // 나이트메어는 보스가 장판 사라지고 미사일 생성하고
    // 헬은 장판 생성과 동시에 미사일 생성하면 되겠네.

    /// <summary>
    /// 누가 불장판을 생성했는지(기본값은 미사일)
    /// </summary>
    public FireFloorSource source = FireFloorSource.Missile;

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
    float damage = 0f;

    /// <summary>
    /// 실행 중인 불 장판 데미지 코루틴의 참조
    /// </summary>
    private Coroutine fireDamageCoroutine;

    private void Awake()
    {
        
    }

    private void Start()
    {
        player_test = GameManager.Instance.Player_Test;

        bossMonsterBase = FindAnyObjectByType<BossMonsterBase>();

        // 미사일로 생성된 경우
        if (source == FireFloorSource.Missile)
        {
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
        }
        // 볼(BounceObject)로 생성된 경우
        else if (source == FireFloorSource.Ball)
        {
            damage = 10f;
            duration = 1.5f;
        }


        StartCoroutine(LifeDuration());
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

    // Stay에서 계속 실행되어야 하지만 122 번 정도밖에 디버그가 나오지 않는 것으로 봐서 중간에 멈추는 듯 함
    // 그래서 Enter와 Exit로 나눠서 기능하도록 수정
    /*private void OnTriggerStay2D(Collider2D collision)
    {
        *//*if (canDamage)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                player_test.OnPlayerApplyFireFloorDamage(damage);
                canDamage = false;
                StartCoroutine(CanDamageCoroutine());
            }
        }*//*

        if (collision.gameObject.CompareTag("Player"))
        {
            //Debug.Log("불 장판과 플레이어의 충돌 확인");
            player_test.OnPlayerApplyFireFloorDamage(damage);
        }
    }*/

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // 이미 코루틴이 실행 중이 아니라면 시작
            if (fireDamageCoroutine == null)
            {
                fireDamageCoroutine = StartCoroutine(FireDamageCoroutine());
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (fireDamageCoroutine != null)
            {
                // 저장된 참조를 사용하여 실행 중인 코루틴을 멈춤
                StopCoroutine(fireDamageCoroutine);
                fireDamageCoroutine = null;
            }
        }
    }

    /// <summary>
    /// 1초 간격으로 불 장판 데미지 적용 시도하는 코루틴
    /// </summary>
    /// <returns></returns>
    IEnumerator FireDamageCoroutine()
    {
        while(player_test.HP > 1)
        {
            //Debug.Log("FireDamageCoroutine 실행");
            player_test.OnPlayerApplyFireFloorDamage(damage);
            yield return new WaitForSeconds(1f);
        }
    }
}
