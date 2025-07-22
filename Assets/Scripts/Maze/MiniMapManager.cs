using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMapManager : MonoBehaviour
{
    public static MiniMapManager Instance;

    // Room과 미니맵 오브젝트 매핑
    private Dictionary<Room, GameObject> roomToMiniMapIcon = new();

    void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// Room과 해당 Room을 나타내는 미니맵 오브젝트를 매핑(등록)하는 함수
    /// </summary>
    /// <param name="room">실제 게임 내 Room 오브젝트</param>
    /// <param name="miniMapIcon">미니맵에 표시될 방 아이콘 오브젝트</param>
    public void RegisterRoom(Room room, GameObject miniMapIcon)
    {
        roomToMiniMapIcon[room] = miniMapIcon;
        // 처음엔 어둡게
        var img = miniMapIcon.GetComponent<UnityEngine.UI.Image>();
        if (img != null)
            img.color = new Color(0.2f, 0.2f, 0.2f, 1f);
    }

    /// <summary>
    /// 특정 Room의 방문 여부에 따라 미니맵 아이콘의 밝기를 갱신하는 함수
    /// </summary>
    /// <param name="room">방문 여부를 확인할 Room 오브젝트</param>
    public void UpdateRoom(Room room)
    {
        if (roomToMiniMapIcon.TryGetValue(room, out var icon))
        {
            var img = icon.GetComponent<UnityEngine.UI.Image>();
            if (img != null)
            {
                // 방문한 방은 밝게, 방문하지 않은 방은 어둡게 표시
                img.color = room.isVisited ? new Color(0.2f, 0.2f, 0.2f, 0f) : new Color(0.2f, 0.2f, 0.2f, 1f);
                //img.color = room.isVisited ? Color.white : new Color(0.2f, 0.2f, 0.2f, 1f);
            }
        }
    }
}
