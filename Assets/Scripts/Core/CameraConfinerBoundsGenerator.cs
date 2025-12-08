using UnityEngine;

/// <summary>
/// RoomGenerator의 radius 값에 따라 PolygonCollider2D로 카메라 외곽 경계 생성
/// </summary>
[RequireComponent(typeof(PolygonCollider2D))]
public class CameraConfinerBoundsGenerator : MonoBehaviour
{
    /// <summary>
    /// 미로 생성기
    /// </summary>
    RoomGenerator roomGenerator;

    /// <summary>
    /// 폴리곤 콜라이더
    /// </summary>
    private PolygonCollider2D polygonCollider;

    /*[Tooltip("RoomGenerator에서 사용한 반지름 값과 동일하게 설정하세요.")]
    [SerializeField] private int radius = 4;*/

    [Tooltip("각 방의 크기 (타일 단위)")]
    [SerializeField] private float roomTileSize = 6f;

    private void Awake()
    {
        polygonCollider = GetComponent<PolygonCollider2D>();

        //GenerateBounds(roomGenerator.radius);
    }

    private void Start()
    {
        roomGenerator = FindAnyObjectByType<RoomGenerator>();
        roomGenerator.onRadiusChange += GenerateBounds;

        //GenerateBounds(roomGenerator.radius); => 델리게이트로 수정
    }

    private void GenerateBounds(int radius)
    {
        int roomsPerSide = 2 * radius + 1;
        float totalSize = roomsPerSide * roomTileSize;
        float half = totalSize / 2f;

        Debug.Log($"roomGenerator로부터 받은 radius : {radius}");
        Debug.Log($"roomsPerSide : 2 * radius + 1 = {roomsPerSide}");
        Debug.Log($"totalSize : roomsPerSide * roomTileSize = {roomsPerSide}");
        Debug.Log($"half : totalSize / 2f = {half} ");

        Vector2[] points = new Vector2[]
        {
            new Vector2(-half, -half),
            new Vector2(-half,  half),
            new Vector2( half,  half),
            new Vector2( half, -half)
        };

        polygonCollider.pathCount = 1; 
        polygonCollider.SetPath(0, points);
    }
}
