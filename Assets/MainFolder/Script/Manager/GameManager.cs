using UnityEngine;
using UnityEngine.SceneManagement; 

/// <summary>
/// 게임의 전반적인 상태(게임 오버, 클리어, 씬 재시작 등)를 관리하는 싱글톤 클래스.
/// Time.timeScale을 활용하여 게임 내 시간 흐름을 제어합니다.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI Panels")]
    public GameObject gameOverPanel;  
    public GameObject gameClearPanel; 

    [Header("Scene Settings")]
    [Tooltip("메인 메뉴 씬의 이름을 정확히 적어주세요.")]
    public string mainMenuSceneName = "MainMenu"; 

    private void Awake()
    {
        // 전역 접근을 위한 싱글톤 인스턴스 할당
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // 씬 시작 시 게임 오버 및 클리어 UI를 초기화하여 숨깁니다.
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (gameClearPanel != null) gameClearPanel.SetActive(false);
    }

    // ==========================================
    // [게임 상태 제어 로직]
    // ==========================================
    
    /// <summary>
    /// 플레이어 체력이 0이 되었을 때 호출. 게임 오버 패널을 띄우고 시간을 정지합니다.
    /// </summary>
    public void ShowGameOver()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        Time.timeScale = 0f; // 화면 렌더링은 유지하되 물리 연산 및 코루틴(Update) 정지
    }

    /// <summary>
    /// 보스의 모든 페이즈를 파괴했을 때 호출. 클리어 패널을 띄우고 시간을 정지합니다.
    /// </summary>
    public void ShowGameClear()
    {
        if (gameClearPanel != null) gameClearPanel.SetActive(true);
        Time.timeScale = 0f; 
    }

    /// <summary>
    /// 게임 재시작 버튼 이벤트. 정지된 시간을 복구하고 현재 씬을 다시 로드합니다.
    /// </summary>
    public void RestartGame()
    {
        Time.timeScale = 1f; // 반드시 일시정지를 해제해야 다음 씬이 정상 작동합니다.
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// 메인 메뉴 복귀 버튼 이벤트. 시간을 복구하고 타이틀 씬으로 넘어갑니다.
    /// </summary>
    public void GoToMainMenu()
    {
        Time.timeScale = 1f; 
        SceneManager.LoadScene(mainMenuSceneName);
    }
}