using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossMonster_1 : BossMonsterBase
{
    protected override void OnEnable()
    {
        bossDieMoney = 100;
        currentHP = 500.0f;
        maxHP = currentHP;
        bossAttackPower = 10.0f;
        bossType = BossType.EasyBoss;
        base.OnEnable();
    }
}
