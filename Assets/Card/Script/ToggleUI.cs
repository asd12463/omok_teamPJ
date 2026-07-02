using UnityEngine;

public class ToggleUI : MonoBehaviour
{
    [Header("숨길 대상들")]
    public GameObject augmentPanel;
    public GameObject dimBackground;
    public GameObject saveSlotPanel; // 인스펙터 매칭 확인용

    [Header("시야 토글 버튼 오브젝트들")]
    public GameObject showFieldButton; // 필드 보기 버튼 (눈 뜬 모양)
    public GameObject hideFieldButton; // 필드 가리기 버튼 (눈 감은 모양)

    void Start()
    {
        // ==================================================================================
        // ★ [시작 상태 강제 고정] 첫 게임 시작 시 화면 상태를 완벽하게 정비합니다.
        // ==================================================================================
        if (augmentPanel != null) augmentPanel.SetActive(true);
        if (dimBackground != null) dimBackground.SetActive(true);
        if (saveSlotPanel != null) saveSlotPanel.SetActive(true);

        // 카드가 보이고 있으므로 -> 다음에 눌러야 할 버튼은 [눈 감은 모양]이어야 합니다.
        if (showFieldButton != null) showFieldButton.SetActive(false); // 눈 뜬 모양 OFF
        if (hideFieldButton != null) hideFieldButton.SetActive(true);   // 눈 감은 모양 ON
    }

    public void Toggle()
    {
        if (augmentPanel == null) return;

        // = "현재 카드 선택 패널이 켜져 있는가?"를 실시간으로 체크 (true / false)
        bool isPanelActive = augmentPanel.activeSelf;

        // 다음 상태는 무조건 현재 상태의 '반대'로 지정합니다.
        bool nextState = !isPanelActive;

        // 1. 카드 패널들의 상태를 일괄 전환합니다.
        augmentPanel.SetActive(nextState);
        if (dimBackground != null) dimBackground.SetActive(nextState);
        if (saveSlotPanel != null) saveSlotPanel.SetActive(nextState);

        // 2. [핵심 싱크] 다음 상태(nextState)에 맞춰서 버튼의 활성화 상태를 칼같이 동기화합니다.
        if (nextState == true)
        {
            // 카드가 다시 화면에 보이는 상태 -> 필드를 가려야 하므로 [눈 감은 모양]이 켜져야 함
            if (showFieldButton != null) showFieldButton.SetActive(false);
            if (hideFieldButton != null) hideFieldButton.SetActive(true);
        }
        else
        {
            // 카드가 숨겨져서 필드가 훤히 보이는 상태 -> 카드를 다시 봐야 하므로 [눈 뜬 모양]이 켜져야 함
            if (showFieldButton != null) showFieldButton.SetActive(true);
            if (hideFieldButton != null) hideFieldButton.SetActive(false);
        }
    }
}