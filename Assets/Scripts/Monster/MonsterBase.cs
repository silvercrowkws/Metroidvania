using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;   // Nav Mesh 를 사용하기 위해 필요한 using 문

public enum MonsterType
{
    None = 0,
    RedChicken,     // 레드 치킨 몬스터
    Skeleton,       // 스켈레톤 몬스터
    FlyingEye,      // 플라잉아이 몬스터
    Goblin,         // 고블린 몬스터
    Mushroom,       // 버섯 몬스터
}

public enum MonsterMoveType
{
    None = 0,
    Walk,           // 비행 불가능 몬스터
    Flying,         // 비행 가능 몬스터
}

public class MonsterBase : MonoBehaviour
{
    // 몬스터 베이스가 정의할 것
    // 1. 몬스터의 체력, 공격력, 이동 속도, 죽였을 때 주는 돈
    // 2. 몬스터의 사망 연출?
    // 3. 몬스터의 플레이어 탐지 범위(탐지하면 bool 변수 건들이고)
    // 4. 몬스터의 플레이어 감지 후 상태(공격) 등은 각 몬스터에서 구현 => 감지 bool 같은 것으로 관리
    // 5. 공격 직전 플레이어의 당시 위치 추적(그곳을 공격하기 위해)
    // 6. 생성된 위치도 기억할 필요가 있음(그래야 플레이어가 너무 멀리 벗어나면 원위치로 돌아갈테니)
    // 7. 몬스터 스폰은 RoomGenerator가 담당하긴 하는데 MonsterBase가 알고 있어야 하나?

    protected MonsterType monsterType = MonsterType.None;
    protected MonsterMoveType monsterMoveType = MonsterMoveType.None;

    /// <summary>
    /// 게임 매니저
    /// </summary>
    protected GameManager gameManager;

    /// <summary>
    /// 몬스터의 이동 속도
    /// </summary>
    protected float moveSpeed = 1.0f;

    /// <summary>
    /// 몬스터가 죽었을 때 주는 돈
    /// </summary>
    protected float dieMoney = 1.0f;

    /// <summary>
    /// 몬스터의 최대 체력
    /// </summary>
    protected float maxHP = 1.0f;

    /// <summary>
    /// 몬스터가 죽었다고 알리는 델리게이트
    /// </summary>
    protected Action onMonsterDie;

    /// <summary>
    /// 몬스터가 죽었는지 확인하는 bool 변수
    /// </summary>
    protected bool isDead = false;

    /// <summary>
    /// 몬스터의 현재 체력
    /// </summary>
    protected float currentHP = 1.0f;

    /// <summary>
    /// 몬스터 체력 프로퍼티
    /// </summary>
    public float HP
    {
        get => currentHP;
        set
        {
            if (currentHP != value)
            {
                //currentHP = value;
                currentHP = Mathf.Clamp(value, 0, maxHP);
                healthSlider.value = currentHP / maxHP;
                if (currentHP < 1)
                {
                    currentHP = 0;

                    //gameManager.Money += dieMoney;      // 돈 증가
                    onMonsterDie?.Invoke();            // 몬스터가 죽었다고 델리게이트로 알림                    
                    //Destroy(gameObject);        // 게임 오브젝트 파괴
                    Debug.Log("몬스터 사망");
                }
            }
        }
    }

    /// <summary>
    /// 몬스터의 공격력
    /// </summary>
    protected float attackPower = 10.0f;

    /// <summary>
    /// 사망 연출
    /// </summary>
    protected GameObject dieEffect;

    /// <summary>
    /// 사망 연출 오브젝트 생성
    /// </summary>
    protected GameObject dieEffectInstance;

    /// <summary>
    /// 애니메이터
    /// </summary>
    protected Animator animator;

    /// <summary>
    /// 스프라이트 랜더러
    /// </summary>
    protected SpriteRenderer spriteRenderer;
    
    /// <summary>
    /// 몬스터의 소환된 위치 저장
    /// </summary>
    protected Vector3 spawnPosition;

    /// <summary>
    /// 몬스터가 추격중인지 확인하는 bool 변수 => 추격중일때 일정 시간 간격으로 플레이어를 공격하도록 추가
    /// ChaseRangeTrigger 클래스에서 조작
    /// </summary>
    public bool isChasing = false;

    /// <summary>
    /// 플레이어의 트랜스폼
    /// </summary>
    public Transform playerTransform;

    /// <summary>
    /// 몬스터가 플레이어를 추격 및 공격하는 범위
    /// </summary>
    public GameObject chaseRange;

    private float chaseTime = 0f;           // 추적 지속 시간
    private float firstAttackDelay = 1.0f;  // 첫 공격 딜레이
    private float nextAttackDelay = 2.0f;   // 이후 공격 딜레이
    private bool firstAttackDone = false;   // 첫 공격 여부

    //ChaseRangeTrigger chaseRamgeTrigger;

    /// <summary>
    /// 플레이어
    /// </summary>
    Player player;

    Player_Test player_test;

    /// <summary>
    /// 공격 받을 수 있는지 확인하는 bool 변수(공격 받은 후 일정 시간 동안 데미지를 입지 않도록)
    /// false : 공격 받을 수 있음, true : 공격 받을 수 없음
    /// </summary>
    private bool damageCoolDown = false;

    /// <summary>
    /// 공겨 할 수 있는지 확인하는 bool 변수(공격한 후 연속으로 데미지 들어가지 않도록)
    /// false : 공격 할 수 있음, true : 공격 할 수 없음
    /// </summary>
    private bool attackCoolDown = false;

    /// <summary>
    /// 공격할 목표 위치(Attack 함수가 호출된 시점의 playerTransform)
    /// </summary>
    private Vector3 targetPosition;

    /// <summary>
    /// 공격 중 여부
    /// </summary>
    private bool isAttacking = false;

    /// <summary>
    /// 하트 패널
    /// </summary>
    HeartPanel heartPanel;

    /// <summary>
    /// 공격 반경
    /// </summary>
    float attackRadius = 1f;

    /// <summary>
    /// 네브매시 에이전트
    /// </summary>
    NavMeshAgent agent;

    /// <summary>
    /// 몬스터의 체력을 보여주기 위한 슬라이더
    /// </summary>
    Slider healthSlider;

    /// <summary>
    /// 적의 공격으로 플레이어에게 데미지를 적용시키는 함수
    /// </summary>
    public Action<float> onPlayerApplyDamage;

    /// <summary>
    /// 원래 색상
    /// </summary>
    private Color originalColor;

    /// <summary>
    /// 깜빡임 횟수
    /// </summary>
    int blinkCount = 3;

    /// <summary>
    /// 깜빡일 때의 색상
    /// </summary>
    public Color blinkColor = Color.red;


    protected virtual void Awake()
    {
        /*gameManager = GameManager.Instance;

        animator = GetComponent<Animator>();

        spriteRenderer = GetComponent<SpriteRenderer>();

        // Resources.Load는 리소스를 로드하는 메서드
        // <Sprite>는 로드할 에셋의 타입을 지정(Texture, AudioClip, GameObject 등 다른 타입도 있음)
        // "Sprites/sprite1" 이 부분은 로딩할 리소스의 경로
        // "Sprites"는 Resources 폴더 내에 있는 서브폴더
        // "sprite1"은 해당 폴더 내에 있는 에셋의 이름
        dieEffect = Resources.Load<GameObject>("GameObjects/dieEffect");

        if (dieEffect != null)
        {
            dieEffectInstance = Instantiate(dieEffect, transform);
            dieEffectInstance.transform.localPosition = Vector3.zero;      // 부모의 중심에 위치
            dieEffectInstance.SetActive(false);                            // 기본적으로 비활성화(사망 시 활성화)
        }

        onMonsterDie += OnMonsterDie;
        // 만약 몬스터의 HP가 0이되면 사망 연출 활성화?
        // 만약 몬스터의 HP가 0이 되면 */
    }

    protected virtual void OnEnable()
    {
        // 초기화
        isDead = false;
        damageCoolDown = false;

        gameManager = GameManager.Instance;

        player = gameManager.Player;
        player_test = gameManager.Player_Test;

        // 플레이어가 null이면 Player_Test와 연결
        if (player == null)
        {
            playerTransform = player_test.transform;
            onPlayerApplyDamage += player_test.OnPlayerApplyDamage;
        }

        // 플레이어가 null이 아니면 Player와 연결
        else
        {
            playerTransform = player.transform;
            //onPlayerApplyDamage += player.OnPlayerApplyDamage;
        }

        animator = GetComponent<Animator>();

        spriteRenderer = GetComponent<SpriteRenderer>();

        // 스프라이트 색상(RGB, 알파) 초기화
        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color(1f, 1f, 1f, 1f);
            originalColor = spriteRenderer.color;
        }

        // Resources.Load는 리소스를 로드하는 메서드
        // <Sprite>는 로드할 에셋의 타입을 지정(Texture, AudioClip, GameObject 등 다른 타입도 있음)
        // "Sprites/sprite1" 이 부분은 로딩할 리소스의 경로
        // "Sprites"는 Resources 폴더 내에 있는 서브폴더
        // "sprite1"은 해당 폴더 내에 있는 에셋의 이름
        dieEffect = Resources.Load<GameObject>("GameObjects/dieEffect");

        if (dieEffect != null)
        {
            dieEffectInstance = Instantiate(dieEffect, transform);
            dieEffectInstance.transform.localPosition = Vector3.zero;      // 부모의 중심에 위치
            dieEffectInstance.SetActive(false);                            // 기본적으로 비활성화(사망 시 활성화)
        }

        animator.speed = 1f;        // 활성화 될 때 애니메이션 속도 정상화
        maxHP = currentHP;          // 체력 정상화

        onMonsterDie += OnMonsterDie;

        heartPanel = FindAnyObjectByType<HeartPanel>();

        Transform child = transform.GetChild(1);

        healthSlider = child.GetChild(0).GetComponent<Slider>();
        healthSlider.value = 1;
    }

    protected virtual void OnDisable()
    {
        this.gameObject.transform.position = spawnPosition;

        dieEffectInstance.SetActive(false);     // 폭발 비활성화
        healthSlider.value = 1;                       // 체력 슬라이더 조정
        onMonsterDie -= OnMonsterDie;
    }

    protected virtual void Start()
    {
        spawnPosition = transform.position;


        if(monsterMoveType == MonsterMoveType.Flying)
        {
            agent = GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                agent.updateRotation = false;
                agent.updateUpAxis = false;
                agent.speed = moveSpeed; // 몬스터 속도와 동기화
                agent.updateUpAxis = false;
            }
        }
    }

    protected virtual void FixedUpdate()
    {
        // 몬스터 사망시 아무 것도 하지 않음
        if (isDead)
        {
            return;
        }

        // 공격이 활성화 되었으면
        if (isAttacking)
        {
            if(monsterMoveType == MonsterMoveType.Walk)
            {
                float attackSpeed = moveSpeed * 5f;       // 공격 속도(원하는 값으로 조절)
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, attackSpeed * Time.deltaTime);     // 적 위치로 빠르게 이동하는 부분

                // 목표 위치에 도달하면 공격 종료, 다시 추격 시작
                if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
                {
                    isAttacking = false;
                    isChasing = true;

                    ResetTrigger();
                    animator.SetTrigger("Moving");
                }
                return; // 공격 중에는 다른 동작 안 함
            }
            else if (monsterMoveType == MonsterMoveType.Flying && agent != null)
            {
                // NavMeshAgent로 목표 위치까지 이동
                agent.SetDestination(targetPosition);

                // 목적지 도달 체크
                bool arrived = !agent.pathPending && agent.remainingDistance <= 0.05f;
                if (arrived)
                {
                    isAttacking = false;
                    isChasing = true;
                    ResetTrigger();
                    animator.SetTrigger("Moving");
                    agent.ResetPath();
                }
                return;
            }
        }        

        // 몬스터 타입에 맞게 행동하는 함수
        OnMonsterActive();
    }

    /// <summary>
    /// 몬스터 타입에 맞게 행동하는 함수
    /// </summary>
    protected virtual void OnMonsterActive()
    {
        // 걷기 가능한 몬스터인 경우 추적 방법
        if (monsterMoveType == MonsterMoveType.Walk)
        {
            if (isChasing && playerTransform != null)
            {
                float distance = Vector3.Distance(transform.position, playerTransform.position);

                // X축 방향만 추적 (Y는 고정)
                Vector3 targetPos = new Vector3(playerTransform.position.x, transform.position.y, transform.position.z);
                Vector3 dir = (targetPos - transform.position).normalized;

                // 바닥 체크 (앞쪽 아래로 Raycast)
                float rayDistance = 1.0f;
                Vector3 rayOrigin = transform.position + new Vector3(dir.x * 0.5f, 0, 0);

                // "GroundBake" 레이어만 체크
                int groundMask = LayerMask.GetMask("GroundBake");
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, rayDistance, groundMask);

                Debug.DrawRay(rayOrigin, Vector2.down * rayDistance, Color.red);

                bool isGroundAhead = hit.collider != null;

                // 디버그로 확인
                if (hit.collider == null)
                {
                    //Debug.Log("Raycast: 바닥 없음");
                }
                else
                {
                    //Debug.Log("Raycast: " + hit.collider.tag);
                }

                // 추격 종료 조건: 거리 or 바닥 없음
                if (distance >= 5f || !isGroundAhead)
                {
                    isChasing = false;
                    chaseTime = 0f;
                    firstAttackDone = false;
                    ResetTrigger();
                    animator.SetTrigger("Idle");
                    return;
                }

                // 추격 중일 때 Moving 트리거
                ResetTrigger();
                animator.SetTrigger("Moving");

                // 몬스터와 플레이어의 X 좌표 차이가 일정 값(임계값)보다 클 때만 Flip X를 변경
                float threshold = 0.1f;
                if (Mathf.Abs(playerTransform.position.x - transform.position.x) > threshold)
                {
                    // 플레이어를 바라보도록 Flip X 처리 (기본 오른쪽)
                    if (playerTransform.position.x < transform.position.x)
                    {
                        spriteRenderer.flipX = true;
                    }
                    else
                    {
                        spriteRenderer.flipX = false;
                    }
                }

                transform.position += dir * moveSpeed * Time.deltaTime;

                // 추적 시간 누적
                chaseTime += Time.deltaTime;

                // 첫번째 공격을 안했고 추적 시간이 첫 공격 딜레이 - 0.5. 즉 첫 공격 0.5초 전이면
                if (!firstAttackDone && chaseTime >= firstAttackDelay - 0.5f)
                {
                    // 모습 잠깐 깜빡이는 코루틴
                    // 0.25초 간격으로 처음에 깜빡 0.25 후 깜빡 0.5초에 깜빡
                    StartCoroutine(BlinkEffect());
                }
                // 첫 번째 공격을 했고 다음 공격 딜레이 - 0.5, 즉 다음 공격 0.5초 전이면
                else if(firstAttackDone && chaseTime >= nextAttackDelay - 0.5f)
                {
                    // 모습 잠깐 깜빡이는 코루틴
                    StartCoroutine(BlinkEffect());
                }

                // 첫 공격을 안했고 추적 시간이 첫 공격 딜레이를 넘었으면
                if (!firstAttackDone && chaseTime >= firstAttackDelay)
                {
                    Attack();
                    firstAttackDone = true;

                    // 플레이어가 반경 내에 있으면 데미지 적용 (Overlap 방식)
                    TryAttackInProximity();
                    chaseTime = 0f;     // 첫 공격 후 타이머 리셋
                }
                // 첫 공격 이후 추적 시간이 다음 공격 딜레이를 넘었으면
                else if (firstAttackDone && chaseTime >= nextAttackDelay)
                {
                    Attack();
                    // 플레이어가 반경 내에 있으면 데미지 적용 (Overlap 방식)
                    TryAttackInProximity();
                    chaseTime = 0f;
                }
            }
            else
            {
                // 추격 중이 아니면 원위치로 돌아감
                if (Vector3.Distance(transform.position, spawnPosition) > 0.1f)
                {
                    chaseTime = 0f;
                    firstAttackDone = false;

                    // 원위치로 돌아가는 중에는 Moving 트리거
                    ResetTrigger();
                    animator.SetTrigger("Moving");

                    // 원위치 방향으로 Flip X 처리 (기본 오른쪽)
                    if (spawnPosition.x < transform.position.x)
                    {
                        spriteRenderer.flipX = true;
                    }
                    else
                    {
                        spriteRenderer.flipX = false;
                    }

                    Vector3 dir = (spawnPosition - transform.position).normalized;
                    transform.position += dir * moveSpeed * Time.deltaTime;
                }
                else
                {
                    // 원위치 도착 시 Idle 트리거
                    ResetTrigger();
                    animator.SetTrigger("Idle");

                    // Idle 시 기본 방향(오른쪽)으로 Flip X 해제
                    spriteRenderer.flipX = false;
                }
            }

            /*if (isChasing && playerTransform != null)
            {
                float distance = Vector3.Distance(transform.position, playerTransform.position);

                // 추격 종료 조건: 플레이어와의 거리가 5 이상
                if (distance >= 5f)
                {
                    isChasing = false;
                    playerTransform = null;
                    chaseTime = 0f;
                    firstAttackDone = false;
                }
                else
                {
                    // 추격 중일 때 Moving 트리거
                    ResetTrigger();
                    animator.SetTrigger("Moving");

                    // 플레이어를 바라보도록 Flip X 처리 (기본 오른쪽)
                    if (playerTransform.position.x < transform.position.x)
                    {
                        spriteRenderer.flipX = true;
                    }
                    else
                    {
                        spriteRenderer.flipX = false;
                    }

                    Vector3 dir = (playerTransform.position - transform.position).normalized;
                    transform.position += dir * moveSpeed * Time.deltaTime;


                    // 추적 시간 누적
                    chaseTime += Time.deltaTime;

                    // 첫 공격을 안했고 추적 시간이 첫 공격 딜레이를 넘었으면
                    if (!firstAttackDone && chaseTime >= firstAttackDelay)
                    {
                        Attack();
                        firstAttackDone = true;

                        // 플레이어가 반경 내에 있으면 데미지 적용 (Overlap 방식)
                        TryAttackInProximity();
                        chaseTime = 0f;     // 첫 공격 후 타이머 리셋
                    }

                    // 첫 공격 이후 추적 시간이 다음 공격 딜레이를 넘었으면
                    else if (firstAttackDone && chaseTime >= nextAttackDelay)
                    {
                        Attack();

                        // 플레이어가 반경 내에 있으면 데미지 적용 (Overlap 방식)
                        TryAttackInProximity();
                        chaseTime = 0f;
                    }
                }
            }
            else
            {
                // 추격 중이 아니면 원위치로 돌아감
                if (Vector3.Distance(transform.position, spawnPosition) > 0.1f)
                {
                    chaseTime = 0f;
                    firstAttackDone = false;

                    // 원위치로 돌아가는 중에는 Moving 트리거
                    ResetTrigger();
                    animator.SetTrigger("Moving");

                    // 원위치 방향으로 Flip X 처리 (기본 오른쪽)
                    if (spawnPosition.x < transform.position.x)
                    {
                        spriteRenderer.flipX = true;
                    }
                    else
                    {
                        spriteRenderer.flipX = false;
                    }

                    Vector3 dir = (spawnPosition - transform.position).normalized;
                    transform.position += dir * moveSpeed * Time.deltaTime;
                }
                else
                {
                    // 원위치 도착 시 Idle 트리거
                    ResetTrigger();
                    animator.SetTrigger("Idle");

                    // Idle 시 기본 방향(오른쪽)으로 Flip X 해제
                    spriteRenderer.flipX = false;
                }
            }*/
        }


        // 비행 가능한 몬스터의 경우 추적 방법
        else if (monsterMoveType == MonsterMoveType.Flying)
        {
            // 나중에 몬스터 만들고 구현 필요

            if (isChasing && playerTransform != null && agent != null)
            {
                float distance = Vector3.Distance(transform.position, playerTransform.position);

                // 추격 종료 조건
                if (distance >= 5f)
                {
                    isChasing = false;
                    playerTransform = null;
                    chaseTime = 0f;
                    firstAttackDone = false;
                    agent.ResetPath(); // 경로 초기화

                    // 원위치로 돌아가기 시작
                    agent.SetDestination(spawnPosition);
                }
                else
                {
                    // NavMeshAgent로 플레이어 추적

                    ResetTrigger();
                    animator.SetTrigger("Moving");

                    // 플레이어를 바라보도록 Flip X 처리 (기본 오른쪽)
                    if (playerTransform.position.x < transform.position.x)
                    {
                        spriteRenderer.flipX = true;
                    }
                    else
                    {
                        spriteRenderer.flipX = false;
                    }

                    agent.SetDestination(playerTransform.position);

                    // 추적 시간 누적 및 공격 타이밍 등 기존 로직
                    chaseTime += Time.deltaTime;

                    // 첫번째 공격을 안했고 추적 시간이 첫 공격 딜레이 - 0.5. 즉 첫 공격 0.5초 전이면
                    if (!firstAttackDone && chaseTime >= firstAttackDelay - 0.5f)
                    {
                        // 모습 잠깐 깜빡이는 코루틴
                        // 0.25초 간격으로 처음에 깜빡 0.25 후 깜빡 0.5초에 깜빡
                        StartCoroutine(BlinkEffect());
                    }
                    // 첫 번째 공격을 했고 다음 공격 딜레이 - 0.5, 즉 다음 공격 0.5초 전이면
                    else if (firstAttackDone && chaseTime >= nextAttackDelay - 0.5f)
                    {
                        // 모습 잠깐 깜빡이는 코루틴
                        StartCoroutine(BlinkEffect());
                    }

                    if (!firstAttackDone && chaseTime >= firstAttackDelay)
                    {
                        Attack();
                        firstAttackDone = true;

                        // 플레이어가 반경 내에 있으면 데미지 적용 (Overlap 방식)
                        TryAttackInProximity();
                        chaseTime = 0f;
                    }

                    // 첫 공격 이후 추적 시간이 다음 공격 딜레이를 넘었으면
                    else if (firstAttackDone && chaseTime >= nextAttackDelay)
                    {
                        Attack();

                        // 플레이어가 반경 내에 있으면 데미지 적용 (Overlap 방식)
                        TryAttackInProximity();
                        chaseTime = 0f;
                    }
                }
            }
            else if (agent != null)
            {
                /*// 추격 중이 아니면 원위치로 돌아감
                float distanceToSpawn = Vector3.Distance(transform.position, spawnPosition);

                if (!isChasing && distanceToSpawn > 0.1f)
                {
                    chaseTime = 0f;
                    firstAttackDone = false;

                    // 원위치로 돌아가는 중에는 Moving 트리거
                    ResetTrigger();
                    animator.SetTrigger("Moving");

                    // 원위치 방향으로 Flip X 처리 (기본 오른쪽)
                    if (spawnPosition.x < transform.position.x)
                    {
                        spriteRenderer.flipX = true;
                    }
                    else
                    {
                        spriteRenderer.flipX = false;
                    }

                    agent.SetDestination(spawnPosition);
                }
                else if(!isChasing && distanceToSpawn <= 0.1f)
                {
                    // 원위치 도착 시 Idle 트리거
                    ResetTrigger();
                    animator.SetTrigger("Idle");

                    // Idle 시 기본 방향(오른쪽)으로 Flip X 해제
                    spriteRenderer.flipX = false;

                    agent.ResetPath();
                }*/

                // NavMeshAgent가 목적지에 거의 도달했는지 체크
                bool arrived = !agent.pathPending && agent.remainingDistance <= 0.05f;

                if (!isChasing && !arrived)
                {
                    chaseTime = 0f;
                    firstAttackDone = false;

                    ResetTrigger();
                    animator.SetTrigger("Moving");

                    if (spawnPosition.x < transform.position.x)
                    {
                        spriteRenderer.flipX = true;
                    }
                    else
                    {
                        spriteRenderer.flipX = false;
                    }

                    agent.SetDestination(spawnPosition);
                }
                else if (!isChasing && arrived)
                {
                    ResetTrigger();
                    animator.SetTrigger("Idle");
                    spriteRenderer.flipX = false;
                    agent.ResetPath();
                }
            }
        }
    }

    /// <summary>
    /// 공격 0.5초 전에 알려주는 코루틴
    /// </summary>
    /// <returns></returns>
    IEnumerator BlinkEffect()
    {
        // 깜빡이는 횟수만큼 반복(0,1,2)
        for (int i = 0; i < blinkCount; i++)
        {
            // 몬스터 색상을 깜빡일 색상으로 변경
            if (spriteRenderer != null)
            {
                spriteRenderer.color = blinkColor;
            }

            // 지정된 간격만큼 기다립니다.
            yield return new WaitForSeconds(0.25f / 2);

            // 원래 색상으로 되돌립니다.
            if (spriteRenderer != null)
            {
                spriteRenderer.color = originalColor;
            }

            // 다시 지정된 간격만큼 기다립니다.
            yield return new WaitForSeconds(0.25f / 2);
        }
    }

    /// <summary>
    /// 몬스터의 HP가 0이 되었을 때 실행될 함수
    /// </summary>
    /// <returns></returns>
    protected virtual void OnMonsterDie()
    {
        // 애니메이터 정지 (현재 프레임에서 멈춤), 플레이어 추적 정지

        //Debug.Log("OnMonsterDie 실행");
        isDead = true;

        if (animator != null)
        {
            animator.speed = 0f;
        }

        // 플레이어 추적 및 이동 정지
        isChasing = false;
        playerTransform = null;

        // Rigidbody2D가 있다면 속도도 0으로 (물리 이동 중지)
        var rb2d = GetComponent<Rigidbody2D>();
        if (rb2d != null)
        {
            rb2d.velocity = Vector2.zero;
        }

        StartCoroutine(dieEffectActiveCoroutine());
    }

    /// <summary>
    /// 애니메이터 정지 후 사망 연출을 위한 코루틴
    /// </summary>
    /// <returns></returns>
    protected IEnumerator dieEffectActiveCoroutine()
    {
        // 오브젝트의 RGB를 서서히 0으로 바꾸고
        // 그 후에 사망 이펙트 활성화

        // 체력은 잘 빠져나가는거 같은데 알파값이 문제인가 서서히 RGB 줄어들고 알파값0으로 바뀌는 부분이 안보임

        //Debug.Log("dieEffectActiveCoroutine 코루틴 실행");

        // 1. RGB를 서서히 0으로
        if (spriteRenderer != null)
        {
            float duration = 0.5f;
            float elapsed = 0f;
            Color startColor = spriteRenderer.color;
            Color endColor = new Color(0f, 0f, 0f, startColor.a);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                spriteRenderer.color = Color.Lerp(startColor, endColor, elapsed / duration);
                yield return null;
            }
            spriteRenderer.color = endColor;

            // 사망 이펙트 활성화
            if (dieEffectInstance != null)
            {
                Debug.Log("폭발 연출 활성화");
                dieEffectInstance.SetActive(true);
            }
        }
        yield return new WaitForSeconds(0.8f);

        // 2. 알파값만 0으로 변경
        Color alphaZero = spriteRenderer.color;
        alphaZero.a = 0f;
        spriteRenderer.color = alphaZero;

        this.gameObject.SetActive(false);
    }

    /// <summary>
    /// 적 본체 피격 처리
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 플레이어의 공격 범위에 충돌되면
        if (collision.CompareTag("AttackRange"))
        {
            // 공격 받을 수 있으면
            if (!damageCoolDown)
            {
                damageCoolDown = true;

                // 피격 처리
                Debug.Log("플레이어 공격에 맞음");
                if(player != null)
                {
                    HP -= player.playerAttackPower;
                    Debug.Log($"{this.gameObject.name}의 남은 HP: {HP}");
                }
                else
                {
                    if(player_test != null)
                    {
                        HP -= player_test.playerAttackPower;
                        Debug.Log($"{this.gameObject.name}의 남은 HP: {HP}");
                    }
                    else
                    {
                        Debug.LogWarning("플레이어 테스트도 없는데?");
                    }
                }

                StartCoroutine(DamageCooldown());
            }
        }
        
        /*// 플레이어에게 공격하는 부분
        if (collision.CompareTag("Player"))
        {
            Debug.Log("플레이어한테 충돌은 함");
            
            if (isAttacking && !attackCoolDown)
            {
                attackCoolDown = true;

                Debug.Log("플레이어한테 공격은 함");

                // 플레이어의 HP 감소 부분
                if (player != null)
                {
                    player.HP -= attackPower;
                    heartPanel.UpdateHearts(player.HP);
                }
                else if (player_test != null)
                {
                    player_test.HP -= attackPower;
                    heartPanel.UpdateHearts(player_test.HP);
                }
                else
                {
                    Debug.Log("아니 플레이어랑 플레이어 테스트 둘 다 null인데?");
                }

                StartCoroutine(MonsterDamageColldown());
            }
        }*/
    }

    /// <summary>
    /// 공격 받은 후 일정 시간동안 공격 받지 않도록 하는 코루틴
    /// </summary>
    /// <returns></returns>
    IEnumerator DamageCooldown()
    {
        yield return new WaitForSeconds(1.0f);
        damageCoolDown = false;
    }

    /// <summary>
    /// 몬스터가 공격한 후 일정 시간동안 또 공격하지 않도록 하는 코루틴
    /// </summary>
    /// <returns></returns>
    IEnumerator MonsterDamageColldown()
    {
        yield return new WaitForSeconds(1.0f);
        attackCoolDown = false;
    }

    protected virtual void ResetTrigger()
    {
        animator.ResetTrigger("Idle");
        animator.ResetTrigger("Moving");
        animator.ResetTrigger("Attack");
        //animator.ResetTrigger("TakeHit");
    }

    /// <summary>
    /// 공격하는 함수
    /// </summary>
    protected void Attack()
    {
        if (monsterMoveType == MonsterMoveType.Walk)
        {
            // X, Z는 플레이어 위치, Y는 스폰 위치로 고정
            targetPosition = new Vector3(playerTransform.position.x, spawnPosition.y, playerTransform.position.z);
        }
        else if(monsterMoveType == MonsterMoveType.Flying)
        {
            targetPosition = playerTransform.position;
        }

        isAttacking = true;
        isChasing = false;      // 추격 중단

        // 몬스터가 RedChicken의 경우 공격 방법
        if (monsterType == MonsterType.RedChicken)
        {
            ResetTrigger();
            animator.SetTrigger("Attack");
        }

        // 몬스터가 Skeleton의 경우 공격 방법
        else if (monsterType == MonsterType.Skeleton)
        {
            ResetTrigger();
            animator.SetTrigger("Attack");
        }

        else if(monsterType == MonsterType.FlyingEye)
        {
            ResetTrigger();
            animator.SetTrigger("Attack");
        }

        else if(monsterType == MonsterType.Goblin)
        {
            ResetTrigger();
            animator.SetTrigger("Attack");
        }
        
        else if(monsterType == MonsterType.Mushroom)
        {
            ResetTrigger();
            animator.SetTrigger("Attack");
        }
    }

    /// <summary>
    /// 플레이어가 공격 반경내에 있으면 데미지 적용하는 함수 (Overlap 방식)
    /// </summary>
    private void TryAttackInProximity()
    {
        if (!isAttacking && attackCoolDown || playerTransform == null)
            return;

        //float attackRadius = 0.5f; // 공격 반경 조절 가능
        LayerMask playerLayer = LayerMask.GetMask("Player"); // 플레이어 레이어

        Collider2D hit = Physics2D.OverlapCircle(transform.position, attackRadius, playerLayer);
        if (hit != null)
        {
            attackCoolDown = true;
            //Debug.Log("OverlapCircle로 플레이어 감지 및 데미지 적용");

            if (player != null)
            {
                //player.HP -= attackPower;         // 플레이어에게 델리게이트로 HP 변경 요청으로 수정
                onPlayerApplyDamage?.Invoke(attackPower);

                if (heartPanel != null)
                {
                    heartPanel.UpdateHearts(player.HP);
                }
            }
            else if (player_test != null)
            {
                //player_test.HP -= attackPower;    // 플레이어에게 델리게이트로 HP 변경 요청으로 수정
                onPlayerApplyDamage?.Invoke(attackPower);

                if (heartPanel != null)
                {
                    heartPanel.UpdateHearts(player_test.HP);
                }
            }

            StartCoroutine(MonsterDamageColldown());
        }
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return; // 실행 중일 때만 그리기 (선택사항)

        Gizmos.color = attackCoolDown ? Color.gray : Color.red;
        //float attackRadius = 0.5f;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }

}
