using UnityEngine;
using Photon.Pun;

public class cost : MonoBehaviour
{
    // [기존 변수들]
    public SpriteRenderer tenRenderer;
    public SpriteRenderer oneRenderer;
    public Sprite[] numberSprites;
    public int costValue = 0;

    [Header("멀티플레이 내 화면 전용 노출 설정")]
    // 유니티 인스펙터 창에서 방장용 코스트 오브젝트는 체크, 게스트용 코스트 오브젝트는 체크 해제합니다.
    [SerializeField] private bool isMasterCostContainer = false;

    // 실시간 감지를 위해 이전 코스트 저장용 변수 추가
    private int lastCostValue = -1;

    void Start()
    {
        // ★ [핵심 해결책] 내 화면에 필요 없는 상대방 코스트 UI를 완전히 숨깁니다.
        FilterCostUIByRole();

        // 게임 시작 시 현재 코스트 수치로 화면을 강제 갱신합니다.
        UpdateDisplay();
    }

    void Update()
    {
        // 매 프레임 감시하며 costValue 변수가 변했다면 화면을 갱신합니다.
        if (costValue != lastCostValue)
        {
            UpdateDisplay();
        }
    }

    /// <summary>
    /// 네트워크 권한을 체크하여 내 주머니가 아닌 상대방 주머니 오브젝트는 화면에서 완전히 끕니다.
    /// </summary>
    private void FilterCostUIByRole()
    {
        // 현재 내가 방장(MasterClient)인지 확인
        bool amIMaster = PhotonNetwork.IsMasterClient;

        // 이 오브젝트가 '나의 주머니' 인가?
        // (내가 방장인데 방장그릇이거나, 내가 게스트인데 게스트그릇인 경우)
        bool isMyCostContainer = (amIMaster && isMasterCostContainer) || (!amIMaster && !isMasterCostContainer);

        if (!isMyCostContainer)
        {
            // ★ 상대방의 코스트 관리 오브젝트라면 내 화면에서 아예 삭제/숨김 처리합니다.
            this.gameObject.SetActive(false);
            Debug.Log($"<color=gray>[UI 필터링] 상대방의 코스트 UI({gameObject.name})를 화면에서 숨겼습니다.</color>");
        }
    }

    // ----------------------------------------------------------------------
    // AugmentUI에서 사용할 코스트 검사 함수
    // ----------------------------------------------------------------------
    public bool CanAfford(int amount)
    {
        return costValue >= amount;
    }

    // [기존 함수들]
    public void AddCost(int amount)
    {
        costValue += amount;
        UpdateDisplay();
    }

    public void ConsumeCost(int amount)
    {
        costValue -= amount;
        if (costValue < 0) costValue = 0;
        UpdateDisplay();
    }

    // [화면 갱신 로직]
    public void UpdateDisplay()
    {
        lastCostValue = costValue;

        if (numberSprites == null || numberSprites.Length < 10)
        {
            Debug.LogWarning($"[{gameObject.name}] numberSprites 배열에 숫자가 10개 이상 등록되지 않았습니다!");
            return;
        }

        int costTen = costValue / 10;
        int costOne = costValue % 10;

        if (tenRenderer != null)
        {
            int tenIndex = Mathf.Clamp(costTen, 0, 9);
            tenRenderer.sprite = numberSprites[tenIndex];
        }

        if (oneRenderer != null)
        {
            int oneIndex = Mathf.Clamp(costOne, 0, 9);
            oneRenderer.sprite = numberSprites[oneIndex];
        }
    }
}