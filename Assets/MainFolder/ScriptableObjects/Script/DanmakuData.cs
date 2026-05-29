using UnityEngine;
using System;
using System.Collections.Generic;

namespace Danmaku.Data
{
    /// <summary>
    /// 탄막 패턴의 성격을 정의합니다.
    /// </summary>
    public enum DanmakuType
    {
        SpellCard,    // 스펠카드 (필살기 패턴)
        SurvivalCard, // 버티기 스펠카드 (시간이 지나야 클리어 됨)
        NonSpellCard  // 통상 패턴 (일반 공격)
    }

    /// <summary>
    /// 패턴 도중 보스의 이동 방식을 정의합니다.
    /// </summary>
    public enum DanmakuMoveType
    {
        None,       // 이동하지 않음
        RandomMove  // 지정된 범위 내에서 무작위 이동
    }

    /// <summary>
    /// 패턴 발사 시 재생할 보스의 애니메이션을 정의합니다.
    /// </summary>
    public enum DanmakuAnimation
    {
        None,
        AttackBeforeFire // 발사 전 공격 모션 재생
    }

    [CreateAssetMenu(fileName = "Danmaku Data", menuName = "DanmakuData/DanmakuData", order = 0)]
    public class DanmakuData : ScriptableObject
    {
        [Tooltip("화면에 표시될 스펠카드의 이름 (예: 마포「파이널 스파크」)")]
        public string danmakuName;

        [Tooltip("이 패턴에서 발사할 탄막(Barrage) 리스트")]
        public List<BarrageData> data;

        [Tooltip("패턴의 성격 (통상, 스펠카드, 버티기)")]
        public DanmakuType type = DanmakuType.SpellCard;
        
        [Tooltip("패턴 사용 시 애니메이션 연출")]
        public DanmakuAnimation animation = DanmakuAnimation.None;

        [Tooltip("패턴 진행 중 보스의 이동 설정")]
        public DanmakuMove move;
        
        /// <summary>통상 패턴 및 스펠카드일 때 보스의 체력</summary>
        [Tooltip("이 페이즈에서의 보스 총 체력")]
        public int hp;

        public void AddBarrage(BarrageData bd)
        {
            data.Add(bd);
        }
    }
    
    [System.Serializable]
    public class DanmakuMove
    {
        [Tooltip("이동 방식 (없음 또는 랜덤 이동)")]
        public DanmakuMoveType type;
        
        [Tooltip("패턴 시작 시 보스가 이동할 초기 위치 (중앙 정렬용)")]
        public Vector3 startPosition;
        
        [Tooltip("초기 위치로 이동하기 전 대기 시간")]
        public float startDelay;
        
        [Header("랜덤 이동 범위 (경계선)")]
        public float minX;
        public float maxX;
        public float minY;
        public float maxY;
        
        [Header("이동 타이밍")]
        [Tooltip("다음 이동까지의 간격")]
        public float interval;
        [Tooltip("이동 전 딜레이")]
        public float delay;
        [Tooltip("한 번 이동할 때 걸리는 시간(속도)")]
        public float duration;
    }
}