using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// 게임 데이터를 파일로 저장하는 클래스
/// /// 게임 내의 씬에 존재할 것이 아니기 때문에 MonoBehaviour 상속하지 않음
/// </summary>
public class SaveManager
{
    public void SaveData(PlayerData data)
    {
        // 저장 경로 설정     Path.Combine()은 여러 경로를 결합해 주는 함수
        string path = Path.Combine(Application.persistentDataPath, "playerData.json");
        // 진짜로 할때는 경로를 바꿔야?

        try
        {
            // JSON 직렬화
            string jsonData = JsonUtility.ToJson(data, true);
            Debug.Log($"JSON Data : \n{jsonData}");

            // 경로 확인 및 생성
            if (!Directory.Exists(Application.persistentDataPath))
            {
                Directory.CreateDirectory(Application.persistentDataPath);
            }

            // JSON 데이터 저장
            File.WriteAllText(path, jsonData);
            Debug.Log($"Data saved to {path}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save data: {e.Message}");
        }
    }
}
