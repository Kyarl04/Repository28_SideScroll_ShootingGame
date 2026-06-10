using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Danmaku.Data
{
    public enum MoveMode { Straight, Circular, Ball };
    public enum WallCheckMode { Clear, Ignore, Bounce }

    /// <summary>
    /// 개별 총알의 물리적 이동 속성(속도, 가속도, 수명 등)을 정의합니다.
    /// </summary>
    [CreateAssetMenu(fileName = "Shot Data", menuName = "DanmakuData/ShotData", order = 0)]
    public class ShotData : ScriptableObject
    {
        [Tooltip("BulletPooler에 등록된 총알 프리팹 인덱스")]
        public int prefabIndex;
        
        [Tooltip("총알의 생존 시간(최대 수명)")]
        public float lifetime = 5f;

        [Tooltip("총알의 속도 제한 및 초기 속도")]
        public LimitedValue speed = new LimitedValue(1, 1, 1);
        [Tooltip("시간에 따른 가속/감속 값")]
        public float deltaSpeed = 0f;

        [Tooltip("총알의 각도 고정/변화 설정")]
        public LimitedValue angle = new LimitedValue(0, 0, 0);
        [Tooltip("시간에 따른 회전 변화량(휘어지는 총알 기믹)")]
        public float deltaAngle = 0f;

        [Tooltip("이동 물리 모드")]
        public MoveMode moveMode = MoveMode.Straight;
        
        [Tooltip("벽면 충돌 시 동작(삭제/튕김/무시)")]
        public WallCheckMode wallCheckMode = WallCheckMode.Clear;
    }
}