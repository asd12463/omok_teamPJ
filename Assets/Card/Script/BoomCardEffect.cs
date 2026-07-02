using UnityEngine;

public class BombEffect : CardEffect
{
    // 유니티 인스펙터에서 'boom' 프리팹을 여기에 드래그해서 넣으세요.
    public GameObject explosionAnimationPrefab;

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
        // 3x3 범위를 순회하며 돌을 지우고 이펙트를 생성합니다.
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                int nx = x + i;
                int ny = y + j;

                if (nx >= 0 && nx < 19 && ny >= 0 && ny < 19)
                {
                    // 1. 이펙트 생성 위치 가져오기
                    Vector3 pos = omok.chagsu[nx, ny].transform.position;
                    pos.z = -1f; // 돌보다 살짝 앞에서 보이게 설정

                    // 2. 애니메이션 프리팹 생성
                    if (explosionAnimationPrefab != null)
                    {
                        Instantiate(explosionAnimationPrefab, pos, Quaternion.identity);
                    }

                    // 3. 돌 삭제 로직 실행
                    StoneManager.Instance.ClearStone(nx, ny);
                }
            }
        }
    }
}