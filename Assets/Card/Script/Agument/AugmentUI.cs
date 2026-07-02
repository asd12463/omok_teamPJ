using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Photon.Pun;

public class AugmentUI : MonoBehaviour
{
    private AugmentManager manager;
    private bool isSaveSlotCard = false;

    [Header("코스트 부족 경고 텍스트 UI")]
    public Text warningText;

    private static Coroutine activeCoroutine;

    void Start()
    {
        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(OnClickCard);
        }

        if (warningText == null)
        {
            GameObject textObj = GameObject.Find("CostWarningText");
            if (textObj != null)
            {
                warningText = textObj.GetComponent<Text>();
            }
        }
    }

    public void SetCardInfo(AugmentManager mgr, bool isSaveSlot)
    {
        manager = mgr;
        isSaveSlotCard = isSaveSlot;
    }

    public void OnClickCard()
    {
        if (manager == null) return;

        // 보관 턴일 때는 코스트를 검사하지 않습니다.
        if (manager.isSavePhase && !isSaveSlotCard)
        {
            manager.OnCardClicked(this.gameObject, isSaveSlotCard);
            return;
        }

        // [사용 턴일 때 코스트 철저 검증]
        CardEffect effect = GetComponent<CardEffect>();
        omokMain omok = FindFirstObjectByType<omokMain>();

        if (effect != null && omok != null)
        {
            cost myCost = PhotonNetwork.IsMasterClient ? omok.masterCostScript : omok.guestCostScript;

            // ★ [안전장치] 인스펙터에 그릇이 연결 안 되어있다면 무조건 차단
            if (myCost == null)
            {
                Debug.LogError($"<color=red>[장전 차단] omokMain에 코스트 스크립트 그릇(Script)이 누락되었습니다!</color>");
                if (activeCoroutine != null) StopCoroutine(activeCoroutine);
                activeCoroutine = StartCoroutine(ShowWarningMessage("시스템 에러: 코스트 그릇 없음"));
                return;
            }

            // 코스트가 부족하면 장전 차단
            if (!myCost.CanAfford(effect.useCost))
            {
                Debug.LogWarning($"<color=red>[장전 실패] 코스트 부족! 필요: {effect.useCost} / 보유: {myCost.costValue}</color>");

                if (activeCoroutine != null) StopCoroutine(activeCoroutine);
                activeCoroutine = StartCoroutine(ShowWarningMessage("코스트가 부족합니다!"));
                return;
            }
        }

        // 검증 통과 완료 시에만 정상 장전
        manager.OnCardClicked(this.gameObject, isSaveSlotCard);
    }

    IEnumerator ShowWarningMessage(string message)
    {
        if (warningText != null)
        {
            warningText.text = message;
            warningText.gameObject.SetActive(true);
            yield return new WaitForSeconds(2.0f);
            warningText.gameObject.SetActive(false);
        }
    }
}