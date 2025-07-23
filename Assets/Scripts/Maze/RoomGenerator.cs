using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.AI.Navigation;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;
using UnityEngine.UI;
using System;
using System.Collections;

public class RoomGenerator : MonoBehaviour
{
    [SerializeField] private GameObject roomPrefab;     // 6x6 방 프리팹
    [SerializeField] private Transform roomParent;      // 부모 오브젝트
    [SerializeField] private GameObject doorPrefab;     // Door 프리팹
    [SerializeField] private GameObject keyPrefab;      // Key 프리팹

    // 미니맵
    [SerializeField] private GameObject miniMapIconPrefab; // UI Image 프리팹
    [SerializeField] private RectTransform miniMapParent;  // Canvas 하위 오브젝트
    [SerializeField] private float miniMapScale = 30f;     // UI에서 방 간격(픽셀)

    [SerializeField] private Camera miniMapCamera;
    [SerializeField] private RenderTexture miniMapRT;
    [SerializeField] private UnityEngine.UI.Image miniMapPanelImage;

    [SerializeField] private GameObject monster_RedChicken;      // monster_RedChicken 프리팹
    [SerializeField] private GameObject monster_Skeleton;
    [SerializeField] private GameObject monster_FlyingEye;
    [SerializeField] private GameObject monster_Goblin;
    [SerializeField] private GameObject monster_Mushroom;

    //[SerializeField] private NavMeshSurface navMeshSurface;

    [Tooltip("미로의 반지름(방 개수는 반지름에 따라 1:3², 2:5², 3:7², 4:9², 5:11², 6: 13², 7:15², 8:17²... 뒷배경이 반지름 8까지 커버 가능함)")]
    [SerializeField] public int radius = 4;

    private List<Vector2Int> mazePositions;
    public int seed = -1;
    private const int AllRandom = -1;

    public Dictionary<Vector2Int, Room> roomDictionary = new();
    private HashSet<Vector2Int> visited = new();

    // 사각형 격자(상, 우, 하, 좌) - 6칸씩 이동
    private static readonly List<(Vector2Int offset, Direction dir)> DirectionOffsets = new()
    {
        (new Vector2Int(0, 6), Direction.Top),
        (new Vector2Int(6, 0), Direction.Right),
        (new Vector2Int(0, -6), Direction.Bottom),
        (new Vector2Int(-6, 0), Direction.Left)
    };

    /// <summary>
    /// 미로 생성이 끝났음을 알림
    /// </summary>
    public Action onRoomGenerated;

    void Awake()
    {
        if (seed != AllRandom)
        {
            UnityEngine.Random.InitState(seed);
            Debug.Log($"[Seed 설정됨] : {seed}");
        }
        else
        {
            Debug.Log("[랜덤 시드] 완전 랜덤으로 실행");
        }

        mazePositions = GenerateOrderedRectangularPositions(radius);
    }

    void Start()
    {
        // RenderTexture가 없으면 직접 생성해서 카메라에 할당
        if (miniMapRT == null)
        {
            miniMapRT = new RenderTexture(512, 512, 0, RenderTextureFormat.ARGB32);
            miniMapRT.Create();
        }
        miniMapCamera.targetTexture = miniMapRT;

        GenerateRooms();
        GenerateMazeWithInitialOpening();
        SpawnDoorInRandomRoom();    // 미로 생성 후 랜덤 Room에 Door 생성
        SpawnKeyInRandomRoom();     // 미로 생성 후 랜덤 Room에 Key 생성
        SpawnMonsterInRandomRoom(); // 미로 생성 후 랜덤 Room에 Monster_0 생성

        // 미로 생성 완료 후 길 굽기
        onRoomGenerated?.Invoke();

        // NavMesh 굽기
        /*if (navMeshSurface != null)
        {
            Debug.Log("navMeshSurface 할당 됬고");
            navMeshSurface.BuildNavMesh();
        }*/

        // 미니맵 이미지 생성
        //GenerateMiniMapTexture();
        //CaptureMiniMap();

        StartCoroutine(CaptureMiniMapNextFrame());
    }

    public Sprite testMiniMapSprite;

    private void CaptureMiniMap()
    {
        // 미니맵 카메라가 미로 전체를 찍도록 위치/사이즈 조정 후 호출

        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = miniMapRT;

        /*Texture2D tex = new Texture2D(miniMapRT.width, miniMapRT.height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, miniMapRT.width, miniMapRT.height), 0, 0);
        tex.Apply();

        RenderTexture.active = currentRT;

        Sprite miniMapSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        miniMapPanelImage.sprite = miniMapSprite;*/

        Texture2D tex = new Texture2D(miniMapRT.width, miniMapRT.height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, miniMapRT.width, miniMapRT.height), 0, 0);
        tex.Apply();

        RenderTexture.active = currentRT;

        // 3. Texture2D를 Sprite로 변환
        Sprite miniMapSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        testMiniMapSprite = miniMapSprite;

        // 4. UI Image에 Sprite 할당
        miniMapPanelImage.sprite = miniMapSprite;
    }

    private IEnumerator CaptureMiniMapNextFrame()
    {
        yield return new WaitForEndOfFrame();
        CaptureMiniMap();
    }

    /// <summary>
    /// 방 생성: 6x6 간격으로 배치
    /// </summary>
    private void GenerateRooms()
    {
        /*foreach (var pos in mazePositions)
        {
            Vector3 worldPos = new Vector3(pos.x, pos.y, 0);
            GameObject roomObj = Instantiate(roomPrefab, worldPos, Quaternion.identity, roomParent);
            roomObj.name = $"Room_({pos.x},{pos.y})";

            Room room = roomObj.GetComponent<Room>();
            if (room == null)
            {
                Debug.LogError($"Room 컴포넌트가 Room Prefab에 없습니다: {roomObj.name}");
                continue;
            }

            roomDictionary[pos] = room;

            // room 생성 후 미니맵처리
            GameObject miniMapIcon = Instantiate(miniMapIconPrefab, miniMapParent);
            miniMapIcon.GetComponent<RectTransform>().anchoredPosition = new Vector2(pos.x, pos.y) * miniMapScale;
            MiniMapManager.Instance.RegisterRoom(room, miniMapIcon);
        }*/



        /*int minX = mazePositions.Min(p => p.x);
        int minY = mazePositions.Min(p => p.y);
        int maxX = mazePositions.Max(p => p.x);
        int maxY = mazePositions.Max(p => p.y);

        float panelWidth = miniMapParent.rect.width;
        float panelHeight = miniMapParent.rect.height;

        float rangeX = Mathf.Max(1, maxX - minX);
        float rangeY = Mathf.Max(1, maxY - minY);

        // 첫 방의 목표 위치
        float targetX = 0f;
        float targetY = 0f;

        float firstNormalizedX = (float)(mazePositions[0].x - minX) / rangeX;
        float firstNormalizedY = (float)(mazePositions[0].y - minY) / rangeY;
        float firstIconX = firstNormalizedX * panelWidth;
        float firstIconY = -firstNormalizedY * panelHeight;

        float offsetX = targetX - firstIconX;
        float offsetY = targetY - firstIconY;

        foreach (var pos in mazePositions)
        {
            Vector3 worldPos = new Vector3(pos.x, pos.y, 0);
            GameObject roomObj = Instantiate(roomPrefab, worldPos, Quaternion.identity, roomParent);
            roomObj.name = $"Room_({pos.x},{pos.y})";

            Room room = roomObj.GetComponent<Room>();
            if (room == null)
            {
                Debug.LogError($"Room 컴포넌트가 Room Prefab에 없습니다: {roomObj.name}");
                continue;
            }

            roomDictionary[pos] = room;

            float normalizedX = (float)(pos.x - minX) / rangeX;
            float normalizedY = (float)(pos.y - minY) / rangeY;

            float iconX = normalizedX * panelWidth + offsetX;
            float iconY = normalizedY * panelHeight + offsetY - 200;

            GameObject miniMapIcon = Instantiate(miniMapIconPrefab, miniMapParent);
            miniMapIcon.GetComponent<RectTransform>().anchoredPosition = new Vector2(iconX, iconY);

            // 수정: roomDictionary[pos] 대신 room 객체 직접 전달
            MiniMapManager.Instance.RegisterRoom(room, miniMapIcon);
        }*/








        int minX = mazePositions.Min(p => p.x);
        int minY = mazePositions.Min(p => p.y);

        float iconSize = GetMiniMapIconSize(radius);

        // 첫 번째 방 기준 좌표
        Vector2Int firstPos = mazePositions[0];
        int firstIdxX = (firstPos.x - minX) / 6;
        int firstIdxY = (firstPos.y - minY) / 6;

        // 첫 방을 (0, 0)에 위치시키기 위해 오프셋 계산
        float offsetX = -firstIdxX * iconSize;
        float offsetY = -firstIdxY * iconSize;
        //float offsetY = firstIdxY * iconSize;

        foreach (var pos in mazePositions)
        {
            Vector3 worldPos = new Vector3(pos.x, pos.y, 0);
            GameObject roomObj = Instantiate(roomPrefab, worldPos, Quaternion.identity, roomParent);
            roomObj.name = $"Room_({pos.x},{pos.y})";

            Room room = roomObj.GetComponent<Room>();
            if (room == null)
            {
                Debug.LogError($"Room 컴포넌트가 Room Prefab에 없습니다: {roomObj.name}");
                continue;
            }

            roomDictionary[pos] = room;

            // 격자 인덱스 계산
            int idxX = (pos.x - minX) / 6;
            int idxY = (pos.y - minY) / 6;

            float iconX = idxX * iconSize + offsetX;
            float iconY = idxY * iconSize + offsetY;
            //float iconY = -idxY * iconSize + offsetY;

            GameObject miniMapIcon = Instantiate(miniMapIconPrefab, miniMapParent);
            RectTransform rt = miniMapIcon.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(iconSize, iconSize);
            rt.anchoredPosition = new Vector2(iconX, iconY);

            MiniMapManager.Instance.RegisterRoom(room, miniMapIcon);
        }
    }

    /// <summary>
    /// 시작 방의 벽 하나를 무작위로 열고 DFS 시작
    /// </summary>
    private void GenerateMazeWithInitialOpening()
    {
        Vector2Int startPos = Vector2Int.zero;
        Room startRoom = roomDictionary[startPos];

        var directions = DirectionOffsets.OrderBy(_ => UnityEngine.Random.value).ToList();

        foreach (var (offset, dir) in directions)
        {
            Vector2Int neighborPos = startPos + offset;
            if (!roomDictionary.ContainsKey(neighborPos)) continue;

            startRoom.RemoveWall(dir);
            roomDictionary[neighborPos].RemoveWall(GetOppositeDirection(dir));

            visited.Clear();
            visited.Add(startPos);
            GenerateMaze(neighborPos);
            break;
        }
    }

    /// <summary>
    /// DFS로 미로 생성
    /// </summary>
    /// <param name="currentPos"></param>
    private void GenerateMaze(Vector2Int currentPos)
    {
        visited.Add(currentPos);

        var directions = DirectionOffsets.OrderBy(_ => UnityEngine.Random.value).ToList();

        foreach (var (offset, dir) in directions)
        {
            Vector2Int neighborPos = currentPos + offset;
            if (!roomDictionary.ContainsKey(neighborPos) || visited.Contains(neighborPos)) continue;

            Room currentRoom = roomDictionary[currentPos];
            Room neighborRoom = roomDictionary[neighborPos];

            currentRoom.RemoveWall(dir);
            neighborRoom.RemoveWall(GetOppositeDirection(dir));

            GenerateMaze(neighborPos);
        }
    }

    /// <summary>
    /// 사각형 격자에 맞는 좌표 생성 (6의 배수로)
    /// </summary>
    /// <param name="radius"></param>
    /// <returns></returns>
    private List<Vector2Int> GenerateOrderedRectangularPositions(int radius)
    {
        List<Vector2Int> positions = new();
        Queue<Vector2Int> frontier = new();
        HashSet<Vector2Int> visited = new();

        Vector2Int start = Vector2Int.zero;
        positions.Add(start);
        visited.Add(start);
        frontier.Enqueue(start);

        while (frontier.Count > 0)
        {
            var current = frontier.Dequeue();

            foreach (var (offset, _) in DirectionOffsets)
            {
                Vector2Int neighbor = current + offset;
                int distance = Mathf.Max(Mathf.Abs(neighbor.x / 6), Mathf.Abs(neighbor.y / 6));
                if (visited.Contains(neighbor) || distance > radius) continue;

                visited.Add(neighbor);
                frontier.Enqueue(neighbor);
                positions.Add(neighbor);
            }
        }

        return positions;
    }

    /// <summary>
    /// 반대 방향 반환
    /// </summary>
    /// <param name="dir"></param>
    /// <returns></returns>
    private Direction GetOppositeDirection(Direction dir)
    {
        return dir switch
        {
            Direction.Top => Direction.Bottom,
            Direction.Bottom => Direction.Top,
            Direction.Left => Direction.Right,
            Direction.Right => Direction.Left,
            _ => dir
        };
    }

    /// <summary>
    /// 미로 내 랜덤 Room 1개에만 Door 생성(0,0 제외)
    /// </summary>
    private void SpawnDoorInRandomRoom()
    {
        // wallBottom이 활성화된 Room만 필터링, Room_0_0(0,0) 제외
        var validRooms = roomDictionary
            .Where(kv => kv.Key != Vector2Int.zero)
            .Select(kv => kv.Value)
            .Where(room => room.wallBottom != null && room.wallBottom.activeSelf)
            .ToList();

        if (validRooms.Count == 0) return;

        int randomIndex = UnityEngine.Random.Range(0, validRooms.Count);
        Room selectedRoom = validRooms[randomIndex];

        // LandObject 위치 찾기 (Room 구조에 맞게 수정)
        Transform landObjectTransform = selectedRoom.transform.GetChild(1).GetChild(0);

        Vector3 spawnPosition = new Vector3(landObjectTransform.position.x, landObjectTransform.position.y + 0.75f, landObjectTransform.position.z);

        GameObject doorObj = Instantiate(doorPrefab, spawnPosition, Quaternion.identity, landObjectTransform);
        doorObj.name = "Door";
    }

    /// <summary>
    /// 미로 내 랜덤 Room 3개에만 Key 생성(0,0 제외)
    /// </summary>
    private void SpawnKeyInRandomRoom()
    {
        // wallBottom이 활성화된 Room만 필터링, Room_0_0(0,0) 제외
        var validRooms = roomDictionary
            .Where(kv => kv.Key != Vector2Int.zero)
            .Select(kv => kv.Value)
            .Where(room => room.wallBottom != null && room.wallBottom.activeSelf)
            .ToList();

        if (validRooms.Count < 3) return;

        // 랜덤하게 섞고 3개 선택
        var selectedRooms = validRooms.OrderBy(_ => UnityEngine.Random.value).Take(3).ToList();

        for (int i = 0; i < selectedRooms.Count; i++)
        {
            Room selectedRoom = selectedRooms[i];

            // LandObject 위치 찾기 (Room 구조에 맞게 수정)
            Transform landObjectTransform = selectedRoom.transform.GetChild(1).GetChild(0);

            Vector3 spawnPosition = new Vector3(landObjectTransform.position.x, landObjectTransform.position.y + 2f, landObjectTransform.position.z);

            GameObject keyObj = Instantiate(keyPrefab, spawnPosition, Quaternion.identity, landObjectTransform);
            keyObj.name = $"Key_{i + 1}";
        }
    }

    /// <summary>
    /// 미로 내 랜덤 Room 5개에만 몬스터 생성(0,0 제외)
    /// </summary>
    private void SpawnMonsterInRandomRoom()
    {
        // wallBottom이 활성화된 Room만 필터링, Room_0_0(0,0) 제외
        var validRooms = roomDictionary
            .Where(kv => kv.Key != Vector2Int.zero)
            .Select(kv => kv.Value)
            .Where(room => room.wallBottom != null && room.wallBottom.activeSelf)
            .ToList();

        if (validRooms.Count < 3) return;

        // 랜덤하게 섞고 3개 선택
        var selectedRooms = validRooms.OrderBy(_ => UnityEngine.Random.value).Take(5).ToList();

        for (int i = 0; i < selectedRooms.Count; i++)
        {
            Room selectedRoom = selectedRooms[i];

            // LandObject 위치 찾기 (Room 구조에 맞게 수정)
            Transform landObjectTransform = selectedRoom.transform.GetChild(1).GetChild(0);

            Vector3 spawnPosition = new Vector3(landObjectTransform.position.x, landObjectTransform.position.y + 0.6f, landObjectTransform.position.z);

            /*GameObject monster_0_Obj = Instantiate(monster_RedChicken, spawnPosition, Quaternion.identity, landObjectTransform);
            monster_0_Obj.name = $"Monster_0_{i + 1}";*/

            /*GameObject monster_1_Obj = Instantiate(monster_Skeleton, spawnPosition, Quaternion.identity, landObjectTransform);
            monster_1_Obj.name = $"Monster_1_{i + 1}";*/

            GameObject monster_2_Obj = Instantiate(monster_FlyingEye, spawnPosition, Quaternion.identity, landObjectTransform);
            monster_2_Obj.name = $"Monster_2_{i + 1}";
        }
    }

    private float GetMiniMapIconSize(int radius)
    {
        // radius별 아이콘 크기 테이블
        switch (radius)
        {
            case 1: return 65f;
            case 2: return 40f;
            case 3: return 30f;
            case 4: return 22.5f;
            case 5: return 18.5f;
            case 6: return 15.25f;
            case 7: return 13.25f;
            case 8: return 11.75f;
            default: return 10f; // 그 이상은 임의값
        }
    }
}