using UnityEngine;
using UnityEngine.SceneManagement; // 씬 이동을 위해 반드시 필요합니다!

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI Panels")]
    public GameObject gameOverPanel;  // 게임 오버 창
    public GameObject gameClearPanel; // 게임 클리어 창

    [Header("Scene Settings")]
    [Tooltip("메인 메뉴 씬의 이름을 정확히 적어주세요.")]
    public string mainMenuSceneName = "MainMenu"; 

    private void Awake()
    {
        // 싱글톤 초기화
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // 시작할 때 패널들은 꺼둡니다.
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (gameClearPanel != null) gameClearPanel.SetActive(false);
    }

    // ==========================================
    // 패널 띄우기 (시간 정지)
    // ==========================================
    public void ShowGameOver()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        Time.timeScale = 0f; // 화면 일시정지
    }

    public void ShowGameClear()
    {
        if (gameClearPanel != null) gameClearPanel.SetActive(true);
        Time.timeScale = 0f; // 화면 일시정지
    }

    // ==========================================
    // 버튼 연결용 함수 (시간 정상화 필수!)
    // ==========================================
    public void RestartGame()
    {
        Time.timeScale = 1f; // 일시정지 해제
        
        // 현재 열려있는 씬(게임 씬)을 다시 로드합니다.
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f; // 일시정지 해제
        SceneManager.LoadScene(mainMenuSceneName);
    }
}