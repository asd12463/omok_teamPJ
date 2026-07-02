using Photon.Pun;
using UnityEngine;

public class chagsu : MonoBehaviour
{
    public int gridX;
    public int gridY;
    private omokMain omokMainScript;
    private bool isOccupied = false;

    void Awake()
    {
        omokMainScript = FindAnyObjectByType<omokMain>();
    }

    private void OnMouseEnter()
    {
        if (omokMainScript == null) return;

        bool canShowPreview = false;

        // ----------------------------------------------------------------------
        // [분기 1] 카드 효과 사용 단계 (GameState.CardUsePhase)
        // ----------------------------------------------------------------------
        if (omokMainScript.currentState == GameState.CardUsePhase)
        {
            // 현재 플레이어가 사용하려고 장전한 카드가 무엇인지 StoneManager에서 알아냅니다.
            CardEffect currentCard = null;
            if (StoneManager.Instance != null && StoneManager.Instance.pendingCardObject != null)
            {
                currentCard = StoneManager.Instance.pendingCardObject.GetComponent<CardEffect>();
            }

            // 장전된 카드가 '금지 구역/금지 돌' 관련 카드인 경우
            if (currentCard != null && (currentCard is deleate || currentCard.gameObject.name.Contains("Forbidden")))
            {
                // 금지 구역 카드는 이미 무언가 놓여있는 칸(!isOccupied)에는 조준경조차 허용하지 않습니다!
                canShowPreview = !isOccupied;
            }
            else
            {
                // 폭탄 카드 등 일반적인 카드는 기존 기획대로 돌이 있든 없든 무조건 조준경(프리뷰) OK!
                canShowPreview = true;
            }
        }
        // ----------------------------------------------------------------------
        // [분기 2] 순수 오목돌 배치 단계 (GameState.PlayerTurn)
        // ----------------------------------------------------------------------
        else if (omokMainScript.currentState == GameState.PlayerTurn)
        {
            // 일반 돌을 놓을 때는 당연히 아무것도 없는 깨끗한 빈 칸만 조준경 OK!
            canShowPreview = !isOccupied;
        }

        // ----------------------------------------------------------------------
        // [프리뷰 출력] 조건이 만족하고 내 네트워크 턴일 때만 화면에 보여줍니다.
        // ----------------------------------------------------------------------
        if (canShowPreview && omokMainScript.IsMyTurn())
        {
            // 기존의 고정된 '1(흑돌)' 대신, 현재 로그인한 로컬 플레이어의 돌 번호(방장:1, 게스트:2)를 유연하게 전달합니다.
            int myNumber = Photon.Pun.PhotonNetwork.IsMasterClient ? 1 : 2;
            omokMainScript.ShowPreview(gridX, gridY, myNumber);
        }
    }

    // ==================================================================================
    // ★ [수정 사항 반영] 마우스가 칸 밖으로 벗어날 때 가이드 돌을 화면에서 깔끔히 지워줍니다.
    // ==================================================================================
    private void OnMouseExit()
    {
        if (omokMainScript == null) return;

        // 카드 사용 단계이거나 일반 착수 단계일 때 마우스가 나간다면 프리뷰를 숨깁니다.
        if (omokMainScript.currentState == GameState.CardUsePhase || omokMainScript.currentState == GameState.PlayerTurn)
        {
            omokMainScript.HidePreview();
        }
    }

    void OnMouseDown()
    {
        if (omokMainScript == null) return;

        if (!omokMainScript.IsMyTurn())
        {
            Debug.Log("상대방의 네트워크 턴입니다. 기다려주세요.");
            return;
        }

        // ----------------------------------------------------------------------
        // [페이즈 1] 카드 효과 사용 단계 (GameState.CardUsePhase)
        // ----------------------------------------------------------------------
        if (omokMainScript.currentState == GameState.CardUsePhase)
        {
            CardEffect currentCard = null;
            if (StoneManager.Instance != null && StoneManager.Instance.pendingCardObject != null)
            {
                currentCard = StoneManager.Instance.pendingCardObject.GetComponent<CardEffect>();
            }

            // 지금 들고 있는 카드가 금지 카드인데, 이미 돌이 있는 칸(isOccupied)을 눌렀다면?
            if (currentCard != null && (currentCard is deleate || currentCard.gameObject.name.Contains("Forbidden")))
            {
                if (isOccupied)
                {
                    Debug.Log("<color=red>[착수 제한] 이미 돌이 있는 곳에는 금지 카드를 사용할 수 없습니다! 다른 칸을 선택하세요.</color>");
                    return;
                }
            }

            // 정상적인 위치일 때만 카드 효과 실행
            bool cardUsed = StoneManager.Instance.ExecuteEffect(gridX, gridY);

            if (cardUsed)
            {
                omokMainScript.HidePreview();

                if (currentCard != null && currentCard is HansuCardEffect)
                {
                    return;
                }
            }
            return;
        }

        // ----------------------------------------------------------------------
        // [페이즈 2] 순수 오목돌 배치 단계 (GameState.PlayerTurn)
        // ----------------------------------------------------------------------
        if (omokMainScript.currentState == GameState.PlayerTurn)
        {
            if (isOccupied)
            {
                Debug.Log("이미 돌이 놓인 자리입니다.");
                return;
            }

            int myNumber = Photon.Pun.PhotonNetwork.IsMasterClient ? 1 : 2;
            omokMainScript.PlaceStone(gridX, gridY, myNumber);
        }
    }

    public void SetOccupied()
    {
        isOccupied = true;
    }

    public void SetEmpty()
    {
        isOccupied = false;
        Debug.Log($"<color=gray>({gridX}, {gridY}) 칸이 비워졌습니다.</color>");
    }
}