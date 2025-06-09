using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayerVirtualCamera : MonoBehaviour
{
    GameManager gameManager;

    Player player;

    private void Start()
    {
        gameManager = GameManager.Instance;
        player = gameManager.Player;

        // CinemachineVirtualCamera 컴포넌트 가져오기
        CinemachineVirtualCamera vcam = GetComponent<CinemachineVirtualCamera>();
        if (vcam != null && player != null)
        {
            vcam.Follow = player.transform;
        }
    }
}
