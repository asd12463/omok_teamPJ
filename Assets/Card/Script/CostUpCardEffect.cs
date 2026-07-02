using UnityEngine;
using Photon.Pun;

public class CostUpCardEffect : CardEffect
{
    public int boostAmount = 2;

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
            // ★ [이중 RPC 차단] 
            // omok.AddPlayerCost(casterNumber, boostAmount); <-- 기존의 이 코드는 내부 RPC를 또 실행하므로 지워야 합니다!

            // 아래와 같이 각 컴퓨터의 스크립트 그릇에 직접(로컬로) 수치를 더해주어야 중복 없이 딱 2만 깔끔하게 오릅니다.
            if (casterNumber == 1 && omok.masterCostScript != null)
            {
                omok.masterCostScript.AddCost(boostAmount);
                Debug.Log($"<color=green>[부스트 효과] 방장 그릇에 로컬로 {boostAmount} 코스트 직접 추가 완료.</color>");
            }
            else if (casterNumber == 2 && omok.guestCostScript != null)
            {
                omok.guestCostScript.AddCost(boostAmount);
                Debug.Log($"<color=yellow>[부스트 효과] 게스트 그릇에 로컬로 {boostAmount} 코스트 직접 추가 완료.</color>");
            }
        }
    }
}