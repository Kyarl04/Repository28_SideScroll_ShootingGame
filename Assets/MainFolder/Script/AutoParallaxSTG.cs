using UnityEngine;

/// <summary>
/// 슈팅 게임 특유의 끊임없이 스크롤되는 다중 레이어 배경(Parallax)을 구현한 클래스.
/// 이미지를 자동으로 이어 붙여주고 지정된 속도로 이동시킨 뒤 재배치하는 무한 스크롤 시스템입니다.
/// </summary>
public class AutoParallaxSTG : MonoBehaviour
{
    [System.Serializable]
    public class ParallaxLayer
    {
        public Transform[] backgrounds;
        public float speed; // 깊이감(Depth)을 주기 위해 레이어마다 속도를 다르게 설정
    }

    [Header("다중 레이어 설정")]
    public ParallaxLayer[] layers;

    [Header("공통 설정")]
    public float spriteWidth = 19.2f; 

    private void Start()
    {
        // 시작 시 각 배경 조각들의 간격을 폭에 맞게 자동으로 정렬(Seam-less)해줍니다.
        foreach (var layer in layers)
        {
            for (int i = 0; i < layer.backgrounds.Length; i++)
            {
                layer.backgrounds[i].position = new Vector3(
                    transform.position.x + (i * spriteWidth),
                    layer.backgrounds[i].position.y,
                    layer.backgrounds[i].position.z
                );
            }
        }
    }

    private void Update()
    {
        if (spriteWidth == 0) return;

        foreach (var layer in layers)
        {
            for (int i = 0; i < layer.backgrounds.Length; i++)
            {
                Transform bg = layer.backgrounds[i];

                // 카메라 이동 방식이 아닌 배경 오브젝트 자체를 이동시키는 고전적인 구현
                bg.Translate(Vector3.left * layer.speed * Time.deltaTime, Space.World);

                // 화면 밖으로 완전히 벗어난 배경 조각을 맨 뒤로 점프시킵니다.
                if (bg.position.x <= transform.position.x - spriteWidth)
                {
                    Vector3 jumpPos = new Vector3(spriteWidth * layer.backgrounds.Length, 0, 0);
                    bg.position += jumpPos;
                }
            }
        }
    }

    /// <summary>
    /// 보스의 페이즈가 변경될 때 호출되어 스크롤 중인 배경 이미지를 새롭게 일괄 교체합니다.
    /// </summary>
    public void ChangeBackgroundSprites(Sprite[] newSprites)
    {
        if (newSprites == null || newSprites.Length == 0) return;

        for (int i = 0; i < layers.Length; i++)
        {
            Sprite targetSprite = (i < newSprites.Length) ? newSprites[i] : newSprites[newSprites.Length - 1];

            foreach (Transform bg in layers[i].backgrounds)
            {
                SpriteRenderer sr = bg.GetComponent<SpriteRenderer>();
                if (sr != null) sr.sprite = targetSprite;

                UnityEngine.UI.Image img = bg.GetComponent<UnityEngine.UI.Image>();
                if (img != null) img.sprite = targetSprite;
            }
        }
    }
}