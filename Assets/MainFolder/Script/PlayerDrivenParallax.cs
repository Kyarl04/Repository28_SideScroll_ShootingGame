using System.Collections;
using UnityEngine;

/// <summary>
/// 플레이어의 이동(위치)에 따라 배경 이미지를 반대 방향으로 미세하게 밀어내어 
/// 입체감(Parallax)과 시각적 역동성을 극대화하는 클래스입니다.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerDrivenParallax : MonoBehaviour
{
    [Header("References")]
    public Transform playerTransform;
    
    [Header("Arena Bounds (플레이어 이동 제한 구역)")]
    public Vector2 arenaMin; 
    public Vector2 arenaMax; 

    [Header("Parallax Settings")]
    public Vector2 maxBackgroundOffset = new Vector2(0.5f, 0.5f);

    [Header("Phase Transition")]
    public float fadeDuration = 1.5f;

    private SpriteRenderer bgRenderer;
    private Vector3 initialPosition; 

    void Awake()
    {
        bgRenderer = GetComponent<SpriteRenderer>();
        initialPosition = transform.position;
    }

    void LateUpdate()
    {
        if (playerTransform == null) return;

        // 1. Mathf.InverseLerp를 사용하여 이동 구역 내 플레이어의 위치를 0.0 ~ 1.0 정규화(Normalize) 비율로 변환합니다.
        float tX = Mathf.InverseLerp(arenaMin.x, arenaMax.x, playerTransform.position.x);
        float tY = Mathf.InverseLerp(arenaMin.y, arenaMax.y, playerTransform.position.y);

        // 2. 플레이어의 이동 방향과 정반대(-offset)로 배경을 밀어내는 보간(Lerp) 연산
        float offsetX = Mathf.Lerp(maxBackgroundOffset.x, -maxBackgroundOffset.x, tX);
        float offsetY = Mathf.Lerp(maxBackgroundOffset.y, -maxBackgroundOffset.y, tY);

        transform.position = new Vector3(
            initialPosition.x + offsetX,
            initialPosition.y + offsetY,
            initialPosition.z
        );
    }

    public void ChangePhase(Sprite newBgSprite)
    {
        StartCoroutine(PhaseTransitionRoutine(newBgSprite));
    }

    private IEnumerator PhaseTransitionRoutine(Sprite newSprite)
    {
        float timer = 0f;
        Color originalColor = bgRenderer.color;

        // 암전(Color.black) 페이드 연출
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            bgRenderer.color = Color.Lerp(originalColor, Color.black, timer / fadeDuration);
            yield return null;
        }

        bgRenderer.sprite = newSprite;
        yield return new WaitForSeconds(0.2f);

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