using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    private Animator animator;

    Player_Test player_test;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        player_test = GameManager.Instance.Player_Test;
        player_test.onKeyCountChanged += OnDoorOpen;
    }

    /// <summary>
    /// 열쇠개수가 변경되면 실행되는 함수
    /// 3개에서 사용
    /// </summary>
    /// <param name="count"></param>
    private void OnDoorOpen(int count)
    {
        if(count == 3)
        {
            if (animator != null)
            {
                animator.SetTrigger("Open");
            }
        }
    }

    /*private void OnTriggerEnter2D(Collider2D other)
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
    }*/
}
