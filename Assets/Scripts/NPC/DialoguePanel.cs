using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine;

public class DialoguePanel : MonoBehaviour
{
    NPC[] npcs;

    CanvasGroup canvasGroup;

    /// <summary>
    /// NPC 이름 텍스트
    /// </summary>
    TextMeshProUGUI nameText;

    /// <summary>
    /// 대화 텍스트
    /// </summary>
    TextMeshProUGUI dialogueText;

    private void Awake()
    {
        Transform child = transform.GetChild(3);

        nameText = child.GetChild(2).GetComponent<TextMeshProUGUI>();

        dialogueText = transform.GetChild(2).GetComponent<TextMeshProUGUI>();

        canvasGroup = GetComponent<CanvasGroup>();
        OnCanvasGroup(false);       // 캔버스 그룹 조절
    }

    private void Start()
    {
        // 씬에 있는 모든 NPC를 찾아서 델리게이트 연결
        npcs = FindObjectsOfType<NPC>();
        foreach (var npc in npcs)
        {
            npc.onNPCDialogue += OnNPCDialogue;
        }
    }

    private void OnDestroy()
    {
        // 메모리 누수 방지
        NPC[] npcs = FindObjectsOfType<NPC>();
        foreach (var npc in npcs)
        {
            npc.onNPCDialogue -= OnNPCDialogue;
        }
    }

    /// <summary>
    /// NPC 버튼 클릭으로 패널이 활성화되고 이름이 바뀌는 함수
    /// </summary>
    /// <param name="npcType"></param>
    private void OnNPCDialogue(NPC.NPCType npcType)
    {
        OnCanvasGroup(true);        // 패널 활성화

        // NPC 타입에 맞게 nameText 변경
        switch (npcType)
        {
            case NPC.NPCType.Bearded:
                nameText.text = "수염난 아저씨";
                dialogueText.text = @"허허, 손님! 허기진 것 같구만.
배도 채우고 힘도 내야 모험이 수월하지 않겠나?";
                // 사간 후 : 맛있게 먹고 힘내시게, 허허.
                // 안사면 : 허허, 힘내고 싶으면 하나 사가지 그래.
                break;
            case NPC.NPCType.HatMan:
                nameText.text = "모자 쓴 남자";
                dialogueText.text = @"자네, 힘 빠진 것 같군. 체력 회복 포션 하나 어떤가?
위급할 때 언제든지 쓸 수 있네만";
                // 사면 : 좋네, 사갔구만. 힘이 빠지면 또 들르게.
                // 안사면 : 허허, 안 살 거야? 어쩔 수 없지. 다음에 또 들르게.
                break;
            case NPC.NPCType.OldMan:
                nameText.text = "늙은 남자";
                dialogueText.text = @"어이, 손님. 부활초 하나 사보겠나?
이게 있으면 쓰러져도 다시 일어날 수 있어. 필요하면 사가게나.";
                // 사면 : 허허, 잘 샀네. 이게 있으면 쓰러져도 금방 일어날 걸세.
                // 안사면 : 가격도 싸게 줄 테니 부담 갖지 말게.
                break;
            case NPC.NPCType.Woman:
                nameText.text = "아름다운 여성";
                dialogueText.text = @"안녕~ 과일 좀 사갈래?
이건 오늘 아침에 딴 거라 신선해.
먹으면 기운도 나고 기분도 좋아질 걸?";
                // 사면 : 고마워~ 맛있게 먹어!
                // 안사면 : 괜찮아, 다음에 또 올 거지? 기다리고 있을게~
                break;
            case NPC.NPCType.MazeKeeper:
                nameText.text = "미궁 문지기";
                dialogueText.text =
                    @"미궁에 들어갈거야?
좋아, 난이도를 선택해줘.";
                break;
        }
    }

    private void OnCanvasGroup(bool value)
    {
        if(value == false)
        {
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        else
        {
            canvasGroup.alpha = 1;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
    }
}
