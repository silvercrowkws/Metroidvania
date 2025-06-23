using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class Player_Test : MonoBehaviour
{
    /// <summary>
    /// 애니메이터
    /// </summary>
    Animator animator;

    /// <summary>
    /// 플레이어 인풋 액션
    /// </summary>
    PlayerInputActions inputActions;

    /// <summary>
    /// 플레이어의 리지드 바디
    /// </summary>
    Rigidbody2D rb;

    /// <summary>
    /// 박스 콜라이더
    /// </summary>
    BoxCollider2D box;

    /// <summary>
    /// 현재 입력값을 저장하기 위한 변수
    /// </summary>
    Vector2 moveInput;

    /// <summary>
    /// 기본 이동 속도
    /// </summary>
    float defaultMoveSpeed = 5f;

    /// <summary>
    /// 이동 속도
    /// </summary>
    public float moveSpeed = 5f;

    /// <summary>
    /// 점프 파워
    /// </summary>
    public float jumpPower = 7.5f;

    /// <summary>
    /// 대쉬 파워
    /// </summary>
    public float dashPower = 15f;

    /// <summary>
    /// 캐릭터가 땅에 있는지 확인하기 위한 bool 변수
    /// </summary>
    public bool isGround = false;

    /// <summary>
    /// 점프 가능 횟수
    /// </summary>
    private int jumpCount = 0;

    /// <summary>
    /// 최대 점프 가능 횟수
    /// </summary>
    private int maxJumpCount = 2;

    /// <summary>
    /// 벽 점프를 했는지 확인하는 변수(false : 점프 안함)
    /// </summary>
    public bool wallJumped = false;

    /// <summary>
    /// 캐릭터가 대쉬 중인지 확인하기 위한 bool 변수
    /// </summary>
    bool isDash = false;
    float dashTime = 0.2f;
    float dashTimer = 0f;

    /// <summary>
    /// 플레이어가 모든 열쇠를 가지고 있는지 확인하는 bool 변수
    /// </summary>
    public bool hasAllKeys;

    /// <summary>
    /// 플레이어가 가진 열쇠 개수
    /// </summary>
    private int keyCount = 0;

    /// <summary>
    /// 플레이어가 가진 키의 개수가 변경되었음을 알리는 델리게이트
    /// </summary>
    public Action<int> onKeyCountChanged;

    /// <summary>
    /// 문과 상호작용할 수 있는지 확인하는 bool 변수
    /// </summary>
    private bool canEnterDoor = false;

    /// <summary>
    /// 문의 중앙 위치
    /// </summary>
    private Transform doorCenter;

    /// <summary>
    /// 문까지 걸어가는 bool 변수
    /// </summary>
    private bool isWalkingToDoor = false;

    /// <summary>
    /// 타겟 문의 위치
    /// </summary>
    private Vector3 targetDoorPosition;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // Resources.Load는 리소스를 로드하는 메서드
        // <Sprite>는 로드할 에셋의 타입을 지정(Texture, AudioClip, GameObject 등 다른 타입도 있음)
        // "Sprites/sprite1" 이 부분은 로딩할 리소스의 경로
        // "Sprites"는 Resources 폴더 내에 있는 서브폴더
        // "sprite1"은 해당 폴더 내에 있는 에셋의 이름
        //jumpDown = Resources.Load<Sprite>("Sprites/JumpDownCopy");
    }

    private void OnEnable()
    {
        inputActions.Actions.Enable();
        inputActions.Actions.Move.performed += OnMove;
        inputActions.Actions.Move.canceled += OnMove;
        inputActions.Actions.Jump.performed += OnJump;
        inputActions.Actions.Dash.performed += OnDash;
        inputActions.Actions.DoorInteract.performed += OnDoorInteract;
    }

    private void OnDisable()
    {
        inputActions.Actions.DoorInteract.performed -= OnDoorInteract;
        inputActions.Actions.Dash.performed -= OnDash;
        inputActions.Actions.Jump.performed -= OnJump;
        inputActions.Actions.Move.canceled -= OnMove;
        inputActions.Actions.Move.performed -= OnMove;
        inputActions.Actions.Disable();
    }

    // 땅 판정용 변수
    public LayerMask groundLayer; // 인스펙터에서 Ground, Wall 등 지정
    public float groundCheckDistance = 0.1f; // 발 아래로 쏘는 거리
    public Vector2 groundCheckOffset = new Vector2(0, -0.5f); // 발 위치 오프셋(콜라이더 중심 기준)

    private void FixedUpdate()
    {
        if (isDash)
        {
            dashTimer -= Time.fixedDeltaTime;
            if (dashTimer <= 0)
            {
                isDash = false;
            }
            return;
        }

        // 방햫에 따라 플레이어 회전
        if (moveInput.x > 0.1f)
        {
            transform.localScale = new Vector3(1, 1, 1);    // 오른쪽
        }
        else if (moveInput.x < -0.1f)
        {
            transform.localScale = new Vector3(-1, 1, 1);   // 왼쪽
        }

        // 이동 처리
        if (!isDash)
        {
            rb.velocity = new Vector2(moveInput.x * moveSpeed, rb.velocity.y);
        }

        // 낙하 상태 판정
        if (rb.velocity.y < -0.01f)
        {
            animator.SetBool("IsFall", true);
        }
        else
        {
            animator.SetBool("IsFall", false);
        }

        // === Raycast로 땅 판정 ===
        Vector2 origin = (Vector2)transform.position + groundCheckOffset;
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, groundCheckDistance, groundLayer);
        isGround = hit.collider != null;
    }

    private void Update()
    {

    }

    /// <summary>
    /// 인풋 시스템으로 플레이어의 움직임을 제어하는 함수
    /// </summary>
    /// <param name="context"></param>
    private void OnMove(InputAction.CallbackContext context)
    {
        // 지금은 AD 로만 작동하는데 밧줄 같은거 넣을거면 WS 도 넣어야 할듯?
        moveInput = context.ReadValue<Vector2>();
        animator.speed = 1f;                        // 애니메이션 재생

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        // 애니메이션 처리
        if (moveInput.magnitude > 0.1f)
        {
            // 움직임이 있고
            //땅에 있고 현재 애니메이션이 점프가 아니면
            if (isGround && !stateInfo.IsName("Jump"))
            {
                ResetTrigger();
                animator.SetTrigger("Run");
            }
        }
        else
        {
            // 움직임이 없고
            // 땅에 있고 현재 애니메이션이 점프가 아니면
            if (isGround && !stateInfo.IsName("Jump"))
            {
                ResetTrigger();
                animator.SetTrigger("Idle");
            }
        }
    }

    /// <summary>
    /// 인풋 시스템으로 플레이어의 점프를 제어하는 함수
    /// </summary>
    /// <param name="context"></param>
    private void OnJump(InputAction.CallbackContext context)
    {
        //Debug.Log("점프");

        // 만약 캐릭터가 땅에 있으면
        // 만약 캐릭터가 점프가 가능하면
        if (jumpCount > 0)
        {
            //jumpAble = false;                                           // 점프 했다고 표시
            jumpCount--;
            ResetTrigger();                                             // 트리거 리셋
            animator.SetTrigger("Jump");                                // 트리거 전환
            rb.velocity = new Vector2(rb.velocity.x, 0f);               // 기존 y 속도 제거
            rb.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);   // 위로 힘 추가

            // 점프를 했으니까 땅이 아닐 것임

            // 만약 벽점프를 했으면 벽점프를 했다고 표시
            if (!isGround)      // 땅이 아닌 곳에서 점프는 벽점프
            {
                wallJumped = true;
            }
            isGround = false;
        }
    }

    /// <summary>
    /// 인풋 시스템으로 플레이어의 대쉬를 제어하는 함수
    /// </summary>
    /// <param name="context"></param>
    private void OnDash(InputAction.CallbackContext context)
    {
        // 만약 플레이어의 대쉬 게이지가 있으면
        //if ()
        {
            isDash = true;
            ResetTrigger();
            animator.SetTrigger("Dash");
            rb.gravityScale = 0;        // 직선적으로 움직이기 위해 중력 끄기

            // 현재 이동 방향을 반영하여 대쉬 방향 결정
            float dashDirection = 0;
            if (transform.localScale.x > 0)
            {
                dashDirection = 1f;
            }
            else
            {
                dashDirection = -1;
            }

            rb.velocity = Vector2.zero;     // 기존 속도 초기화
            rb.AddForce(Vector2.right * dashPower * dashDirection, ForceMode2D.Impulse);
            isDash = true;
            rb.gravityScale = 1;        // 중력 원래대로
            dashTimer = dashTime;
        }
    }

    /*private void OnCollisionEnter2D(Collision2D collision)
    {
        *//*if (!animator.enabled)
        {
            animator.enabled = true;
        }*//*

        // 플레이어가 땅이 아닐때
        if (!isGround)
        {
            // 만약 땅에 충돌한다면
            if (collision.gameObject.CompareTag("Ground"))
            {
                Debug.Log("땅에 충돌!");

                jumpCount = maxJumpCount;       // 2단 점프 가능
                isGround = true;
                //jumpAble = true;        // 땅에 충돌했으니 점프 가능

                // 착지 시 낙하 상태 해제
                animator.SetBool("IsFall", false);

                // 움직임이 있으면?
                if (moveInput.magnitude != 0)
                {
                    Debug.Log("움직임 있음");
                    ResetTrigger();
                    //animator.SetTrigger("Idle");
                    animator.SetTrigger("Run");
                }
                else
                {
                    ResetTrigger();
                    animator.SetTrigger("Idle");
                }
                //isGround = true;
            }
            // 만약 벽에 충돌한다면
            else if (collision.gameObject.CompareTag("Wall"))
            {
                Vector2 normal = collision.contacts[0].normal;
                if (Mathf.Abs(normal.y) < 0.1f && Mathf.Abs(normal.x) > 0.7f)
                //if (collision.contacts[0].normal.y <= 0)    // 벽의 옆면에서 충돌을 감지했을 경우
                {
                    Debug.Log("벽에는 정상 충돌함");
                    // 벽의 옆면
                    ResetTrigger();
                    animator.SetTrigger("Hang");
                    jumpCount = 1;          // 벽점프는 1번만 가능
                    //jumpAble = true;        // 점프 가능
                    wallJumped = false;     // 벽 점프 아직 안했다고 표시
                }
                //else                                        // 벽의 위쪽에 충돌해서 땅 판정
                else if (normal.y > 0.7f)
                {
                    // 벽 위쪽(땅 취급)
                    jumpCount = maxJumpCount;       // 2단 점프 가능
                    isGround = true;

                    // 움직임이 있으면?
                    if (moveInput.magnitude != 0)
                    {
                        Debug.Log("움직임 있음");
                        ResetTrigger();
                        //animator.SetTrigger("Idle");
                        animator.SetTrigger("Run");
                    }
                    else
                    {
                        ResetTrigger();
                        animator.SetTrigger("Idle");
                    }
                    //isGround = true;
                    //jumpAble = true;
                }
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        // 벽에서 떨어지면
        if (collision.gameObject.CompareTag("Wall"))
        {
            // 벽에서 떨어지면(벽의 위쪽이든 옆쪽이든) 무조건 땅은 아님
            isGround = false;

            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            // 만약 현재 애니메이터가 행이고?
            if (stateInfo.IsName("Hang"))
            {
                Debug.Log("벽에서 떨어짐!");

                // 벽에서 떨어졌을 때 점프 가능 + 벽점프 안함 이면
                *//*if (jumpAble && !wallJumped)
                {
                    Debug.Log("점프 가능 + 벽점프도 안함!");
                    ResetTrigger();
                    animator.SetTrigger("Idle");
                }*//*
            }
        }
        // 땅에서 떨어지면
        else if (collision.gameObject.CompareTag("Ground"))
        {
            isGround = false; // 땅에서 떨어짐 표시

            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

            // 점프가 가능하면 점프를 안하고 떨어졌다는 소리
            if (jumpCount > 0)
            {
                // 만약 현재 애니메이터가 Idle or Run 이면
                if (stateInfo.IsName("Idle") || stateInfo.IsName("Run"))
                {
                    //ResetTrigger();
                    //animator.enabled = false; // 애니메이터 중단
                    //spriteRenderer.sprite = jumpDown;
                    //Debug.Log("이미지 변경 완료");
                }
            }
        }
    }*/

    private int groundContactCount = 0;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            //groundContactCount++;
            //isGround = true;
            animator.SetBool("IsFall", false);
            jumpCount = maxJumpCount;
        }
        else if (collision.gameObject.CompareTag("Wall"))
        {
            //bool isWallTop = false;
            bool isWallSide = false;

            foreach (var contact in collision.contacts)
            {
                if (contact.normal.y > 0.7f)
                {
                    // Wall의 윗면: 땅 판정
                    //groundContactCount++;
                    //isGround = true;
                    animator.SetBool("IsFall", false);
                    jumpCount = maxJumpCount;
                    //isWallTop = true;
                    break;
                }
                else if (Mathf.Abs(contact.normal.x) > 0.7f && Mathf.Abs(contact.normal.y) < 0.1f)
                {
                    // Wall의 옆면: 메달림 판정
                    isWallSide = true;
                }
            }

            // 땅이 아니고 벽의 옆면에 닿았을 때 Hang 트리거
            if (!isGround && isWallSide)
            {
                ResetTrigger();
                animator.SetTrigger("Hang");
                jumpCount = 1; // 벽점프 1회 가능 등 추가 로직
                wallJumped = false;
            }
        }
    }

    /// <summary>
    /// 충돌 중일때
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionStay2D(Collision2D collision)
    {
        // 땅 판정은 Raycast에서만 처리!
        // 여기서는 점프 카운트, 애니메이션 등만 처리

        // 플레이어가 땅에 충돌 중일때
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGround = true;

            // 착지 시 낙하 상태 해제
            animator.SetBool("IsFall", false);

            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

            // 점프 중이 아닐 때만 Idle/Run 트리거를 준다
            if (!stateInfo.IsName("Jump") && !isDash)
            {
                // 움직임이 있으면
                if (moveInput.magnitude != 0)
                {
                    ResetTrigger();
                    animator.SetTrigger("Run");
                }
                else
                {
                    ResetTrigger();
                    animator.SetTrigger("Idle");
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Key"))
        {
            keyCount++;
            if (keyCount >= 3)
            {
                hasAllKeys = true;
            }
            onKeyCountChanged?.Invoke(keyCount);
        }

        // 문과 충돌했을 때
        if (collision.CompareTag("Door"))
        {
            // 열쇠가 전부 있을때
            if (hasAllKeys)
            {
                // W or 위키를 누르면?
                // 플레이어 입력 막고 문 중앙까지 걸어가고
                // BackWalk 애니메이터 실행
                // n초 후 씬 전환
                canEnterDoor = true;
                doorCenter = collision.transform;       // 문의 중앙은 충돌한 문의 위치
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Door"))
        {
            canEnterDoor = false;
            doorCenter = null;
        }
    }

    /// <summary>
    /// 문과 상호작용하는 함수(플레이어 상호작용 입력 정지 포함)
    /// </summary>
    /// <param name="context"></param>
    private void OnDoorInteract(InputAction.CallbackContext context)
    {
        if (canEnterDoor && !isWalkingToDoor)
        {
            // 입력 막기
            inputActions.Actions.Disable();
            isWalkingToDoor = true;
            targetDoorPosition = new Vector3(doorCenter.position.x, transform.position.y, transform.position.z);
            StartCoroutine(WalkToDoorAndEnter());
        }
    }

    /// <summary>
    /// 문 중앙까지 걸어가는 코루틴
    /// </summary>
    /// <returns></returns>
    private IEnumerator WalkToDoorAndEnter()
    {
        float walkSpeed = 3f;
        while (Vector3.Distance(transform.position, targetDoorPosition) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetDoorPosition, walkSpeed * Time.deltaTime);
            yield return null;
        }

        ResetTrigger();
        animator.SetTrigger("BackWalk");

        yield return new WaitForSeconds(1.5f);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);       // 현재 실행 중인 씬 +1 로 이동
    }


    /// <summary>
    /// 모든 트리거를 리셋하는 함수
    /// </summary>
    private void ResetTrigger()
    {
        animator.ResetTrigger("Idle");
        animator.ResetTrigger("Jump");
        animator.ResetTrigger("Run");
        animator.ResetTrigger("Dash");
        animator.ResetTrigger("Hang");
        animator.ResetTrigger("BackWalk");
    }

    /// <summary>
    /// 행동 전의 트리거를 기억해 놓는 함수
    /// </summary>
    private void RememberTrigger()
    {

    }

}
