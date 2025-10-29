using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallDamage : MonoBehaviour
{
    /// <summary>
    /// 플레이어
    /// </summary>
    Player_Test player_test;

    /// <summary>
    /// 쿨다운 변수: 데미지 중복 적용 방지
    /// </summary>
    private bool canDamage = true;

    /// <summary>
    /// 1.5 초 쿨다운
    /// </summary>
    private float damageCooldownTime = 1.5f;

    private void Start()
    {
        player_test = GameManager.Instance.Player_Test;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 충돌 대상이 플레이어이고, 데미지 줄 수 있으면
        if (collision.gameObject.CompareTag("Player") && canDamage)
        {
            Debug.Log("공의 자식과 플레이어의 충돌 감지");

            // 1. 데미지 적용
            player_test.OnPlayerApplyDamage(5);

            // 2. 쿨다운 시작
            canDamage = false;
            StartCoroutine(ResetDamageCooldown());
        }
    }

    /// <summary>
    /// 중복 데미지 쿨다운 코루틴
    /// </summary>
    /// <returns></returns>
    IEnumerator ResetDamageCooldown()
    {
        yield return new WaitForSeconds(damageCooldownTime);
        canDamage = true;
        //Debug.Log("플레이어 데미지 쿨다운 종료.");
    }
}
