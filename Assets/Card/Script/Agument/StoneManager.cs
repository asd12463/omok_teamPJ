using UnityEngine;
using Photon.Pun;

public class StoneManager : MonoBehaviourPunCallbacks
{
    public static StoneManager Instance;
    public GameObject pendingCardObject;
    private omokMain omok;

    void Awake()
    {
        if (Instance == null) Instance = this;
        omok = GetComponent<omokMain>();
    }

    public bool ExecuteEffect(int x, int y)
    {
        if (pendingCardObject != null)
        {
            CardEffect effect = pendingCardObject.GetComponent<CardEffect>();
            if (effect != null)
            {
                int id = effect.cardID;

                bool isMaster = PhotonNetwork.IsMasterClient;
                Debug.Log($"<color=orange>[카드 발동 시도] ID: {id}, 좌표: ({x},{y}), 시전자 방장여부: {isMaster}</color>");

                photonView.RPC("RPC_NetworkExecuteEffectByID", RpcTarget.All, id, x, y, isMaster);

                Destroy(pendingCardObject);
                pendingCardObject = null;
                return true;
            }
        }
        return false;
    }

    [PunRPC]
    void RPC_NetworkExecuteEffectByID(int id, int x, int y, bool isMaster)
    {
        Debug.Log($"<color=magenta>[RPC 수신] 카드 발동 명령 수신. 카드ID: {id}, 시전자 방장여부: {isMaster}</color>");

        if (AugmentManager.Instance != null && AugmentManager.Instance.cardPrefabsPool != null)
        {
            GameObject targetCardPrefab = null;
            foreach (GameObject prefab in AugmentManager.Instance.cardPrefabsPool)
            {
                CardEffect prefabEffect = prefab.GetComponent<CardEffect>();
                if (prefabEffect != null && prefabEffect.cardID == id)
                {
                    targetCardPrefab = prefab;
                    break;
                }
            }

            if (targetCardPrefab != null)
            {
                CardEffect effect = targetCardPrefab.GetComponent<CardEffect>();
                if (effect != null && omok != null)
                {
                    cost targetCost = isMaster ? omok.masterCostScript : omok.guestCostScript;

                    if (targetCost != null)
                    {
                        // 1. 코스트 정상 차감
                        targetCost.ConsumeCost(effect.useCost);
                        Debug.Log($"<color=cyan>[코스트 최종 차감] {(isMaster ? "방장" : "게스트")} 차감 완료!</color>");

                        bool amIMaster = PhotonNetwork.IsMasterClient;

                        // ----------------------------------------------------------------------
                        // ★ [교정 핵심] 카드 종류(한수쉬기 vs 일반카드)에 따른 정밀 턴 제어
                        // ----------------------------------------------------------------------
                        if (effect is HansuCardEffect)
                        {
                            // [분기 A] 한수쉬기는 돌을 안 놓으므로 즉시 턴 강제 종료 및 전환 진행!
                            if (amIMaster == isMaster)
                            {
                                omok.currentState = GameState.AITurn;
                                if (omok.timerScript != null) omok.timerScript.StopTimer();
                                Debug.Log("<color=red>[한수쉬기 턴 전환] 내가 카드를 사용했으므로 대기 상태(AITurn)로 즉시 전환합니다.</color>");
                            }
                            else
                            {
                                Debug.Log("<color=blue>[한수쉬기 턴 전환] 상대방이 카드를 사용했으므로 즉시 내 드로우 턴을 시작합니다!</color>");
                                int myNumber = amIMaster ? 1 : 2;
                                omok.currentState = GameState.draw;
                                omok.AddPlayerCost(myNumber, 1);
                                if (omok.timerScript != null) omok.timerScript.ResumeTimer();

                                if (AugmentManager.Instance != null)
                                {
                                    AugmentManager.Instance.gameObject.SetActive(true);
                                    if (omok.augmentCanvas != null) omok.augmentCanvas.SetActive(true);
                                    AugmentManager.Instance.StartUserTurn();
                                }
                            }
                        }
                        else
                        {
                            // [분기 B] 부스트 등 일반 카드는 효과만 터트리고 제어권을 유지해야 합니다.
                            if (amIMaster == isMaster)
                            {
                                // 카드를 쓴 로컬 플레이어 상태를 돌 배치 상태로 안전하게 전환하여 2번째 클릭을 준비시킵니다.
                                omok.currentState = GameState.PlayerTurn;
                                Debug.Log("<color=lime>[카드 발동 완료] 효과 연산 완료. 돌을 배치할 때까지 턴 소유권을 정직하게 유지합니다.</color>");
                            }
                            else
                            {
                                // 상대방 컴퓨터는 시전자가 돌을 놓을 때까지 턴 상태를 바꾸지 않고 조용히 기다리게 만듭니다.
                                Debug.Log("<color=gray>[대기] 상대방이 카드 효과를 사용했습니다. 돌을 착수할 때까지 대기합니다.</color>");
                            }
                        }

                        // 2. 카드 로컬 효과 및 연출 연산 최종 실행
                        int casterNumber = isMaster ? 1 : 2;
                        effect.ExecuteWithCaster(x, y, omok, casterNumber);
                    }
                    else
                    {
                        Debug.LogError($"<color=red>[RPC 무효화] 시전자 그릇 스크립트 연결 유실</color>");
                    }
                }
            }
        }
    }

    public void ClearStone(int x, int y)
    {
        if (x < 0 || x >= 19 || y < 0 || y >= 19) return;

        omokMain omok = FindAnyObjectByType<omokMain>();
        if (omok != null && omok.chagsu[x, y] != null)
        {
            omok.chagsu[x, y].SetEmpty();
        }

        if (omok != null)
        {
            omok.board[x, y] = 0;
        }

        // ★ [교정 핵심] 초록색 물결선이 안 뜨도록 유니티 하위/상위 버전 모두 호환되는 강력한 문법으로 변경했습니다.
        System.Collections.Generic.List<GameObject> allObjects = new System.Collections.Generic.List<GameObject>();
        foreach (GameObject go in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            // 씬에 실제로 활성화되어 있는 오브젝트들만 필터링해서 담습니다.
            if (go.hideFlags == HideFlags.None)
            {
                allObjects.Add(go);
            }
        }

        string targetNormalName = $"Stone_{x}_{y}";
        string targetForbiddenName = $"ForbiddenStone_{x}_{y}";

        foreach (GameObject obj in allObjects)
        {
            if (obj == null) continue;

            if (obj.name.StartsWith(targetNormalName))
            {
                Destroy(obj);
            }

            if (obj.name.StartsWith(targetForbiddenName))
            {
                Destroy(obj);
            }
        }

        Debug.Log($"<color=cyan>[StoneManager] ({x}, {y}) 지점 청소 완료 (버전 경고 해결 패치 적용)</color>");
    }
}