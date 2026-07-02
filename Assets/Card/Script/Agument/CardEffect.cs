using UnityEngine;

public class CardEffect : MonoBehaviour
{
    public int cardID;
    public int useCost;
    // ★ [핵심] 모든 카드 프리팹이 인스펙터에서 사운드 파일을 가질 수 있게 주머니를 만듭니다.
    [Header("카드 고유 효과음")]
    public AudioClip cardSFX;

    // 기존 사용하던 가상 함수 기본 뼈대
    public virtual void Execute(int x, int y, omokMain omok) { }

    // ★ [새로 추가된 시전자 추적 기능] 
    // 하위 카드들이 내 컴퓨터 기준(로컬)이 아닌, 패킷에 담겨온 진짜 주인의 번호를 사용할 수 있게 해주는 다리 역할입니다.
    public virtual void ExecuteWithCaster(int x, int y, omokMain omok, int casterNumber)
    {
        // 따로 오버라이드하지 않은 일반 카드는 기존 Execute 로직을 기본으로 수행하게 만듭니다.
        Execute(x, y, omok);
    }
}