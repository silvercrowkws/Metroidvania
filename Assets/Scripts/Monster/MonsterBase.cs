using System;
using System.Collections;
using System.Collections.Generic;
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
    GameManager gameManager;

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
    public Action onDie;

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
                    onDie?.Invoke();            // 몬스터가 죽었다고 델리게이트로 알림                    
                    Destroy(gameObject);        // 게임 오브젝트 파괴
                    Debug.Log("몬스터 사망");
                }
            }
        }
    }

    /// <summary>
    /// 몬스터의 공격력
    /// </summary>
    protected float attackPower = 1.0f;



}
