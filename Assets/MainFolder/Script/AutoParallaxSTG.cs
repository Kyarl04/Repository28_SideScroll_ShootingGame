using UnityEngine;

public class AutoParallaxSTG : MonoBehaviour
{
    [System.Serializable]
    public class ParallaxLayer
    {
        [Tooltip("이 레이어에 속한 배경 이미지들 (2~3장씩)")]
        public Transform[] backgrounds;
        [Tooltip("이 레이어의 스크롤 속도 (멀리 있을수록 느리게 설정)")]
        public float speed;
    }

    [Header("다중 레이어 설정")]
    public ParallaxLayer[] layers;

    [Header("공통 설정")]
    [Tooltip("배경 이미지 1장의 정확한 가로 길이")]
    public float spriteWidth = 19.2f; // 본인 배경 크기에 맞게 수정하세요.

    private void Start()
    {
        // 시작 시 자동 정렬 (에디터에서 대충 겹쳐놔도 빈틈없이 붙여줍니다)
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

                // 카메라가 아닌 배경 자체가 설정된 속도로 왼쪽으로 이동합니다.
                bg.Translate(Vector3.left * layer.speed * Time.deltaTime, Space.World);

                // 화면 밖으로 완전히 나갔다면 맨 뒤로 점프 (무한 스크롤)
                if (bg.position.x <= transform.position.x - spriteWidth)
                {
                    Vector3 jumpPos = new Vector3(spriteWidth * layer.backgrounds.Length, 0, 0);
                    bg.position += jumpPos;
                }
            }
        }
    }
}