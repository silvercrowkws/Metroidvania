using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EdgeCollider : MonoBehaviour
{
    public Action onAttackRange;

    // 만약 플레이어의 공격 범위에 충돌되면
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("AttackRange"))
        {
            Debug.Log("적 공격 범위에 충돌은 확인");
            onAttackRange?.Invoke();
        }
    }

    
}
