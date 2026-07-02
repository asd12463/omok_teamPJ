using UnityEngine;

public class deleate : CardEffect
{
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
        // 1. 좌표 안전 검사
        if (x < 0 || x >= 19 || y < 0 || y >= 19) return;

        // ★ [철저한 차단] 빈칸(0)이 아니라 흑돌(1), 백돌(2), 이미 설치된 금지돌(3)이 있다면 절대 발동 불가!
        if (omok.board[x, y] != 0)
        {
            Debug.Log("<color=red>[경고] 이미 돌이 배치된 위치에는 금지 구역을 만들 수 없습니다!</color>");
            return;
        }

        Debug.Log($"<color=orange>[카드 발동] 빈 공간인 ({x}, {y}) 지점을 영구 금지 구역으로 지정합니다!</color>");

        // 방장 컴퓨터가 계산하여 모든 클라이언트 화면에 금지 돌 동기화 생성
        omok.photonView.RPC("RPC_PlaceForbiddenStone", Photon.Pun.RpcTarget.All, x, y);
    }
}
