using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossMonster_5 : BossMonsterBase
{
    protected override void OnEnable()
    {
        bossDieMoney = 1000;
        maxHP = 3000f;
        HP = maxHP;
        //bossAttackPower = 10.0f;
        bossType = BossType.HellBoss;
        base.OnEnable();
    }
}
