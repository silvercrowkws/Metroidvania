using UnityEngine;

public class MiniMapCameraController : MonoBehaviour
{
    [Header("미로 반지름(RoomGenerator에서 할당)")]
    public int radius = 4;

    [Header("카메라가 보여줄 여유(월드 단위)")]
    public float margin = 3f;

    private Camera cam;

    void Awake()
    {
        cam = GetComponent<Camera>();
        cam.orthographic = true;
    }

    /// <summary>
    /// 미로 반지름에 따라 카메라 사이즈 자동 조절
    /// </summary>
    public void SetCameraSize(int mazeRadius)
    {
        radius = mazeRadius;

        // 미로의 전체 크기 계산 (격자 간격 6, 방 개수 = 2*radius+1)
        int mazeSize = (radius * 2 + 1) * 6;

        // 정사각형 뷰, 전체가 보이게 orthographicSize 설정
        cam.aspect = 1f;
        cam.orthographicSize = mazeSize / 2f + margin;

        // 카메라 위치 중앙으로 이동
        cam.transform.position = new Vector3(0, 0, cam.transform.position.z);
    }

    // 예시: 시작 시 자동 적용
    void Start()
    {
        RoomGenerator roomGenerator = FindAnyObjectByType<RoomGenerator>();
        radius = roomGenerator.radius;

        SetCameraSize(radius);
    }
}