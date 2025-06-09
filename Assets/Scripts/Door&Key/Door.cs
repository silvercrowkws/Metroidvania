using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 플레이어와 충돌했는지 확인
        Player player = other.GetComponent<Player>();
        if (player != null && player.hasAllKeys)        // hasAllKeys는 플레이어가 Key 3개를 모았는지 나타내는 bool
        {
            if (animator != null)
            {
                animator.SetTrigger("Open");
            }
        }
    }
}
