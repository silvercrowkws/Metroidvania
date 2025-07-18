using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster_FlyingEye : MonsterBase
{
    protected override void OnEnable()
    {
        moveSpeed = 5.0f;
        dieMoney = 10;
        currentHP = 50.0f;
        maxHP = currentHP;
        attackPower = 10.0f;
        monsterType = MonsterType.FlyingEye;
        monsterMoveType = MonsterMoveType.Flying;
        base.OnEnable();
    }

    protected override void Start()
    {
        base.Start();

        // chaseRange가 할당되지 않았다면 자식에서 찾아서 할당
        if (chaseRange == null)
            chaseRange = transform.GetChild(1).gameObject;

        if (chaseRange == null)
        {
            Debug.LogError("chaseRange 오브젝트를 찾을 수 없습니다.");
            return;
        }

        ChaseRangeTrigger chaseRangeTrigger = transform.GetChild(0).GetComponent<ChaseRangeTrigger>();
        if (chaseRangeTrigger == null)
        {
            chaseRangeTrigger = transform.GetChild(1).gameObject.AddComponent<ChaseRangeTrigger>();
        }

        chaseRangeTrigger.Init(this);


    }
}
