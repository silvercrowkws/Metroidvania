using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomLightingManager : MonoBehaviour
{
    RoomGenerator roomGenerator; // RoomGenerator 참조

    Player player;
    Player_Test player_test;

    Transform playerTransform;   // 플레이어 Transform 참조

    // Room의 크기(간격)와 반지름은 RoomGenerator와 동일하게 맞춰야 함
    private const int RoomInterval = 6;

    private void Start()
    {
        GameManager gameManager = GameManager.Instance;
        player = gameManager.Player;
        if(player == null)
        {
            player_test = gameManager.Player_Test;
            playerTransform = player_test.transform;
        }
        else
        {
            playerTransform = player.transform;
        }

        roomGenerator = FindAnyObjectByType<RoomGenerator>();
    }

    void Update()
    {
        Vector2Int playerRoomPos = GetPlayerRoomPosition();
        if (!roomGenerator) return;

        // 3x3 영역의 Room 좌표 구하기
        List<Vector2Int> brightRoomPositions = new();
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                brightRoomPositions.Add(new Vector2Int(playerRoomPos.x + dx * RoomInterval, playerRoomPos.y + dy * RoomInterval));
            }
        }

        // 모든 Room을 순회하며 밝기 설정
        foreach (var room in roomGenerator.roomDictionary)
        {
            bool isBright = brightRoomPositions.Contains(room.Key);
            room.Value.SetLight(isBright);
        }
    }

    // 플레이어가 위치한 Room 좌표 계산
    private Vector2Int GetPlayerRoomPosition()
    {
        Vector3 pos = playerTransform.position;
        int x = Mathf.RoundToInt(pos.x / RoomInterval) * RoomInterval;
        int y = Mathf.RoundToInt(pos.y / RoomInterval) * RoomInterval;
        return new Vector2Int(x, y);
    }
}
