using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayerVirtualCamera : MonoBehaviour
{
    GameManager gameManager;

    Player player;

    Player_Test player_test;

    private void Start()
    {
        gameManager = GameManager.Instance;
        player = gameManager.Player;
        if(player == null)
        {
            player_test = gameManager.Player_Test;
        }

        // CinemachineVirtualCamera 컴포넌트 가져오기
        CinemachineVirtualCamera vcam = GetComponent<CinemachineVirtualCamera>();
        if (vcam != null && player != null)
        {
            vcam.Follow = player.transform;
        }
        else if(vcam != null && player == null)
        {
            vcam.Follow = player_test.transform;
        }
    }
}
