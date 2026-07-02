using UnityEngine;

public class DestroyEffect : MonoBehaviour
{
    // 애니메이션 길이에 맞춰 적절한 시간을 입력하세요 (예: 0.5초)
    public float delay = 1f;

    void Start()
    {
        // 생성되자마자 delay 초 후에 오브젝트를 파괴합니다.
        Destroy(gameObject, delay);
    }
}