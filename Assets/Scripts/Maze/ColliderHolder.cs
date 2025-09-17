using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderHolder : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("EdgeCollider"))
        {
            //Debug.Log("ColliderHolider에서 몬스터와 충돌 감지");
        }
    }
}
