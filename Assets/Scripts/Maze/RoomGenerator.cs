using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.AI.Navigation;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;
using UnityEngine.UI;
using System;
using System.Collections;
using UnityEngine.WSA;

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

    [Tooltip("미로의 반지름(방 개수는 반지름에 따라 1:3², 2:5², 3:7², 4:9², 5:11², 6:13², 7:15², 8:17²... 뒷배경이 반지름 8까지 커버 가능함)")]
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

    GameManager gameManager;

    int monstersPerType = 0;

    /// <summary>
    /// 룸이 준비되었다고 알리는 bool 변수
    /// </summary>
    public bool onRoomReady = false;

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

        /*switch (gameManager.GameDifficulty)
        {
            case GameDifficulty.Easy:
                radius = 4;
                break;
            case GameDifficulty.Normal:
                radius = 5;
                break;
            case GameDifficulty.Hard:
                radius = 6;
                break;
            case GameDifficulty.Nightmare:
                radius = 7;
                break;
            case GameDifficulty.Hell:
                radius = 8;
                break;
        }
        mazePositions = GenerateOrderedRectangularPositions(radius);*/
    }

    /// <summary>
    /// CameraConfinerBoundsGenerator 클래스에 반지름을 알려 버츄얼 카메라 끝을 수정하는 함수
    /// </summary>
    public Action<int> onRadiusChange;

    void Start()
    {
        gameManager = GameManager.Instance;

        switch (gameManager.GameDifficulty)
        {
            // [Tooltip("미로의 반지름(방 개수는 반지름에 따라 4:9², 5:11², 6:13², 7:15², 8:17²... 뒷배경이 반지름 8까지 커버 가능함)")]
            case GameDifficulty.Easy:
                monstersPerType = 2;
                radius = 4;
                break;
            case GameDifficulty.Normal:
                monstersPerType = 3;
                radius = 5;
                break;
            case GameDifficulty.Hard:
                monstersPerType = 5;
                radius = 6;
                break;
            case GameDifficulty.Nightmare:
                monstersPerType = 7;
                radius = 7;
                break;
            case GameDifficulty.Hell:
                monstersPerType = 10;
                radius = 8;
                break;
        }
        onRadiusChange?.Invoke(radius);
        mazePositions = GenerateOrderedRectangularPositions(radius);

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

        // 기즈모 위치에 BoxCollider2D 생성
        CreateBoxCollidersAtGizmoPositions();

        onRoomReady = true;
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

    /*/// <summary>
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

        // 몬스터 종류의 개수
        int monsterTypes = 5;

        // 각 몬스터 별로 생성할 개수
        int monstersPerType = 3;

        // 생성할 몬스터의 총 개수
        int totalMonsters = monsterTypes * monstersPerType;

        //if (validRooms.Count < 3) return;
        if (validRooms.Count < totalMonsters)
        {
            Debug.LogWarning("방 개수가 부족해서 모든 몬스터를 배치할 수 없습니다.");
            return;
        }

        *//*// 랜덤하게 섞고 3개 선택
        var selectedRooms = validRooms.OrderBy(_ => UnityEngine.Random.value).Take(5).ToList();*//*

        // 방 랜덤 섞기
        var shuffledRooms = validRooms.OrderBy(_ => UnityEngine.Random.value).ToList();

        // 몬스터 프리팹 배열
        GameObject[] monsterPrefabs = new GameObject[]
        {
            monster_RedChicken,
            monster_Skeleton,
            monster_FlyingEye,
            monster_Goblin,
            monster_Mushroom
        };


        *//*for (int i = 0; i < selectedRooms.Count; i++)
        {
            Room selectedRoom = selectedRooms[i];

            // LandObject 위치 찾기 (Room 구조에 맞게 수정)
            Transform landObjectTransform = selectedRoom.transform.GetChild(1).GetChild(0);

            Vector3 spawnPosition = new Vector3(landObjectTransform.position.x, landObjectTransform.position.y + 0.6f, landObjectTransform.position.z);

            *//*GameObject monster_0_Obj = Instantiate(monster_RedChicken, spawnPosition, Quaternion.identity, landObjectTransform);
            monster_0_Obj.name = $"Monster_0_{i + 1}";*/

    /*GameObject monster_1_Obj = Instantiate(monster_Skeleton, spawnPosition, Quaternion.identity, landObjectTransform);
    monster_1_Obj.name = $"Monster_1_{i + 1}";*//*

    GameObject monster_2_Obj = Instantiate(monster_FlyingEye, spawnPosition, Quaternion.identity, landObjectTransform);
    monster_2_Obj.name = $"Monster_2_{i + 1}";
}*//*

    int roomIndex = 0;
    for (int type = 0; type < monsterTypes; type++)
    {
        for (int i = 0; i < monstersPerType; i++)
        {
            Room selectedRoom = shuffledRooms[roomIndex++];
            Transform landObjectTransform = selectedRoom.transform.GetChild(1).GetChild(0);
            Vector3 spawnPosition = new Vector3(landObjectTransform.position.x, landObjectTransform.position.y + 0.6f, landObjectTransform.position.z);

            GameObject monsterObj = Instantiate(monsterPrefabs[type], spawnPosition, Quaternion.identity, landObjectTransform);
            monsterObj.name = $"Monster_{type}_{i + 1}";
        }
    }
}*/

    /// <summary>
    /// 미로 내 랜덤 Room에 몬스터 생성. radius에 따라 종류별 소환 개수가 변경됨. (0,0 제외)
    /// </summary>
    private void SpawnMonsterInRandomRoom()
    {
        // 1. 유효한 방 목록 필터링 (시작 방 제외, wallBottom이 활성화된 방 = 미로 외곽 쪽 방)
        var validRooms = roomDictionary
            .Where(kv => kv.Key != Vector2Int.zero)
            .Select(kv => kv.Value)
            .Where(room => room.wallBottom != null && room.wallBottom.activeSelf)
            .ToList();

        // 2. radius에 따른 각 몬스터 종류별 소환 개수 계산
        // 요구사항: radius 4->1마리, 5->2마리, 6->3마리, 7->4마리, 8->5마리
        // 공식: monstersPerType = radius - 3
        //int monstersPerType = radius - 3;
        //int monstersPerType = radius - 3;

        // radius가 4보다 작으면 몬스터를 소환하지 않거나 최소 1마리로 조정 가능 (여기서는 1마리 미만이면 0으로 처리)
        if (monstersPerType <= 0)
        {
            Debug.Log($"[Monster Spawn] radius({radius})가 4 미만이므로 몬스터를 소환하지 않습니다.");
            return;
        }

        // 3. 몬스터 프리팹 배열
        GameObject[] monsterPrefabs = new GameObject[]
        {
            monster_RedChicken,
            monster_Skeleton,
            monster_FlyingEye,
            monster_Goblin,
            monster_Mushroom
        };
        int monsterTypes = monsterPrefabs.Length; // 몬스터 종류의 개수 (5개)
        int totalMonsters = monsterTypes * monstersPerType; // 생성할 몬스터의 총 개수

        // 4. 방 개수 확인
        if (validRooms.Count < totalMonsters)
        {
            Debug.LogWarning($"방 개수({validRooms.Count})가 부족해서 모든 몬스터({totalMonsters}마리)를 배치할 수 없습니다. 배치 가능한 만큼만 배치합니다.");
            totalMonsters = validRooms.Count; // 배치 가능한 최대 개수로 제한
        }

        // 5. 방 랜덤하게 섞기
        var shuffledRooms = validRooms.OrderBy(_ => UnityEngine.Random.value).Take(totalMonsters).ToList();

        Debug.Log($"[Monster Spawn] 난이도에 따라 몬스터 종류별 {monstersPerType}마리씩, 총 {totalMonsters}마리 배치 시작.");

        // 6. 몬스터 배치
        int roomIndex = 0;
        for (int type = 0; type < monsterTypes; type++)
        {
            for (int i = 0; i < monstersPerType; i++)
            {
                // 배치 가능한 방 개수를 초과하면 루프 종료
                if (roomIndex >= totalMonsters) goto EndSpawn;

                Room selectedRoom = shuffledRooms[roomIndex++];

                // LandObject 위치 찾기 (Room 구조에 맞게 수정)
                // 현재 코드에서 Transform landObjectTransform = selectedRoom.transform.GetChild(1).GetChild(0); 로 되어 있으나,
                // Room 구조에 따라 달라질 수 있으므로, 해당 위치를 몬스터가 소환될 중심 위치로 가정하고 진행합니다.
                Transform landObjectTransform = selectedRoom.transform.GetChild(1).GetChild(0);
                Vector3 spawnPosition = new Vector3(landObjectTransform.position.x, landObjectTransform.position.y + 0.6f, landObjectTransform.position.z);

                GameObject monsterObj = Instantiate(monsterPrefabs[type], spawnPosition, Quaternion.identity, landObjectTransform);
                monsterObj.name = $"Monster_{type}_{i + 1}";
            }
        }

    EndSpawn:
        Debug.Log("[Monster Spawn] 몬스터 배치 완료.");
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

    /*private void OnDrawGizmos()
    {
        if (roomDictionary == null || roomDictionary.Count == 0)
            return;

        Gizmos.color = Color.green;

        foreach (var kv in roomDictionary)
        {
            Vector2Int pos = kv.Key;
            Room room = kv.Value;

            // 연결된 방향 리스트
            List<Direction> connectedDirs = new List<Direction>();
            foreach (var (offset, dir) in DirectionOffsets)
            {
                Vector2Int neighborPos = pos + offset;
                if (!roomDictionary.ContainsKey(neighborPos))
                    continue;
                if (!room.HasWall(dir))
                    connectedDirs.Add(dir);
            }

            // 두 방향 이상 연결된 경우만 체크
            if (connectedDirs.Count >= 2)
            {
                // 모든 방향 쌍을 비교해서 직각(꺾임)인 경우가 있으면 표시
                for (int i = 0; i < connectedDirs.Count; i++)
                {
                    for (int j = i + 1; j < connectedDirs.Count; j++)
                    {
                        // 직각(상/하 vs 좌/우) 판정
                        bool isBend =
                            (IsVertical(connectedDirs[i]) && IsHorizontal(connectedDirs[j])) ||
                            (IsHorizontal(connectedDirs[i]) && IsVertical(connectedDirs[j]));
                        if (isBend)
                        {
                            Vector3 worldPos = new Vector3(pos.x, pos.y, 0);
                            Gizmos.DrawCube(worldPos, Vector3.one * 1.5f);
                            // 한 번만 찍고 break
                            goto NextRoom;
                        }
                    }
                }
            }
        NextRoom:;
        }

        // 내부 함수: 방향이 상/하인지
        bool IsVertical(Direction dir) => dir == Direction.Top || dir == Direction.Bottom;
        // 내부 함수: 방향이 좌/우인지
        bool IsHorizontal(Direction dir) => dir == Direction.Left || dir == Direction.Right;
    }*/

    private void OnDrawGizmos()
    {
        if (roomDictionary == null || roomDictionary.Count == 0)
            return;

        Gizmos.color = Color.green;

        foreach (var kv in roomDictionary)
        {
            Vector2Int pos = kv.Key;
            Room room = kv.Value;

            // wallBottom이 활성화된 방은 표시하지 않음
            if (room.wallBottom != null && room.wallBottom.activeSelf)
                continue;

            // 연결된 방향 리스트
            List<Direction> connectedDirs = new List<Direction>();
            foreach (var (offset, dir) in DirectionOffsets)
            {
                Vector2Int neighborPos = pos + offset;
                if (!roomDictionary.ContainsKey(neighborPos))
                    continue;
                if (!room.HasWall(dir))
                    connectedDirs.Add(dir);
            }

            // 좌우 연결이 있는지
            bool hasLeft = connectedDirs.Contains(Direction.Left);
            bool hasRight = connectedDirs.Contains(Direction.Right);
            // 상하 연결이 있는지
            bool hasTop = connectedDirs.Contains(Direction.Top);
            bool hasBottom = connectedDirs.Contains(Direction.Bottom);

            // 좌우 + 상하가 동시에 연결된 경우만 표시
            if ((hasLeft || hasRight) && (hasTop || hasBottom))
            {
                Vector3 worldPos = new Vector3(pos.x, pos.y, 0);
                Gizmos.DrawCube(worldPos, Vector3.one * 1.5f);
            }
        }
    }

    private void CreateBoxCollidersAtGizmoPositions()
    {
        if (roomDictionary == null || roomDictionary.Count == 0)
            return;

        foreach (var kv in roomDictionary)
        {
            Vector2Int pos = kv.Key;
            Room room = kv.Value;

            // wallBottom이 활성화된 방은 제외
            if (room.wallBottom != null && room.wallBottom.activeSelf)
                continue;

            // 연결된 방향 리스트
            List<Direction> connectedDirs = new List<Direction>();
            foreach (var (offset, dir) in DirectionOffsets)
            {
                Vector2Int neighborPos = pos + offset;
                if (!roomDictionary.ContainsKey(neighborPos))
                    continue;
                if (!room.HasWall(dir))
                    connectedDirs.Add(dir);
            }

            bool hasLeft = connectedDirs.Contains(Direction.Left);
            bool hasRight = connectedDirs.Contains(Direction.Right);
            bool hasTop = connectedDirs.Contains(Direction.Top);
            bool hasBottom = connectedDirs.Contains(Direction.Bottom);

            // 좌우 + 상하가 동시에 연결된 경우만
            if ((hasLeft || hasRight) && (hasTop || hasBottom))
            {
                Transform targetChild = room.transform.GetChild(4);
                if(targetChild != null)
                {
                    BoxCollider2D box = targetChild.gameObject.AddComponent<BoxCollider2D>();
                    box.offset = Vector2.zero;
                    box.size = new Vector2(4f, 4f);
                    box.isTrigger = false;
                }
            }
        }
    }
}