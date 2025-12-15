using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossMonster_4 : BossMonsterBase
{
    protected override void OnEnable()
    {
        bossDieMoney = 500;
        bossDieXP = 500;
        maxHP = 1500f;
        HP = maxHP;
        //bossAttackPower = 10.0f;
        bossType = BossType.NightmareBoss;
        base.OnEnable();
    }
}
