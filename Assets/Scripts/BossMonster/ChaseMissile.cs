using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseMissile : MonoBehaviour
{
    // 보스가 미사일을 생성하고
    // 미사일은 n초 동안 플레이어를 바라보게 회전하다가
    // n초에 찍힌 플레이어의 위치 가장 아래 바닥으로 n+1초에 발사
    // 미사일은 바닥에 꽂친 후 폭발 이펙트 나오고
    // @초 동안 지속되며, 일정 시간 간격으로 데미지를 주는 불 장판을 남긴다

    /// <summary>
    /// 게임 매니저
    /// </summary>
    GameManager gameManager;

    /// <summary>
    /// 플레이어
    /// </summary>
    Player_Test player_test;

    /// <summary>
    /// Player_Test의 Transform 참조
    /// </summary>
    public Transform playerTransform;

    /// <summary>
    /// 회전 속도(도/초 단위)
    /// </summary>
    public float rotateSpeed = 200f;

    /// <summary>
    /// 추적 시간
    /// </summary>
    public float chaseDuration = 5f;

    /// <summary>
    /// chaseDuration 초 째의 플레이어의 위치를 기억
    /// </summary>
    private Vector3 lockedPlayerPosition;

    /// <summary>
    /// 플레이어를 바라보며 추적 중 여부
    /// </summary>
    private bool isChasing = true;

    private void Awake()
    {
        
    }

    private void Start()
    {
        gameManager = GameManager.Instance;
        player_test = gameManager.Player_Test;

        playerTransform = player_test.transform;

        // 추적 코루틴 시작
        StartCoroutine(ChaseForSeconds());
    }

    private void Update()
    {
        // 추적 중 일때만
        if (isChasing && playerTransform != null)
        {
            // 플레이어까지의 방향 벡터
            Vector2 direction = (playerTransform.position - transform.position).normalized;

            // 목표 각도 (위쪽이 앞이므로 -90도 보정)
            float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;

            // 현재 Z축 회전 각도
            float currentAngle = transform.eulerAngles.z;

            // 부드럽게 회전 (Time.deltaTime 사용)
            float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, rotateSpeed * Time.deltaTime);

            // 회전 적용
            transform.rotation = Quaternion.Euler(0f, 0f, newAngle);
        }
        else
        {
            return;
        }
    }

    IEnumerator ChaseForSeconds()
    {
        // 5초 동안 회전 유지
        yield return new WaitForSeconds(chaseDuration);

        // 추적 종료 + 플레이어의 당시 위치 저장
        if (playerTransform != null)
            lockedPlayerPosition = playerTransform.position;

        isChasing = false; // 회전 중단

        Debug.Log($"[ChaseMissile] 추적 종료. 저장된 플레이어 위치: {lockedPlayerPosition}");

        // 저장된 위치를 기준으로 (x위치 동일, y위치는 -8.7 정도에)
        // 1초 후 미사일 발사하여 폭발 및 장판을 남김
    }
}
