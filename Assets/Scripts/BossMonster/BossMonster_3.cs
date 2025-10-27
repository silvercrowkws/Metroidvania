using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossMonster_3 : BossMonsterBase
{
    protected override void OnEnable()
    {
        bossDieMoney = 300;
        currentHP = 1000.0f;
        maxHP = currentHP;
        //bossAttackPower = 10.0f;
        bossType = BossType.HardBoss;
        base.OnEnable();
    }
}
