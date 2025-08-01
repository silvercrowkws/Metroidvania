using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// 데이터 로드와 관련된 메서드가 정의된 매니저 클래스
/// 게임 내의 씬에 존재할 것이 아니기 때문에 MonoBehaviour 상속하지 않음
/// </summary>
public class LoadManager
{
    /// <summary>
    /// JSON 파일에서 PlayerData를 불러와, 반환해주는 함수
    /// </summary>
    /// <returns></returns>
    public PlayerData LoadData()
    {
        string path = Path.Combine(Application.persistentDataPath, "playerData.json");
        if (!File.Exists(path))
        {
            Debug.LogWarning("세이브 파일을 찾을 수 없습니다. 디폴트 파일을 불러옵니다.");
            return new PlayerData { name = "DefaultPlayer", str = 1, dex = 1, hp = 1, items = new string[] { } };
        }

        try
        {
            string jsonData = File.ReadAllText(path);
            PlayerData playerData = JsonUtility.FromJson<PlayerData>(jsonData);

            Debug.Log("데이터 로드 성공!");
            return playerData;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"JSON 파일을 불러오는데 오류 발생: {e.Message}");
            return new PlayerData { name = "DefaultPlayer", str = 1, dex = 1, hp = 1, items = new string[] { } };
        }
    }
}