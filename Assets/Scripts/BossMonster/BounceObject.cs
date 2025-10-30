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

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;

        circleCollider = GetComponent<CircleCollider2D>();
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
        // 충돌한 오브젝트의 태그를 가져옴
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

        rb.velocity = velocity;
    }
}
