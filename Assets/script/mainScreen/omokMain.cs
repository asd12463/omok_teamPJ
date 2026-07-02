using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public enum GameState
{
    draw,
    CardUsePhase, // 2단계 클릭(카드 효과 발동 단계)을 위한 전용 징검다리 상태
    PlayerTurn,
    AITurn,
    minigame,
    GameOver
}

public class omokMain : MonoBehaviourPunCallbacks
{
    private const int boardSize = 19;
    public int[,] board = new int[boardSize, boardSize];

    public GameState currentState;
    public timer timerScript;

    [Header("플레이어별 코스트 그릇")]
    public cost masterCostScript; // 인스펙터에서 방장(흑돌)용 cost 오브젝트 연결
    public cost guestCostScript;  // 인스펙터에서 게스트(백돌)용 cost 오브젝트 연결

    public GameObject augmentCanvas;

    public GameObject blackStone;
    public GameObject whiteStone;
    public GameObject forbiddenStone;
    private GameObject previewStone;

    public GameObject resultPanel;
    public GameObject winImage;
    public GameObject loseImage;

    public chagsu[,] chagsu = new chagsu[boardSize, boardSize];

    void Awake()
    {
        chagsu[] allGrids = FindObjectsByType<chagsu>(FindObjectsSortMode.None);
        foreach (var grid in allGrids)
        {
            chagsu[grid.gridX, grid.gridY] = grid;
        }
    }

    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("방장: 첫 턴 드로우를 시작합니다.");
            StartMyLocalTurn();
        }
        else
        {
            currentState = GameState.AITurn;
            Debug.Log("게스트: 방장의 턴을 기다립니다.");

            if (AugmentManager.Instance != null) AugmentManager.Instance.gameObject.SetActive(false);
            if (augmentCanvas != null) augmentCanvas.SetActive(false);
        }

        if (timerScript == null) timerScript = FindFirstObjectByType<timer>();
        if (timerScript != null) timerScript.StartTimer();
    }

    public void ChangeTurn() { }

    // ==================================================================================
    // ★ [수정 사항 반영] 백돌 유저일 때 투명한 백돌로 가이드돌이 정상 노출되도록 보정 완료
    // ==================================================================================
    public void ShowPreview(int x, int y, int player)
    {
        if (!IsMyTurn()) return;

        // 1. 프리뷰 오브젝트가 아예 없다면 해당 플레이어의 돌 프리팹을 기반으로 새로 생성합니다.
        if (previewStone == null)
        {
            GameObject stonePrefab = (player == 1) ? blackStone : whiteStone;
            previewStone = Instantiate(stonePrefab);

            SpriteRenderer sr = previewStone.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                // 플레이어 성별(흑/백)에 맞춰 색상 세팅 후 반투명화(0.5f) 적용
                Color color = (player == 1) ? Color.black : Color.white;
                color.a = 0.5f;
                sr.color = color;
            }
        }
        else
        {
            // 2. 이미 프리뷰 오브젝트가 존재한다면 스프라이트 이미지 리소스를 실시간으로 알맞게 스위칭합니다.
            SpriteRenderer sr = previewStone.GetComponent<SpriteRenderer>();
            GameObject targetPrefab = (player == 1) ? blackStone : whiteStone;

            if (sr != null && targetPrefab != null)
            {
                SpriteRenderer targetSr = targetPrefab.GetComponent<SpriteRenderer>();
                if (targetSr != null)
                {
                    sr.sprite = targetSr.sprite;
                }

                // 색상도 유저 데이터에 따라 실시간으로 완벽하게 동적 강제 채색합니다.
                Color color = (player == 1) ? Color.black : Color.white;
                color.a = 0.5f;
                sr.color = color;
            }
        }

        if (chagsu[x, y] != null)
        {
            previewStone.transform.position = chagsu[x, y].transform.position;
            previewStone.SetActive(true);
        }
    }

    public void HidePreview()
    {
        if (previewStone != null) previewStone.SetActive(false);
    }

    public bool CheckWin(int x, int y, int player)
    {
        int[] dx = { 1, 0, 1, 1 };
        int[] dy = { 0, 1, 1, -1 };

        for (int i = 0; i < 4; i++)
        {
            int count = 1;
            count += CountStones(x, y, dx[i], dy[i], player);
            count += CountStones(x, y, -dx[i], -dy[i], player);

            if (count >= 5) return true;
        }
        return false;
    }

    public void PlaceStone(int x, int y, int player)
    {
        photonView.RPC("RPC_PlaceStone", RpcTarget.All, x, y, player);
    }

    [PunRPC]
    void RPC_PlaceStone(int x, int y, int player)
    {
        HidePreview();
        board[x, y] = player;
        chagsu[x, y].SetOccupied();

        GameObject stonePrefab = (player == 1) ? blackStone : whiteStone;
        if (stonePrefab != null)
        {
            Vector3 spawnPos = chagsu[x, y].transform.position;
            spawnPos.z = 0;
            GameObject newStone = Instantiate(stonePrefab, spawnPos, Quaternion.identity);
            newStone.name = $"Stone_{x}_{y}";
        }

        if (CheckWin(x, y, player))
        {
            currentState = GameState.GameOver;
            if (timerScript != null) timerScript.StopTimer();
            ShowResult(player);
            return;
        }

        int myNumber = PhotonNetwork.IsMasterClient ? 1 : 2;

        if (player == myNumber)
        {
            currentState = GameState.AITurn;
            if (timerScript != null) timerScript.StopTimer();
            Debug.Log("<color=red>상대방의 턴을 기다립니다.</color>");
        }
        else
        {
            Debug.Log("<color=blue>내 드로우 턴 시작!</color>");
            StartMyLocalTurn();
        }
    }

    public void AddPlayerCost(int playerNumber, int amount)
    {
        photonView.RPC("RPC_NetworkAddPlayerCost", RpcTarget.All, playerNumber, amount);
    }

    [PunRPC]
    void RPC_NetworkAddPlayerCost(int playerNumber, int amount)
    {
        if (playerNumber == 1)
        {
            if (masterCostScript != null)
            {
                masterCostScript.AddCost(amount);
                Debug.Log($"<color=green>[네트워크 동기화] 방장 그릇에 {amount} 코스트 가산 완료.</color>");
            }
        }
        else if (playerNumber == 2)
        {
            if (guestCostScript != null)
            {
                guestCostScript.AddCost(amount);
                Debug.Log($"<color=yellow>[네트워크 동기화] 게스트 그릇에 {amount} 코스트 가산 완료.</color>");
            }
        }
    }

    [PunRPC]
    public void RPC_SkipMyTurn(int senderActorNumber)
    {
        HidePreview();
        if (timerScript != null) timerScript.StopTimer();

        int myActorNumber = PhotonNetwork.LocalPlayer.ActorNumber;

        if (senderActorNumber == myActorNumber)
        {
            currentState = GameState.AITurn;
        }
        else
        {
            StartMyLocalTurn();
        }
    }

    [PunRPC]
    public void RPC_NetworkGiveUp(int surrenderedActorNumber)
    {
        currentState = GameState.GameOver;
        if (timerScript != null) timerScript.StopTimer();

        GameObject cardUI = GameObject.Find("CardSelectionPanel");
        if (cardUI != null) cardUI.SetActive(false);

        GameObject settingPanel = GameObject.Find("SettingPanel");
        if (settingPanel != null) settingPanel.SetActive(false);

        if (PhotonNetwork.LocalPlayer.ActorNumber == surrenderedActorNumber)
        {
            ShowResult(PhotonNetwork.IsMasterClient ? 2 : 1);
        }
        else
        {
            ShowResult(PhotonNetwork.IsMasterClient ? 1 : 2);
        }
    }

    void StartMyLocalTurn()
    {
        if (currentState == GameState.GameOver) return;

        currentState = GameState.draw;

        int myNumber = PhotonNetwork.IsMasterClient ? 1 : 2;

        AddPlayerCost(myNumber, 1);

        if (timerScript != null) timerScript.ResumeTimer();

        if (AugmentManager.Instance != null)
        {
            AugmentManager.Instance.gameObject.SetActive(true);
            if (augmentCanvas != null) augmentCanvas.SetActive(true);
            AugmentManager.Instance.StartUserTurn();
        }
        else
        {
            currentState = GameState.PlayerTurn;
        }
    }

    public bool IsMyTurn()
    {
        return (currentState == GameState.PlayerTurn || currentState == GameState.draw || currentState == GameState.CardUsePhase);
    }

    int CountStones(int x, int y, int dx, int dy, int player)
    {
        int count = 0;
        int nx = x + dx;
        int ny = y + dy;

        while (nx >= 0 && nx < boardSize && ny >= 0 && ny < boardSize && board[nx, ny] == player)
        {
            count++;
            nx += dx;
            ny += dy;
        }
        return count;
    }

    public void OnTimeOut()
    {
        HidePreview();
        if (timerScript != null) timerScript.StopTimer();
        currentState = GameState.GameOver;
    }

    public void ShowResult(int winner)
    {
        if (resultPanel == null) return;
        resultPanel.SetActive(true);

        int myNumber = PhotonNetwork.IsMasterClient ? 1 : 2;

        if (winner == myNumber)
        {
            if (loseImage != null) loseImage.SetActive(false);
            if (winImage != null) winImage.SetActive(true);
        }
        else
        {
            if (winImage != null) winImage.SetActive(false);
            if (loseImage != null) loseImage.SetActive(true);
        }
    }

    public void OnClickExit()
    {
        if (PhotonNetwork.IsConnected) PhotonNetwork.Disconnect();
        else SceneManager.LoadScene("startScreen");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        SceneManager.LoadScene("startScreen");
    }

    [PunRPC]
    public void RPC_ApplyShuffleResult(int[] xs, int[] ys, int[] owners)
    {
        Debug.Log("<color=purple>[셔플 시작] 기존 일반 돌들(흑/백)만 청소하고 무작위로 재배치합니다.</color>");

        GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        foreach (GameObject obj in allObjects)
        {
            if (obj != null && obj.name.StartsWith("Stone_"))
            {
                Destroy(obj);
            }
        }

        for (int r = 0; r < 19; r++)
        {
            for (int c = 0; c < 19; c++)
            {
                if (board[r, c] == 1 || board[r, c] == 2)
                {
                    board[r, c] = 0;

                    if (chagsu[r, c] != null)
                    {
                        chagsu[r, c].SetEmpty();
                    }
                }
            }
        }

        for (int i = 0; i < owners.Length; i++)
        {
            int nx = xs[i];
            int ny = ys[i];
            int owner = owners[i];

            board[nx, ny] = owner;
            if (chagsu[nx, ny] != null)
            {
                chagsu[nx, ny].SetOccupied();
            }

            GameObject stonePrefab = (owner == 1) ? blackStone : whiteStone;
            if (stonePrefab != null && chagsu[nx, ny] != null)
            {
                Vector3 spawnPos = chagsu[nx, ny].transform.position;
                spawnPos.z = 0;
                GameObject newStone = Instantiate(stonePrefab, spawnPos, Quaternion.identity);
                newStone.name = $"Stone_{nx}_{ny}";
            }
        }

        Debug.Log("<color=purple>[셔플 동기화 완료] 금지 구역을 제외한 모든 일반 돌의 셔플이 완료되었습니다!</color>");
    }

    [PunRPC]
    public void RPC_PlaceForbiddenStone(int x, int y)
    {
        board[x, y] = 3;

        if (chagsu[x, y] != null)
        {
            chagsu[x, y].SetOccupied();
        }

        if (forbiddenStone != null && chagsu[x, y] != null)
        {
            Vector3 spawnPos = chagsu[x, y].transform.position;
            spawnPos.z = 0;
            GameObject newForbiddenStone = Instantiate(forbiddenStone, spawnPos, Quaternion.identity);
            newForbiddenStone.name = $"ForbiddenStone_{x}_{y}";
        }

        Debug.Log($"<color=orange>[동기화] ({x}, {y}) 칸에 금지 돌 배치가 완료되었습니다.</color>");
    }
}