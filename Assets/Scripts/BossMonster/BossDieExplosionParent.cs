using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossDieExplosionParent : MonoBehaviour
{
    public GameObject prefab;
    public float radius = 2.5f;

    private void Start()
    {
        StartCoroutine(SpawnObjects());
    }

    IEnumerator SpawnObjects()
    {
        for (int i = 0; i < 10; i++)
        {
            Vector2 randomPos = Random.insideUnitCircle * radius;
            Vector3 spawnPos = transform.position + new Vector3(randomPos.x, randomPos.y, 0);

            Instantiate(prefab, spawnPos, Quaternion.identity);

            yield return new WaitForSeconds(0.5f);
        }
    }
}
