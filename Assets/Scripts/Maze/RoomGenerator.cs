using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.AI.Navigation;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class RoomGenerator : MonoBehaviour
{
    [SerializeField] private GameObject roomPrefab;     // 6x6 방 프리팹
    [SerializeField] private Transform roomParent;      // 부모 오브젝트
    [SerializeField] private GameObject doorPrefab;     // Door 프리팹
    [SerializeField] private GameObject keyPrefab;      // Key 프리팹
    [SerializeField] private GameObject monster_0;      // monster_0 프리팹

    [SerializeField] private NavMeshSurface navMeshSurface;

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
        GenerateRooms();
        GenerateMazeWithInitialOpening();
        SpawnDoorInRandomRoom();    // 미로 생성 후 랜덤 Room에 Door 생성
        SpawnKeyInRandomRoom();     // 미로 생성 후 랜덤 Room에 Key 생성
        SpawnMonsterInRandomRoom(); // 미로 생성 후 랜덤 Room에 Monster_0 생성

        // NavMesh 굽기
        if (navMeshSurface != null)
        {
            navMeshSurface.BuildNavMesh();
        }
    }

    /// <summary>
    /// 방 생성: 6x6 간격으로 배치
    /// </summary>
    private void GenerateRooms()
    {
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

        int randomIndex = Random.Range(0, validRooms.Count);
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
        var selectedRooms = validRooms.OrderBy(_ => Random.value).Take(3).ToList();

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
    /// 미로 내 랜덤 Room 5개에만 Monster 생성(0,0 제외)
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
        var selectedRooms = validRooms.OrderBy(_ => Random.value).Take(5).ToList();

        for (int i = 0; i < selectedRooms.Count; i++)
        {
            Room selectedRoom = selectedRooms[i];

            // LandObject 위치 찾기 (Room 구조에 맞게 수정)
            Transform landObjectTransform = selectedRoom.transform.GetChild(1).GetChild(0);

            Vector3 spawnPosition = new Vector3(landObjectTransform.position.x, landObjectTransform.position.y + 0.6f, landObjectTransform.position.z);

            GameObject monster_0_Obj = Instantiate(monster_0, spawnPosition, Quaternion.identity, landObjectTransform);
            monster_0_Obj.name = $"Monster_0_{i + 1}";
        }
    }
}