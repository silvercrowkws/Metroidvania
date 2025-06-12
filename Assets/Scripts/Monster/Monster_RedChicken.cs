using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster_RedChicken : MonsterBase
{
    protected override void Start()
    {
        moveSpeed = 3.0f;
        dieMoney = 10;
        currentHP = 50.0f;
        maxHP = currentHP;
        attackPower = 10.0f;
        base.Start();
    }
}
