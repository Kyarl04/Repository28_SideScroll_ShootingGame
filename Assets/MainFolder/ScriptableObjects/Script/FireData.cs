using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Danmaku.Data
{
    public enum FireType { Round, Sector, Spray }
    public enum DirectionType { Fixed, Aimed, Random }
    public enum ShotOperationType { ChangeDirectionAndSpeed }

    /// <summary>
    /// 탄막의 발사 방향, 개수, 모양을 정의하는 설정 파일.
    /// FireRound, FireSector, FireSpray 등 파생 데이터를 포함하여 다양한 탄막 기하학 패턴을 생성합니다.
    /// </summary>
    [CreateAssetMenu(fileName = "Fire Data", menuName = "DanmakuData/FireData", order = 0)]
    public class FireData : ScriptableObject
    {   
        [Header("기본 발사 설정")]
        public Vector3 startDir = Vector3.left;
        public DirectionType directionType = DirectionType.Fixed;
        public float startAngle = 0f;
        public float startDistance = 0f;
        [Range(1, 50)] public int count = 1; // 1회 발사 당 탄막 수

        [Header("위치 오프셋 설정")]
        public Vector3 posDir;
        public float posStartDistance;
        
        [Header("지연 동작 설정")]
        public List<DelayOperation> delayOperations; // 발사 도중 방향/속도 변경 로직

        [Header("발사 타입 세부 설정")]
        public FireType type = FireType.Round;

        public FireRound round;
        public FireSector sector;
        public FireSpray spray;
        public FireGroupData group; // 연속적인 발사 그룹(웨이브) 설정
    }

    [System.Serializable]
    public class FireGroupData
    {
        [Range(1, 20)] public int num = 1;
        public float interval = 0.1f;
        public float deltaDistance = 0f; // 웨이브마다 총알 생성 거리 증가량
        public float deltaAngle = 0f;    // 웨이브마다 탄막 전체의 회전각 증가량
        public float posDeltaAngle = 0f; 
        public float posDeltaDistance = 0f;

        public FireGroupData() { num = 1; interval = 0f; }
    }

    //  
    // 발사 형태 구조체들입니다.
    [System.Serializable] public class FireRound { }
    [System.Serializable] public class FireSector { public float deltaAngle; }
    [System.Serializable] public class FireSpray { public LimitedValue fire; public LimitedValue angle; }

    [System.Serializable]
    public class DelayOperation
    {
        public ShotOperationType type = ShotOperationType.ChangeDirectionAndSpeed;
        public float delay;
        public DirectionType directionType;
        public Vector2 direction;
        public LimitedValue speed;
        public float deltaSpeed;
        public LimitedValue angle;
        public float deltaAngle;
    }
}