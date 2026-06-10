using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using System.Collections;

/// <summary>
/// 유니티 비동기 씬 로드(LoadSceneAsync)와 DOTween을 결합하여 
/// 씬이 로드되는 동안 자연스러운 셰이더 기반 디졸브(Dissolve) 연출을 처리하는 매니저 클래스.
/// </summary>
public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance;

    [Header("UI Elements")]
    public CanvasGroup transitionCanvasGroup; // 레이캐스트 블록(터치 방지) 및 UI 표시 제어용
    public Image fadeImage; 

    [Header("Material Settings")]
    public string dissolvePropertyName = "_D_Intensity"; // 셰이더 내에서 조작할 프로퍼티 이름
    public float dissolveDuration = 1.0f;

    private Material dissolveMaterial; // 원본 보호를 위한 머티리얼 인스턴스

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
            // 인스턴스화하여 원본 에셋의 값이 영구적으로 변형되는 것을 방지합니다.
            dissolveMaterial = new Material(fadeImage.material);
            fadeImage.material = dissolveMaterial; 
            
            fadeImage.color = new Color(1, 1, 1, 1);
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
        // 씬 전환 중 플레이어의 입력을 방지하기 위해 캔버스 활성화 및 블록 처리
        transitionCanvasGroup.gameObject.SetActive(true);
        transitionCanvasGroup.blocksRaycasts = true; 

        // 1. 디졸브 연출 시작 (화면 덮기)
        dissolveMaterial.SetFloat(dissolvePropertyName, 1f); 
        // DOTween을 사용해 비동기로 머티리얼 수치를 조절하고 완료될 때까지 대기
        yield return dissolveMaterial.DOFloat(0f, dissolvePropertyName, dissolveDuration).WaitForCompletion();

        // 2. 비동기 씬 로딩 (백그라운드에서 씬 데이터를 읽어옴)
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false; // 로딩이 끝나도 자동으로 씬이 켜지지 않게 보류

        // 유니티 시스템상 비동기 로딩은 0.9 (90%)에서 완료 신호를 대기합니다.
        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }

        // 3. 씬 활성화 및 초기화 대기
        asyncLoad.allowSceneActivation = true;

        // 씬 로딩이 100% 완료될 때까지 대기
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // 새 씬의 무거운 오브젝트들이 Awake/Start 되는 동안 프레임 드랍을 숨기기 위한 짧은 대기
        yield return new WaitForSeconds(0.2f); 

        // 4. 화면 디졸브 연출 종료 (화면 열기)
        Tween dissolveTween = dissolveMaterial.DOFloat(1f, dissolvePropertyName, dissolveDuration).SetEase(Ease.InOutSine);
        yield return dissolveTween.WaitForCompletion();

        // 5. 원상복구 및 입력 제한 해제
        transitionCanvasGroup.gameObject.SetActive(false);
    }
}