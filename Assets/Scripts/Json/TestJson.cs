using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestJson : MonoBehaviour
{
    /// <summary>
    /// 세이브 매니저
    /// </summary>
    SaveManager saveManager;

    /// <summary>
    /// 로드 매니저
    /// </summary>
    LoadManager loadManager;

    void Start()
    {
        /*PlayerData player = new PlayerData                  // 데이터 객체 생성
        {
            name = "Player",
            level = 5,
            items = new string[] { "sword", "shield" }
        };

        //SaveManager saveManager = new SaveManager();        // SaveManager 객체 생성
        saveManager = GameManager.Instance.SaveManager;

        saveManager.SaveData(player);                       // 데이터 저장

        //LoadManager loadManager = new LoadManager();        // LoadManager 객체 생성
        loadManager = GameManager.Instance.LoadManager;

        PlayerData playerData = loadManager.LoadData();     // 데이터 불러오기
        PrintData(playerData);                              // 불러온 데이터 출력*/
    }

    void PrintData(PlayerData playerData)
    {
        Debug.Log($"name : {playerData.name}");
        Debug.Log($"str : {playerData.str}");
        Debug.Log($"dex : {playerData.dex}");
        Debug.Log($"hp : {playerData.hp}");
        foreach (string item in playerData.items)
        {
            Debug.Log($"item : {item}");
        }
    }
}
