using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NPC : MonoBehaviour
{
    public enum NPCType
    {
        Bearded,
        HatMan,
        OldMan,
        Woman,
        MazeKeeper,
    }

    /// <summary>
    /// NPC 타입
    /// </summary>
    public NPCType npcType;

    Animator animator;

    SpriteRenderer spriteRenderer;
    Transform player;

    Button button;

    /// <summary>
    /// NPC 클릭으로 Dialogue 패널에 알리는 델리게이트
    /// </summary>
    public Action<NPCType> onNPCDialogue;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        button = GetComponentInChildren<Button>();
        button.onClick.AddListener(OnClickNPCButton);
    }

    private void Start()
    {
        Player_Test playerTest = FindObjectOfType<Player_Test>();
        if (playerTest != null)
            player = playerTest.transform;

        if (animator != null)
        {
            StartCoroutine(PlayAnimationRoutine());
        }
    }

    private void Update()
    {
        if (player == null) return;

        // 플레이어 방향 바라보기 (FlipX)
        if (player.position.x < transform.position.x)
            spriteRenderer.flipX = true;   // 왼쪽
        else
            spriteRenderer.flipX = false;  // 오른쪽
    }

    IEnumerator PlayAnimationRoutine()
    {
        while (true)
        {
            // 애니메이션 실행
            animator.SetTrigger("Idle");

            // 다음 프레임까지 대기 (상태 전환될 시간)
            yield return null;

            /*// 현재 재생 중인 애니메이션 길이 가져오기
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            float animLength = stateInfo.length;

            // Idle 애니메이션 길이만큼 대기
            yield return new WaitForSeconds(animLength);

            animator.SetTrigger("None");*/

            // 1 ~ 5초 랜덤 대기
            float waitTime = UnityEngine.Random.Range(1f, 5f);
            yield return new WaitForSeconds(waitTime);
        }
    }

    private void OnClickNPCButton()
    {
        switch (npcType)
        {
            case NPCType.Bearded:
                Debug.Log("Bearded 버튼 클릭");
                onNPCDialogue?.Invoke(NPCType.Bearded);
                //OpenShop();
                break;

            case NPCType.HatMan:
                Debug.Log("HatMan 버튼 클릭");
                onNPCDialogue?.Invoke(NPCType.HatMan);
                //OpenQuest();
                break;

            case NPCType.OldMan:
                Debug.Log("OldMan 버튼 클릭");
                onNPCDialogue?.Invoke(NPCType.OldMan);
                break;

            case NPCType.Woman:
                Debug.Log("Woman 버튼 클릭");
                onNPCDialogue?.Invoke(NPCType.Woman);
                break;

            case NPCType.MazeKeeper:
                Debug.Log("MazeKeeper 버튼 클릭");
                onNPCDialogue?.Invoke(NPCType.MazeKeeper);
                break;
        }
    }
}
