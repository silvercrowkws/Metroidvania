using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloseDoor : MonoBehaviour
{
    // 신호를 받으면 애니메이션 실행
    Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        animator.speed = 0;
    }
}
