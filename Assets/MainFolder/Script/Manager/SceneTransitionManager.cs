using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using System.Collections;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance;

    [Header("UI Elements")]
    public CanvasGroup transitionCanvasGroup; 
    public Image fadeImage; // 디졸브 머티리얼이 적용된 검은색 전체 화면 이미지

    [Header("Material Settings")]
    [Tooltip("셰이더 그래프에서 설정한 디졸브 프로퍼티의 Reference 이름 (예: _DissolveAmount)")]
    public string dissolvePropertyName = "_DIntensity2";
    
    [Tooltip("디졸브 연출에 걸리는 시간")]
    public float dissolveDuration = 1.5f;

    private Material dissolveMaterial;

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
        if (fadeImage != null)
        {
            // [수정됨] 원본 머티리얼을 복사하여 독립적인 '나만의 인스턴스'로 만듭니다!
            dissolveMaterial = new Material(fadeImage.material);
            fadeImage.material = dissolveMaterial; 
        }
        
        if (transitionCanvasGroup != null)
        {
            transitionCanvasGroup.gameObject.SetActive(false);
        }
    }

    public void TransitionToScene(string sceneName)
    {
        StartCoroutine(TransitionRoutine(sceneName));
    }

    private IEnumerator TransitionRoutine(string sceneName)
    {
        transitionCanvasGroup.gameObject.SetActive(true);

        // 1. 화면 덮기: 디졸브 수치를 0(전혀 찢어지지 않은 온전한 검은 화면)으로 초기화하고, 알파를 1로 만듭니다.
        // (주의: 셰이더에 따라 0과 1의 역할이 반대일 수 있습니다. 반대라면 숫자를 바꿔주세요)
        dissolveMaterial.SetFloat(dissolvePropertyName, 1f);
        fadeImage.color = new Color(0, 0, 0, 0); 
        
        // 부드럽게 검은 화면으로 페이드 아웃
        yield return fadeImage.DOFade(0f, 0.5f).WaitForCompletion();

        // 2. 비동기 씬 로드 시작 (보이지 않는 곳에서 다음 씬 불러오기)
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false; 

        // 3. 디졸브 효과 발동! 
        // 머티리얼의 프로퍼티 값을 0에서 1로 지정한 시간(dissolveDuration)동안 서서히 변화시킵니다.
        Tween dissolveTween = dissolveMaterial.DOFloat(1f, dissolvePropertyName, dissolveDuration).SetEase(Ease.InOutSine);

        // 디졸브가 절반쯤 진행되었을 때 다음 씬을 켜서, 찢어지는 화면 사이로 다음 씬이 보이게 합니다.
        yield return new WaitForSeconds(dissolveDuration * 0.5f);
        asyncLoad.allowSceneActivation = true;

        // 디졸브 연출이 완전히 끝날 때까지 남은 시간 대기
        yield return dissolveTween.WaitForCompletion();

        // 4. 정리
        transitionCanvasGroup.gameObject.SetActive(false);
    }
}