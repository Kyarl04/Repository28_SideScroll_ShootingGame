using UnityEngine;
using UnityEngine.SceneManagement; 

/// <summary>
/// 메인 메뉴(타이틀)에서 버튼을 눌러 게임에 진입하거나 종료할 때 작동하는 UI 매니저 클래스.
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    [Header("Scene Settings")]
    public string gameSceneName = "GameScene";

    /// <summary>
    /// '게임 스타트' 버튼 클릭 시 호출
    /// 오디오 매니저와 씬 트랜지션 매니저를 연결하여 매끄러운 씬 진입 연출을 발생시킵니다.
    /// </summary>
    public void StartGame()
    {
        Debug.Log("게임을 시작합니다!");
        
        // 싱글톤 패턴으로 접근하여 씬 전환 중 끊기지 않는 사운드 페이드 인 적용
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayButtonClick(); 
            SoundManager.Instance.PlayGameBGM();     
        }

        // 화면 전환 연출(디졸브 효과) 트리거
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.TransitionToScene(gameSceneName);
        }
        else
        {
            SceneManager.LoadScene(gameSceneName);
        }
    }

    /// <summary>
    /// '게임 종료' 버튼 클릭 시 호출
    /// </summary>
    public void ExitGame()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayButtonClick();
            SoundManager.Instance.PlayMenuBGM();     
        }
        
        Debug.Log("게임을 종료합니다!");

// 유니티 에디터와 실제 빌드된 환경을 구분하여 종료 처리하는 전처리기 지시문
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}