using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossMonster_3 : BossMonsterBase
{
    protected override void OnEnable()
    {
        bossDieMoney = 300;
        bossDieXP = 300;
        maxHP = 1000.0f;
        HP = maxHP;
        //bossAttackPower = 10.0f;
        bossType = BossType.HardBoss;
        base.OnEnable();
    }
}
