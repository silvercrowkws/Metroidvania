using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class BossMonster_2 : BossMonsterBase
{
    protected override void OnEnable()
    {
        bossDieMoney = 150;
        currentHP = 750.0f;
        maxHP = currentHP;
        //bossAttackPower = 10.0f;
        bossType = BossType.NormalBoss;
        base.OnEnable();
    }
}
