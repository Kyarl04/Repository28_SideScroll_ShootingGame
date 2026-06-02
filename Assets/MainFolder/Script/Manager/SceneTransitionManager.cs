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
    public Image fadeImage; // 여기에 'Panel'을 넣으시면 됩니다!

    [Header("Material Settings")]
    public string dissolvePropertyName = "_D_Intensity"; // 찾으신 진짜 레퍼런스 이름 입력
    public float dissolveDuration = 1.0f;

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
            // 원본 머티리얼 복사 (인스턴스화)
            dissolveMaterial = new Material(fadeImage.material);
            fadeImage.material = dissolveMaterial; 
            
            // [중요] Image(패널)의 기본 색상은 무조건 불투명하게(알파 1) 고정합니다!
            // 그래야 셰이더가 정상적으로 화면에 그려집니다.
            fadeImage.color = new Color(1, 1, 1, 1);
        }
        
        if (transitionCanvasGroup != null)
        {
            // 평소에는 캔버스 그룹을 꺼서 화면에 안 보이고 클릭도 안 되게 합니다.
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
        transitionCanvasGroup.blocksRaycasts = true; 

        // 1. 화면 덮기 (1 ➔ 0)
        dissolveMaterial.SetFloat(dissolvePropertyName, 1f); 
        yield return dissolveMaterial.DOFloat(0f, dissolvePropertyName, dissolveDuration).WaitForCompletion();

        // 2. 비동기 씬 로드
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false; 

        // 로딩이 거의 끝날 때까지 대기 (유니티 비동기 로딩은 0.9에서 멈춥니다)
        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }

        // 3. 씬 활성화! (이때 다음 씬의 무거운 연산들이 쏟아집니다)
        asyncLoad.allowSceneActivation = true;

        // ==========================================
        // [핵심 해결책] 씬이 열리고 프레임이 안정화될 때까지 잠시 대기합니다!
        // ==========================================
        // 비동기 씬 로드가 완전히 끝날 때까지 1프레임씩 기다립니다.
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // 씬 전환이 끝나고 Awake/Start가 처리될 수 있도록 0.2초(또는 0.1초) 정도 약간의 여유를 줍니다.
        // 이 짧은 대기 시간 동안은 화면이 완전히 까만(0) 상태로 멈춰있어 플레이어는 전혀 어색함을 느끼지 못합니다.
        yield return new WaitForSeconds(0.2f); 

        // ==========================================
        // 4. 이제 프레임이 부드러워졌으니 화면 찢기 연출을 시작합니다! (0 ➔ 1)
        // ==========================================
        Tween dissolveTween = dissolveMaterial.DOFloat(1f, dissolvePropertyName, dissolveDuration).SetEase(Ease.InOutSine);
        yield return dissolveTween.WaitForCompletion();

        // 5. 연출 종료
        transitionCanvasGroup.gameObject.SetActive(false);
    }
}