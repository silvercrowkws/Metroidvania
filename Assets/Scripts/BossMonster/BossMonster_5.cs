using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossMonster_5 : BossMonsterBase
{
    protected override void OnEnable()
    {
        bossDieMoney = 1000;
        currentHP = 3000f;
        maxHP = currentHP;
        //bossAttackPower = 10.0f;
        bossType = BossType.HellBoss;
        base.OnEnable();
    }
}
