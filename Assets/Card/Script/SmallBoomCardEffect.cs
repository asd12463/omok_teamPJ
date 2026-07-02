using UnityEngine;

public class SmallBoomCardEffect : CardEffect
{
    [Header("폭발 애니메이션 프리팹")]
    // 유니티 인스펙터에서 'boom' 프리팹을 여기에 드래그해서 넣으세요.
    public GameObject explosionAnimationPrefab;

    // ★ 우리 시스템의 규격에 맞게 ExecuteWithCaster를 오버라이드합니다.
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
        // 범위 루프(for문)를 전부 제거하고, 입력받은 (x, y) 좌표 딱 1칸만 검사합니다.
        if (x >= 0 && x < 19 && y >= 0 && y < 19)
        {
            Debug.Log($"<color=red>[폭탄 발동] ({x}, {y}) 지점의 돌을 파괴합니다!</color>");

            // 1. 이펙트 생성 위치 가져오기
            Vector3 pos = omok.chagsu[x, y].transform.position;
            pos.z = -1f; // 바둑돌보다 살짝 레이어를 앞으로 당겨서 연출이 돌을 덮도록 설정

            // 2. 폭발 애니메이션 프리팹 생성
            if (explosionAnimationPrefab != null)
            {
                Instantiate(explosionAnimationPrefab, pos, Quaternion.identity);
            }

            // 3. StoneManager를 통해 해당 칸의 오목돌 오브젝트 파괴 및 데이터(board, chagsu) 초기화
            if (StoneManager.Instance != null)
            {
                StoneManager.Instance.ClearStone(x, y);
            }
        }
    }
}

