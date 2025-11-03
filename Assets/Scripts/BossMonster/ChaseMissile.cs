using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;

public class ChaseMissile : MonoBehaviour
{
    // 보스가 미사일을 생성하고
    // 미사일은 n초 동안 플레이어를 바라보게 회전하다가  v
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
    /// 보스 몬스터 베이스
    /// </summary>
    BossMonsterBase bossMonsterBase;

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
    /// 미사일의 이동속도
    /// </summary>
    public float moveSpeed = 20f;

    /// <summary>
    /// chaseDuration 초 째의 플레이어의 위치를 기억
    /// </summary>
    private Vector3 lockedPlayerPosition;

    /// <summary>
    /// 플레이어를 바라보며 추적 중 여부
    /// </summary>
    private bool isChasing = true;

    /// <summary>
    /// BigExplosion 프리팹 원본
    /// </summary>
    private GameObject bigExplosionObject;

    /// <summary>
    /// BigExplosion 오브젝트 생성
    /// </summary>
    private GameObject bigExplosionInstance;

    private Rigidbody2D rb;

    private void Awake()
    {
        bigExplosionObject = Resources.Load<GameObject>("GameObjects/BigExplosion");
        rb = GetComponent<Rigidbody2D>();

        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.isKinematic = false;

    }

    private void Start()
    {
        gameManager = GameManager.Instance;
        player_test = gameManager.Player_Test;

        playerTransform = player_test.transform;

        bossMonsterBase = FindAnyObjectByType<BossMonsterBase>();

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

    /// <summary>
    /// 플레이어의 위치를 기억하고 회전 유지하는 코루틴
    /// </summary>
    /// <returns></returns>
    IEnumerator ChaseForSeconds()
    {
        // 5초 동안 회전 유지
        yield return new WaitForSeconds(chaseDuration);

        // 추적 종료 + 플레이어의 당시 위치 저장
        if (playerTransform != null)
        {
            lockedPlayerPosition = playerTransform.position;
        }

        isChasing = false;      // 플레이어 바라보게 회전 중단

        Debug.Log($"[ChaseMissile] 추적 종료. 저장된 플레이어 위치: {lockedPlayerPosition}");

        // 저장된 위치를 기준으로 (x위치 동일, y위치는 -8.7 정도에)
        // 1초 후 미사일 발사하여 폭발 및 장판을 남김
        // 미사일이 재생성되는 시간은 나이트메이 기준?
        // 장판 지속시간이 6초이면 바로 생기고
        // 5초이면 1초 여유가 있네

        if (bossMonsterBase.bossType == BossType.NightmareBoss)
        {
            float rotateDuration = 1f;
            float elapsed = 0f;

            // 🔸 플레이어가 있었던 X 위치, 바닥(Y=-8.7)
            Vector3 targetPoint = new Vector3(lockedPlayerPosition.x, -8.7f, transform.position.z);

            // 현재 각도
            float startAngle = transform.eulerAngles.z;

            // 목표 각도 계산
            Vector2 direction = (targetPoint - transform.position).normalized;
            float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;

            // 1초 동안 부드럽게 회전
            while (elapsed < rotateDuration)
            {
                elapsed += Time.deltaTime;

                float newAngle = Mathf.LerpAngle(startAngle, targetAngle, elapsed / rotateDuration);
                transform.rotation = Quaternion.Euler(0f, 0f, newAngle);

                yield return null;
            }

            // 정확히 목표 각도로 고정
            transform.rotation = Quaternion.Euler(0f, 0f, targetAngle);

            Debug.Log($"[ChaseMissile] 나이트메어 회전 완료 → lockedPlayerPosition.x={lockedPlayerPosition.x}, y=-8.7 방향을 바라봄");
        }
        // 여기서는 Hell 보스만 해당됨
        // Hell 보스는 회전 1초가 생략되기 때문에 추가
        else
        {
            // 1초 기다렸다가 발사 코루틴 실행
            yield return new WaitForSeconds(1f);
        }

        // 발사 코루틴 시작
        StartCoroutine(FireMissile());
    }

    /// <summary>
    /// 미사일 발사 코루틴
    /// </summary>
    /// <returns></returns>
    IEnumerator FireMissile()
    {
        /*float groundY = -8.7f;                  // Y 기본값
        Vector3 targetPos = Vector3.zero;       // 최종 낙하 위치

        Vector3 hitNormal = Vector3.up;  // 기본값

        RaycastHit2D hitInfo = new RaycastHit2D();

        // 나이트메어 보스의 경우 바닥에서만 장판 생성 가능
        if (bossMonsterBase.bossType == BossType.NightmareBoss)
        {
            Vector3 firePos = new Vector3(lockedPlayerPosition.x, 100f, lockedPlayerPosition.z);
            RaycastHit2D[] hits = Physics2D.RaycastAll(firePos, Vector2.down, Mathf.Infinity);

            foreach (var hit in hits)
            {
                if (hit.collider != null && hit.collider.CompareTag("Ground"))
                {
                    groundY = hit.point.y;
                    targetPos = hit.point;
                    hitNormal = hit.normal;   // ✅ 표면 법선 저장
                    hitInfo = hit;
                    break;
                }
            }

            if (targetPos == Vector3.zero)
                targetPos = new Vector3(lockedPlayerPosition.x, groundY, lockedPlayerPosition.z);
        }
        else // Hell 방식
        {
            Vector2 fireDir = transform.up;
            Vector3 startPos = transform.position;

            RaycastHit2D[] hits = Physics2D.RaycastAll(startPos, fireDir, Mathf.Infinity);
            foreach (var hit in hits)
            {
                if (hit.collider != null && (hit.collider.CompareTag("Ground") || hit.collider.CompareTag("Wall") || hit.collider.CompareTag("TopWall")))
                {
                    targetPos = hit.point;
                    hitNormal = hit.normal;   // ✅ 표면 법선 저장
                    hitInfo = hit;
                    break;
                }
            }

            if (targetPos == Vector3.zero)
                targetPos = new Vector3(lockedPlayerPosition.x, groundY, lockedPlayerPosition.z);
        }

        // 낙하 애니메이션
        float speed = 20f;
        while (Vector3.Distance(transform.position, targetPos) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
            yield return null;
        }

        //Debug.Log("[ChaseMissile] 착지! 폭발 발생!");
        MissileExplosion(hitNormal);*/

        Vector3 targetPos = Vector3.zero;
        float groundY = -8.7f;

        if (bossMonsterBase.bossType == BossType.NightmareBoss)
        {
            Vector3 firePos = new Vector3(lockedPlayerPosition.x, 100f, lockedPlayerPosition.z);
            RaycastHit2D[] hits = Physics2D.RaycastAll(firePos, Vector2.down, Mathf.Infinity);

            foreach (var hit in hits)
            {
                if (hit.collider != null && hit.collider.CompareTag("Ground"))
                {
                    targetPos = hit.point;
                    groundY = hit.point.y;
                    break;
                }
            }

            if (targetPos == Vector3.zero)
                targetPos = new Vector3(lockedPlayerPosition.x, groundY, lockedPlayerPosition.z);
        }
        else
        {
            Vector2 fireDir = transform.up;
            RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, fireDir, Mathf.Infinity);

            foreach (var hit in hits)
            {
                if (hit.collider != null &&
                    (hit.collider.CompareTag("Ground") || hit.collider.CompareTag("Wall") || hit.collider.CompareTag("TopWall")))
                {
                    targetPos = hit.point;
                    break;
                }
            }

            if (targetPos == Vector3.zero)
                targetPos = new Vector3(lockedPlayerPosition.x, groundY, lockedPlayerPosition.z);
        }

        // 🔸 변경: 직접 MoveTowards 대신 Rigidbody2D 이동으로 전환
        Vector2 dir = (targetPos - transform.position).normalized;
        rb.velocity = dir * moveSpeed;

        yield break; // 🔹 추가: 이동은 물리로 진행, 이후 Trigger에서 폭발 처리
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        string tag = other.tag;

        if (tag == "Ground" || tag == "Wall" || tag == "TopWall")
        {
            // 🔹 역방향 Raycast로 충돌 표면 법선 계산
            Vector2 backDir = -rb.velocity.normalized;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, backDir, 1f);
            Vector3 normal = hit ? (Vector3)hit.normal : Vector3.up;

            if (hit)
                transform.position = hit.point;

            // 🔹 이동 멈추고 충돌체 비활성화
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;
            GetComponent<Collider2D>().enabled = false;

            MissileExplosion(normal); // 폭발 처리
        }
    }

    /// <summary>
    /// 로켓이 폭발하는 연출 함수
    /// </summary>
    void MissileExplosion(Vector3 surfaceNormal)
    {
        /*// 이 오브젝트의 알파값 0으로 변경

        // 자식으로 폭발 생성
        bigExplosionInstance = Instantiate(bigExplosionObject, transform);
        bigExplosionInstance.transform.localPosition = Vector3.zero;

        // ✅ 법선 방향에 맞춰 회전
        Quaternion normalRotation = Quaternion.FromToRotation(Vector3.up, surfaceNormal);
        bigExplosionInstance.transform.rotation = normalRotation;*/

        // 🔸 부모 대신 독립적인 폭발 오브젝트로 생성
        GameObject explosion = Instantiate(bigExplosionObject, transform.position, Quaternion.identity);

        Quaternion normalRot = Quaternion.FromToRotation(Vector3.up, surfaceNormal);
        explosion.transform.rotation = normalRot;

        // 🔹 폭발 후 미사일 제거
        Destroy(gameObject);
    }
}
