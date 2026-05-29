using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Danmaku.Data
{
    /// <summary>
    /// 발사 형태를 정의합니다.
    /// </summary>
    public enum FireType
    {
        /// <summary>원형(전방위) 발사</summary>
        Round,
        /// <summary>부채꼴 발사</summary>
        Sector,
        /// <summary>산탄(랜덤 퍼짐) 발사</summary>
        Spray
    }

    /// <summary>
    /// 발사 방향의 기준을 정의합니다.
    /// </summary>
    public enum DirectionType
    {
        Fixed,  // 고정된 방향
        Aimed,  // 플레이어를 조준하는 방향
        Random  // 무작위 방향
    }

    public enum ShotOperationType
    {
        ChangeDirectionAndSpeed // 발사 후 방향 및 속도 변경
    }

    [CreateAssetMenu(fileName = "Fire Data", menuName = "DanmakuData/FireData", order = 0)]
    public class FireData : ScriptableObject
    {   
        [Header("기본 발사 설정")]
        /// <summary>총알의 초기 방향 (기본값: 왼쪽)</summary>
        public Vector3 startDir = Vector3.left;

        /// <summary>방향 지정 방식 (고정, 조준, 랜덤)</summary>
        public DirectionType directionType = DirectionType.Fixed;

        /// <summary>초기 방향을 기준으로 한 오프셋(틀어짐) 각도</summary>
        public float startAngle = 0f;
        
        /// <summary>발사 원점으로부터 얼마나 떨어져서 생성될지 결정하는 거리</summary>
        public float startDistance = 0f;
        
        /// <summary>1회 사격 시 발사되는 총알의 수량</summary>
        [Range(1, 50)] public int count = 1;

        [Header("위치 오프셋 설정")]
        /// <summary>생성 위치가 이동할 방향</summary>
        public Vector3 posDir;
        /// <summary>생성 위치의 초기 거리 오프셋</summary>
        public float posStartDistance;
        
        [Header("지연 동작 설정")]
        /// <summary>발사된 총알에 나중에 적용될 동작 리스트 (예: 도중에 멈췄다 날아가기)</summary>
        public List<DelayOperation> delayOperations;

        [Header("발사 타입 세부 설정")]
        /// <summary>발사 모양 타입</summary>
        public FireType type = FireType.Round;

        public FireRound round;
        public FireSector sector;
        public FireSpray spray;
        
        /// <summary>연속 발사(웨이브) 설정</summary>
        public FireGroupData group;
    }

    [System.Serializable]
    public class FireGroupData
    {
        /// <summary>1회 패턴 시 발사되는 그룹(웨이브) 수, 기본값 1</summary>
        [Range(1, 20)] public int num = 1;

        /// <summary>그룹(웨이브) 간의 발사 시간 간격</summary>
        public float interval = 0.1f;

        /// <summary>그룹마다 증가하는 발사 거리 변화량</summary>
        public float deltaDistance = 0f;

        /// <summary>그룹마다 증가하는 발사 각도 변화량 (예: 회전하는 탄막)</summary>
        public float deltaAngle = 0f;

        /// <summary>그룹마다 변경되는 생성 위치 각도</summary>
        public float posDeltaAngle = 0f;
        /// <summary>그룹마다 변경되는 생성 위치 거리</summary>
        public float posDeltaDistance = 0f;

        public FireGroupData()
        {
            num = 1;
            interval = 0f;
        }

        public FireGroupData(int count, float interval)
        {
            this.num = count;
            this.interval = interval;
        }
    }

    [System.Serializable]
    public class FireRound { }

    [System.Serializable]
    public class FireSector
    {
        [Tooltip("부채꼴 발사 시 총알 간의 각도 간격")]
        public float deltaAngle;
    }

    [System.Serializable]
    public class FireSpray
    {
        [Tooltip("산탄 발사 속도 제한")]
        public LimitedValue fire;
        [Tooltip("산탄 발사 각도 제한")]
        public LimitedValue angle;
    }

    [System.Serializable]
    public class DelayOperation
    {
        public ShotOperationType type = ShotOperationType.ChangeDirectionAndSpeed;
        [Tooltip("명령이 실행되기까지의 대기 시간")]
        public float delay;
        
        public DirectionType directionType;
        public Vector2 direction;

        public LimitedValue speed;
        public float deltaSpeed;

        public LimitedValue angle;
        public float deltaAngle;
    }
}