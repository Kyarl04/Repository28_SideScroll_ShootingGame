using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 게임 도중 ESC를 눌러 호출되는 설정 창을 관리하며,
/// 전역 볼륨과 플레이어의 투명도(시인성) 옵션을 제어하는 클래스입니다.
/// </summary>
public class GameOptionManager : MonoBehaviour
{
    public static GameOptionManager Instance { get; private set; }

    [Header("UI Panels")]
    public GameObject optionPanel;

    [Header("Sliders")]
    public Slider transparencySlider;
    public Slider volumeSlider;

    [Header("Player Settings")]
    public SpriteRenderer playerSprite; 
    
    // 외부 스크립트(PlayerController 등)가 투명도 값을 조회할 수 있도록 프로퍼티 노출
    public float CurrentPlayerAlpha { get; private set; } = 1f;

    private bool isPaused = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (optionPanel != null) optionPanel.SetActive(false);

        // 플레이어가 할당되지 않았다면 태그로 자동 검색
        if (playerSprite == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerSprite = player.GetComponent<SpriteRenderer>();
        }

        // UI 슬라이더의 이벤트 리스너에 함수를 연결하여 실시간 변경을 적용합니다.
        if (transparencySlider != null)
        {
            transparencySlider.value = CurrentPlayerAlpha;
            transparencySlider.onValueChanged.AddListener(SetPlayerTransparency);
        }

        if (volumeSlider != null)
        {
            volumeSlider.value = AudioListener.volume;
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }
    }

    private void Update()
    {
        // ESC 토글을 통한 메뉴 On/Off
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        optionPanel.SetActive(true);
        Time.timeScale = 0f; // 배경 및 로직 일시 정지
    }

    public void ResumeGame()
    {
        isPaused = false;
        optionPanel.SetActive(false);
        Time.timeScale = 1f; 
    }

    /// <summary>
    /// 탄막 게임 특성상 플레이어의 히트박스 시인성을 높이기 위해 본체의 투명도를 조절하는 함수
    /// </summary>
    public void SetPlayerTransparency(float alpha)
    {
        CurrentPlayerAlpha = alpha; 

        if (playerSprite != null)
        {
            Color color = playerSprite.color;
            color.a = CurrentPlayerAlpha; 
            playerSprite.color = color;
        }
    }

    public void SetVolume(float volume)
    {
        AudioListener.volume = volume; // 유니티 전역(Master) 볼륨 조절
    }
}