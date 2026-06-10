using UnityEngine;
using System.Collections; 

/// <summary>
/// 씬(Scene)이 넘어가도 유지되며 게임 전체의 배경음악(BGM)과 효과음(SFX)을 관리하는 싱글톤 클래스.
/// 자연스러운 BGM 전환을 위한 코루틴 페이드(Fade) 기법과 효과음 중복 재생 방지 로직이 구현되어 있습니다.
/// </summary>
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("오디오 소스 (Audio Sources)")]
    public AudioSource bgmSource;         // BGM 전용 채널
    public AudioSource sfxSource;         // 단발성 효과음 전용 채널
    public AudioSource loopingSfxSource;  // 레이저 등 지속되는 효과음 전용 채널 (루프)

    [Header("배경음악 (BGM)")]
    public AudioClip menuBGM;
    public AudioClip gameBGM;
    
    [Header("BGM Fade Settings")]
    public float fadeDuration = 1.5f;     // 음악 전환에 걸리는 시간
    public float maxBgmVolume = 1.0f;     
    
    private Coroutine currentFadeRoutine; // 겹침 방지를 위해 현재 실행 중인 페이드 코루틴 추적

    [Header("효과음 (SFX)")]
    public AudioClip buttonClickSFX;      
    public AudioClip playerShootSFX;      
    public AudioClip playerBulletHitSFX;  
    public AudioClip playerLaserFireSFX;  
    public AudioClip playerLaserHitSFX;   
    public AudioClip bossPhaseChangeSFX;  
    public AudioClip playerHitSFX;        
    public AudioClip obstacleDestroySFX;

    // 피격음 중복 재생으로 인한 오디오 증폭(볼륨 폭발) 방지를 위한 쿨타임 변수
    private float bulletHitCooldown = 0.05f; 
    private float lastBulletHitTime = -999f;

    private void Awake()
    {
        // 씬 전환 시에도 오디오가 끊기지 않도록 DontDestroyOnLoad 적용
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
        PlayBGM(menuBGM);
    }

    // ==========================================
    // [BGM 제어 - 코루틴을 활용한 오디오 페이드 인/아웃]
    // ==========================================
    public void PlayBGM(AudioClip newClip)
    {
        if (newClip == null || bgmSource.clip == newClip) return; 

        // 이전에 진행 중이던 페이드 효과가 있다면 충돌을 막기 위해 중단
        if (currentFadeRoutine != null)
        {
            StopCoroutine(currentFadeRoutine);
        }

        currentFadeRoutine = StartCoroutine(FadeRoutine(newClip));
    }

    private IEnumerator FadeRoutine(AudioClip newClip)
    {
        // 1. 기존 음악 서서히 줄이기 (Fade Out)
        if (bgmSource.isPlaying)
        {
            float startVolume = bgmSource.volume;
            while (bgmSource.volume > 0f)
            {
                bgmSource.volume -= startVolume * (Time.deltaTime / fadeDuration);
                yield return null; 
            }
            bgmSource.Stop();
        }

        // 2. 새로운 음악으로 클립 교체
        bgmSource.clip = newClip;
        bgmSource.loop = true;
        bgmSource.Play();

        // 3. 새로운 음악 서서히 키우기 (Fade In)
        bgmSource.volume = 0f;
        while (bgmSource.volume < maxBgmVolume)
        {
            bgmSource.volume += maxBgmVolume * (Time.deltaTime / fadeDuration);
            yield return null; 
        }

        bgmSource.volume = maxBgmVolume; // 부동소수점 오차 보정
    }
    
    public void PlayGameBGM() => PlayBGM(gameBGM);
    public void PlayMenuBGM() => PlayBGM(menuBGM);

    // ==========================================
    // [단발성 SFX 제어]
    // ==========================================
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip); // 기존 소리를 끊지 않고 중첩해서 재생
    }

    public void PlayButtonClick() => PlaySFX(buttonClickSFX);
    public void PlayPlayerShoot() => PlaySFX(playerShootSFX);
    
    /// <summary>
    /// 여러 발의 총알이 동시에 적중했을 때 발생하는 사운드 증폭 버그 방지 래퍼 함수
    /// </summary>
    public void PlayPlayerBulletHit() 
    {
        if (Time.time >= lastBulletHitTime + bulletHitCooldown)
        {
            PlaySFX(playerBulletHitSFX);
            lastBulletHitTime = Time.time; 
        }
    }

    public void PlayPlayerLaserFire() => PlaySFX(playerLaserFireSFX);
    public void PlayBossPhaseChange() => PlaySFX(bossPhaseChangeSFX);
    public void PlayPlayerHit() => PlaySFX(playerHitSFX);
    public void PlayObstacleDestroy() => PlaySFX(obstacleDestroySFX);
    // ==========================================
    // [지속성 SFX 제어 (레이저 명중 사운드 등)]
    // ==========================================
    public void StartLaserHitSFX()
    {
        if (playerLaserHitSFX == null || loopingSfxSource == null) return; 
        
        if (!loopingSfxSource.isPlaying)
        {
            loopingSfxSource.clip = playerLaserHitSFX;
            loopingSfxSource.loop = true;
            loopingSfxSource.Play();
        }
    }

    public void StopLaserHitSFX()
    {
        if (loopingSfxSource != null) 
        {
            loopingSfxSource.Stop();
        }
    }
}