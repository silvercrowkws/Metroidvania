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
                bossHealthSlider.value = currentHP / maxHP;
                if (currentHP < 1)
                {
                    currentHP = 0;
                    
                    onBossDie?.Invoke();          // 보스 몬스터가 죽었다고 델리게이트로 알림
                    //Destroy(gameObject);        // 게임 오브젝트 파괴
                    Debug.Log("보스 몬스터 사망");
                }
            }
        }
    }

    /// <summary>
    /// 몬스터가 죽었다고 알리는 델리게이트
    /// </summary>
    protected Action onBossDie;

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
    /// 레이저 프리팹 원본
    /// </summary>
    private GameObject horizontalLaser;

    /// <summary>
    /// horizontal 레이저 오브젝트 생성
    /// </summary>
    protected GameObject horizontalLaserInstance;

    /// <summary>
    /// 낙하 오브젝트의 배열
    /// </summary>
    private GameObject[] fallingObjects;

    /// <summary>
    /// bounceObject 프리팹 원본
    /// </summary>
    private GameObject bounceObject;

    /// <summary>
    /// bounceObject 레이저 오브젝트 생성
    /// </summary>
    protected GameObject bounceObjectInstance;

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
    }

    protected virtual void OnEnable()
    {
        // 초기화
        isDead = false;
        damageCoolDown = false;
        maxHP = currentHP;

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
        
    }

    /// <summary>
    /// 보스 패턴 실행 함수
    /// </summary>
    protected void StartAttackPattern()
    {
        // 레이저 충돌 시 데미지는 Laser 클래스에서 처리

        switch(bossType)
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
                // 시작 시 가로로 레이저를 쏘고 시계 방향으로 회전
                StartCoroutine(HorizontalLaserCoroutine(15));
                // 및 위에서 오브젝트 낙하(에 맞으면 잠시 못움직이게 기절)
                StartCoroutine(FallingObjectCoroutine());
                // 및 레이저 맞으면 피 5개 깎기 : 50
                // 및 맵 전역을 튕기는 오브젝트 추가(각도는 시작시 조절)
                BounceObjectInstantiate();
                break;

            // NightmareBoss의 경우
            case BossType.NightmareBoss:
                // 시작 시 X 모양으로 레이저를 쏘고 시계 방향으로 회전 중간중간 방향 변경
                // 및 위에서 오브젝트 낙하(에 맞으면 잠시 못움직이게 기절)
                // 및 레이저 맞으면 피 10개 깎기 : 100
                // 및 맵 전역을 튕기는 오브젝트 추가(각도는 시작시 조절)
                BounceObjectInstantiate();
                // 일정 간격으로 플레이어를 조준 및 바닥에 꽂히고 데미지를 주는 장판을 남기는 오브젝트 추가
                // 그 후 새로운 튕기는 오브젝트 생성(장판이 사라지는 시점에 새로 꽂히도록 조절 필요)
                break;

            // HellBoss의 경우
            case BossType.HellBoss:
                // 시작 시 X 모양으로 레이저를 쏘고 시계 방향으로 회전 중간중간 방향 변경
                // 및 위에서 오브젝트 낙하(에 맞으면 잠시 못움직이게 기절)
                // 및 레이저 맞으면 즉사
                // 및 맵 전역을 튕기는 오브젝트 추가(각도는 시작시 조절)
                BounceObjectInstantiate();
                // 일정 간격으로 플레이어를 조준 및 바닥에 꽂히고 데미지를 주는 장판을 남기는 오브젝트 추가
                // 그 후 새로운 튕기는 오브젝트 생성(장판이 사라지는 시점에 새로 꽂히도록 조절 필요)
                break;
        }
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

        float elapsed = 0f;

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

        // 플레이어가 살아있는 동안 반복
        while (!player_test.playerDie)
        {
            //Debug.Log("레이저 회전 중");

            // LaserParent를 Z축 기준 시계 방향 회전
            horizontalLaserInstance.transform.Rotate(0f, 0f, -speed * Time.deltaTime);

            elapsed += Time.deltaTime;
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

        GameObject fallingObjectParent = new GameObject("FallingObjectParent");
        fallingObjectParent.transform.SetParent(transform);         // 이 보스의 자식으로 설정
        fallingObjectParent.transform.localPosition = Vector3.zero; // 위치 초기화

        float spawnInterval = 1.5f; // 오브젝트 떨어뜨리는 간격 (원하는 값으로 조절 가능)

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

        for(int i = 0; i < count; i++)
        {
            bounceObjectInstance = Instantiate(bounceObject, transform);
            bounceObjectInstance.transform.localPosition = new Vector2(0, (2 * i) + 2);   // 0, 4 6 8에 생성
            bounceObjectInstance.transform.localRotation = Quaternion.identity;           // 회전을 0,0,0으로 설정
        }
    }
}
