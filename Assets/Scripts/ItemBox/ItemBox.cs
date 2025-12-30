using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBox : MonoBehaviour
{
    Animator animator;

    [SerializeField] private GameObject[] Items;

    /// <summary>
    /// 각 아이템의 가중치 설정 (Items 배열과 순서가 같아야 함)
    /// </summary>
    private int[] weights = { 3, 2, 1, 5, 5, 5 };

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("플레이어와 아이템 박스가 충돌");
            animator.SetTrigger("Open");

            // 혹시 박스가 중복으로 열리지 않게 콜라이더를 끔
            GetComponent<Collider2D>().enabled = false;

            // 1. 몇 개를 생성할지 결정 (1~3개)
            int spawnCount = UnityEngine.Random.Range(1, 4);

            for (int i = 0; i < spawnCount; i++)
            {
                // 🔥 가중치에 따라 인덱스 선택
                int randomIndex = GetRandomIndexByWeight();

                GameObject item = Instantiate(Items[randomIndex], transform.position, Quaternion.identity);

                Rigidbody2D rb = item.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    Vector2 force = new Vector2(Random.Range(-1f, 1f), 1f).normalized * 5f;
                    rb.AddForce(force, ForceMode2D.Impulse);
                }
            }
        }
    }

    /// <summary>
    /// 가중치 랜덤 선택 함수
    /// </summary>
    /// <returns></returns>
    private int GetRandomIndexByWeight()
    {
        // 1. 전체 가중치 합산 (21)
        int totalWeight = 0;
        foreach (int w in weights) totalWeight += w;

        // 2. 0 ~ 21 사이의 랜덤 값 추출
        int pivot = UnityEngine.Random.Range(0, totalWeight);

        // 3. 가중치만큼 차감하며 어떤 구간에 속하는지 확인
        for (int i = 0; i < weights.Length; i++)
        {
            if (pivot < weights[i])
            {
                return i;
            }
            pivot -= weights[i];
        }

        return 0; // 예외 처리
    }
}
