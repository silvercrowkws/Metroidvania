using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UI;

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

    /// <summary>
    /// NPC 이미지
    /// </summary>
    Image NPCImage;

    /// <summary>
    /// 패널의 NPC이미지 변경용 배열
    /// </summary>
    Sprite[] npcSprites;

    /// <summary>
    /// 상호작용 이름 텍스트
    /// </summary>
    TextMeshProUGUI interactionNameText;

    /// <summary>
    /// 각 NPC들의 상품 버튼 배열
    /// </summary>
    Button[] itemButtons;

    /// <summary>
    /// 취소 버튼
    /// </summary>
    Button cancelButton;

    /// <summary>
    /// 구매 수량 패널
    /// </summary>
    PurchaseQuantityPanel purchaseQuantityPanel;

    /// <summary>
    /// 미궁 난이도 확인 패널
    /// </summary>
    MazeDifficultyPanel mazeDifficultyPanel;

    /// <summary>
    /// NPC가 판매하는 아이템들의 데이터 (인스펙터에서 할당)
    /// </summary>
    [SerializeField] private ItemDataSO[] sellableItems;

    /// <summary>
    /// 최대로 겹칠 수 있는 카운트(인벤토리에 해당 아이템을 몇개까지 가지고 있을 수 있는지)
    /// </summary>
    int maxStackCount = 0;

    TextMeshProUGUI moneyText;

    Player_Test player_test;

    private void Awake()
    {
        Transform child = transform.GetChild(3);

        nameText = child.GetChild(2).GetComponent<TextMeshProUGUI>();

        dialogueText = transform.GetChild(2).GetComponent<TextMeshProUGUI>();

        NPCImage = transform.GetChild(0).GetComponent<Image>();

        npcSprites = new Sprite[5];
        npcSprites[0] = Resources.Load<Sprite>("NPC/Bearded");
        npcSprites[1] = Resources.Load<Sprite>("NPC/HatMan");
        npcSprites[2] = Resources.Load<Sprite>("NPC/OldMan");
        npcSprites[3] = Resources.Load<Sprite>("NPC/Woman");
        npcSprites[4] = Resources.Load<Sprite>("NPC/MazeKeeper");

        child = transform.GetChild(4);
        interactionNameText = child.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();

        child = transform.GetChild(4).GetChild(1);
        itemButtons = new Button[11];
        for(int i = 0; i< itemButtons.Length; i++)
        {
            itemButtons[i] = child.GetChild(i).GetComponent<Button>();

            int index = i;
            itemButtons[i].onClick.AddListener(() => OnItemButtonClick(index));

            itemButtons[i].gameObject.SetActive(false);
        }

        cancelButton = transform.GetChild(5).GetChild(0).GetComponent<Button>();
        cancelButton.onClick.AddListener(OnCancelButton);

        child = transform.GetChild(4);
        moneyText = child.GetChild(0).GetChild(2).GetComponent<TextMeshProUGUI>();

        canvasGroup = GetComponent<CanvasGroup>();
        //  OnCanvasGroup(false);       // 캔버스 그룹 조절 => Start로 이동
    }

    private void Start()
    {
        // 씬에 있는 모든 NPC를 찾아서 델리게이트 연결
        npcs = FindObjectsOfType<NPC>();
        foreach (var npc in npcs)
        {
            npc.onNPCDialogue += OnNPCDialogue;
        }

        // 구매 수량 패널 찾기
        purchaseQuantityPanel = FindAnyObjectByType<PurchaseQuantityPanel>();
        purchaseQuantityPanel.onItemPurchased += OnItemPurchased;
        purchaseQuantityPanel.gameObject.SetActive(false);

        mazeDifficultyPanel = FindAnyObjectByType<MazeDifficultyPanel>();
        mazeDifficultyPanel.onMazeCanceled += OnMazeCanceled;
        mazeDifficultyPanel.gameObject.SetActive(false);

        OnCanvasGroup(false);       // 캔버스 그룹 조절

        player_test = GameManager.Instance.Player_Test;
        player_test.onPlayerMoneyChange += OnMoneyChanged;
        player_test.ResetMotionAndPosition();

        StartCoroutine(firstMonsyChange());
    }

    /// <summary>
    /// 처음 게임매니저의 데이터 복구 후 소지금을 반영하는 코루틴
    /// </summary>
    /// <returns></returns>
    IEnumerator firstMonsyChange()
    {
        while (!GameManager.Instance.isDataRecovered)
        {
            yield return null;
        }
        moneyText.text = player_test.Money.ToString();
    }

    private void OnDisable()
    {
        player_test.onPlayerMoneyChange -= OnMoneyChanged;
        purchaseQuantityPanel.onItemPurchased -= OnItemPurchased;
    }

    /// <summary>
    /// 플레이어의 소지금 변경 시 바뀔 UI
    /// </summary>
    /// <param name="money"></param>
    private void OnMoneyChanged(int money)
    {
        moneyText.text = money.ToString();
    }

    /// <summary>
    /// 아이템 클릭으로 실행되는 함수(난이도 포함)
    /// </summary>
    /// <param name="index">아이템 번호</param>
    private void OnItemButtonClick(int index)
    {
        // 1. 유효한 아이템 인덱스인지 확인 (0~5번 버튼만 아이템 판매)
        if (index < 6)// && index < sellableItems.Length && sellableItems[index] != null)
        {
            // 인덱스로 아이템 추출
            ItemDataSO selectedItem = sellableItems[index];

            // 2. SO에서 최대 겹침 수 가져오기
            int maxStack = selectedItem.MaxStackCount;

            // 선택한 아이템의 가격
            int price = selectedItem.SelliPrice;

            // 3. 내 인벤토리에 현재 해당 아이템이 몇 개 있는지 가져오기
            // Inventory.Instance.GetItemCount()가 있다고 가정합니다.
            int currentOwnedCount = Inventory.Instance.GetItemCount(selectedItem);

            // 인벤토리에 해당 아이템이 비었을 때 -1로 표시되는 듯
            if(currentOwnedCount == -1)
            {
                currentOwnedCount = 0;
            }

            // 4. 구매 가능한 수량 계산 (최대치 - 현재 보유량)
            // 결과가 0보다 작아지지 않도록 Mathf.Max를 사용합니다.
            int buyableCount = Mathf.Max(0, maxStack - currentOwnedCount);

            maxStackCount = buyableCount;

            Debug.Log($"{selectedItem.ItemName} 클릭됨. 최대: {maxStack}, 보유: {currentOwnedCount}, 구매가능: {buyableCount}");

            if (purchaseQuantityPanel != null)
            {
                purchaseQuantityPanel.gameObject.SetActive(true);

                // 5. 계산된 수량을 패널에 전달
                purchaseQuantityPanel.Show(selectedItem, maxStackCount, price);
            }
        }

        // indxe가 6 7 8 9 10 인 경우
        else if (5 < index && index < 11)
        {
            switch (index)
            {
                case 6:
                    Debug.Log("이지 난이도 선택");
                    mazeDifficultyPanel.targetDifficultyName = "Easy";
                    GameManager.Instance.GameDifficulty = GameDifficulty.Easy;
                    break;

                case 7:
                    Debug.Log("노말 난이도 선택");
                    mazeDifficultyPanel.targetDifficultyName = "Normal";
                    GameManager.Instance.GameDifficulty = GameDifficulty.Normal;
                    break;

                case 8:
                    Debug.Log("하드 난이도 선택");
                    mazeDifficultyPanel.targetDifficultyName = "Hard";
                    GameManager.Instance.GameDifficulty = GameDifficulty.Hard;
                    break;

                case 9:
                    Debug.Log("나이트메어 난이도 선택");
                    mazeDifficultyPanel.targetDifficultyName = "Nightmare";
                    GameManager.Instance.GameDifficulty = GameDifficulty.Nightmare;
                    break;

                case 10:
                    Debug.Log("헬 난이도 선택");
                    mazeDifficultyPanel.targetDifficultyName = "Hell";
                    GameManager.Instance.GameDifficulty = GameDifficulty.Hell;
                    break;
            }
            mazeDifficultyPanel.gameObject.SetActive(true);
        }
        else
        {
            Debug.Log($"sellableItems 인스펙터에서 비었는지 봐라");
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
        // NPC 타입에 맞게 nameText 변경
        switch (npcType)
        {
            // 빵 판매 NPC
            case NPC.NPCType.Bearded:
                NPCImage.sprite = npcSprites[0];
                itemButtons[0].gameObject.SetActive(true);      // 빵 활성화
                interactionNameText.text = "아이템 선택";
                nameText.text = "수염난 아저씨";
                dialogueText.text = @"허허, 손님! 허기진 것 같구만.
배도 채우고 힘도 내야 모험이 수월하지 않겠나?";
                // 사간 후 : 맛있게 먹고 힘내시게, 허허.
                // 안사면 : 허허, 힘내고 싶으면 하나 사가지 그래.
                break;

            // 물약 판매 NPC
            case NPC.NPCType.HatMan:
                NPCImage.sprite = npcSprites[1];
                itemButtons[1].gameObject.SetActive(true);      // 물약 활성화
                interactionNameText.text = "아이템 선택";
                nameText.text = "모자 쓴 남자";
                dialogueText.text = @"자네, 힘 빠진 것 같군. 체력 회복 포션 하나 어떤가?
위급할 때 언제든지 쓸 수 있네만";
                // 사면 : 좋네, 사갔구만. 힘이 빠지면 또 들르게.
                // 안사면 : 허허, 안 살 거야? 어쩔 수 없지. 다음에 또 들르게.
                break;

            // 부활초 판매 NPC
            case NPC.NPCType.OldMan:
                NPCImage.sprite = npcSprites[2];
                itemButtons[2].gameObject.SetActive(true);      // 부활초 활성화
                interactionNameText.text = "아이템 선택";
                nameText.text = "늙은 남자";
                dialogueText.text = @"어이, 손님. 부활초 하나 사보겠나?
이게 있으면 쓰러져도 다시 일어날 수 있어. 필요하면 사가게나.";
                // 사면 : 허허, 잘 샀네. 이게 있으면 쓰러져도 금방 일어날 걸세.
                // 안사면 : 가격도 싸게 줄 테니 부담 갖지 말게.
                break;

            // 과일 판매 NPC
            case NPC.NPCType.Woman:
                NPCImage.sprite = npcSprites[3];
                itemButtons[3].gameObject.SetActive(true);      // 사과
                itemButtons[4].gameObject.SetActive(true);      // 바나나
                itemButtons[5].gameObject.SetActive(true);      // 체리 활성화
                interactionNameText.text = "아이템 선택";
                nameText.text = "아름다운 여성";
                dialogueText.text = @"안녕~ 과일 좀 사갈래?
이건 오늘 아침에 딴 거라 신선해.
먹으면 기운도 나고 기분도 좋아질 걸?";
                // 사면 : 고마워~ 맛있게 먹어!
                // 안사면 : 괜찮아, 다음에 또 올 거지? 기다리고 있을게~
                break;

            // 미궁 난이도 NPC
            case NPC.NPCType.MazeKeeper:
                NPCImage.sprite = npcSprites[4];
                itemButtons[6].gameObject.SetActive(true);      // Easy
                itemButtons[7].gameObject.SetActive(true);      // Normal
                itemButtons[8].gameObject.SetActive(true);      // Hard
                itemButtons[9].gameObject.SetActive(true);      // Nightmare
                itemButtons[10].gameObject.SetActive(true);     // Hell 활성화
                interactionNameText.text = "난이도 선택";
                nameText.text = "미궁 문지기";
                dialogueText.text =
                    @"미궁에 들어갈거야?
좋아, 네가 선택한 난이도를 확인하는 중이야.";
                // 미궁 확인 : ㅇㅇ 난이도 맞지? 무사귀환을 빌게??
                // 미궁 취소 : 역시 그건 무리지???
                break;
        }

        OnCanvasGroup(true);        // 패널 활성화
    }

    private void OnCanvasGroup(bool value)
    {
        if(value == false)
        {
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            for(int i = 0; i < itemButtons.Length; i++)
            {
                itemButtons[i].gameObject.SetActive(false);
            }

            purchaseQuantityPanel.gameObject.SetActive(false);
            mazeDifficultyPanel.gameObject.SetActive(false);
        }
        else
        {
            canvasGroup.alpha = 1;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
    }

    /// <summary>
    /// 취소 버튼 클릭
    /// </summary>
    private void OnCancelButton()
    {
        OnCanvasGroup(false);
    }

    /// <summary>
    /// 아이템 구매 여부로 NPC 대사를 바꾸는 함수
    /// </summary>
    /// <param name="purchased">구매했으면 true, 아니면 false</param>
    private void OnItemPurchased(bool purchased)
    {
        // 1. NPC 이미지(Sprite)를 비교하여 대사 결정
        if (NPCImage.sprite == npcSprites[0]) // Bearded
        {
            dialogueText.text = purchased ? "맛있게 먹고 힘내시게, 허허." : "허허, 힘내고 싶으면 하나 사가지 그래.";
        }
        else if (NPCImage.sprite == npcSprites[1]) // HatMan
        {
            dialogueText.text = purchased ? "좋네, 사갔구만. 힘이 빠지면 또 들르게." : "허허, 안 살 거야? 어쩔 수 없지. 다음에 또 들르게.";
        }
        else if (NPCImage.sprite == npcSprites[2]) // OldMan
        {
            dialogueText.text = purchased ? "허허, 잘 샀네. 이게 있으면 쓰러져도 금방 일어날 걸세." : "가격도 싸게 줄 테니 너무 부담 갖지 말게.";
        }
        else if (NPCImage.sprite == npcSprites[3]) // Woman
        {
            dialogueText.text = purchased ? "고마워~ 맛있게 먹어!" : "괜찮아, 다음에 또 올 거지? 기다리고 있을게~";
        }
    }


    /// <summary>
    /// 미궁 이동 여부로 대사를 변경하는 함수
    /// </summary>
    /// <param name="go">true: 미궁 진입, false: 취소</param>
    private void OnMazeCanceled(bool go)
    {
        if (!go)
        {
            dialogueText.text =
                    @"조금 더 준비하고 싶은 거지? 알겠어.
충분히 준비가 되면 언제든 다시 말해줘. 여기서 기다릴테니까.";
        }
        else
        {
            dialogueText.text =
            @"위험한 곳이지만 너라면 충분히 해낼 수 있을 거야.
다치지 않게 조심해서 다녀와!";
        }
    }
}
