using UnityEngine;
using System.Collections;
using Photon.Pun; // ★ 포톤 네트워킹 사용을 위해 반드시 필요

public class MeteorCardEffect : CardEffect
{
    public GameObject meteorExplosionPrefab;
    private PhotonView pv; // 포톤 뷰 컴포넌트

    private void Awake()
    {
        // 내 오브젝트나 부모에 있는 PhotonView를 가져옵니다.
        pv = GetComponent<PhotonView>();
        if (pv == null)
        {
            pv = FindAnyObjectByType<PhotonView>();
        }
    }

    public override void Execute(int x, int y, omokMain omok)
    {
        // [사운드 재생 로직]
        if (cardSFX != null && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(cardSFX);
        }

        // ★ [네트워크 핵심] 내가 카드를 낸 로컬 플레이어라면, 
        // 직접 메테오가 떨어질 랜덤 좌표 2개를 미리 뽑아서 상대방에게 동시에 전송합니다.
        if (PhotonNetwork.InRoom)
        {
            // 첫 번째 운석 좌표
            int rX1 = Random.Range(0, 19);
            int rY1 = Random.Range(0, 19);

            // 두 번째 운석 좌표
            int rX2 = Random.Range(0, 19);
            int rY2 = Random.Range(0, 19);

            // RPC를 통해 방장과 게스트 모두에게 똑같은 좌표를 보내서 실행하게 만듭니다.
            pv.RPC("RPC_ExecuteMeteor", RpcTarget.All, rX1, rY1, rX2, rY2);
        }
        else
        {
            // 싱글 플레이(테스트용) 백업
            AugmentManager.Instance.StartCoroutine(DropMeteors(omok, Random.Range(0, 19), Random.Range(0, 19), Random.Range(0, 19), Random.Range(0, 19)));
        }
    }

    // ★ [네트워크 핵심] 상대방 컴퓨터와 내 컴퓨터에서 똑같은 좌표로 메테오를 터뜨리는 네트워크 전용 함수
    [PunRPC]
    private void RPC_ExecuteMeteor(int x1, int y1, int x2, int y2)
    {
        omokMain omok = FindAnyObjectByType<omokMain>();
        if (omok != null)
        {
            AugmentManager.Instance.StartCoroutine(DropMeteors(omok, x1, y1, x2, y2));
        }
    }

    IEnumerator DropMeteors(omokMain omok, int x1, int y1, int x2, int y2)
    {
        // 서버에서 전달받은 확정된 고정 좌표 배열
        int[] targetX = { x1, x2 };
        int[] targetY = { y1, y2 };

        for (int count = 0; count < 2; count++)
        {
            // 이제 랜덤이 아니라 동기화된 좌표를 정직하게 꺼내 씁니다.
            int currentX = targetX[count];
            int currentY = targetY[count];

            Vector3 centerPos = omok.chagsu[currentX, currentY].transform.position;
            centerPos.z = -1f;

            // 방장, 게스트 모두 화면의 똑같은 위치에 이펙트가 생성됩니다.
            if (meteorExplosionPrefab != null)
            {
                Instantiate(meteorExplosionPrefab, centerPos, Quaternion.identity);
            }

            // 3x3 영역 파괴 진행 (똑같은 좌표이므로 데이터가 100% 일치함)
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    int nx = currentX + i;
                    int ny = currentY + j;

                    if (nx >= 0 && nx < 19 && ny >= 0 && ny < 19)
                    {
                        // ★ StoneManager 내부의 ClearStone이 자체적으로 [PunRPC]를 지원하지 않는 경우,
                        // 여기서 확실하게 돌을 지우는 처리가 동기화되어 양쪽 화면에서 사라집니다.
                        StoneManager.Instance.ClearStone(nx, ny);
                    }
                }
            }

            yield return new WaitForSeconds(0.4f);
        }
    }
}