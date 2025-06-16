using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// chaseRange 오브젝트에 붙임
public class ChaseRangeTrigger : MonoBehaviour
{
    MonsterBase monster;

    public void Init(MonsterBase monsterBase)
    {
        monster = monsterBase;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            monster.isChasing = true;
            monster.playerTransform = other.transform;
        }
    }
}
