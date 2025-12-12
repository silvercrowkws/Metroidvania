using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossDieExplosionParent : MonoBehaviour
{
    private GameObject bossDieExplosion;
    public float radius = 2.5f;

    /// <summary>
    /// 폭발 간격
    /// </summary>
    public float explosionDelay = 0.5f;

    /// <summary>
    /// 폭발 스폰 개수
    /// </summary>
    public int spawnCount = 10;

    /// <summary>
    /// 보스 사망 연출이 끝났는지 확인하는 변수
    /// </summary>
    public bool isBossDieSequenceEnd = false;

    private void Awake()
    {
        bossDieExplosion = Resources.Load<GameObject>("GameObjects/BossDieExplosion");
    }

    private void Start()
    {
        StartCoroutine(SpawnObjects());
    }

    IEnumerator SpawnObjects()
    {
        for (int i = 0; i < spawnCount; i++)
        {
            Vector2 randomPos = Random.insideUnitCircle * radius;
            Vector3 spawnPos = transform.position + new Vector3(randomPos.x, randomPos.y, 0);

            Instantiate(bossDieExplosion, spawnPos, Quaternion.identity ,transform);

            yield return new WaitForSeconds(explosionDelay);
        }

        isBossDieSequenceEnd = true;
        Debug.Log($"isBossDieSequenceEnd: {isBossDieSequenceEnd}");
    }
}
