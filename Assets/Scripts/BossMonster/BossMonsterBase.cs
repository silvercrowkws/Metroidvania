using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

/// <summary>
/// 게임 난이도에 따른 보스 종류
/// </summary>
public enum BossType
{
    None = 0,

    EasyBoss,           // 시작 시 가로로 레이저를 쏘고 시계 방향으로 회전(레이저에 맞으면 피 2개 깎기 : 20)

    NormalBoss,         // 시작 시 가로로 레이저를 쏘고 시계 방향으로 회전(레이저에 맞으면 피 3개 깎기 : 30)
                        // 및 위에서 오브젝트 낙하

    HardBoss,           // 시작 시 가로로 레이저를 쏘고 시계 방향으로 회전
                        // 및 위에서 오브젝트 낙하(에 맞으면 잠시 못움직이게 기절)
                        // 및 레이저 맞으면 피 5개 깎기 : 50

    NightmareBoss,      // 시작 시 X 모양으로 레이저를 쏘고 시계 방향으로 회전 중간중간 방향 변경
                        // 및 위에서 오브젝트 낙하(에 맞으면 잠시 못움직이게 기절)
                        // 및 레이저 맞으면 피 10개 깎기 : 100
                        // 및 맵 전역을 튕기는 오브젝트 추가(각도는 시작시 조절)

    HellBoss,           // 시작 시 X 모양으로 레이저를 쏘고 시계 방향으로 회전 중간중간 방향 변경
                        // 및 위에서 오브젝트 낙하(에 맞으면 잠시 못움직이게 기절)
                        // 및 레이저 맞으면 즉사
                        // 및 맵 전역을 튕기는 오브젝트 추가(각도는 시작시 조절)
                        // 튕기는 오브젝트가 주기적으로 플레이어를 조준 및 바닥에 꽂히고 데미지를 주는 장판을 남긴다
                        // 그 후 새로운 튕기는 오브젝트 생성(장판이 사라지는 시점에 새로 꽂히도록 조절 필요)
}

public class BossMonsterBase : MonoBehaviour
{
    // 모든 보스가 공통 적으로 해야 할 일
    // 체력
    // 일정 시간 간격으로 공격을 할건데 패턴은 자식에서?

    public BossType bossType = BossType.None;

    /// <summary>
    /// 게임 매니저
    /// </summary>
    protected GameManager gameManager;

    /// <summary>
    /// 보스가 죽었을 때 주는 돈
    /// </summary>
    protected float bossDieMoney = 1.0f;

    /// <summary>
    /// 보스가 죽었을 때 주는 경험치
    /// </summary>
    protected float bossDieieXP = 1.0f;

    /// <summary>
    /// 몬스터의 최대 체력
    /// </summary>
    protected float maxHP = 1.0f;
    
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

                if(bossHealthSlider!= null)
                {
                    bossHealthSlider.value = currentHP / maxHP;
                }

                if (currentHP < 1)
                {
                    currentHP = 0;
                    
                    onBossDie?.Invoke();          // 보스 몬스터가 죽었다고 델리게이트로 알림
                    Debug.Log("보스 몬스터 사망");

                    //Destroy(gameObject);        // 게임 오브젝트 파괴
                }
            }
        }
    }

    /// <summary>
    /// 몬스터가 죽었다고 알리는 델리게이트
    /// </summary>
    public Action onBossDie;

    /// <summary>
    /// 몬스터가 죽었는지 확인하는 bool 변수
    /// </summary>
    protected bool isDead = false;

    /// <summary>
    /// 몬스터의 체력을 보여주기 위한 슬라이더
    /// </summary>
    Slider bossHealthSlider;

    /// <summary>
    /// 보스 몬스터의 공격력
    /// </summary>
    //protected float bossAttackPower = 1.0f;

    /// <summary>
    /// 적의 공격으로 플레이어에게 데미지를 적용시키는 함수
    /// </summary>
    public Action<float> onPlayerApplyDamage;

    /// <summary>
    /// 스프라이트 랜더러
    /// </summary>
    protected SpriteRenderer spriteRenderer;

    /// <summary>
    /// 플레이어의 트랜스폼
    /// </summary>
    public Transform playerTransform;

    /// <summary>
    /// 플레이어
    /// </summary>
    Player_Test player_test;

    /// <summary>
    /// 공격 받을 수 있는지 확인하는 bool 변수(공격 받은 후 일정 시간 동안 데미지를 입지 않도록)
    /// false : 공격 받을 수 있음, true : 공격 받을 수 없음
    /// </summary>
    private bool damageCoolDown = false;

    /// <summary>
    /// 공격할 목표 위치(Attack 함수가 호출된 시점의 playerTransform)
    /// </summary>
    private Vector3 targetPosition;

    /// <summary>
    /// 공격 중 여부
    /// </summary>
    public bool isAttacking = false;

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

    Rigidbody2D rb2d;

    /// <summary>
    /// horizontal 레이저 프리팹 원본
    /// </summary>
    private GameObject horizontalLaser;

    /// <summary>
    /// horizontal 레이저 오브젝트 생성
    /// </summary>
    protected GameObject horizontalLaserInstance;

    /// <summary>
    /// cross 레이저 프리팹 원본
    /// </summary>
    private GameObject crossLaser;

    /// <summary>
    /// cross 레이저 오브젝트 생성
    /// </summary>
    protected GameObject crossLaserInstance;

    /// <summary>
    /// 낙하 오브젝트의 배열
    /// </summary>
    private GameObject[] fallingObjects;

    /// <summary>
    /// bounceObject 프리팹 원본
    /// </summary>
    private GameObject bounceObject;

    /// <summary>
    /// bounceObject 오브젝트 생성
    /// </summary>
    protected GameObject bounceObjectInstance;

    /// <summary>
    /// 추적 미사일 오브젝트 프리팹
    /// </summary>
    private GameObject[] chaseMissileObject;

    /// <summary>
    /// 보스 몬스터 피격 처리용 콜라이더
    /// </summary>
    BoxCollider2D boxCollider2D;

    /// <summary>
    /// 낙하 오브젝트 부모
    /// </summary>
    GameObject fallingObjectParent;

    /// <summary>
    /// 추적 미사일 오브젝트 부모
    /// </summary>
    GameObject chaseMissileObjectParent;

    /// <summary>
    /// 바운스 오브젝트 부모
    /// </summary>
    GameObject bounceObjectParent;

    protected void Awake()
    {
        // Resources.Load는 리소스를 로드하는 메서드
        // <Sprite>는 로드할 에셋의 타입을 지정(Texture, AudioClip, GameObject 등 다른 타입도 있음)
        // "Sprites/sprite1" 이 부분은 로딩할 리소스의 경로
        // "Sprites"는 Resources 폴더 내에 있는 서브폴더
        // "sprite1"은 해당 폴더 내에 있는 에셋의 이름
        horizontalLaser = Resources.Load<GameObject>("GameObjects/HorizontalLaser");
        // 생성은 HorizontalLaserCoroutine 코루틴에서 함

        bounceObject = Resources.Load<GameObject>("GameObjects/BounceObject");

        crossLaser = Resources.Load<GameObject>("GameObjects/CrossLaser");

        // FallingObject_1 ~ FallingObject_4 불러오기
        fallingObjects = new GameObject[4];
        for (int i = 0; i < 4; i++)
        {
            string path = $"GameObjects/FallingObject_{i + 1}";
            fallingObjects[i] = Resources.Load<GameObject>(path);

            if (fallingObjects[i] == null)
            {
                Debug.LogWarning($"{path} 프리팹을 찾을 수 없습니다!");
            }
            else
            {
                Debug.Log($"{path} 로드 성공!");
            }
        }

        chaseMissileObject = new GameObject[4];
        for (int i = 0; i < 4; i++)
        {
            string path = $"GameObjects/ChaseMissile_{i + 1}";
            chaseMissileObject[i] = Resources.Load<GameObject>(path);

            if (chaseMissileObject[i] == null)
            {
                Debug.LogWarning($"{path} 프리팹을 찾을 수 없습니다!");
            }
            else
            {
                Debug.Log($"{path} 로드 성공!");
            }
        }

        boxCollider2D = GetComponent<BoxCollider2D>();
    }

    protected virtual void OnEnable()
    {
        // 초기화
        isDead = false;
        damageCoolDown = false;
        //maxHP = HP;   => 세분화된 BossMonster_1 같은 곳에서 처리됨

        gameManager = GameManager.Instance;
        
        player_test = gameManager.Player_Test;
        playerTransform = player_test.transform;
        onPlayerApplyDamage += player_test.OnPlayerApplyDamage;     // => 레이저는 패링 안되도록 조치 필요

        spriteRenderer = GetComponent<SpriteRenderer>();

        // 스프라이트 색상(RGB, 알파) 초기화
        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color(1f, 1f, 1f, 1f);
            originalColor = spriteRenderer.color;
        }

        onBossDie += OnBossDie;

        Transform child = transform.GetChild(0);

        bossHealthSlider = child.GetChild(0).GetComponent<Slider>();

        rb2d = GetComponent<Rigidbody2D>();

        // 슬라이더 초기화
        bossHealthSlider.value = 1;
    }

    protected virtual void OnDisable()
    {
        bossHealthSlider.value = 1;                       // 체력 슬라이더 조정
        onBossDie -= OnBossDie;
    }

    protected virtual void Start()
    {
        StartAttackPattern();
    }

    /// <summary>
    /// 보스가 죽었을 때 실행될 함수
    /// </summary>
    private void OnBossDie()
    {
        // 여기서 경험치, 돈, 아이템등 추가 필요

        // 보스 몬스터가 죽었으니 코루틴 모두 정지 시키고
        // 정지되는 것: 레이저 회전, 미사일 재생성
        StopAllCoroutines();

        // 레이저 파괴 부분
        StartCoroutine(DestroyLaser());

        // 이미 생성된 미사일, 낙하 오브젝트, 바운스 오브젝트 파괴하는 부분
        if (fallingObjectParent != null)
        {
            Destroy(fallingObjectParent);
            Debug.Log("낙하 오브젝트 전체 파괴");
        }

        if (bounceObjectParent != null)
        {
            Destroy(bounceObjectParent);
            Debug.Log("튕기는 오브젝트 전체 파괴");
        }

        if (chaseMissileObjectParent != null)
        {
            Destroy(chaseMissileObjectParent);
            Debug.Log("추적 미사일 전체 파괴");
        }

        // 보스 사망 연출 부분
    }

    IEnumerator DestroyLaser()
    {
        // onBossDie 델리게이트로 Laser 클래스도 받아서 데미지 0으로 바꾸는 부분있는데,
        // 먼저 파괴가 되어버릴 지 몰라서 0.1초 늦춤

        yield return new WaitForSeconds(0.1f);

        // 가로 레이저가 있으면
        if (horizontalLaserInstance != null)
        {
            Debug.Log("가로 레이저 파괴");
            Destroy(horizontalLaserInstance);
        }

        // 세로 레이저가 있으면
        else if (crossLaserInstance != null)
        {
            Debug.Log("세로 레이저 파괴");
            Destroy(crossLaserInstance);
        }
    }

    /// <summary>
    /// 보스 패턴 실행 함수
    /// </summary>
    protected void StartAttackPattern()
    {
        // 레이저 충돌 시 데미지는 Laser 클래스에서 처리

        // 생성되고 플레이어가 준비되기를 기다리는 시간이 필요
        StartCoroutine(YieldPlayer());

        // 시간 조절을 위해 YieldPlayer 코루틴에서 하도록 변경
        /*switch(bossType)
        {
            // EasyBoss의 경우
            case BossType.EasyBoss:
                // 시작 시 가로로 레이저를 쏘고 시계 방향으로 회전(레이저에 맞으면 피 2개 깎기) => Laser 클래스에서 처리
                StartCoroutine(HorizontalLaserCoroutine(15));
                break;

            // NormalBoss의 경우
            case BossType.NormalBoss:
                // 시작 시 가로로 레이저를 쏘고 시계 방향으로 회전(레이저에 맞으면 피 3개 깎기 : 30)
                StartCoroutine(HorizontalLaserCoroutine(15));

                // 및 위에서 오브젝트 낙하
                StartCoroutine(FallingObjectCoroutine());
                break;

            // HardBoss의 경우
            case BossType.HardBoss:
                // 시작 시 가로로 레이저를 쏘고 시계 방향으로 회전 중간중간 방향 변경
                // 및 레이저 맞으면 피 5개 깎기 : 50
                StartCoroutine(HorizontalLaserCoroutine(15));

                // 및 위에서 오브젝트 낙하(에 맞으면 잠시 못움직이게 기절)
                StartCoroutine(FallingObjectCoroutine());

                // 및 맵 전역을 튕기는 오브젝트 추가(각도는 시작시 조절)
                BounceObjectInstantiate();
                break;

            // NightmareBoss의 경우
            case BossType.NightmareBoss:
                // 시작 시 X 모양으로 레이저를 쏘고 시계 방향으로 회전 중간중간 방향 변경
                // 및 레이저 맞으면 피 8개 깎기 : 80
                StartCoroutine(CrossLaserCoroutine(15));

                // 및 위에서 오브젝트 낙하(에 맞으면 잠시 못움직이게 기절)
                StartCoroutine(FallingObjectCoroutine());

                // 및 맵 전역을 튕기는 오브젝트 추가(각도는 시작시 조절)
                BounceObjectInstantiate();

                // 일정 간격으로 플레이어를 조준 및 바닥에 꽂히고 데미지를 주는 장판을 남기는 오브젝트 추가
                // 그 후 새로운 튕기는 오브젝트 생성(장판이 사라지는 시점에 새로 꽂히도록 조절 필요)
                StartCoroutine(ChaseMissileCoroutine());
                break;

            // HellBoss의 경우
            case BossType.HellBoss:
                // 시작 시 X 모양으로 레이저를 쏘고 시계 방향으로 회전 중간중간 방향 변경
                // 및 레이저 맞으면 즉사
                StartCoroutine(CrossLaserCoroutine(15));

                // 및 위에서 오브젝트 낙하(에 맞으면 잠시 못움직이게 기절)
                StartCoroutine(FallingObjectCoroutine());

                // 및 맵 전역을 튕기는 오브젝트 추가(각도는 시작시 조절)
                // 여기서 오브젝트가 튕길때 마다 그 자리에 불 장판을 짧게 남기면 어떨까
                BounceObjectInstantiate();

                // 일정 간격으로 플레이어를 조준 및 바닥에 꽂히고 데미지를 주는 장판을 남기는 오브젝트 추가
                // 그 후 새로운 튕기는 오브젝트 생성(장판이 사라지는 시점에 새로 꽂히도록 조절 필요)
                StartCoroutine(ChaseMissileCoroutine());
                break;
        }*/
    }

    /// <summary>
    /// 가로 레이저 패턴
    /// </summary>
    /// <param name="speed">1초당 회전 각도(속도)</param>
    /// <returns></returns>
    IEnumerator HorizontalLaserCoroutine(float speed)
    {
        // 플레이어의 등장 연출이 있으면 그 만큼 기다리기
        //yield return new WaitForSeconds(5);

        if (horizontalLaser != null)
        {
            horizontalLaserInstance = Instantiate(horizontalLaser, transform);
            horizontalLaserInstance.transform.localPosition = Vector3.zero;         // 부모의 중심에 위치
            horizontalLaserInstance.transform.localRotation = Quaternion.identity;  // 회전을 0,0,0으로 설정
            //horizontalLaserInstance.SetActive(false);                             // 기본적으로 비활성화
        }
        else
        {
            Debug.Log("HorizontalLaser를 못찾았다!");
        }

        // 회전 방향 (1 = 시계 방향, -1 = 반시계 방향)
        int direction = 1;

        // 하드보스 여부
        bool isHardBoss = false;
        if (bossType == BossType.HardBoss)
        {
            isHardBoss = true;
        }

        // 시간 관련 변수
        float directionTimer = 0f;
        float nextDirectionChangeTime = 0f;

        // 하드보스라면 첫 랜덤 시간 설정
        if (isHardBoss)
        {
            nextDirectionChangeTime = UnityEngine.Random.Range(20f, 24f);
        }

        // 플레이어가 살아있는 동안 반복
        while (!player_test.playerDie)
        {
            //Debug.Log("레이저 회전 중");

            // 기본 회전 (시계 방향)
            horizontalLaserInstance.transform.Rotate(0f, 0f, -direction * speed * Time.deltaTime);

            // 하드보스일 경우에만 방향 전환 로직 작동
            if (isHardBoss)
            {
                directionTimer += Time.deltaTime;

                if (directionTimer >= nextDirectionChangeTime)
                {
                    // 회전 방향 반전
                    direction = direction * -1;

                    // 타이머 초기화
                    directionTimer = 0f;

                    // 다음 전환 시간 다시 랜덤 설정
                    nextDirectionChangeTime = UnityEngine.Random.Range(10f, 20f);
                    
                    //Debug.Log($"HardBoss 레이저 회전 방향 전환! (다음 전환까지 {nextDirectionChangeTime.ToString("F1")} 초");
                }
            }

            yield return null;
        }

        Debug.Log($"플레이어 사망으로 레이저 정지");
    }

    /// <summary>
    /// 크로스 레이저 패턴
    /// </summary>
    /// <param name="speed">1초당 회전 각도(속도)</param>
    /// <returns></returns>
    IEnumerator CrossLaserCoroutine(float speed)
    {
        // 플레이어의 등장 연출이 있으면 그 만큼 기다리기
        //yield return new WaitForSeconds(5);

        if (crossLaser != null)
        {
            crossLaserInstance = Instantiate(crossLaser, transform);
            crossLaserInstance.transform.localPosition = Vector3.zero;         // 부모의 중심에 위치
            crossLaserInstance.transform.localRotation = Quaternion.identity;  // 회전을 0,0,0으로 설정
        }
        else
        {
            Debug.Log("crossLaser를 못찾았다!");
        }

        // 회전 방향 (1 = 시계 방향, -1 = 반시계 방향)
        int direction = 1;

        // 시간 관련 변수
        float directionTimer = 0f;
        float nextDirectionChangeTime = 0f;

        // 첫 랜덤 시간 설정
        nextDirectionChangeTime = UnityEngine.Random.Range(10f, 20f);

        // 플레이어가 살아있는 동안 반복
        while (!player_test.playerDie)
        {
            //Debug.Log("레이저 회전 중");

            // 기본 회전 (시계 방향)
            crossLaserInstance.transform.Rotate(0f, 0f, -direction * speed * Time.deltaTime);
            
            directionTimer += Time.deltaTime;

            if (directionTimer >= nextDirectionChangeTime)
            {
                // 회전 방향 반전
                direction = direction * -1;

                // 타이머 초기화
                directionTimer = 0f;

                // 다음 전환 시간 다시 랜덤 설정
                nextDirectionChangeTime = UnityEngine.Random.Range(20f, 40f);
            }

            yield return null;
        }

        Debug.Log($"플레이어 사망으로 레이저 정지");
    }

    /// <summary>
    /// 낙하 오브젝트를 주기적으로 생성하는 코루틴
    /// </summary>
    /// <returns></returns>
    IEnumerator FallingObjectCoroutine()
    {
        //yield return null;

        fallingObjectParent = new GameObject("FallingObjectParent");
        fallingObjectParent.transform.SetParent(transform);         // 이 보스의 자식으로 설정
        fallingObjectParent.transform.localPosition = Vector3.zero; // 위치 초기화

        float spawnInterval = 1.5f; // 오브젝트 떨어뜨리는 간격

        // 플레이어가 살아있는 동안 반복
        while (!player_test.playerDie)
        {
            // 랜덤 위치 설정
            float randomX = UnityEngine.Random.Range(-15f, 15f);
            Vector3 spawnPos = new Vector3(randomX, 8f, 0f);

            // 랜덤한 낙하 오브젝트 선택
            int randomIndex = UnityEngine.Random.Range(0, fallingObjects.Length);
            GameObject prefab = fallingObjects[randomIndex];

            // 해당 낙하 오브젝트의 존재 확인 후 생성
            if (prefab != null)
            {
                GameObject obj = Instantiate(prefab, spawnPos, Quaternion.identity, fallingObjectParent.transform);
            }
            else
            {
                Debug.LogWarning($"fallingObjects[{randomIndex}] 프리팹이 null입니다!");
            }

            // 다음 스폰까지 대기
            yield return new WaitForSeconds(spawnInterval);
        }

        Debug.Log("플레이어 사망으로 낙하 오브젝트 생성 중단");
    }

    /// <summary>
    /// 튕기는 오브젝트 생성 함수
    /// </summary>
    private void BounceObjectInstantiate()
    {
        // 부모 오브젝트 생성 및 설정
        bounceObjectParent = new GameObject("BounceObjectParent");
        bounceObjectParent.transform.SetParent(transform);
        bounceObjectParent.transform.localPosition = Vector3.zero;

        int count = 0;
        switch (bossType)
        {
            case BossType.HardBoss:
                count = 1;
                break;
            case BossType.NightmareBoss:
                count = 2;
                break;
            case BossType.HellBoss:
                count = 3;
                break;
        }

        for (int i = 0; i < count; i++)
        {
            bounceObjectInstance = Instantiate(bounceObject, bounceObjectParent.transform);
            //bounceObjectInstance.transform.localPosition = new Vector2(0, (2 * i) + 2);   // 0, 4 6 8에 생성

            // x 좌표 계산
            float xOffset = 0f;

            if (count == 1)
            {
                xOffset = 0f;
            }
            else if (count == 2)
            {
                xOffset = (i == 0) ? -2f : 2f;
            }
            else if (count == 3)
            {
                xOffset = (i - 1) * 2f; // i=0→-2, i=1→0, i=2→2
            }

            bounceObjectInstance.transform.localPosition = new Vector2(xOffset, 1);   // 0, 2, -2에 생성
            bounceObjectInstance.transform.localRotation = Quaternion.identity;           // 회전을 0,0,0으로 설정
        }
    }

    /// <summary>
    /// 추적 미사일을 주기적으로 생성하는 패턴
    /// </summary>
    /// <returns></returns>
    private IEnumerator ChaseMissileCoroutine()
    {
        chaseMissileObjectParent = new GameObject("ChaseMissileObjectParent");
        chaseMissileObjectParent.transform.SetParent(transform);         // 이 보스의 자식으로 설정
        chaseMissileObjectParent.transform.localPosition = Vector3.zero; // 위치 초기화

        // 난이도에 따른 미사일 개수 및 스폰 간격 설정
        int missileCount = 0;
        float spawnInterval = 0f;

        Vector2[] spawnPos = new Vector2[4];

        /*spawnPos[0] = new Vector2(3f, 0f);
        spawnPos[1] = new Vector2(-3f, 0f);
        spawnPos[2] = new Vector2(0f, 3f);
        spawnPos[3] = new Vector2(0f, -3f);*/

        spawnPos[0] = new Vector2(2f, 2f);
        spawnPos[1] = new Vector2(2f, -2f);
        spawnPos[2] = new Vector2(-2f, 2f);
        spawnPos[3] = new Vector2(-2f, -2f);

        switch (bossType)
        {
            case BossType.NightmareBoss:                
                missileCount = 2;
                spawnInterval = 11f;
                break;
            case BossType.HellBoss:                
                missileCount = 4;
                spawnInterval = 7f;
                break;
            default:
                // 이 패턴을 사용하지 않는 보스는 바로 종료
                Debug.LogWarning($"BossType.{bossType}은 ChaseMissileCoroutine을 사용하지 않는데?");
                yield break;
        }

        // 플레이어가 살아있는 동안 반복
        while (!player_test.playerDie)
        {
            for (int i = 0; i < missileCount; i++)
            {
                // 미사일을 스폰 위치에 생성하고 부모에 연결
                GameObject obj = Instantiate(chaseMissileObject[i], spawnPos[i], Quaternion.identity, chaseMissileObjectParent.transform);
            }

            // 다음 스폰까지 대기
            yield return new WaitForSeconds(spawnInterval);
        }

        Debug.Log("플레이어 사망으로 낙하 오브젝트 생성 중단");
    }

    /// <summary>
    /// 플레이어가 준비될때까지 패턴을 실행하지 않고 기다리는 코루틴
    /// </summary>
    /// <returns></returns>
    IEnumerator YieldPlayer()
    {
        // 플레이어가 준비되지 않았으면 계속 반복
        while (!player_test.isPlayerReady)
        {
            yield return null;
        }

        // 빠져나오면 n초 후 패턴 시작
        yield return new WaitForSeconds(0.5f);
        Debug.Log("보스 패턴 실행");

        switch (bossType)
        {
            // EasyBoss의 경우
            case BossType.EasyBoss:
                // 시작 시 가로로 레이저를 쏘고 시계 방향으로 회전(레이저에 맞으면 피 2개 깎기) => Laser 클래스에서 처리
                StartCoroutine(HorizontalLaserCoroutine(15));
                break;

            // NormalBoss의 경우
            case BossType.NormalBoss:
                // 시작 시 가로로 레이저를 쏘고 시계 방향으로 회전(레이저에 맞으면 피 3개 깎기 : 30)
                StartCoroutine(HorizontalLaserCoroutine(15));

                // 및 위에서 오브젝트 낙하
                StartCoroutine(FallingObjectCoroutine());
                break;

            // HardBoss의 경우
            case BossType.HardBoss:
                // 시작 시 가로로 레이저를 쏘고 시계 방향으로 회전 중간중간 방향 변경
                // 및 레이저 맞으면 피 5개 깎기 : 50
                StartCoroutine(HorizontalLaserCoroutine(15));

                // 및 위에서 오브젝트 낙하(에 맞으면 잠시 못움직이게 기절)
                StartCoroutine(FallingObjectCoroutine());

                // 및 맵 전역을 튕기는 오브젝트 추가(각도는 시작시 조절)
                BounceObjectInstantiate();
                break;

            // NightmareBoss의 경우
            case BossType.NightmareBoss:
                // 시작 시 X 모양으로 레이저를 쏘고 시계 방향으로 회전 중간중간 방향 변경
                // 및 레이저 맞으면 피 8개 깎기 : 80
                StartCoroutine(CrossLaserCoroutine(15));

                // 및 위에서 오브젝트 낙하(에 맞으면 잠시 못움직이게 기절)
                StartCoroutine(FallingObjectCoroutine());

                // 및 맵 전역을 튕기는 오브젝트 추가(각도는 시작시 조절)
                BounceObjectInstantiate();

                // 일정 간격으로 플레이어를 조준 및 바닥에 꽂히고 데미지를 주는 장판을 남기는 오브젝트 추가
                // 그 후 새로운 튕기는 오브젝트 생성(장판이 사라지는 시점에 새로 꽂히도록 조절 필요)
                StartCoroutine(ChaseMissileCoroutine());
                break;

            // HellBoss의 경우
            case BossType.HellBoss:
                // 시작 시 X 모양으로 레이저를 쏘고 시계 방향으로 회전 중간중간 방향 변경
                // 및 레이저 맞으면 즉사
                StartCoroutine(CrossLaserCoroutine(15));

                // 및 위에서 오브젝트 낙하(에 맞으면 잠시 못움직이게 기절)
                StartCoroutine(FallingObjectCoroutine());

                // 및 맵 전역을 튕기는 오브젝트 추가(각도는 시작시 조절)
                // 여기서 오브젝트가 튕길때 마다 그 자리에 불 장판을 짧게 남기면 어떨까
                BounceObjectInstantiate();

                // 일정 간격으로 플레이어를 조준 및 바닥에 꽂히고 데미지를 주는 장판을 남기는 오브젝트 추가
                // 그 후 새로운 튕기는 오브젝트 생성(장판이 사라지는 시점에 새로 꽂히도록 조절 필요)
                StartCoroutine(ChaseMissileCoroutine());
                break;
        }
    }

    

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 만약 충돌한 대상이 플레이어의 공격 범위이고
        if (collision.CompareTag("AttackRange"))
        {
            // 데미지 쿨다운이 안돌아가고 있으면
            if (!damageCoolDown)
            {
                // 쿨타임 돌리고
                damageCoolDown = true;

                // 피격 처리
                Debug.Log("플레이어 공격에 맞음");

                if (player_test != null)
                {
                    HP -= player_test.playerAttackPower;
                    Debug.Log($"{this.gameObject.name}의 남은 HP: {HP}");
                }
                else
                {
                    Debug.LogWarning("플레이어 테스트도 없는데?");
                }

                StartCoroutine(DamageCooldown());
            }
        }
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
}
