using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BounceObject : MonoBehaviour
{
    /// <summary>
    /// 공의 속도를 저장하고 제어하는 변수
    /// </summary>
    public Vector2 initialVelocity;// = new Vector2(5f, -5f);

    /// <summary>
    /// 리지드 바디
    /// </summary>
    Rigidbody2D rb;

    /// <summary>
    /// 현재 속도를 관리할 변수
    /// </summary>
    private Vector2 velocity;

    /// <summary>
    /// 이 오브젝트(공)의 콜라이더(벽, 바닥, 천장과 튕김용)
    /// </summary>
    CircleCollider2D circleCollider;

    /// <summary>
    /// 플레이어
    /// </summary>
    Player_Test player_test;

    /// <summary>
    /// 보스 몬스터 베이스
    /// </summary>
    BossMonsterBase bossMonsterBase;

    /// <summary>
    /// FireFloor 프리팹
    /// </summary>
    private GameObject fireFloor;

    /// <summary>
    /// FireFloor 클래스
    /// </summary>
    FireFloor fireFloorClass;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;

        circleCollider = GetComponent<CircleCollider2D>();

        fireFloor = Resources.Load<GameObject>("GameObjects/FireFloor");
    }

    void Start()
    {
        player_test = GameManager.Instance.Player_Test;

        bossMonsterBase = FindAnyObjectByType<BossMonsterBase>();

        // 플레이어와의 충돌 무시
        Physics2D.IgnoreCollision(circleCollider, player_test.GetComponent<Collider2D>(), true);

        /*float xValue1 = 0;
        float xValue2 = 0;

        float yValue1 = 0;
        float yValue2 = 0;*/

        float minSpeedAbs = 0f;     // 속도의 절댓값 최소
        float maxSpeedAbs = 0f;     // 속도의 절댓값 최대

        switch (bossMonsterBase.bossType)
        {
            case BossType.HardBoss:
                // 4 ~ 6    -4 ~ -6
                minSpeedAbs = 4f;
                maxSpeedAbs = 6f;
                break;

            case BossType.NightmareBoss:
                minSpeedAbs = 6f;
                maxSpeedAbs = 8f;
                break;

            case BossType.HellBoss:
                minSpeedAbs = 8f;
                maxSpeedAbs = 10f;
                break;
        }

        float randomXAbs = UnityEngine.Random.Range(minSpeedAbs, maxSpeedAbs);
        // 50% 확률로 방향을 결정
        // (true일 경우 음수, false일 경우 양수)
        if (UnityEngine.Random.value < 0.5f)
        {
            randomXAbs = -randomXAbs;       // 음수 방향으로 반전
        }

        // Y축 속도 생성 (X축과 동일한 방식으로 처리)
        float randomYAbs = UnityEngine.Random.Range(minSpeedAbs, maxSpeedAbs);
        if (UnityEngine.Random.value < 0.5f)
        {
            randomYAbs = -randomYAbs;       // 음수 방향으로 반전
        }

        initialVelocity = new Vector2(randomXAbs, randomYAbs);

        // 게임 시작 시 초기 속도 적용
        velocity = initialVelocity;
    }

    void FixedUpdate()
    {
        rb.velocity = velocity;
    }

    /// <summary>
    /// 땅, 벽, 천장에 부딪히면 튕기는 함수
    /// </summary>
    void OnCollisionEnter2D(Collision2D collision)
    {
        // 태그 당 반전 -> 법선벡터로 변경

        /*// 충돌한 오브젝트의 태그를 가져옴
        string tag = collision.gameObject.tag;

        // 만약 땅, 천장에 부딪히면
        if (tag == "Ground" || tag == "TopWall")
        {
            //Debug.Log("Ground 또는 TopWall과 충돌");
            //Debug.Log($"충돌 전 velocity : {velocity}");

            // y축 방향 반전
            velocity.y = -velocity.y;

            //Debug.Log($"충돌 후 velocity : {velocity}");
        }

        // 만약 벽에 충돌하면
        else if (tag == "Wall")
        {
            //Debug.Log("Wall과 충돌");
            //Debug.Log($"충돌 전 velocity : {velocity}");

            // x축 방향 반전
            velocity.x = -velocity.x;

            //Debug.Log($"충돌 후 velocity : {velocity}");
        }

        rb.velocity = velocity;*/

        string tag = collision.gameObject.tag;
        if (tag == "Ground" || tag == "TopWall" || tag == "Wall")
        {
            // 충돌한 첫 번째 접촉점의 법선 벡터
            ContactPoint2D contact = collision.contacts[0];
            Vector2 normal = contact.normal;

            // 디버깅용
            // Debug.Log($"충돌: {collision.gameObject.tag}, normal: {normal}");

            // 법선 방향에 따라 반사 처리
            if (Mathf.Abs(normal.y) > Mathf.Abs(normal.x))
            {
                // 수직 충돌(바닥 또는 천장)
                velocity.y = -velocity.y;
            }
            else
            {
                // 수평 충돌(벽)
                velocity.x = -velocity.x;
            }

            rb.velocity = velocity;

            // 만약 보스가 Hell 이면
            if (bossMonsterBase.bossType == BossType.HellBoss)
            {
                if (fireFloor != null)
                {
                    Vector3 spawnPos = contact.point;

                    // 법선 방향 기준 회전
                    Quaternion rot = Quaternion.FromToRotation(Vector2.up, normal);

                    // 표면으로부터 약간 씌우기
                    float offset = 0.3f;
                    spawnPos += (Vector3)normal * offset;

                    GameObject obj = Instantiate(fireFloor, spawnPos, rot);

                    // 스케일을 1로
                    obj.transform.localScale = Vector3.one;

                    fireFloorClass = obj.GetComponent<FireFloor>();
                    fireFloorClass.source = FireFloorSource.Ball;
                }
            }
        }
    }
}
