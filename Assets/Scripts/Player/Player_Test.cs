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

    /// <summary>
    /// 플레이어의 최대 체력
    /// </summary>
    private float maxHP;

    /// <summary>
    /// 플레이어의 현재 체력
    /// </summary>
    private float currentHP;

    public float HP
    {
        get => currentHP;
        set
        {
            if (currentHP != value)
            {
                //currentHP = value;
                currentHP = Mathf.Clamp(value, 0, maxHP);
                if (currentHP < 1)
                {
                    currentHP = 0;

                    onPlayerDie?.Invoke(currentHP);     // 플레이어가 죽었다고 델리게이트로 알림
                    Debug.Log("플레이어 사망");
                    
                    // 사망 연출 실행 부분

                }
            }
        }
    }

    /// <summary>
    /// 플레이어가 죽었음을 알리는 델리게이트
    /// </summary>
    public Action<float> onPlayerDie;

    /// <summary>
    /// 플레이어의 공격력
    /// </summary>
    public float playerAttackPower = 25.0f;

    /// <summary>
    /// Hang 트리거 이후에 다른 트리거들 들어가지 않도록 코루틴에서 딜레이 주는 용도
    /// </summary>
    private bool justHanged = false;

    /// <summary>
    /// 점프한 이후에 다른 트리거들 들어가지 않도록 코루틴에서 딜레이 주는 용도
    /// </summary>
    private bool justJumped = false;

    /// <summary>
    /// 대쉬한 이후에 다른 트리거들 들어가지 않도록 코루틴에서 딜레이 주는 용도
    /// </summary>
    private bool justDashed = false;

    /// <summary>
    /// 벽의 옆면과 충돌 중인지 확인하기 위한 bool 변수(false : 벽과 접촉 중, true : 벽과 접촉 해제)
    /// </summary>
    public bool canFall = true;

    /// <summary>
    /// 공격 범위 확인용
    /// </summary>
    GameObject attackRange;

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

    // 몬스터와 상호작용 관련 --------------------------------------------------


    private void Awake()
    {
        inputActions = new PlayerInputActions();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        // 리지드 바디의 충돌 감지 모드를 연속으로 변경
        // 기존 값은 느린 속도에서는 충돌을 잘 감지하지만,빠르게 움직이면(대쉬) 한 프레임에 오브젝트가 벽을 통과(터널링)할 수 있음
    }

    private void OnEnable()
    {
        inputActions.Actions.Enable();
        inputActions.Actions.Move.performed += OnMove;
        inputActions.Actions.Move.canceled += OnMove;
        inputActions.Actions.Jump.performed += OnJump;
        inputActions.Actions.Dash.performed += OnDash;
        inputActions.Actions.DoorInteract.performed += OnDoorInteract;
        inputActions.Actions.Attack.performed += OnAttack;

        Transform child = transform.GetChild(0);
        attackRange = child.gameObject;

        if( attackRange != null )
        {
            attackRange.SetActive(false);
        }
    }

    private void OnDisable()
    {
        inputActions.Actions.Attack.performed -= OnAttack;
        inputActions.Actions.DoorInteract.performed -= OnDoorInteract;
        inputActions.Actions.Dash.performed -= OnDash;
        inputActions.Actions.Jump.performed -= OnJump;
        inputActions.Actions.Move.canceled -= OnMove;
        inputActions.Actions.Move.performed -= OnMove;
        inputActions.Actions.Disable();
    }

    /// <summary>
    /// 인스펙터에서 확인하기 위한 변수
    /// </summary>
    public bool isFall = false;

    private void FixedUpdate()
    {
        if (isDash)
        {
            dashTimer -= Time.fixedDeltaTime;
            if (dashTimer <= 0)
            {
                isDash = false;
                ResetTrigger();
                if (isGround)
                {
                    if (moveInput.magnitude > 0.1f)
                        animator.SetTrigger("Run");
                    else
                        animator.SetTrigger("Idle");
                }
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

        if (canFall)
        {
            if (stateInfo.IsName("Jump"))
            {
                if (!justJumped && rb.velocity.y < -0.01f)
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
            // 아이들 or 런 상태일 때 떨어지면
            else if(stateInfo.IsName("Idle") && stateInfo.IsName("Run"))
            {
                if(rb.velocity.y < -0.01f)
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
        else
        {

            isFall = false;
            animator.SetBool("IsFall", false);
        }

        /*// Jump 상태일 때만 y속도가 음수로 바뀌었을 때 Fall로 전환
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
        }*/
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

            justDashed = true;
            /*rb.velocity = Vector2.zero;     // 기존 속도 초기화
            rb.AddForce(Vector2.right * dashPower * dashDirection, ForceMode2D.Impulse);*/
            rb.velocity = new Vector2(dashPower * dashDirection, 0f); // 직접 속도 지정
            rb.gravityScale = 1;        // 중력 원래대로
            dashTimer = dashTime;

            StartCoroutine(DashDelay());
        }
    }

    /// <summary>
    /// 인풋 시스템으로 플레이어의 공격을 제어하는 함수
    /// </summary>
    /// <param name="context"></param>
    private void OnAttack(InputAction.CallbackContext context)
    {
        // 만약 Idle이나 Run 상태에서 공격하면 그냥 Attack이 나가고
        // Dash 상태에서 공격하면 Dash-Attack이 나가고?

        // 달리는 상태에서 Attack을 DashAttack이라고 합시다
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName("Run") || stateInfo.IsName("Dash"))
        {
            animator.SetTrigger("DashAttack");
        }
        else
        {
            animator.SetTrigger("Attack");
        }
    }

    /// <summary>
    /// 공격 시작할 때 이동 속도 0으로 만드는 함수(애니메이터 이벤트용)
    /// </summary>
    private void AttackStart()
    {
        moveSpeed = 0;
        attackRange.SetActive(true);
    }

    /// <summary>
    /// 공격이 끝났을 때 이동속도 되돌리는 함수(애니메이터 이벤트용)
    /// </summary>
    private void AttackEnd()
    {
        moveSpeed = defaultMoveSpeed;
        attackRange.SetActive(false);
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
        animator.ResetTrigger("Attack");
        animator.ResetTrigger("DashAttack");
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

    private IEnumerator DashDelay()
    {
        yield return new WaitForSeconds(0.1f); // 0.1~0.15초 정도
        justDashed = false;
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
        animator.SetTrigger("Idle");

        yield return new WaitForSeconds(1.5f);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);       // 현재 실행 중인 씬 +1 로 이동
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

            // 1. 벽 위쪽(땅 취급) 먼저 체크
            if (normal.y > 0.7f)
            {
                // 벽 위쪽(땅 취급)
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                if (!stateInfo.IsName("Jump"))
                {
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
                jumpCount = maxJumpCount;
                isGround = true;
                return;     // 벽 위쪽이면 여기서 끝내고 Hang 실행 안 함
            }

            // 2. 벽 옆면(Hang)
            if (Mathf.Abs(normal.y) < 0.1f && Mathf.Abs(normal.x) > 0.7f)
            {
                Debug.Log("벽에는 정상 충돌함");
                ResetTrigger();
                animator.SetTrigger("Hang");
                jumpCount = 1;
                justHanged = true;
                StartCoroutine(HangDelay());
            }

            /*if (Mathf.Abs(normal.y) < 0.1f && Mathf.Abs(normal.x) > 0.7f)
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
            }*/
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        // 벽에서 떨어지면 다시 canFall = true
        if (collision.gameObject.CompareTag("Wall"))
        {
            canFall = true;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            foreach (var contact in collision.contacts)
            {
                // 벽의 옆면에 충돌했으면
                if (Mathf.Abs(contact.normal.x) > 0.7f && Mathf.Abs(contact.normal.y) < 0.1f)
                {
                    canFall = false;
                    return;
                }
            }
        }
        canFall = true;
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
}
