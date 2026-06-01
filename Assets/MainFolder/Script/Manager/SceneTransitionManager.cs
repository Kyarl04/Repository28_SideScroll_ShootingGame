using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using System.Collections;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance;

    [Header("UI Elements")]
    public CanvasGroup transitionCanvasGroup; // 트랜지션 UI 전체를 껐다 켤 그룹
    public Image fadeImage; // 페이드 아웃용 검은색 전체 화면 이미지

    [Header("Particles")]
    [Tooltip("화면이 찢어지거나 디졸브되는 파티클 시스템")]
    public ParticleSystem dissolveParticle; 

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

    public void TransitionToScene(string sceneName)
    {
        StartCoroutine(TransitionRoutine(sceneName));
    }

    private IEnumerator TransitionRoutine(string sceneName)
    {
        // 0. 초기화
        transitionCanvasGroup.gameObject.SetActive(true);
        fadeImage.color = new Color(0, 0, 0, 0); // 완전 투명한 상태로 시작
        
        // 1. 화면 검게 페이드 아웃 (0.5초 대기)
        yield return fadeImage.DOFade(1f, 0.5f).WaitForCompletion();

        // 2. 비동기 씬 로드 시작 (보이지 않는 곳에서 다음 씬 불러오기)
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false; // 연출이 끝날 때까지 씬 전환 대기

        // 3. 디졸브/찢어지는 파티클 효과 발동!
        if (dissolveParticle != null) 
        {
            dissolveParticle.Play();
        }

        // 파티클이 화면에 확 퍼지는 타격감을 주기 위해 약간 대기 (파티클 속도에 맞춰 조절하세요)
        yield return new WaitForSeconds(0.5f); 

        // 4. 파티클이 흩날리는 사이, 배경의 검은 화면을 서서히 걷어냅니다.
        fadeImage.DOFade(0f, 0.5f);

        // 검은 화면이 걷어짐과 동시에 다음 씬을 활성화하여, 파티클 사이로 다음 씬이 보이게 합니다!
        asyncLoad.allowSceneActivation = true;

        // 파티클 연출이 완전히 끝날 때까지 충분히 대기 (예: 1.5초)
        yield return new WaitForSeconds(1.5f);

        // 5. 정리
        transitionCanvasGroup.gameObject.SetActive(false);
    }
}