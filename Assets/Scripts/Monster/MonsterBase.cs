using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

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
    protected float attackPower = 1.0f;

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
        gameManager = GameManager.Instance;

        animator = GetComponent<Animator>();

        spriteRenderer = GetComponent<SpriteRenderer>();

        // 스프라이트 색상(RGB, 알파) 초기화
        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color(1f, 1f, 1f, 1f);
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

        onMonsterDie += OnMonsterDie;
        // 만약 몬스터의 HP가 0이되면 사망 연출 활성화?
        // 만약 몬스터의 HP가 0이 되면 
    }

    protected virtual void OnDisable()
    {
        onMonsterDie -= OnMonsterDie;
    }

    protected virtual void Start()
    {

    }

    /// <summary>
    /// 몬스터의 HP가 0이 되었을 때 실행될 함수
    /// </summary>
    /// <returns></returns>
    protected virtual void OnMonsterDie()
    {
        // 애니메이터 정지 (현재 프레임에서 멈춤)
        if (animator != null)
        {
            animator.speed = 0f;
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

            // 2. 알파값만 0으로 변경
            Color alphaZero = spriteRenderer.color;
            alphaZero.a = 0f;
            spriteRenderer.color = alphaZero;
        }

        // 사망 이펙트 활성화
        if (dieEffectInstance != null)
        {
            dieEffectInstance.SetActive(true);
        }

        yield return new WaitForSeconds(0.8f);
        this.gameObject.SetActive(false);
    }
}
