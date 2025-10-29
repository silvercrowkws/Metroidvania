using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossMonster_4 : BossMonsterBase
{
    protected override void OnEnable()
    {
        bossDieMoney = 500;
        currentHP = 1500f;
        maxHP = currentHP;
        //bossAttackPower = 10.0f;
        bossType = BossType.NightmareBoss;
        base.OnEnable();
    }
}
