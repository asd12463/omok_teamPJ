using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class NetworkLauncher : MonoBehaviourPunCallbacks
{
    [Header("UI 연결 - 입력창")]
    public TMP_InputField roomInputField;

    [Header("UI 연결 - 버튼 및 텍스트")]
    public Button createRoomButton;   // 인스펙터 연결 또는 addBtn으로 자동 할당
    public Button joinRoomButton;     // 인스펙터 연결 또는 joinBtn으로 자동 할당
    public Button exitRoomButton;
    public TextMeshProUGUI connectStatusText;

    void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;

        // [이름 수정] 알려주신 오브젝트 이름(addBtn, joinBtn)으로 에디터에서 자동으로 찾습니다.
        if (createRoomButton == null) createRoomButton = GameObject.Find("addBtn")?.GetComponent<Button>();
        if (joinRoomButton == null) joinRoomButton = GameObject.Find("joinBtn")?.GetComponent<Button>();

        // 나가기 버튼 이름도 혹시 다를 경우를 위해 인스펙터 확인 권장 (여기서는 기본값 유지)
        if (exitRoomButton == null) exitRoomButton = GameObject.Find("ExitRoomButton")?.GetComponent<Button>();

        if (exitRoomButton != null) exitRoomButton.gameObject.SetActive(true);

        if (!PhotonNetwork.IsConnected)
        {
            SetStatusText("서버 접속 중...");
            PhotonNetwork.ConnectUsingSettings();
            SetButtonsInteractable(false);
        }
        else
        {
            SetStatusText("마스터 서버에 연결되어 있습니다.");
            SetButtonsInteractable(true);
        }
    }

    public override void OnConnectedToMaster()
    {
        SetStatusText("마스터 서버 접속 완료! 로비 대기 중...");
        SetButtonsInteractable(true);
    }

    // [버튼 이벤트] 방 생성
    public void OnClickCreateRoom()
    {
        if (string.IsNullOrEmpty(roomInputField.text))
        {
            SetStatusText("<color=red>생성할 방 ID를 입력해주세요.</color>");
            return;
        }

        // 클릭 즉시 입력창과 모든 버튼의 클릭 판정을 완전히 차단합니다.
        SetButtonsInteractable(false);

        SetStatusText($"방 생성 시도 중: {roomInputField.text}");
        RoomOptions roomOptions = new RoomOptions { MaxPlayers = 2 };
        PhotonNetwork.CreateRoom(roomInputField.text, roomOptions);
    }

    // [버튼 이벤트] 방 참가
    public void OnClickJoinRoom()
    {
        if (string.IsNullOrEmpty(roomInputField.text))
        {
            SetStatusText("<color=red>참가할 방 ID를 입력해주세요.</color>");
            return;
        }

        // 클릭 즉시 입력창과 모든 버튼의 클릭 판정을 완전히 차단합니다.
        SetButtonsInteractable(false);

        SetStatusText($"방 참가 시도 중: {joinRoomButton != null} {roomInputField.text}");
        PhotonNetwork.JoinRoom(roomInputField.text);
    }

    // [버튼 이벤트] 나가기 버튼
    public void OnClickExitRoom()
    {
        if (PhotonNetwork.InRoom)
        {
            SetStatusText("방을 파괴하고 나가는 중...");
            PhotonNetwork.LeaveRoom();
        }
        else
        {
            SetStatusText("로비를 나갑니다.");
            if (PhotonNetwork.IsConnected)
            {
                PhotonNetwork.Disconnect();
            }
            SceneManager.LoadScene("startScreen");
        }
    }

    public override void OnJoinedRoom()
    {
        SetStatusText($"방 입장 성공! 상대방을 기다리는 중... ({PhotonNetwork.CurrentRoom.PlayerCount}/2)");
        SetButtonsInteractable(false); // 입장 완료 상태 잠금 유지
        CheckAndStartGame();
    }

    public override void OnLeftRoom()
    {
        SetStatusText("방이 파괴되었습니다. 메인 화면으로 돌아갑니다.");
        if (PhotonNetwork.IsConnected) PhotonNetwork.Disconnect();
        SceneManager.LoadScene("startScreen");
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        SetStatusText("상대방이 입장했습니다! 게임을 시작합니다.");
        CheckAndStartGame();
    }

    void CheckAndStartGame()
    {
        if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            SetStatusText("게임 화면으로 이동합니다...");
            PhotonNetwork.LoadLevel("mainScreen");
        }
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        SetStatusText($"<color=red>방 생성 실패: {message}</color>");
        SetButtonsInteractable(true); // 실패했으므로 다시 버튼 활성화
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        SetStatusText($"<color=red>방 참가 실패: {message}</color>");
        SetButtonsInteractable(true); // 실패했으므로 다시 버튼 활성화
    }

    // ★ 버튼 차단 방식을 더 강력하게 보완했습니다.
    void SetButtonsInteractable(bool state)
    {
        if (createRoomButton != null)
        {
            createRoomButton.interactable = state;
            createRoomButton.enabled = state; // 컴포넌트 자체를 켜고 끔으로써 클릭 이벤트 완전 봉쇄
        }
        if (joinRoomButton != null)
        {
            joinRoomButton.interactable = state;
            joinRoomButton.enabled = state; // 컴포넌트 자체를 켜고 끔으로써 클릭 이벤트 완전 봉쇄
        }
        if (roomInputField != null)
        {
            roomInputField.interactable = state;
        }
    }

    void SetStatusText(string msg)
    {
        if (connectStatusText != null) connectStatusText.text = msg;
        Debug.Log(msg);
    }
}