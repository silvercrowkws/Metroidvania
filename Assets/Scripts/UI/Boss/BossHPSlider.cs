using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossHPSlider : MonoBehaviour
{
    /// <summary>
    /// 보스 몬스터 베이스
    /// </summary>
    BossMonsterBase bossMonsterBase;

    private void Awake()
    {
        bossMonsterBase = transform.root.GetComponent<BossMonsterBase>();
        if(bossMonsterBase != null)
        {
            // 보스 타입 받아올 수 있고
            // 보스의 체력
            // 보니까 그냥 보스몬스터베이스에서 다 처리하네???
        }
    }
}
