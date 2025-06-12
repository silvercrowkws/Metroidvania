using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Test_03_Portal : TestBase
{
#if UNITY_EDITOR
    GameManager gameManager;

    /// <summary>
    /// 플레이어
    /// </summary>
    Player player;

    SceneManager sceneManager;

    GameObject monster;

    private void Start()
    {
        gameManager = GameManager.Instance;
        player = gameManager.Player;
        monster = GameObject.Find("Monster_0_1");
    }

    protected override void OnTest1(InputAction.CallbackContext context)
    {
        SceneManager.LoadScene(1);
    }

    protected override void OnTest2(InputAction.CallbackContext context)
    {
        SceneManager.LoadScene(0);
    }

    protected override void OnTest3(InputAction.CallbackContext context)
    {
        /*if (monster != null)
        {
            MonsterBase monsterBase = monster.GetComponent<MonsterBase>();
            if (monsterBase != null)
            {
                monsterBase.HP -= 1f; // HP 1 감소
                Debug.Log("Monster_0_1 HP 감소");
            }
            else
            {
                Debug.LogWarning("Monster_0_1에 MonsterBase 컴포넌트가 없습니다.");
            }
        }
        else
        {
            Debug.LogWarning("Monster_0_1 오브젝트를 찾을 수 없습니다.");
        }*/

        Monster_RedChicken redChicken = monster.GetComponent<Monster_RedChicken>();
        if (redChicken != null)
        {
            redChicken.HP -= 15f;
        }
    }

    protected override void OnTest4(InputAction.CallbackContext context)
    {
        
    }

    protected override void OnTest5(InputAction.CallbackContext context)
    {
        
    }

    protected override void OnTest6(InputAction.CallbackContext context)
    {
        
    }
#endif
}
