using UnityEngine;

// CardEffect를 상속받아 구조만 유지합니다.
public class PadoCardEffect : CardEffect
{
    // 유니티 인스펙터에서 아까 만든 가로줄용 애니메이션 프리팹을 넣으세요.
    public GameObject PadoPrefab;

    public override void Execute(int x, int y, omokMain omok)
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
        // 바둑판의 가로 크기만큼 반복합니다 (보통 19칸)
        int boardSize = 19;

        // x값에 상관없이 클릭한 지점의 y(행) 좌표를 기준으로 전체 순회
        for (int nx = 0; nx < boardSize; nx++)
        {
            // 1. 이펙트 생성 위치 가져오기 (해당 행의 모든 x칸)
            // omok.chagsu[nx, y]를 사용하여 해당 줄의 모든 좌표를 가져옵니다.
            Vector3 pos = omok.chagsu[nx, y].transform.position;
            pos.z = -1f;

            // 2. 애니메이션 프리팹 생성
            if (PadoPrefab != null)
            {
                Instantiate(PadoPrefab, pos, Quaternion.identity);
            }

            // 3. 돌 삭제 로직 실행 (이미 만들어두신 ClearStone 활용)
            // 이 함수가 내부적으로 돌 오브젝트 Destroy와 데이터 0 처리를 다 해줄 겁니다.
            StoneManager.Instance.ClearStone(nx, y);
        }

        Debug.Log($"{y}번 가로줄 전체 청소 완료!");
    }
}

