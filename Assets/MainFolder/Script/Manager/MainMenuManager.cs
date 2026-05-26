using UnityEngine;
using UnityEngine.SceneManagement; // 씬(Scene) 이동을 위해 반드시 필요한 네임스페이스입니다.

public class MainMenuManager : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("이동할 게임 씬의 이름을 정확히 적어주세요. (예: GameScene)")]
    public string gameSceneName = "GameScene";

    // '게임 스타트' 버튼을 눌렀을 때 실행될 함수
    public void StartGame()
    {
        Debug.Log("게임을 시작합니다!");
        // 지정된 이름의 씬을 불러옵니다.
        SoundManager.Instance.PlayButtonClick(); // 버튼 소리 재생
        SoundManager.Instance.PlayGameBGM();     // 게임 씬 BGM으로 교체
        SceneManager.LoadScene(gameSceneName);
    }

    // '게임 종료' 버튼을 눌렀을 때 실행될 함수
    public void ExitGame()
    {
        SoundManager.Instance.PlayButtonClick();
        SoundManager.Instance.PlayMenuBGM();     // 메뉴 씬 BGM으로 교체
        Debug.Log("게임을 종료합니다!");

        // 1. 유니티 에디터 환경에서 테스트할 때 멈추는 기능
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        // 2. 실제 파일(exe, apk 등)로 빌드된 게임에서 끄는 기능
#else
        Application.Quit();
#endif
    }
}