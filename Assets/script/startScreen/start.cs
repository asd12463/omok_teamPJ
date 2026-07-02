using UnityEngine;
using UnityEngine.SceneManagement;

public class start : MonoBehaviour
{
    //public NetworkLauncher launcher; // 인스펙터에서 NetworkLauncher가 붙은 오브젝트를 드래그 앤 드롭 하세요.

    private void OnMouseDown()
    {
        SceneManager.LoadScene("lobbyScreen");
        //if (launcher != null)
        //{
        //    
        //    // 대신 포톤 접속 시작!
        //    launcher.ConnectToServer();

        //    // 버튼이 중복 클릭되지 않게 처리 (선택사항)
        //    var col = GetComponent<BoxCollider2D>(); // 또는 CircleCollider2D
        //    if (col != null)
        //    {
        //        col.enabled = false;
        //    }
        //}
        //else
        //{
        //    Debug.LogError("NetworkLauncher가 할당되지 않았습니다!");
        //}
    }
}