using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Danmaku.Data
{
    /// <summary>
    /// 총알의 이동 방식을 정의합니다.
    /// </summary>
    public enum MoveMode
    {
        Straight, // 직진
        Circular, // 원운동
        Ball      // 공처럼 튕기거나 중력 적용
    };

    /// <summary>
    /// 화면 경계(벽)에 닿았을 때의 처리 방식을 정의합니다.
    /// </summary>
    public enum WallCheckMode
    {
        Clear,  // 벽에 닿으면 삭제
        Ignore, // 벽을 무시하고 계속 이동
        Bounce  // 벽에 닿으면 튕겨냄
    }

    [CreateAssetMenu(fileName = "Shot Data", menuName = "DanmakuData/ShotData", order = 0)]
    public class ShotData : ScriptableObject
    {
        [Tooltip("BulletPooler에 등록된 총알 프리팹의 인덱스 번호")]
        public int prefabIndex;
        
        [Tooltip("총알의 수명 (화면에 존재하는 시간)")]
        public float lifetime = 5f;

        [Tooltip("총알의 속도 제한 범위 및 기본값")]
        public LimitedValue speed = new LimitedValue(1, 1, 1);
        
        [Tooltip("시간에 따른 속도 변화량 (가속/감속)")]
        public float deltaSpeed = 0f;

        [Tooltip("총알의 각도 제한 범위 및 기본값")]
        public LimitedValue angle = new LimitedValue(0, 0, 0);
        
        [Tooltip("시간에 따른 각도 변화량 (휘어지는 총알)")]
        public float deltaAngle = 0f;

        [Tooltip("총알의 이동 방식")]
        public MoveMode moveMode = MoveMode.Straight;
        
        [Tooltip("벽면 충돌 처리 방식")]
        public WallCheckMode wallCheckMode = WallCheckMode.Clear;
        
        public ShotData(int index, float baseSpeed)
        {
            prefabIndex = index;
            speed = new LimitedValue(baseSpeed, baseSpeed, baseSpeed);
        }
    }
}