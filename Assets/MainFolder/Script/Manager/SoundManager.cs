using UnityEngine;

public class SoundManager : MonoBehaviour
{
    // 어디서든 SoundManager.Instance 로 접근할 수 있게 해주는 싱글톤
    public static SoundManager Instance;

    [Header("오디오 소스 (Audio Sources)")]
    public AudioSource bgmSource;         // 배경음악 전용
    public AudioSource sfxSource;         // 단발성 효과음 전용
    public AudioSource loopingSfxSource;  // 레이저 명중 등 지속되는 효과음 전용

    [Header("배경음악 (BGM)")]
    public AudioClip menuBGM;
    public AudioClip gameBGM;

    [Header("효과음 (SFX)")]
    public AudioClip buttonClickSFX;      // 버튼 클릭
    public AudioClip playerShootSFX;      // 총알 발사
    public AudioClip playerBulletHitSFX;  // 적 명중
    public AudioClip playerLaserFireSFX;  // 레이저 발사
    public AudioClip playerLaserHitSFX;   // 레이저 명중 (지직거리는 소리)
    public AudioClip bossPhaseChangeSFX;  // 보스 페이즈 전환
    public AudioClip playerHitSFX;        // 플레이어 피격

    private void Awake()
    {
        // 씬이 넘어가도 파괴되지 않는 싱글톤 설정
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // 시작 시 메인 메뉴 BGM 재생
        PlayBGM(menuBGM);
    }

    // ==========================================
    // [BGM 제어]
    // ==========================================
    public void PlayBGM(AudioClip clip)
    {
        if (clip == null) return;
        if (bgmSource.clip == clip) return; // 이미 재생 중이면 무시

        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.Play();
    }
    
    public void PlayGameBGM() => PlayBGM(gameBGM);
    public void PlayMenuBGM() => PlayBGM(menuBGM);

    // ==========================================
    // [단발성 SFX 제어]
    // ==========================================
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip); // 소리가 겹쳐도 끊기지 않고 재생됨
    }

    // 외부에서 쉽게 호출하기 위한 헬퍼 함수들
    public void PlayButtonClick() => PlaySFX(buttonClickSFX);
    public void PlayPlayerShoot() => PlaySFX(playerShootSFX);
    public void PlayPlayerBulletHit() => PlaySFX(playerBulletHitSFX);
    public void PlayPlayerLaserFire() => PlaySFX(playerLaserFireSFX);
    public void PlayBossPhaseChange() => PlaySFX(bossPhaseChangeSFX);
    public void PlayPlayerHit() => PlaySFX(playerHitSFX);

    // ==========================================
    // [지속성 SFX 제어 (레이저 명중)]
    // ==========================================
    public void StartLaserHitSFX()
    {
        if (playerLaserHitSFX == null) return;
        if (!loopingSfxSource.isPlaying)
        {
            loopingSfxSource.clip = playerLaserHitSFX;
            loopingSfxSource.loop = true;
            loopingSfxSource.Play();
        }
    }

    public void StopLaserHitSFX()
    {
        loopingSfxSource.Stop();
    }
}