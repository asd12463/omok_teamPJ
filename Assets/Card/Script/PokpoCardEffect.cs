using UnityEngine;

public class PokpoCardEffect : CardEffect
{
    public GameObject clearEffectPrefab;

    public override void Execute(int x, int y, omokMain omok)
    {
        // [싱글톤 매니저 호출 방식으로 변경]
        if (cardSFX != null)
        {
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySFX(cardSFX);
                Debug.Log($"[SoundManager] {cardSFX.name} 재생 요청 성공.");
            }
            else
            {
                Debug.LogError("[사운드 에러] 씬에 SoundManager 오브젝트가 배치되어 있지 않습니다!");
                // 백업용 즉시 재생 안전장치
                AudioSource.PlayClipAtPoint(cardSFX, Camera.main.transform.position);
            }
        }
        else
        {
            Debug.LogWarning("[사운드 경고] 현재 카드 프리팹에 cardSFX(오디오 클립)가 등록되어 있지 않습니다.");
        }

        // --- 이하 폭포 연출 및 돌 파괴 로직 (동일) ---
        int boardSize = 19;
        int topY = boardSize - 1;

        if (clearEffectPrefab != null && omok.chagsu[x, topY] != null)
        {
            Vector3 effectPos = omok.chagsu[x, topY].transform.position;
            effectPos.y += 3.0f;
            effectPos.z = -5f;

            Instantiate(clearEffectPrefab, effectPos, Quaternion.identity);
            Debug.Log($"[폭포 연출] {x}열 맨 위 좌표({x}, {topY})에서 이펙트 시작.");
        }

        for (int ny = 0; ny < boardSize; ny++)
        {
            if (omok.board[x, ny] != 0)
            {
                StoneManager.Instance.ClearStone(x, ny);
            }
        }
        Debug.Log($"카드 효과 발동: 세로 {x}열의 모든 돌을 파괴했습니다.");
    }
}