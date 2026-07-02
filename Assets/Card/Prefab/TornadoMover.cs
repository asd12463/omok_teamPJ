using UnityEngine;

public class TornadoMover : MonoBehaviour
{
    [Header("이동 및 소멸 설정")]
    public float moveSpeed = 10f; // 소용돌이가 오른쪽으로 나아가는 속도
    public float destroyTime = 3f; // 화면 밖으로 완전히 벗어난 후 자동으로 파괴될 시간

    void Start()
    {
        // 생성된 지 3초가 지나면 씬에서 자동으로 삭제됩니다.
        Destroy(gameObject, destroyTime);
    }

    void Update()
    {
        // 매 프레임마다 오른쪽(X축 + 방향)으로 이동합니다.
        transform.Translate(Vector3.right * moveSpeed * Time.deltaTime);
    }
}