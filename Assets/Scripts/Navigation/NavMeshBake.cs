using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NavMeshPlus.Components;
using System;  // NavMeshPlus 네임스페이스

public class NavMeshBake : MonoBehaviour
{
    public NavMeshSurface navMeshSurface;

    [SerializeField] private RoomGenerator roomGenerator;

    private void Start()
    {
        //roomGenerator = FindAnyObjectByType<RoomGenerator>();
    }

    private void OnEnable()
    {
        roomGenerator.onRoomGenerated += OnRoomGenerated;
    }

    /// <summary>
    /// 미로 생성 후에 네비게이션을 Bake하는 함수
    /// </summary>
    private void OnRoomGenerated()
    {
        navMeshSurface.BuildNavMesh();
        Debug.Log("길 생성 완료");
    }
}
