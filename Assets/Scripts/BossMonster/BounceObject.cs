using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BounceObject : MonoBehaviour
{
    /// <summary>
    /// 공의 속도를 저장하고 제어하는 변수
    /// </summary>
    public Vector2 initialVelocity = new Vector2(5f, -5f);

    /// <summary>
    /// 리지드 바디
    /// </summary>
    Rigidbody2D rb;

    /// <summary>
    /// 현재 속도를 관리할 변수
    /// </summary>
    private Vector2 velocity;

    /// <summary>
    /// 이 오브젝트(공)의 콜라이더
    /// </summary>
    CircleCollider2D circleCollider;

    /// <summary>
    /// 코루틴 중복 방지 핸들
    /// </summary>
    private Coroutine ignoreCoroutine;

    /// <summary>
    /// 플레이어
    /// </summary>
    Player_Test player_test;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;

        circleCollider = GetComponent<CircleCollider2D>(); // 자신의 콜라이더 정보 가져오기

        // 게임 시작 시 초기 속도 적용
        velocity = initialVelocity;
    }

    void Start()
    {
        player_test = GameManager.Instance.Player_Test;
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
            Debug.Log("Ground 또는 TopWall과 충돌");
            Debug.Log($"충돌 전 velocity : {velocity}");

            // y축 방향 반전
            velocity.y = -velocity.y;

            Debug.Log($"충돌 후 velocity : {velocity}");
        }

        // 만약 벽에 충돌하면
        else if (tag == "Wall")
        {
            Debug.Log("Wall과 충돌");
            Debug.Log($"충돌 전 velocity : {velocity}");

            // x축 방향 반전
            velocity.x = -velocity.x;

            Debug.Log($"충돌 후 velocity : {velocity}");
        }

        // 만약 플레이어와 충돌하면
        else if (tag == "Player")
        {
            Debug.Log("OnColliderEnter2D에서 공과 플레이어의 충돌은 확인");

            // 데미지 적용 부분
            player_test.OnPlayerApplyDamage(5);

            // 코루틴 진행 확인 후 무시 코루틴 적용
            if (ignoreCoroutine != null)
            {
                StopCoroutine(ignoreCoroutine);
            }
            ignoreCoroutine = StartCoroutine(IgnoreCoroutine(collision));
        }

        rb.velocity = velocity;
    }

    /// <summary>
    /// 플레이어와의 충돌을 무시하는 코루틴
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    IEnumerator IgnoreCoroutine(Collision2D other)
    {
        // 플레이어와 충돌 무시 시작
        Debug.Log("Player와 충돌! (충돌 무시 시작)");
        Physics2D.IgnoreCollision(circleCollider, other.collider, true);

        yield return new WaitForSeconds(1.5f);

        // 플레이어와 충돌 무시 끝
        Debug.Log("Player와 충돌! (충돌 무시 끝)");
        Physics2D.IgnoreCollision(circleCollider, other.collider, false);
    }
}
