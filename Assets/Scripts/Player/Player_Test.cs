using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class Player_Test : MonoBehaviour
{
    // 플레이어 조작 관련 --------------------------------------------------

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
    /// 캐릭터가 대쉬 중인지 확인하기 위한 bool 변수
    /// </summary>
    bool isDash = false;
    float dashTime = 0.2f;
    float dashTimer = 0f;

    private bool justHanged = false;
    private bool justJumped = false;

    // 플레이어 조작 관련 끝 --------------------------------------------------

    // 문 및 열쇠 관련 --------------------------------------------------

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

    // 문 및 열쇠 관련 끝 --------------------------------------------------

    private void Awake()
    {
        inputActions = new PlayerInputActions();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        inputActions.Actions.Enable();
        inputActions.Actions.Move.performed += OnMove;
        inputActions.Actions.Move.canceled += OnMove;
        inputActions.Actions.Jump.performed += OnJump;
        /*inputActions.Actions.Dash.performed += OnDash;
        inputActions.Actions.Crawl.performed += OnCrawlStart;
        inputActions.Actions.Crawl.canceled += OnCrawlEnd;
        inputActions.Actions.DoorInteract.performed += OnDoorInteract;*/
    }

    private void OnDisable()
    {
        /*inputActions.Actions.DoorInteract.performed -= OnDoorInteract;
        inputActions.Actions.Crawl.canceled -= OnCrawlEnd;
        inputActions.Actions.Crawl.performed -= OnCrawlStart;
        inputActions.Actions.Dash.performed -= OnDash;*/
        inputActions.Actions.Jump.performed -= OnJump;
        inputActions.Actions.Move.canceled -= OnMove;
        inputActions.Actions.Move.performed -= OnMove;
        inputActions.Actions.Disable();
    }

    public bool isFall = false;

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

        /*AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName("Hang"))
        {
            if (!justHanged && rb.velocity.y < -0.01f)
            {
                animator.SetBool("IsFall", true);
            }
            else
            {
                animator.SetBool("IsFall", false);
            }
        }

        // 낙하 상태 판정
        if (rb.velocity.y < -0.01f)
        {
            isFall = true;
            animator.SetBool("IsFall", true);
        }
        else
        {
            isFall = false;
            animator.SetBool("IsFall", false);
        }*/

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        // Jump 상태일 때만 y속도가 음수로 바뀌었을 때 Fall로 전환
        if (stateInfo.IsName("Jump"))
        {
            if (!justJumped && rb.velocity.y < -0.01f)
            {
                animator.SetBool("IsFall", true);
            }
            else
            {
                animator.SetBool("IsFall", false);
            }
        }
        // Hang 상태일 때도 별도 처리
        else if (stateInfo.IsName("Hang"))
        {
            if (!justHanged && rb.velocity.y < -0.01f)
            {
                animator.SetBool("IsFall", true);
            }
            else
            {
                animator.SetBool("IsFall", false);
            }
        }
        // 그 외 상태에서는 기존대로 처리
        else
        {
            if (rb.velocity.y < -0.01f)
            {
                isFall = true;
                animator.SetBool("IsFall", true);
            }
            else
            {
                isFall = false;
                animator.SetBool("IsFall", false);
            }
        }
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

        // 애니메이션 처리
        if (moveInput.magnitude > 0.1f)
        {
            if (isGround)               // 땅에 있고
            {
                ResetTrigger();
                animator.SetTrigger("Run");
            }
        }
        else
        {
            if (isGround)
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
        
        // 만약 점프가 가능하면
        if (jumpCount > 0)
        {
            jumpCount--;                                                // 점프 가능 횟수 --
            ResetTrigger();                                             // 트리거 리셋
            animator.SetTrigger("Jump");                                // 트리거 전환
            rb.velocity = new Vector2(rb.velocity.x, 0f);               // 기존 y 속도 제거
            rb.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);   // 위로 힘 추가

            // 점프를 했으니까 땅이 아닐 것임
            isGround = false;

            justJumped = true;
            StartCoroutine(JumpDelay());
        }
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 땅과 충돌하면
        if (collision.gameObject.CompareTag("Ground"))
        {
            Debug.Log("땅에 충돌!");

            // 점프 중이 아닐 때만 Idle/Run 트리거 실행
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (!stateInfo.IsName("Jump"))
            {
                if (moveInput.magnitude != 0)
                {
                    Debug.Log("땅인데 움직임 있음");
                    ResetTrigger();
                    animator.SetTrigger("Run");
                }
                else
                {
                    Debug.Log("땅인데 움직임 없음");
                    ResetTrigger();
                    animator.SetTrigger("Idle");
                }
            }
            jumpCount = maxJumpCount;       // 2단 점프 가능
            isGround = true;
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
                justHanged = true;
                StartCoroutine(HangDelay());
            }
            //else                                        // 벽의 위쪽에 충돌해서 땅 판정
            else if (normal.y > 0.7f)
            {
                // 벽 위쪽(땅 취급)

                // 점프 중이 아닐 때만 Idle/Run 트리거 실행
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                if (!stateInfo.IsName("Jump"))
                {
                    // 움직임이 있으면?
                    if (moveInput.magnitude != 0)
                    {
                        Debug.Log("벽 위인데 움직임 있음");
                        ResetTrigger();
                        animator.SetTrigger("Run");
                    }
                    else
                    {
                        Debug.Log("벽 위인데 움직임 없음");
                        ResetTrigger();
                        animator.SetTrigger("Idle");
                    }
                }
                jumpCount = maxJumpCount;       // 2단 점프 가능
                isGround = true;
            }
        }
    }
    private IEnumerator HangDelay()
    {
        yield return new WaitForSeconds(0.1f);
        justHanged = false;
    }

    private IEnumerator JumpDelay()
    {
        yield return new WaitForSeconds(0.1f); // 0.1~0.15초 정도
        justJumped = false;
    }


    private void OnCollisionExit2D(Collision2D collision)
    {
        
    }
}
