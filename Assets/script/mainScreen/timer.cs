using UnityEngine;

public class timer : MonoBehaviour
{
    public SpriteRenderer tenMinuteDigitRenderer; // 10분 단위
    public SpriteRenderer oneMinuteDigitRenderer;    // 1분 단위
    public SpriteRenderer tenSecondDigitRenderer; // 10초 단위
    public SpriteRenderer oneSecondDigitRenderer; // 1초 단위
    public Sprite[] numberSprites;          // 0~9까지 숫자 스프라이트 (10개)

    public float timeLimit = 30f;
    private float currentTimer;
    private bool isRunning = false;

    private omokMain mainScript;

    void Awake()
    {
        mainScript = FindFirstObjectByType<omokMain>();
        ResetTimer();
    }

    void Update()
    {
        if (!isRunning) return;

        if (currentTimer > 0)
        {
            currentTimer -= Time.deltaTime;
            UpdateDisplay(Mathf.CeilToInt(currentTimer));
        }
        else
        {
            currentTimer = 0;
            isRunning = false;
            UpdateDisplay(0);

            // 시간이 다 되면 omokMain의 타임아웃 함수 호출
            if (mainScript != null) mainScript.OnTimeOut();
        }
    }

    public void StartTimer()
    {
        currentTimer = timeLimit;
        isRunning = true;
    }

    public void StopTimer()
    {
        isRunning = false;
    }

    public void ResumeTimer()
    {
        isRunning = true; // 시간 초기화 없이 다시 흐르게 함
    }

    public void ResetTimer()
    {
        currentTimer = timeLimit;
        UpdateDisplay(Mathf.CeilToInt(currentTimer));
    }
    
    private void UpdateDisplay(int time)
    {
        if (numberSprites.Length < 10) return;

        // 전체 분과 초 계산
        int totalMinutes = time / 60;
        int seconds = time % 60;

        // 분 단위 쪼개기
        int minTen = totalMinutes / 10; // 10분 자리
        int minOne = totalMinutes % 10; // 1분 자리

        // 초 단위 쪼개기
        int secTen = seconds / 10; // 10초 자리
        int secOne = seconds % 10; // 1초 자리

        // 각 SpriteRenderer에 숫자 할당
        if (tenMinuteDigitRenderer != null)
            tenMinuteDigitRenderer.sprite = numberSprites[Mathf.Clamp(minTen, 0, 9)];

        if (oneMinuteDigitRenderer != null)
            oneMinuteDigitRenderer.sprite = numberSprites[Mathf.Clamp(minOne, 0, 9)];

        if (tenSecondDigitRenderer != null)
            tenSecondDigitRenderer.sprite = numberSprites[Mathf.Clamp(secTen, 0, 9)];

        if (oneSecondDigitRenderer != null)
            oneSecondDigitRenderer.sprite = numberSprites[Mathf.Clamp(secOne, 0, 9)];
    }
}