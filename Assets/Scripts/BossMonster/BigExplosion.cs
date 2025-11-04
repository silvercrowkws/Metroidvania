using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigExplosion : MonoBehaviour
{
    /// <summary>
    /// 추적 미사일
    /// </summary>
    ChaseMissile chaseMissile;

    private void Awake()
    {
        
    }

    private void isExplosionDone()
    {
        // 폭발 연출이 끝났을 때 부모에서 찾기
        chaseMissile = GetComponentInParent<ChaseMissile>();

        //Debug.Log("폭발 연출 끝");
        chaseMissile.FireFloorInstantiate();

        // 이제 여기서 폭발 연출 비활성화
        //Destroy(this.gameObject);

    }
}
