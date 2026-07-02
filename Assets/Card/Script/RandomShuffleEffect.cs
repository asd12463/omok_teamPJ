using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

public class RandomShuffleEffect : CardEffect
{
    [Header("셔플 연출 프리팹 (선택)")]
    public GameObject shuffleVisualPrefab;

    // Invoke 기능을 쓰기 위해 omokMain 객체를 임시 저장할 변수
    private omokMain cachedOmok;

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
        Debug.Log("<color=purple>[카드 발동] 모든 돌 배치를 무작위로 섞습니다!</color>");

        // 1. 소용돌이 연출 즉시 생성
        SpawnShuffleTornado(omok);

        // 2. 방장 컴퓨터인 경우에만 1초 뒤에 계산 메서드가 실행되도록 예약(Invoke)합니다.
        if (PhotonNetwork.IsMasterClient)
        {
            cachedOmok = omok;

            // ★ 코루틴 대신 유니티의 Invoke를 사용하여 1.0초 뒤에 아래 함수를 강제 실행합니다.
            // 이 방식은 오브젝트 파괴 여부와 상관없이 엔진이 타이머를 돌려줍니다.
            Invoke("CalculateAndSyncShuffleWithoutCoroutine", 1.0f);
        }
    }

    private void SpawnShuffleTornado(omokMain omok)
    {
        if (shuffleVisualPrefab != null)
        {
            Vector3 centerLeftPos = omok.chagsu[0, 12].transform.position;
            centerLeftPos.x -= 10f;
            centerLeftPos.z = -5f;

            GameObject tornado = Instantiate(shuffleVisualPrefab, centerLeftPos, Quaternion.identity);
            tornado.transform.localScale = new Vector3(3f, 3f, 3f);
        }
    }

    // 1초 뒤에 호출될 실제 계산 함수 (코루틴 아님)
    private void CalculateAndSyncShuffleWithoutCoroutine()
    {
        if (cachedOmok == null) return;

        List<int> existingStones = new List<int>();
        List<Vector2Int> allPositions = new List<Vector2Int>();

        for (int r = 0; r < 19; r++)
        {
            for (int c = 0; c < 19; c++)
            {
                allPositions.Add(new Vector2Int(r, c));
                if (cachedOmok.board[r, c] == 1 || cachedOmok.board[r, c] == 2)
                {
                    existingStones.Add(cachedOmok.board[r, c]);
                }
            }
        }

        for (int i = allPositions.Count - 1; i > 0; i--)
        {
            int rnd = Random.Range(0, i + 1);
            Vector2Int temp = allPositions[i];
            allPositions[i] = allPositions[rnd];
            allPositions[rnd] = temp;
        }

        int[] newXPositions = new int[existingStones.Count];
        int[] newYPositions = new int[existingStones.Count];
        int[] stoneOwners = new int[existingStones.Count];

        for (int i = 0; i < existingStones.Count; i++)
        {
            newXPositions[i] = allPositions[i].x;
            newYPositions[i] = allPositions[i].y;
            stoneOwners[i] = existingStones[i];
        }

        // 기존에 omokMain에 있던 원래 RPC 함수를 그대로 호출!
        cachedOmok.photonView.RPC("RPC_ApplyShuffleResult", RpcTarget.All, newXPositions, newYPositions, stoneOwners);
    }
}