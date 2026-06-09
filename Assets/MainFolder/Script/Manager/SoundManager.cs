using UnityEngine;
using System.Collections; // 코루틴을 사용하기 위해 필요합니다.

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("오디오 소스 (Audio Sources)")]
    public AudioSource bgmSource;         
    public AudioSource sfxSource;         
    public AudioSource loopingSfxSource;  

    [Header("배경음악 (BGM)")]
    public AudioClip menuBGM;
    public AudioClip gameBGM;
    
    [Header("BGM Fade Settings")]
    [Tooltip("음악이 서서히 바뀌는 데 걸리는 시간 (초 단위)")]
    public float fadeDuration = 1.5f;     
    [Tooltip("배경음악의 최대 볼륨 (0.0 ~ 1.0)")]
    public float maxBgmVolume = 1.0f;     
    
    private Coroutine currentFadeRoutine; // 현재 진행 중인 페이드 코루틴을 기억하는 변수

    [Header("효과음 (SFX)")]
    public AudioClip buttonClickSFX;      
    public AudioClip playerShootSFX;      
    public AudioClip playerBulletHitSFX;  
    public AudioClip playerLaserFireSFX;  
    public AudioClip playerLaserHitSFX;   
    public AudioClip bossPhaseChangeSFX;  
    public AudioClip playerHitSFX;        

    private float bulletHitCooldown = 0.05f; // 0.05초 (필요에 따라 조절 가능)
    private float lastBulletHitTime = -999f;

    private void Awake()
    {
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
        // 시작 시 메인 메뉴 BGM을 부드럽게 페이드 인 재생
        PlayBGM(menuBGM);
    }

    // ==========================================
    // [BGM 제어 - 페이드 인/아웃 적용]
    // ==========================================
    public void PlayBGM(AudioClip newClip)
    {
        if (newClip == null) return;
        if (bgmSource.clip == newClip) return; // 이미 같은 곡이 재생 중이면 무시

        // 기존에 진행 중이던 페이드 효과가 있다면 즉시 중단합니다.
        if (currentFadeRoutine != null)
        {
            StopCoroutine(currentFadeRoutine);
        }

        // 새로운 페이드 코루틴 시작!
        currentFadeRoutine = StartCoroutine(FadeRoutine(newClip));
    }

    private IEnumerator FadeRoutine(AudioClip newClip)
    {
        // 1. 현재 음악 페이드 아웃 (소리 점점 작아짐)
        if (bgmSource.isPlaying)
        {
            float startVolume = bgmSource.volume;

            while (bgmSource.volume > 0f)
            {
                bgmSource.volume -= startVolume * (Time.deltaTime / fadeDuration);
                yield return null; // 다음 프레임까지 대기
            }
            bgmSource.Stop();
        }

        // 2. 새로운 음악으로 교체
        bgmSource.clip = newClip;
        bgmSource.loop = true;
        bgmSource.Play();

        // 3. 새로운 음악 페이드 인 (소리 점점 커짐)
        bgmSource.volume = 0f;

        while (bgmSource.volume < maxBgmVolume)
        {
            bgmSource.volume += maxBgmVolume * (Time.deltaTime / fadeDuration);
            yield return null; 
        }

        // 혹시 모를 오차를 방지하기 위해 목표 볼륨으로 딱 맞춰줌
        bgmSource.volume = maxBgmVolume;
    }
    
    public void PlayGameBGM() => PlayBGM(gameBGM);
    public void PlayMenuBGM() => PlayBGM(menuBGM);

    // ==========================================
    // [단발성 SFX 제어]
    // ==========================================
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip); 
    }

    public void PlayButtonClick() => PlaySFX(buttonClickSFX);
    public void PlayPlayerShoot() => PlaySFX(playerShootSFX);
    
    public void PlayPlayerBulletHit() 
    {
        // 현재 시간이 '마지막 재생 시간 + 0.05초'를 지났을 때만 재생!
        if (Time.time >= lastBulletHitTime + bulletHitCooldown)
        {
            PlaySFX(playerBulletHitSFX);
            lastBulletHitTime = Time.time; // 마지막으로 재생한 시간을 지금으로 갱신
        }
    }

    public void PlayPlayerLaserFire() => PlaySFX(playerLaserFireSFX);
    public void PlayBossPhaseChange() => PlaySFX(bossPhaseChangeSFX);
    public void PlayPlayerHit() => PlaySFX(playerHitSFX);

    // ==========================================
    // [지속성 SFX 제어 (레이저 명중)]
    // ==========================================
    public void StartLaserHitSFX()
    {
        // [수정됨] loopingSfxSource가 비어있으면 아예 실행하지 않도록 방어막 추가!
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
        // [수정됨] 끌 때도 스피커가 있는지 먼저 확인합니다.
        if (loopingSfxSource != null) 
        {
            loopingSfxSource.Stop();
        }
    }
}