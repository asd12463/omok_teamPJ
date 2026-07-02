using UnityEngine;
using Photon.Pun;

public class HansuCardEffect : CardEffect
{
    [Header("시전자(나)가 획득할 보너스 코스트 양")]
    public int boostAmount = 4;

    public override void Execute(int x, int y, omokMain omok) { }

    public override void ExecuteWithCaster(int x, int y, omokMain omok, int casterNumber)
    {
        // [사운드 재생 로직 시작] 다른 카드 함수 맨 위에 이대로 붙여넣으세요!
        if (cardSFX != null)
        {
            if (SoundManager.Instance != null)
            {
                // 싱글톤 매니저를 통해 오디오 믹서가 적용된 효과음 재생
                SoundManager.Instance.PlaySFX(cardSFX);
                Debug.Log($"[SoundManager] {cardSFX.name} 재생 요청 성공.");
            }
            else
            {
                Debug.LogError("[사운드 에러] 씬에 SoundManager 오브젝트가 배치되어 있지 않습니다!");
                // 매니저가 없을 때를 대비한 3D 공간음 백업 안전장치
                AudioSource.PlayClipAtPoint(cardSFX, Camera.main.transform.position);
            }
        }
        else
        {
            Debug.LogWarning("[사운드 경고] 현재 카드 프리팹에 cardSFX(오디오 클립)가 등록되어 있지 않습니다.");
        }
        // [사운드 재생 로직 끝] 이 아래부터 카드의 고유 효과(돌 파괴, 이동 등)를 작성하시면 됩니다.
        if (omok != null)
        {
            // ----------------------------------------------------------------------
            // 1단계: 카드를 쓴 '시전자 본인'에게만 보너스 4코스트를 지급합니다.
            // ----------------------------------------------------------------------
            if (casterNumber == 1 && omok.masterCostScript != null)
            {
                omok.masterCostScript.AddCost(boostAmount);
                Debug.Log($"<color=green>[한수쉬기] 방장이 카드를 써서 보너스 {boostAmount} 코스트 획득.</color>");
            }
            else if (casterNumber == 2 && omok.guestCostScript != null)
            {
                omok.guestCostScript.AddCost(boostAmount);
                Debug.Log($"<color=yellow>[한수쉬기] 게스트가 카드를 써서 보너스 {boostAmount} 코스트 획득.</color>");
            }

            // ----------------------------------------------------------------------
            // 2단계: 1인칭 시점으로 안전하게 턴 넘기기
            // ----------------------------------------------------------------------
            int myLocalNumber = PhotonNetwork.IsMasterClient ? 1 : 2;

            if (casterNumber == myLocalNumber)
            {
                // [A. 내가 카드를 쓴 본인 컴퓨터] -> 대기 상태로 전환 및 UI 차단
                omok.currentState = GameState.AITurn;
                if (omok.timerScript != null) omok.timerScript.StopTimer();

                if (AugmentManager.Instance != null) AugmentManager.Instance.gameObject.SetActive(false);
                if (omok.augmentCanvas != null) omok.augmentCanvas.SetActive(false);

                Debug.Log("<color=red>[턴 제어] 한수쉬기 발동 완료. 내 화면 대기 모드 진입.</color>");
            }
            else
            {
                // [B. 카드를 당한 상대방 컴퓨터] 
                // ★ [핵심] 여기서 직접 AddCost(1) 하던 코드를 완전히 삭제했습니다!
                // 상대방은 이 아래 줄에 의해 GameState.draw 상태가 되는 순간,
                // 원래 omokMain 시스템에 들어있는 'StartMyLocalTurn()'이 발동하면서 
                // 정직하게 기본 턴 보너스 코스트 딱 1만 오르게 됩니다.

                omok.currentState = GameState.draw;

                if (omok.timerScript != null) omok.timerScript.ResumeTimer();

                // 상대방 화면에만 카드 드로우창 활성화
                if (AugmentManager.Instance != null)
                {
                    AugmentManager.Instance.gameObject.SetActive(true);
                    if (omok.augmentCanvas != null) omok.augmentCanvas.SetActive(true);
                    AugmentManager.Instance.StartUserTurn();
                }

                Debug.Log("<color=blue>[턴 제어] 상대방이 한수쉬기를 썼으므로, 시스템 기본 턴 보너스(+1)만 받으며 내 턴을 시작합니다!</color>");
            }
        }
    }
}