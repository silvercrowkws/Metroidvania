using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using static Cinemachine.DocumentationSortingAttribute;

public class Player_Test : Singleton<Player_Test>
{
    /// <summary>
    /// 플레이어의 이름
    /// </summary>
    public string PlayerName { get; set; }

    /// <summary>
    /// 플레이어의 레벨
    /// </summary>
    public int Level { get; set; }

    /// <summary>
    /// 힘 스탯
    /// </summary>
    public int Strength { get; set; }

    /// <summary>
    /// 민첩 스탯
    /// </summary>
    public int Dexterity { get; set; }

    /// <summary>
    /// 체력 스탯
    /// </summary>
    public int Health { get; set; }

    /// <summary>
    /// 스탯 포인트
    /// </summary>
    public int StatePoint { get; set; }

    private int money;
    public int Money
    {
        get => money;
        set
        {
            money = value;
            // 돈이 바뀔 때마다 등록된 함수(UI 갱신 등)를 모두 실행!
            onMoneyChanged?.Invoke(money);
            Debug.Log($"플레이어의 돈 변경 : {Money}");
        }
    }

    /// <summary>
    /// 돈 변경 시 발생하는 이벤트
    /// </summary>
    public Action<int> onMoneyChanged;

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
    public int jumpCount = 0;

    /// <summary>
    /// 최대 점프 가능 횟수
    /// </summary>
    private int maxJumpCount = 2;

    /// <summary>
    /// 인스펙터에서 확인하기 위한 변수
    /// </summary>
    public bool isFall = false;

    /// <summary>
    /// 캐릭터가 대쉬 중인지 확인하기 위한 bool 변수
    /// </summary>
    bool isDash = false;

    /// <summary>
    /// 대쉬 가능한지 확인하는 bool 변수
    /// </summary>
    bool isDashAble = true;

    float dashTime = 0.2f;
    float dashTimer = 0f;

    /// <summary>
    /// 플레이어가 죽었는지 확인하기 위한 bool 변수
    /// </summary>
    public bool playerDie = false;

    /// <summary>
    /// HP 변경 UI 패널
    /// </summary>
    HeartPanel heartPanel;

    /// <summary>
    /// 플레이어의 최대 체력(체력 10당 하트 1칸)
    /// public 이기 때문에 인스펙터에서 바꿔야 의미 있음
    /// </summary>
    public float maxHP = 100;

    /// <summary>
    /// 플레이어의 현재 체력
    /// </summary>
    private float currentHP;

    /// <summary>
    /// 무적시 보일 실드
    /// </summary>
    GameObject shield;

    public float HP
    {
        get => currentHP;
        set
        {
            if (currentHP != value)
            {
                // --- 추가: 무적 상태일 때 데미지 무시 로직 ---
                // 새로 들어온 값(value)이 현재 체력보다 낮다면 데미지를 입는 상황임
                if (isInvincible && value < currentHP)
                {
                    Debug.Log("무적 상태이므로 데미지를 무시합니다.");
                    return; // 체력을 깎지 않고 종료
                }
                // ------------------------------------------

                //currentHP = value;
                currentHP = Mathf.Clamp(value, 0, maxHP);

                Debug.Log($"플레이어의 남은 체력: {HP}");

                // 만약 플레이어가 죽었고 부활하지 않았는데 회복되었으면(나중에 초기화하는 경우인가)
                if (playerDie)
                {
                    playerDie = false;
                    animator.SetBool("playerDie", false);
                    ResetTrigger();
                    animator.SetTrigger("Idle");
                    OnEnable();
                }

                if (currentHP < 1)
                {
                    // 1. 인벤토리에서 부활 아이템이 있는지 확인
                    ItemDataSO resurrectionItem = Inventory.Instance.FindItemByRecoveryType(RecoveryType.Resurrection);

                    if (resurrectionItem != null)
                    {
                        // 2. 아이템이 있다면 1개 사용
                        Inventory.Instance.UseItem(resurrectionItem);

                        // 3. HP를 최대치로 회복 (자기 자신을 다시 호출하여 부활 로직 실행)
                        HP = maxHP;
                        Debug.Log($"{resurrectionItem.ItemName}을 사용하여 부활했습니다!");

                        // 4. 일정시간 무적 부여
                        // 🔥 추가: 일정 시간 무적 부여
                        StartCoroutine(BecomeInvincible(3.0f));     // 3초간 무적
                        //return; // 사망 로직으로 가지 않고 종료
                    }

                    // 플레이어의 HP가 0이되었는데 부활 아이템이 없는 경우 사망 처리
                    else
                    {
                        currentHP = 0;
                        playerDie = true;
                        animator.SetBool("playerDie", true);

                        onPlayerDie?.Invoke(currentHP);     // 플레이어가 죽었다고 델리게이트로 알림
                        Debug.Log("플레이어 사망");

                        // 사망 연출 실행 부분
                        rb.velocity = Vector3.zero;
                        //rb.Sleep();

                        ResetTrigger();
                        animator.SetTrigger("Die");

                        StartCoroutine(PlayerDieCoroutine());       // 플레이어 사망 후 씬 이동시 처리 부분
                        OnDisable();
                    }
                }

                heartPanel.UpdateHearts(currentHP);
            }
        }
    }

    // Player_Test 클래스 상단 변수 선언부에 추가
    private bool isInvincible = false;

    /// <summary>
    /// 무적 연출 코루틴
    /// </summary>
    /// <param name="duration"></param>
    /// <returns></returns>
    private IEnumerator BecomeInvincible(float duration)
    {
        isInvincible = true;
        Debug.Log("무적 시작");

        // 시각적 효과 추가
        if (shield != null)
        {
            shield.gameObject.SetActive(true);

            yield return new WaitForSeconds(duration);

            shield.gameObject.SetActive(false);
        }
        Debug.Log("무적 종료");
        isInvincible = false;
    }

    /// <summary>
    /// 플레이어의 최대 배부름
    /// public 이기 때문에 인스펙터에서 바꿔야 의미 있음
    /// </summary>
    public float maxFullness = 100;

    /// <summary>
    /// 플레이어의 현재 배부름이 변경했음을 알리는 델리게이트
    /// </summary>
    public Action<float> onPlayerFullnessChange;

    /// <summary>
    /// 플레이어의 현재 배부름 수치
    /// </summary>
    private float currentFullness;

    public float Fullness
    {
        get => currentFullness;
        set
        {
            if (currentFullness != value)
            {
                currentFullness = Mathf.Clamp(value, 0, maxFullness);

                //Debug.Log($"플레이어의 남은 배부름: {Fullness}");

                // % 경우 계산
                float Percent70 = maxFullness * 0.7f;
                float Percent50 = maxFullness * 0.5f;
                float Percent30 = maxFullness * 0.3f;
                float Percent10 = maxFullness * 0.1f;

                // ---------- 버프 로직 ----------

                // 만약 배부름이 70% 초과이면
                if (currentFullness > Percent70)
                {
                    // 버프 추가
                    if (!isBuff)
                    {
                        ApplyBuff();
                    }
                }
                else
                {
                    // 버프 제거
                    if (isBuff)
                    {
                        RemoveBuff();
                    }
                }

                // ---------- 디버프 로직 ----------

                if(currentFullness < Percent10)
                {
                    ApplyDeBuff(1);
                }
                else if(currentFullness < Percent30)
                {
                    ApplyDeBuff(3);
                }
                else if(currentFullness < Percent50)
                {
                    ApplyDeBuff(5);
                }
                // 모든 디버프 조건(50% 미만)에 해당하지 않을 때
                else
                {
                    // 모든 디버프 제거
                    // 만약 버프 중이면 그것도 고려해야 함
                    RemoveDeBuff();
                }

                // 적절한 패널로 수정 필요
                onPlayerFullnessChange?.Invoke(Fullness);
            }
        }
    }

    private const float BuffSpeedMultiplier = 1.25f;      // 이동 속도 25% 증가
    private const float Debuff_C_SpeedMultiplier = 0.9f;   // 이동 속도 10% 감소
    private const float Debuff_B_SpeedMultiplier = 0.75f;  // 이동 속도 25% 감소
    private const float Debuff_A_SpeedMultiplier = 0.5f;   // 이동 속도 50% 감소
    private const float Debuff_A_AttackMultiplier = 0.5f;  // 공격력 50% 감소

    private bool isBuff = false;

    /// <summary>
    /// 배부름 게이지 초당 감소량
    /// 🔽 난이도별 적당한 감소 속도 제안
    /// 난이도 추천 배부름 감소(n/sec)
    /// Easy	    0.25/sec 배부름 스트레스를 거의 느끼지 않고 탐험
    /// Normal	    0.3/sec 일반적인 플레이 감
    /// Hard	    0.35/sec 자원 관리 체감 시작
    /// Nightmare	0.4/sec 디버프 구간을 자주 경험
    /// Hell	    0.45/sec 식량이 중요한 난이도 공략 요소가 됨
    /// 0.172f 는 10분에 배부름 게이지가 다 떨어지는 속도
    /// </summary>
    private float FullnessDrainRate = 0.172f;

    /// <summary>
    /// 버프를 적용하는 함수
    /// </summary>
    private void ApplyBuff()
    {
        // 기본 이동 속도에 배율 적용
        moveSpeed = defaultMoveSpeed * BuffSpeedMultiplier;
        isBuff = true;
        Debug.Log($"버프 적용: 이동 속도 {moveSpeed}로 증가");
    }

    /// <summary>
    /// 버프를 제거하는 함수
    /// </summary>
    private void RemoveBuff()
    {
        // 이동 속도를 기본값으로 복구
        moveSpeed = defaultMoveSpeed;
        isBuff = false;
        Debug.Log("버프 제거: 이동 속도 기본값으로 복구");
    }

    /// <summary>
    /// 디버프를 적용하는 함수
    /// </summary>
    private void ApplyDeBuff(int index)
    {
        switch (index)
        {
            case 1:
                moveSpeed = defaultMoveSpeed * Debuff_A_SpeedMultiplier;                    // 이동 속도 감소
                playerAttackPower = playerBaseAttackPower * Debuff_A_AttackMultiplier;      // 공격력 감소
                //Debug.Log($"디버프 A 적용: 속도/공격력 감소. 속도: {moveSpeed}, 공격력: {playerAttackPower}");
                break;

            case 3:
                moveSpeed = defaultMoveSpeed * Debuff_B_SpeedMultiplier;
                // 공격력 변화 없음 (ResetAttackPower()에 의해 기본값 유지)
                //Debug.Log($"디버프 B 적용: 속도 감소. 속도: {moveSpeed}");
                break;

            case 5:
                moveSpeed = defaultMoveSpeed * Debuff_C_SpeedMultiplier;
                // 공격력 변화 없음
                //Debug.Log($"디버프 C 적용: 약한 속도 감소. 속도: {moveSpeed}");
                break;

            default:
                //Debug.LogError($"ApplyDeBuff: 알 수 없는 인덱스 ({index})");
                break;
        }
    }

    /// <summary>
    /// 디버프를 제거하는 함수
    /// </summary>
    private void RemoveDeBuff()
    {
        if (isBuff)
        {
            // 버프 중이면 애초에 피가 70% 이상이기 때문에 제거할 일이 없다
            return;
        }
        else
        {
            // 공격력 원위치
            playerAttackPower = playerBaseAttackPower;

            // 이동 속도 원위치
            moveSpeed = defaultMoveSpeed;
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

    private float playerBaseAttackPower;

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
    /// 불 장판 데미지를 받을 수 있는지 확인하는 변수
    /// </summary>
    public bool canFireDamage = true;

    /// <summary>
    /// 공격 범위 확인용
    /// </summary>
    GameObject attackRange;

    /// <summary>
    /// 공격 중 여부 true : 공격 중, false : 공격 중 아님
    /// </summary>
    public bool isAttacking = false;

    /// <summary>
    /// 방어 중 여부 true : 방어 중, false : 방어 중 아님
    /// </summary>
    public bool isGuard = false;
    
    /// <summary>
    /// 패링 여부 true : 패링 성공, false : 패링 실패
    /// </summary>
    public bool isParrying = false;

    /// <summary>
    /// 현재 가드가 가능한 상태인지 true : 가드 가능, false : 가드 불가능
    /// </summary>
    bool isGuardAble = false;

    /// <summary>
    /// 몇 초 사이에 가드를 해야 패링이 되는지
    /// </summary>
    public float parryingTimerate = 1f;

    /// <summary>
    /// 플레이어가 인벤토리를 열었는지 확인하는 bool 변수
    /// </summary>
    private bool isInventoryOpen = false;

    /// <summary>
    /// 인벤토리 패널
    /// </summary>
    private GameObject inventoryPanel;

    /// <summary>
    /// 플레이어의 최대 경험치
    /// 현재 경험치가 최대 경험치보다 크거나 같게 되면 레벨업 하고 최대 경험치량 증가
    /// </summary>
    //private float maxXP = 100f;
    private float maxXP;

    public float MaxXP
    {
        get => maxXP;
        set
        {
            if (maxXP != value)
            {
                maxXP = value;

                Debug.Log($"플레이어의 현재 레벨업에 필요한 경험치 : {MaxXP}");
                onPlayerMaxXPChange?.Invoke(MaxXP);
            }
        }
    }

    /// <summary>
    /// 레벨업 시 필요 경험치가 올라가는 양
    /// </summary>
    public float xpGrowthRate = 20f;

    /// <summary>
    /// 플레이어의 현재 경험치
    /// </summary>
    private float currentXP;

    public float XP
    {
        get => currentXP;
        set
        {
            if(currentXP != value)
            {
                currentXP = value;
                //currentXP = Mathf.Clamp(value, 0, maxXP);

                Debug.Log($"플레이어의 현재 경험치: {XP}");

                /*if(currentXP >= maxXP)
                {
                    // 남은 경험치는 누적
                    currentXP = currentXP - maxXP;

                    onPlayerLevelUP?.Invoke(currentXP);
                    Debug.Log("플레이어 레벨업!");
                }*/

                // 대량의 경험치를 얻어서 레벨업이 연속으로 되도록 변경
                while (currentXP >= MaxXP)
                {
                    currentXP -= MaxXP;         // 남은 경험치 누적
                    MaxXP += xpGrowthRate;      // 레벨업 필요 경험치량 증가
                    Level++;                    // 레벨 증가
                    onPlayerLevelUP?.Invoke(Level);     // 델리게이트

                    StatePoint++;
                    Debug.Log("플레이어 레벨업!");
                }

                onPlayerXPChange?.Invoke(currentXP);
            }
        }
    }

    /// <summary>
    /// 플레이어가 레벨업 했음을 알리는 델리게이트
    /// </summary>
    public Action<int> onPlayerLevelUP;

    /// <summary>
    /// 플레이어의 현재 경험치가 변경했음을 알리는 델리게이트
    /// </summary>
    public Action<float> onPlayerXPChange;

    /// <summary>
    /// 플레이어의 최대 경험치가 변경했음을 알리는 델리게이트
    /// </summary>
    public Action<float> onPlayerMaxXPChange;

    /// <summary>
    /// 스프라이트 렌더러
    /// </summary>
    SpriteRenderer spriteRenderer;

    /// <summary>
    /// 중복 방지를 위한 코루틴 핸들
    /// </summary>
    private Coroutine hitFlashCoroutine;

    /// <summary>
    /// 플레이어의 피격 범위 콜라이더
    /// </summary>
    BoxCollider2D box2D;

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
    public bool canEnterDoor = false;

    /// <summary>
    /// 문의 중앙 위치
    /// </summary>
    private Transform doorCenter;

    /// <summary>
    /// 문까지 걸어가는 bool 변수
    /// </summary>
    public bool isWalkingToDoor = false;

    /// <summary>
    /// 타겟 문의 위치
    /// </summary>
    private Vector3 targetDoorPosition;

    // 문 및 열쇠 관련 끝 --------------------------------------------------

    // 애니메이션의 이름을 해쉬로 변환 시작 --------------------------------------------------

    // 애니메이션 이름을 Hash로 변환
    /*int idleHash = Animator.StringToHash("Idle");
    int jumpHash = Animator.StringToHash("Jump");
    int RunHash = Animator.StringToHash("Run");
    int dashHash = Animator.StringToHash("Dash");
    int HangHash = Animator.StringToHash("Hang");
    int attackHash = Animator.StringToHash("Attack");
    int dashAttackHash = Animator.StringToHash("DashAttack");
    int guardHash = Animator.StringToHash("Guard");
    int parryingHash = Animator.StringToHash("Parrying");*/

    // 애니메이션의 이름을 해쉬로 변환 끝 --------------------------------------------------

    /// <summary>
    /// 씬을 이동하라고 알리는 델리게이트
    /// </summary>
    public Action<int> onSceneChange;

    /// <summary>
    /// 플레이어가 시작할 준비가 되었는지 확인하는 bool 변수
    /// </summary>
    public bool isPlayerReady = false;

    private void Awake()
    {
        var others = FindObjectsOfType<Player_Test>();
        if (others.Length > 1)
        {
            // 이미 다른 인스턴스가 존재하면 자신을 파괴하고 초기화 중단
            Destroy(gameObject);
            return;
        }

        /*var players = FindObjectsOfType<Player_Test>();

        foreach (var player in players)
        {
            // 이미 다른 인스턴스가 존재하면 그 인스턴스를 파괴하고 자신은 남음
            if (player != this)
            {
                Destroy(player.gameObject);
            }
        }*/

        // 씬 전환 시 이 게임오브젝트가 파괴되지 않도록 설정
        DontDestroyOnLoad(gameObject);

        inputActions = new PlayerInputActions();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        // 리지드 바디의 충돌 감지 모드를 연속으로 변경
        // 기존 값은 느린 속도에서는 충돌을 잘 감지하지만,빠르게 움직이면(대쉬) 한 프레임에 오브젝트가 벽을 통과(터널링)할 수 있음

        spriteRenderer = GetComponent<SpriteRenderer>();
        currentHP = maxHP;

        box2D = GetComponent<BoxCollider2D>();

        // 기본 공격력 초기화
        playerBaseAttackPower = playerAttackPower;

        // 기본 속도 초기화
        defaultMoveSpeed = moveSpeed;

        // 배부름 초기화
        Fullness = maxFullness;

        shield = transform.GetChild(3).gameObject;
        shield.gameObject.SetActive(false);
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
        //inputActions.Actions.Guard.performed += OnGuard;
        inputActions.Actions.Guard.started += OnGuard;
        inputActions.Actions.Guard.canceled += OnGuard;
        inputActions.Actions.Inventory.performed += OnInventory;

        Transform child = transform.GetChild(0);
        attackRange = child.gameObject;

        if( attackRange != null )
        {
            attackRange.SetActive(false);
        }
    }

    private void Start()
    {
        //inventoryPanel = GameObject.Find("InventoryPanel");
        inventoryPanel = FindAnyObjectByType<InventoryPanel>().gameObject;
        heartPanel = FindAnyObjectByType<HeartPanel>();

        /*GameManager gameManager = GameManager.Instance;
        if (gameManager.gameDifficulty == GameDifficulty.Easy)
        {

        }*/
        
        // 배부름 게이지 초당 감소량
        // 🔽 난이도별 적당한 감소 속도 제안
        // 난이도 추천 배부름 감소(n/sec)
        // Easy	        0.25/sec 배부름 스트레스를 거의 느끼지 않고 탐험    약 6분 40초
        // Normal	    0.3/sec 일반적인 플레이 감                          약 5분 33초
        // Hard	        0.35/sec 자원 관리 체감 시작                        약 4분 45초
        // Nightmare	0.4/sec 디버프 구간을 자주 경험                     약 4분 10초
        // Hell	        0.45/sec 식량이 중요한 난이도 공략 요소가 됨        약 3분 42초
        // 0.167f 는 9분 58초 에 배부름 게이지가 다 떨어지는 속도
        switch (GameManager.Instance.gameDifficulty)
        {
            case GameDifficulty.Easy:
                FullnessDrainRate = 0.25f;
                break;
            case GameDifficulty.Normal:
                FullnessDrainRate = 0.3f;
                break;
            case GameDifficulty.Hard:
                FullnessDrainRate = 0.35f;
                break;
            case GameDifficulty.Nightmare:
                FullnessDrainRate = 0.4f;
                break;
            case GameDifficulty.Hell:
                FullnessDrainRate = 0.45f;
                break;
        }
    }

    private void OnDisable()
    {
        inputActions.Actions.Inventory.performed -= OnInventory;
        inputActions.Actions.Guard.canceled -= OnGuard;
        inputActions.Actions.Guard.started -= OnGuard;
        //inputActions.Actions.Guard.performed -= OnGuard;
        inputActions.Actions.Attack.performed -= OnAttack;
        inputActions.Actions.DoorInteract.performed -= OnDoorInteract;
        inputActions.Actions.Dash.performed -= OnDash;
        inputActions.Actions.Jump.performed -= OnJump;
        inputActions.Actions.Move.canceled -= OnMove;
        inputActions.Actions.Move.performed -= OnMove;
        inputActions.Actions.Disable();
    }

    /// <summary>
    /// 가드 함수
    /// </summary>
    /// <param name="context"></param>
    private void OnGuard(InputAction.CallbackContext context)
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        
        // 만약 현재 애니메이션이
        if (stateInfo.IsName("Idle"))
        {
            isGuardAble = true;
        }
        else if (stateInfo.IsName("Jump"))
        {
            isGuardAble = false;
        }
        else if (stateInfo.IsName("Run"))
        {
            isGuardAble = true;
        }
        else if (stateInfo.IsName("Dash"))
        {
            isGuardAble = false;
        }
        else if (stateInfo.IsName("Edge-Idle"))
        {
            isGuardAble = false;
        }
        else if (stateInfo.IsName("Edge-Grab"))
        {
            isGuardAble = false;
        }
        else if (stateInfo.IsName("Attack"))
        {
            isGuardAble = false;
            /*moveSpeed = defaultMoveSpeed;
            attackRange.SetActive(false);
            isAttacking = false;        // 공격 종료

            isGuardAble = true;*/
        }
        else if (stateInfo.IsName("Dash-Attack"))
        {
            isGuardAble = false;
            /*moveSpeed = defaultMoveSpeed;
            attackRange.SetActive(false);
            isAttacking = false;        // 공격 종료

            isGuardAble = true;*/
        }
        else
        {
            
        }

        // 가드가 가능한 상태면
        if (isGuardAble)
        {
            switch (context.phase)
            {
                case InputActionPhase.Started:          // 가드 활성화
                    if (!isGuard)
                    {
                        // 입력이 시작된 시점
                        rb.velocity = Vector3.zero;     // 기존에 가해지던 힘 제거
                        isGuard = true;                 // 방어 시작
                        ResetTrigger();
                        animator.SetTrigger("Guard");

                        // 데미지 감소 부분()

                        StartCoroutine(OnParrying());
                    }
                    break;

                case InputActionPhase.Canceled:         // 가드 비활성화

                    if (isGuard)
                    {
                        // 입력이 취소된 시점
                        isGuard = false;        // 방어 종료
                        isGuardAble = false;    // 가드 가능 초기화
                        Debug.Log("가드 입력 종료");
                        ResetTrigger();
                        animator.SetTrigger("Idle");
                    }
                    break;

                default:
                    break;
            }
        }
    }

    /// <summary>
    /// 가드 후 몇 초 동안 패링되게 할지 결정하는 코루틴
    /// </summary>
    /// <returns></returns>
    private IEnumerator OnParrying()
    {
        isParrying = true;

        Debug.Log($"패링 시간 시작");

        /*int startFrame = Time.frameCount;  // 현재 프레임 저장

        while (Time.frameCount < startFrame + parryingTimerate)  // n프레임이 지나면 종료
        {
            // 현재 프레임에서 시작 프레임을 뺀 값이 경과된 프레임 수
            Debug.Log($"{Time.frameCount - startFrame} 프레임 경과");

            yield return null;  // 매 프레임마다 확인
        }

        // n프레임이 지나면 실행되는 코드
        //Debug.Log($"{parryingTimerate} 프레임이 지났다!");*/

        float timeElapsed = 0f;     // 시간 경과
        int secondsPassed = 0;      // 초 단위로 경과 시간을 저장

        while (secondsPassed < parryingTimerate)        // parryingTimerate가 될 때까지 반복
        {
            timeElapsed += Time.deltaTime;              // 각 프레임마다 흐른 시간을 더함

            // 1초마다 정수 단위로 출력
            if (timeElapsed >= 1f)
            {
                secondsPassed++;
                Debug.Log($"경과 시간: {secondsPassed}초");
                timeElapsed -= 1f;
            }

            yield return null;      // 한 프레임을 기다림
        }

        isParrying = false;

        Debug.Log($"패링 시간 종료");
    }

    private void Update()
    {
        if (playerDie) return;

        // 현재 게임 상태가 미궁 탐색 or 보스 전이면
        if (GameManager.Instance.GameState == GameState.MazeExploration || GameManager.Instance.GameState == GameState.BossRoom)
        {
            // 배부름이 0보다 클 때만 감소
            if (currentFullness > 0)
            {
                Fullness -= FullnessDrainRate * Time.deltaTime;
            }
        }
    }

    private void FixedUpdate()
    {
        if (playerDie) return;

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
        if (!isDash && !isGuard)
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

        // 플레이어가 가드 중이라면 낙하 애니메이션 재생 방지
        if (isGuard)
        {
            isFall = false;
            animator.SetBool("IsFall", false);
            return; // 가드 중일 때는 더 이상 낙하 로직을 진행하지 않음
        }

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
        // 로비에서는 움직임 불가
        if(GameManager.Instance.GameState != GameState.Lobby)
        {
            // 지금은 AD 로만 작동하는데 밧줄 같은거 넣을거면 WS 도 넣어야 할듯?
            moveInput = context.ReadValue<Vector2>();
            animator.speed = 1f;                        // 애니메이션 재생

            // 가드 중이 아닐때만
            if (!isGuard)
            {
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
        }
    }

    /// <summary>
    /// 인풋 시스템으로 플레이어의 점프를 제어하는 함수
    /// </summary>
    /// <param name="context"></param>
    private void OnJump(InputAction.CallbackContext context)
    {
        // 공격 가드 중 에는 점프 불가
        if (isAttacking || isGuard)
        {
            return;
        }

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

        // 공격 가드 중 에는 점프 불가
        if (isAttacking || isGuard)
        {
            return;
        }

        if (isDashAble)
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

        if (!isGuard)
        {
            //isAttacking = true;         // 공격 시작

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
    }

    public Action<bool> onInventoryToggle;

    /// <summary>
    /// 인풋 시스템으로 플레이어의 인벤토리를 제어하는 함수
    /// </summary>
    /// <param name="context"></param>
    private void OnInventory(InputAction.CallbackContext context)
    {
        // 인벤토리 상태를 토글
        isInventoryOpen = !isInventoryOpen;

        onInventoryToggle?.Invoke(isInventoryOpen);

        /*inventoryPanel.SetActive(isInventoryOpen);
        Debug.Log("인벤토리 상태: " + (isInventoryOpen ? "열림" : "닫힘"));*/
    }

    /// <summary>
    /// 데미지를 적용시키는 함수
    /// </summary>
    /// <param name="damage"></param>
    public void OnPlayerApplyDamage(float damage)
    {
        //Debug.Log("OnPlayerApplyDamage 함수가 호출");

        // 패링에 성공했으면
        if(isParrying)
        {
            Debug.Log("패링 성공!!!");

            ResetTrigger();
            animator.SetTrigger("Parrying");

            damage = 0f;
        }

        // 가드를 안했으면
        if (!isGuard)
        {
            //Debug.Log("가드 없이 플레이어 체력 감소");
            // 플레이어의 1 이상일 때만 데미지 적용
            if(HP > 1)
            {
                HP -= damage;

                // 플레이어의 이미지 깜빡깜빡 필요
                StartHitFlash();
            }
        }

        // 가드를 했으면
        else if (isGuard)
        {
            /*Debug.Log("가드 성공으로 데미지 50% 감소");
            Debug.Log($"원래 데미지 : {damage}");*/

            damage = damage / 2;

            //Debug.Log($"감소한 데미지 : {damage}");

            HP -= damage;

            // 플레이어의 이미지 깜빡깜빡 필요
            StartHitFlash();
        }

        else
        {
            Debug.Log("OnPlayerApplyDamage 에서 가드 버그");
        }
        //Debug.Log($"몬스터에게 공격받음! 남은 HP: {HP}");
    }

    /// <summary>
    /// 불 장판 적용 데미지
    /// </summary>
    /// <param name="damage"></param>
    public void OnPlayerApplyFireFloorDamage(float damage)
    {
        if (canFireDamage)
        {
            if (HP > 1)
            {
                canFireDamage = false;
                StartCoroutine(CanFireDamage());
                HP -= damage;

                // 플레이어의 이미지 깜빡깜빡 필요
                StartHitFlash();
            }
        }
    }

    /// <summary>
    /// 불장판 데미지 bool 변수 컨트롤하는 코루틴
    /// </summary>
    /// <returns></returns>
    IEnumerator CanFireDamage()
    {
        yield return new WaitForSeconds(1);
        canFireDamage = true;
    }

    /// <summary>
    /// 플레이어가 짧은 시간에 여러 번 공격을 받으면 코루틴이 중복 실행되기 때문에 우회하는 함수
    /// 색상 깜빡임이 꼬이거나, 마지막에 원래 색으로 복귀 안 되는 문제가 생길 수도 있음
    /// </summary>
    private void StartHitFlash()
    {
        if (hitFlashCoroutine != null)
            StopCoroutine(hitFlashCoroutine);

        hitFlashCoroutine = StartCoroutine(HitFlashRoutine());
    }
    
    private IEnumerator HitFlashRoutine()
    {
        Color original = Color.white;                           // 원래 (1, 1, 1)
        Color flash = new Color(1f, 0.5f, 0.5f);                // 바뀐 (1, 0.5, 0.5)
        float flashDuration = 1f;                               // 총 1초 동안
        int blinkCount = 3;                                     // 바↔원 세 번 반복
        float interval = flashDuration / (blinkCount * 2f);     // 각 색상 유지 시간

        for (int i = 0; i < blinkCount; i++)
        {
            spriteRenderer.color = flash;   // 바
            yield return new WaitForSeconds(interval);
            spriteRenderer.color = original; // 원
            yield return new WaitForSeconds(interval);
        }

        spriteRenderer.color = original; // 원상 복귀
        hitFlashCoroutine = null;
    }

    /// <summary>
    /// 공격 시작할 때 이동 속도 0으로 만드는 함수(애니메이터 이벤트용)
    /// </summary>
    private void AttackStart()
    {
        moveSpeed = 0;
        isAttacking = true;         // 공격 시작
        attackRange.SetActive(true);
    }

    /// <summary>
    /// 공격이 끝났을 때 이동속도 되돌리는 함수(애니메이터 이벤트용)
    /// </summary>
    private void AttackEnd()
    {
        moveSpeed = defaultMoveSpeed;
        attackRange.SetActive(false);
        isAttacking = false;        // 공격 종료
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
        animator.ResetTrigger("Guard");
        animator.ResetTrigger("Parrying");
        animator.ResetTrigger("Die");
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
        animator.SetTrigger("Idle");        // 아마 Disable 한 상태라 안먹힐 거임

        yield return new WaitForSeconds(1.5f);

        // 게임매니저에서 처리하도록 수정
        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);       // 현재 실행 중인 씬 +1 로 이동

        // 현재 게임 상태가 미궁 탐색씬
        if(GameManager.Instance.GameState == GameState.MazeExploration)
        {
            // 보스 방으로 이동
            onSceneChange?.Invoke(3);
        }
        // 만약 게임 상태가 보스 룸씬이면
        else if(GameManager.Instance.GameState == GameState.BossRoom)
        {
            // 로비 씬2 으로 이동
            onSceneChange?.Invoke(4);

            // 이동 후 HP, 배부름 초기화
            
            HP = maxHP;
            Fullness = maxFullness;
        }
        
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        this.gameObject.transform.position = new Vector3(0,-9.05f, 0);
        //inputActions.Actions.Enable();        => 씬 이동 후 활성화하도록 변경
    }

    public void EnableFC()
    {
        inputActions.Actions.Enable();
    }

    public void DisableFC()
    {
        inputActions.Actions.Disable();
    }

    /// <summary>
    /// 플레이어 기절 코루틴 실행 함수
    /// </summary>
    public void StunPlayer()
    {
        StartCoroutine(StunCoroutine(1));
    }

    /// <summary>
    /// 플레이어를 기절 시키는 코루틴
    /// </summary>
    /// <param name="player"></param>
    /// <param name="duration"></param>
    /// <returns></returns>
    private IEnumerator StunCoroutine(float duration)
    {
        Debug.Log("플레이어 기절 시작!");

        inputActions.Actions.Disable(); // 조작 막기
        rb.velocity = Vector2.zero;

        /*// 만약 애니메이터 트리거가 있다면
        player_test.SendMessage("ResetTrigger", SendMessageOptions.DontRequireReceiver);
        animator.SetTrigger("Stun");*/

        yield return new WaitForSeconds(duration);

        inputActions.Actions.Enable();
        //animator.ResetTrigger("Stun");
        //animator.SetTrigger("Idle");

        Debug.Log("플레이어 기절 해제!");
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 땅과 충돌하면
        if (collision.gameObject.CompareTag("Ground"))
        {
            //Debug.Log("땅에 충돌!");

            // 점프 중이 아닐 때만 Idle/Run 트리거 실행
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (!stateInfo.IsName("Jump"))
            {
                if (moveInput.magnitude != 0)
                {
                    //Debug.Log("땅인데 움직임 있음");
                    ResetTrigger();
                    animator.SetTrigger("Run");
                }
                else
                {
                    //Debug.Log("땅인데 움직임 없음");
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
                        //Debug.Log("벽 위인데 움직임 있음");
                        ResetTrigger();
                        animator.SetTrigger("Run");
                    }
                    else
                    {
                        //Debug.Log("벽 위인데 움직임 없음");
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
                //Debug.Log("벽에는 정상 충돌함");
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
            }
            doorCenter = collision.transform;       // 문의 중앙은 충돌한 문의 위치
        }
    }

    /// <summary>
    /// 씬 이동으로 콜라이더를 컨트롤하는 함수
    /// </summary>
    public void OnColliderControll(bool onoff)
    {
        if(onoff)
        {
            box2D.enabled = true;
            isPlayerReady = true;
            rb.gravityScale = 1;
            canEnterDoor = false;       // 문과의 상호작용 초기화
            isWalkingToDoor = false;    // 문과의 초기화2
            hasAllKeys = false;         // 3
            keyCount = 0;               // 4
            moveSpeed = defaultMoveSpeed;
            isDashAble = true;
        }
        else
        {
            isDashAble = false;
            box2D.enabled = false;
            isPlayerReady = false;
            rb.gravityScale = 0;
        }
    }

    /// <summary>
    /// 플레이어의 속도와 위치를 조정하는 함수
    /// </summary>
    public  void ResetMotionAndPosition()
    {
        if (rb != null)
        {
            // 1. 선속도 (Linear Velocity, 일반적인 이동 속도/낙하 속도)를 0으로 설정
            // 각속도(Angular Velocity)와 함께, 오브젝트의 움직임(관성 포함)을 멈춥니다.
            // 요청하신 '낙하할 때 속도'는 이 '선속도'에 해당합니다.
            rb.velocity = Vector2.zero;
            rb.gravityScale = 0;
            jumpCount = 0;          // 점프 불가하도록
            isGround = false;       // 땅 판정이 있었더라도 취소
            isDashAble = false;

            // 2. 각속도 (Angular Velocity, 회전 속도)를 0으로 설정
            // 오브젝트의 회전 움직임을 멈춥니다.
            rb.angularVelocity = 0f;
        }

        // 3. 오브젝트의 위치(Position)를 (0, 0, 0)으로 설정 (2D에서는 Z축 무시)
        this.transform.position = Vector3.zero;

        Debug.Log("플레이어의 움직임과 위치가 초기화");
    }

    /// <summary>
    /// 플레이어가 죽고 하는 행동들
    /// </summary>
    /// <returns></returns>
    IEnumerator PlayerDieCoroutine()
    {
        yield return new WaitForSeconds(3);

        // 로비 씬2 으로 이동
        onSceneChange?.Invoke(4);

        // 이동 후에 플레이어 트리거 초기화
        ResetTrigger();
        animator.SetTrigger("Idle");

        playerDie = false;
        animator.SetBool("playerDie", false);


        ResetMotionAndPosition();
        moveSpeed = 0;

        HP = maxHP;                 // 체력 초기화
        Fullness = maxFullness;     // 배부름 초기화

        OnEnable();
    }
}
