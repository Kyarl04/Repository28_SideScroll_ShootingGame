using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PlayerDrivenParallax : MonoBehaviour
{
    [Header("References")]
    [Tooltip("탄막을 피하는 플레이어의 Transform을 연결하세요.")]
    public Transform playerTransform;
    
    [Header("Arena Bounds (플레이어 이동 제한 구역)")]
    [Tooltip("플레이어가 이동 가능한 박스의 좌측 하단 좌표")]
    public Vector2 arenaMin; 
    [Tooltip("플레이어가 이동 가능한 박스의 우측 상단 좌표")]
    public Vector2 arenaMax; 

    [Header("Parallax Settings")]
    [Tooltip("배경이 상하좌우로 최대 얼마나 밀릴 것인지 설정 (카메라 밖으로 안 나가게 조절)")]
    public Vector2 maxBackgroundOffset = new Vector2(0.5f, 0.5f);

    [Header("Phase Transition")]
    [Tooltip("페이즈 전환 시 배경 암전/복구에 걸리는 시간")]
    public float fadeDuration = 1.5f;

    private SpriteRenderer bgRenderer;
    private Vector3 initialPosition; // 배경의 초기 중앙 위치

    void Awake()
    {
        bgRenderer = GetComponent<SpriteRenderer>();
        initialPosition = transform.position;
    }

    void LateUpdate()
    {
        if (playerTransform == null) return;

        // 1. 박스 내 플레이어의 상대적 위치 비율 계산 (0.0 ~ 1.0)
        // 플레이어가 정중앙에 있으면 tX, tY는 0.5가 됩니다.
        float tX = Mathf.InverseLerp(arenaMin.x, arenaMax.x, playerTransform.position.x);
        float tY = Mathf.InverseLerp(arenaMin.y, arenaMax.y, playerTransform.position.y);

        // 2. 비율에 따른 배경 오프셋 계산 (Lerp)
        // 플레이어가 우측(1.0)으로 갈수록 배경은 좌측(-maxBackgroundOffset.x)으로 밀립니다.
        float offsetX = Mathf.Lerp(maxBackgroundOffset.x, -maxBackgroundOffset.x, tX);
        float offsetY = Mathf.Lerp(maxBackgroundOffset.y, -maxBackgroundOffset.y, tY);

        // 3. 배경 위치 갱신
        transform.position = new Vector3(
            initialPosition.x + offsetX,
            initialPosition.y + offsetY,
            initialPosition.z
        );
    }

    // 보스 페이즈 전환 시 보스 스크립트에서 호출
    public void ChangePhase(Sprite newBgSprite)
    {
        StartCoroutine(PhaseTransitionRoutine(newBgSprite));
    }

    private IEnumerator PhaseTransitionRoutine(Sprite newSprite)
    {
        float timer = 0f;
        Color originalColor = bgRenderer.color;

        // 1. 암전 (점점 검은색으로)
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            bgRenderer.color = Color.Lerp(originalColor, Color.black, timer / fadeDuration);
            yield return null;
        }

        // 2. 어두워진 상태에서 배경 이미지 교체
        bgRenderer.sprite = newSprite;

        // 약간의 대기 시간 (연출적 여운)
        yield return new WaitForSeconds(0.2f);

        // 3. 다시 밝아짐 (검은색 -> 원래 색)
        timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            bgRenderer.color = Color.Lerp(Color.black, Color.white, timer / fadeDuration);
            yield return null;
        }
        
        bgRenderer.color = Color.white;
    }
}