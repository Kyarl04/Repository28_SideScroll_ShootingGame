using UnityEngine;
using UnityEngine.SceneManagement; 

public class MainMenuManager : MonoBehaviour
{
    [Header("Scene Settings")]
    public string gameSceneName = "GameScene";

    // '게임 스타트' 버튼을 눌렀을 때 실행될 함수
    public void StartGame()
    {
        Debug.Log("게임을 시작합니다!");
        
        // 1. 사운드 매니저에게 버튼 소리와 게임 BGM(페이드 인/아웃)을 명령합니다!
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayButtonClick(); 
            SoundManager.Instance.PlayGameBGM();     
        }

        // 2. 올려주신 화면 전환 매니저(SceneTransitionManager)를 통해 부드럽게 씬을 넘깁니다!
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.TransitionToScene(gameSceneName);
        }
        else
        {
            // 혹시 전환 매니저가 없다면 기존 방식대로 바로 씬을 넘깁니다.
            SceneManager.LoadScene(gameSceneName);
        }
    }

    // '게임 종료' 버튼을 눌렀을 때 실행될 함수
    public void ExitGame()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayButtonClick();
            SoundManager.Instance.PlayMenuBGM();     
        }
        
        Debug.Log("게임을 종료합니다!");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}