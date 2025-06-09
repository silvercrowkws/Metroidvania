using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

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
}
