using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 배경의 자동 스크롤 및 무한 루프 기능을 제어합니다.
/// </summary>
public class ScrollingScript : MonoBehaviour {

    [Header("이동 설정")]
    public Vector2 speed = new Vector2(2, 2);       // 이동 속도
    public Vector2 direction = new Vector2(-1, 0); // 이동 방향

    [Header("옵션")]
    public bool isLinkedToCamera = false; // 카메라와 연동 여부
    public bool isLooping = false;        // 배경 루프 여부

    private List<Transform> backgroundPart;   // 루프할 배경 조각들

    void Start()
    {
        if (isLooping)
        {
            backgroundPart = new List<Transform>();

            // 자식 오브젝트 중 렌더러가 있는 것들만 배경 조각으로 등록
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                if (child.GetComponent<Renderer>() != null)
                {
                    backgroundPart.Add(child);
                }
            }

            // X축 위치를 기준으로 왼쪽에서 오른쪽 순서로 정렬
            backgroundPart = backgroundPart.OrderBy(t => t.position.x).ToList();
        }
    }

    void Update()
    {
        // 1. 이동 처리
        Vector3 movement = new Vector3(speed.x * direction.x, speed.y * direction.y, 0);
        movement *= Time.deltaTime;
        transform.Translate(movement);

        // 2. 카메라 연동 처리
        if (isLinkedToCamera)
        {
            Camera.main.transform.Translate(movement);
        }

        // 3. 무한 루프 처리
        if (isLooping)
        {
            // 가장 왼쪽에 있는 배경 조각 가져오기
            Transform firstChild = backgroundPart.FirstOrDefault();

            if (firstChild != null)
            {
                // 배경이 카메라의 왼쪽 화면 밖으로 완전히 나갔는지 확인
                if (firstChild.position.x < Camera.main.transform.position.x)
                {
                    if (!firstChild.GetComponent<Renderer>().IsVisibleFrom(Camera.main))
                    {
                        // 가장 오른쪽에 있는 배경 조각의 위치와 크기 계산
                        Transform lastChild = backgroundPart.LastOrDefault();
                        Vector3 lastPosition = lastChild.transform.position;
                        Vector3 lastSize = (lastChild.GetComponent<Renderer>().bounds.max - lastChild.GetComponent<Renderer>().bounds.min);

                        // 나간 조각을 가장 오른쪽 뒤로 이동 (현재 가로 스크롤 전용)
                        firstChild.position = new Vector3(lastPosition.x + lastSize.x, firstChild.position.y, firstChild.position.z);

                        // 리스트 순서 갱신 (맨 앞을 제거하고 맨 뒤로 추가)
                        backgroundPart.Remove(firstChild);
                        backgroundPart.Add(firstChild);
                    }
                }
            }
        }
    }
}