using UnityEngine;
using UnityEngine.UI;

public class GameOptionManager : MonoBehaviour
{
    // 전역에서 쉽게 접근할 수 있도록 싱글톤 인스턴스 생성
    public static GameOptionManager Instance { get; private set; }

    [Header("UI Panels")]
    public GameObject optionPanel;

    [Header("Sliders")]
    public Slider transparencySlider;
    public Slider volumeSlider;

    [Header("Player Settings")]
    public SpriteRenderer playerSprite; 
    
    // 핵심: 현재 설정된 투명도를 기억할 변수 (기본값 1f)
    public float CurrentPlayerAlpha { get; private set; } = 1f;

    private bool isPaused = false;

    private void Awake()
    {
        // 싱글톤 초기화
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (optionPanel != null) optionPanel.SetActive(false);

        if (playerSprite == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerSprite = player.GetComponent<SpriteRenderer>();
        }

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
        Time.timeScale = 0f; 
    }

    public void ResumeGame()
    {
        isPaused = false;
        optionPanel.SetActive(false);
        Time.timeScale = 1f; 
    }

    public void SetPlayerTransparency(float alpha)
    {
        CurrentPlayerAlpha = alpha; // 슬라이더 값을 전역 변수에 저장

        if (playerSprite != null)
        {
            Color color = playerSprite.color;
            color.a = CurrentPlayerAlpha; 
            playerSprite.color = color;
        }
    }

    public void SetVolume(float volume)
    {
        AudioListener.volume = volume; 
    }
}