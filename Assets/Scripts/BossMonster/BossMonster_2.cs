using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class BossMonster_2 : BossMonsterBase
{
    protected override void OnEnable()
    {
        bossDieMoney = 150;
        maxHP = 750.0f;
        HP = maxHP;
        //bossAttackPower = 10.0f;
        bossType = BossType.NormalBoss;
        base.OnEnable();
    }
}
