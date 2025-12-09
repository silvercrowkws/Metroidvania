using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Test_16_BossRoom : TestBase
{
# if UNITY_EDITOR
    GameManager gameManager;

    Player_Test player_test;

    BossMonsterBase bossMonsterBase;

    private void Start()
    {
        gameManager = GameManager.Instance;
        player_test = gameManager.Player_Test;

        // 이 스크립트가 실행되는 시점에 아직 보스 몬스터가 만들어지지 않았어서 이렇게 함
        StartCoroutine(AAA());
    }

    IEnumerator AAA()
    {
        yield return new WaitForSeconds(1.5f);
        bossMonsterBase = FindAnyObjectByType<BossMonsterBase>();
    }

    protected override void OnTest1(InputAction.CallbackContext context)
    {
        player_test.HP -= 10;
    }

    protected override void OnTest2(InputAction.CallbackContext context)
    {
        bossMonsterBase.HP -= 50;
    }

#endif
}
