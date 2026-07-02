using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SettingsManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject settingsPanel;
    // =========================
    // 시작 시 저장값 불러오기
    // =========================
    private void Start()
    {
        settingsPanel.SetActive(false);
    }

    // =========================
    // 설정창 열기
    // =========================
    public void OnSettingsButtonClicked()
    {
        settingsPanel.SetActive(true);
    }

    // =========================
    // 설정창 닫기
    // =========================
    public void OnExitButtonClicked()
    {
        PlayerPrefs.Save();
        settingsPanel.SetActive(false);
    }

    // =========================
    // 게임 포기 버튼
    // =========================
    public void OnGiveUpButtonClicked()
    {
        PlayerPrefs.Save();

        Debug.Log("게임 포기");

        // SceneManager.LoadScene("MainMenu");
    }
}