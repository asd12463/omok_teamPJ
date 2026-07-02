using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

// 인스펙터에서 깜빡하고 AudioSource를 안 붙여도 스크립트가 자동으로 강제 생성해 줍니다.
[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    // 🌟 어디서든 접근할 수 있는 싱글톤 인스턴스
    public static SoundManager Instance { get; private set; }

    [Header("Audio Mixer")]
    public AudioMixer mixer;

    [Header("UI Sliders")]
    public Slider masterSlider;
    public Slider bgmSlider;
    public Slider sfxSlider;

    // 내부 오디오 스피커 캐싱
    private AudioSource sfxSource;

    // 저장 키
    private const string MASTER_KEY = "MasterVolume";
    private const string BGM_KEY = "BGMVolume";
    private const string SFX_KEY = "SFXVolume";

    private void Awake()
    {
        // 싱글톤 세팅
        if (Instance == null)
        {
            Instance = this;
            // 씬이 바뀌어도 사운드가 끊기지 않고 유지되길 원한다면 주석을 해제하세요.
            // DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 컴포넌트 내부 캐싱
        sfxSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        // 저장된 값 불러오기 (기본값 1f)
        float master = PlayerPrefs.GetFloat(MASTER_KEY, 1f);
        float bgm = PlayerPrefs.GetFloat(BGM_KEY, 1f);
        float sfx = PlayerPrefs.GetFloat(SFX_KEY, 1f);

        // 슬라이더 값 적용 (UI가 배치되지 않은 씬 대비 null 체크)
        if (masterSlider != null) masterSlider.value = master;
        if (bgmSlider != null) bgmSlider.value = bgm;
        if (sfxSlider != null) sfxSlider.value = sfx;

        // 실제 볼륨 적용
        SetMasterVolume(master);
        SetBGMVolume(bgm);
        SetSFXVolume(sfx);

        // 슬라이더 이벤트 연결
        if (masterSlider != null) masterSlider.onValueChanged.AddListener(SetMasterVolume);
        if (bgmSlider != null) bgmSlider.onValueChanged.AddListener(SetBGMVolume);
        if (sfxSlider != null) sfxSlider.onValueChanged.AddListener(SetSFXVolume);
    }

    // 볼륨 설정 함수 (슬라이더 최솟값 0일 때 무한대 에러 방지 안전장치 포함)
    public void SetMasterVolume(float value)
    {
        float volume = (value <= 0.001f) ? -80f : Mathf.Log10(value) * 20;
        mixer.SetFloat("MasterVolume", volume);

        PlayerPrefs.SetFloat(MASTER_KEY, value);
        PlayerPrefs.Save();
    }

    public void SetBGMVolume(float value)
    {
        float volume = (value <= 0.001f) ? -80f : Mathf.Log10(value) * 20;
        mixer.SetFloat("BGMVolume", volume);

        PlayerPrefs.SetFloat(BGM_KEY, value);
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float value)
    {
        float volume = (value <= 0.001f) ? -80f : Mathf.Log10(value) * 20;
        mixer.SetFloat("SFXVolume", volume);

        PlayerPrefs.SetFloat(SFX_KEY, value);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// 외부(카드 스크립트 등)에서 사운드를 재생할 때 호출하는 함수
    /// </summary>
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;

        if (sfxSource != null)
        {
            // 한 몸으로 묶인 오디오 소스를 통해 효과음 중첩 재생
            sfxSource.PlayOneShot(clip);
        }
    }
}