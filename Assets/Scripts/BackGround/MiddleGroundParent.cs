using UnityEngine;

public class MiddleGroundParent : MonoBehaviour
{
    public GameObject backGroundPrefab;
    public float spawnOffset = 18.28f;

    private void Start()
    {
        // 최초 배경 생성
        Spawn(Vector3.zero);
    }

    public void SpawnNext(Vector3 currentPos)
    {
        Vector3 newPos = currentPos;
        newPos.x += spawnOffset;

        Spawn(newPos);
    }

    private void Spawn(Vector3 position)
    {
        Instantiate(
            backGroundPrefab,
            position,
            Quaternion.identity,
            transform
        );
    }
}