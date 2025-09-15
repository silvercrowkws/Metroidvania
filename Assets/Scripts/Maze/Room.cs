using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.AI;

public enum Direction
{
    Top,
    Bottom,
    Right,
    Left
}

public class Room : MonoBehaviour
{
    public GameObject wallTop;
    public GameObject wallBottom;
    public GameObject wallRight;
    public GameObject wallLeft;

    /// <summary>
    /// 스프라이트 렌더러
    /// </summary>
    SpriteRenderer spriteRenderer;

    /// <summary>
    /// 지형지물 오브젝트(풀, 버섯 등등)
    /// </summary>
    Sprite[] landObjects;

    /*/// <summary>
    /// Room 본인의 타일맵
    /// </summary>
    private Tilemap tilemap;*/

    /// <summary>
    /// 플레이어가 이 방에 방문했는지 확인하는 bool 변수(미니맵 밝히기 용도)
    /// </summary>
    public bool isVisited = false;

    private void Awake()
    {
        Transform child = transform.GetChild(1);        // Bottom
        child = child.GetChild(0);                      // LandObject

        spriteRenderer = child.GetComponent<SpriteRenderer>();
        landObjects = Resources.LoadAll<Sprite>("Sprites/LandObjects");

        // 배열이 비어있지 않다면
        if (landObjects.Length > 0)
        {
            // 0~99 사이의 랜덤 값 생성
            int randomChance = Random.Range(0, 100);

            // 30 이하일 때만 실행 (30% 확률로 스프라이트 변경)
            if (randomChance < 30)
            {
                spriteRenderer.sprite = landObjects[Random.Range(0, landObjects.Length)];
            }
        }
        else
        {
            Debug.LogError("폴더에 스프라이트가 없습니다!");
        }

        //tilemap = GetComponent<Tilemap>();
    }

    /// <summary>
    /// 방향에 맞는 벽을 비활성화 하는 함수
    /// </summary>
    /// <param name="dir"></param>
    public void RemoveWall(Direction dir)
    {
        switch (dir)
        {
            case Direction.Top:
                if (wallTop != null)
                {
                    wallTop.SetActive(false);
                }
                break;
            case Direction.Bottom:
                if (wallBottom != null)
                {
                    wallBottom.SetActive(false);
                }
                break;
            case Direction.Right:
                if (wallRight != null)
                {
                    wallRight.SetActive(false);
                }
                break;
            case Direction.Left:
                if (wallLeft != null)
                {
                    wallLeft.SetActive(false);
                }
                break;
        }
    }

    public void SetLight(bool isBright)
    {
        Color color = isBright ? new Color(1f, 1f, 1f, 1f) : new Color(0f, 0f, 0f, 1f);

        Tilemap[] tilemaps = GetComponentsInChildren<Tilemap>(true);
        foreach (var tm in tilemaps)
        {
            tm.color = color;
        }

        /*Transform child = transform.GetChild(1).GetChild(0);
        
        SpriteRenderer[] spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var sp in spriteRenderers)
        {
            sp.color = color;
        }*/

        if (transform.childCount > 1)
        {
            Transform firstChild = transform.GetChild(1);
            if (firstChild.childCount > 0)
            {
                Transform target = firstChild.GetChild(0);
                SpriteRenderer sr = target.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.color = color;
                }
            }
        }
    }

    /// <summary>
    /// 플레이어가 방에 들어올 때 호출
    /// </summary>
    /// <param name="other"></param>
    void OnTriggerEnter2D(Collider2D other)
    {
        /*Room room = other.GetComponent<Room>();
        if (room != null && other.CompareTag("Player"))
        {
            Debug.Log("방과 플레이어 충돌 확인");
            room.isVisited = true;
            MiniMapManager.Instance.UpdateRoom(room); // 미니맵 갱신
        }*/

        if (other.CompareTag("Player"))
        {
            //Debug.Log("방과 플레이어 충돌 확인");
            if (!isVisited)
            {
                isVisited = true;

                if (MiniMapManager.Instance != null)
                {
                    MiniMapManager.Instance.UpdateRoom(this);
                }
            }
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if(MiniMapManager.Instance != null)
            {
                MiniMapManager.Instance.UpdateCurrentRoom(this);
            }
        }
    }

    public bool HasWall(Direction dir)
    {
        switch (dir)
        {
            case Direction.Top:
                return wallTop != null && wallTop.activeSelf;
            case Direction.Bottom:
                return wallBottom != null && wallBottom.activeSelf;
            case Direction.Right:
                return wallRight != null && wallRight.activeSelf;
            case Direction.Left:
                return wallLeft != null && wallLeft.activeSelf;
            default:
                return false;
        }
    }
}
