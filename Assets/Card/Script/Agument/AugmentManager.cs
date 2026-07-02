using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using Photon.Pun;

public class AugmentManager : MonoBehaviour
{
    public static AugmentManager Instance;

    [Header("UI 연결")]
    public GameObject selectionPanel;
    public Transform cardSpawnParent;
    public Transform saveSlotParent;
    public Text phaseText;
    public GameObject drawTurnImage;
    public GameObject dimBackground;

    [Header("프리팹 데이터")]
    public List<GameObject> cardPrefabsPool = new List<GameObject>();

    [Header("상태 관리")]
    private List<GameObject> centerCards = new List<GameObject>();
    public bool isSavePhase = true;

    [Header("턴 관리")]
    public Button endTurnButton;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        if (drawTurnImage != null) drawTurnImage.SetActive(false);
        if (selectionPanel != null) selectionPanel.SetActive(false);
    }

    public void StartUserTurn()
    {
        StopAllCoroutines();
        Debug.Log("<color=yellow>[턴 시작 알림] AugmentManager: StartUserTurn() 연출을 시작합니다.</color>");
        StartCoroutine(DrawTurnSequence());
    }

    // ==================================================================================
    // ★ [수정 사항 반영] 턴 연출 시작 시, 매니저 오브젝트 전체를 먼저 깨워 드로우 이미지를 정상 노출합니다.
    // ==================================================================================
    IEnumerator DrawTurnSequence()
    {
        // 1. 혹시 부모 캔버스나 오브젝트 자체가 꺼져있을 경우를 대비해 스크립트가 붙은 자신을 확실하게 켭니다.
        this.gameObject.SetActive(true);

        // 2. 드로우 연출 이미지를 활성화하여 화면에 정상적으로 띄웁니다.
        if (drawTurnImage != null) drawTurnImage.SetActive(true);

        // 3초 동안 연출 유지
        yield return new WaitForSeconds(3.0f);

        if (drawTurnImage != null) drawTurnImage.SetActive(false);

        // 카드 선택 패널 오픈 및 카드 선택지 생성
        if (selectionPanel != null) selectionPanel.SetActive(true);
        ShowAugmentChoices();
    }

    public void ShowAugmentChoices()
    {
        if (selectionPanel != null) selectionPanel.SetActive(true);
        isSavePhase = true;

        if (phaseText != null) phaseText.text = "보관할 카드";

        foreach (GameObject card in centerCards)
        {
            if (card != null) Destroy(card);
        }
        centerCards.Clear();

        foreach (Transform child in cardSpawnParent)
        {
            Destroy(child.gameObject);
        }

        if (cardPrefabsPool != null && cardPrefabsPool.Count >= 2)
        {
            List<GameObject> tempPool = new List<GameObject>(cardPrefabsPool);

            for (int i = 0; i < 2; i++)
            {
                int randomIndex = Random.Range(0, tempPool.Count);
                GameObject randomPrefab = tempPool[randomIndex];

                GameObject cardInst = Instantiate(randomPrefab, cardSpawnParent);
                centerCards.Add(cardInst);

                tempPool.RemoveAt(randomIndex);

                AugmentUI ui = cardInst.GetComponent<AugmentUI>();
                if (ui != null) ui.SetCardInfo(this, false);
            }
        }
        else
        {
            Debug.LogError("Card Prefabs Pool에 카드가 부족합니다 (최소 2개 필요)");
        }
    }

    public void OnCardClicked(GameObject clickedCard, bool isFromSaveSlot)
    {
        if (isSavePhase && isFromSaveSlot)
        {
            isSavePhase = false;
            if (phaseText != null) phaseText.text = "사용할 카드";
            return;
        }

        if (isSavePhase)
        {
            if (!isFromSaveSlot) HandleSavePhase(clickedCard);
        }
        else
        {
            HandleUsePhase(clickedCard);
        }
    }

    private void HandleSavePhase(GameObject clickedCard)
    {
        clickedCard.transform.SetParent(saveSlotParent);

        RectTransform rect = clickedCard.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchoredPosition = Vector2.zero;
            rect.localScale = Vector3.one;
        }

        AugmentUI ui = clickedCard.GetComponent<AugmentUI>();
        if (ui != null) ui.SetCardInfo(this, true);

        centerCards.Remove(clickedCard);
        isSavePhase = false;
        if (phaseText != null) phaseText.text = "사용할 카드";
    }

    private void HandleUsePhase(GameObject clickedCard)
    {
        Debug.Log($"<color=orange>[카드 선택] {clickedCard.name} 손에 장전 완료. 카드 효과 사용 단계를 시작합니다.</color>");

        omokMain omok = FindAnyObjectByType<omokMain>();
        if (omok != null)
        {
            if (StoneManager.Instance != null)
            {
                StoneManager.Instance.pendingCardObject = clickedCard;
            }

            this.StartCoroutine(EnableCardPhaseDelayed(omok));
        }

        foreach (GameObject card in centerCards)
        {
            if (card != null && card != clickedCard) Destroy(card);
        }
        centerCards.Clear();

        clickedCard.SetActive(false);
        if (selectionPanel != null) selectionPanel.SetActive(false);
        isFieldVisible = false;
    }

    private System.Collections.IEnumerator EnableCardPhaseDelayed(omokMain omok)
    {
        yield return new WaitForEndOfFrame();
        if (omok != null)
        {
            omok.currentState = GameState.CardUsePhase;
            Debug.Log("<color=yellow>[페이즈 전환] GameState가 CardUsePhase(카드 효과 사용 단계)로 변경되었습니다.</color>");
        }
    }

    public void OnEndTurnButtonClicked()
    {
        if (selectionPanel != null) selectionPanel.SetActive(false);

        if (StoneManager.Instance != null)
        {
            if (StoneManager.Instance.pendingCardObject != null)
            {
                Destroy(StoneManager.Instance.pendingCardObject);
            }
            StoneManager.Instance.pendingCardObject = null;
        }

        omokMain omok = FindAnyObjectByType<omokMain>();
        if (omok != null)
        {
            omok.currentState = GameState.PlayerTurn;
        }
    }

    private bool isFieldVisible = false;
    public void ToggleFieldVisibility()
    {
        isFieldVisible = !isFieldVisible;
        if (cardSpawnParent != null) cardSpawnParent.gameObject.SetActive(!isFieldVisible);
        if (dimBackground != null) dimBackground.SetActive(!isFieldVisible);
        if (phaseText != null) phaseText.gameObject.SetActive(!isFieldVisible);
        if (endTurnButton != null) endTurnButton.gameObject.SetActive(!isFieldVisible);
    }
}