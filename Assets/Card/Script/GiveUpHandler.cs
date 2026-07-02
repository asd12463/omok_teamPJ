using UnityEngine;
using Photon.Pun; // [추가] 포톤 멀티플레이 기능을 위해 필수

public class GiveUpHandler : MonoBehaviour
{
    // 오목의 메인 로직이 들어있는 스크립트를 연결합니다.
    public omokMain omokLogic;

    public void OnGiveUpButtonClicked()
    {
        if (omokLogic == null) omokLogic = Object.FindFirstObjectByType<omokMain>();

        if (omokLogic != null)
        {
            // [멀티플레이 핵심] 
            // 내가 항복을 눌렀다면, "누가(방장인지 손님인지)" 항복했는지 정보를 담아
            // 네트워크 상의 모든 플레이어(상대방 포함)에게 승패 처리 RPC를 날립니다.
            int myActorNumber = PhotonNetwork.LocalPlayer.ActorNumber;

            Debug.Log($"<color=red>[항복 시도] 내가 항복 버튼을 눌렀습니다. ActorNumber: {myActorNumber}</color>");

            // omokMain에 달린 photonView를 이용해 양쪽 화면에 똑같이 항복 함수를 실행시킵니다.
            omokLogic.photonView.RPC("RPC_NetworkGiveUp", RpcTarget.All, myActorNumber);
        }
    }
}