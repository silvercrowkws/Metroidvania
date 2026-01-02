using UnityEngine;

public class BackGroundItem : MonoBehaviour
{
    public float moveSpeed = 1f;

    private MiddleGroundParent middleGroundParent;
    private bool hasNotified = false;

    private void Start()
    {
        // 부모가 MiddleGroundParent
        middleGroundParent = transform.parent.GetComponent<MiddleGroundParent>();
    }

    private void Update()
    {
        // 왼쪽으로 이동
        transform.Translate(Vector3.left * moveSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // CreateZone에 닿으면 다음 배경 생성 요청
        if (collision.CompareTag("CreateZone") && !hasNotified)
        {
            hasNotified = true;
            if (middleGroundParent != null)
            {
                Debug.Log("뒷 배경 다음 생성 요청");
                middleGroundParent.SpawnNext(transform.position);
            }
            else
            {
                // 첫번째 충돌은 Start에서 부모를 찾는 것보다 충돌이 더 빨리 일어남
                middleGroundParent = transform.parent.GetComponent<MiddleGroundParent>();
                Debug.Log("뒷 배경 다음 생성 요청2");
                middleGroundParent.SpawnNext(transform.position);
            }
        }

        // DeadZone에 닿으면 파괴
        if (collision.CompareTag("DeadZone"))
        {
            Debug.Log("뒷 배경 파괴");
            Destroy(gameObject);
        }
    }
}
