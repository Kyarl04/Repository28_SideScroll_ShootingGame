using UnityEngine;
using System;
using System.Collections.Generic;

namespace Danmaku.Data
{
    public enum DanmakuType { SpellCard, SurvivalCard, NonSpellCard }
    public enum DanmakuMoveType { None, RandomMove }
    public enum DanmakuAnimation { None, AttackBeforeFire }

    /// <summary>
    /// 보스 페이즈별 탄막 패턴, 보스 체력, 배경 전환 정보를 묶은 최상위 데이터 컨테이너.
    /// ScriptableObject를 사용하여 에디터상에서 패턴을 데이터 파일로 관리할 수 있습니다.
    /// </summary>
    [CreateAssetMenu(fileName = "Danmaku Data", menuName = "DanmakuData/DanmakuData", order = 0)]
    public class DanmakuData : ScriptableObject
    {
        [Tooltip("화면에 표시될 스펠카드의 이름")]
        public string danmakuName;

        [Tooltip("이 패턴에서 순차적으로 실행될 탄막(Barrage) 웨이브 리스트")]
        public List<BarrageData> data;

        [Tooltip("패턴의 성격 (보스 체력 소진 등 클리어 조건에 영향)")]
        public DanmakuType type = DanmakuType.SpellCard;
        
        [Tooltip("패턴 발동 시 보스가 취할 특정 애니메이션")]
        public DanmakuAnimation animation = DanmakuAnimation.None;

        [Tooltip("패턴 진행 중 보스의 이동 규칙")]
        public DanmakuMove move;
        
        [Tooltip("이 페이즈에서의 보스 총 체력")]
        public int hp;

        public void AddBarrage(BarrageData bd) => data.Add(bd);
    }
    
    /// <summary>
    /// 패턴 도중 보스의 이동 경로와 속도를 제어하는 구조체.
    /// </summary>
    [System.Serializable]
    public class DanmakuMove
    {
        public DanmakuMoveType type;
        public Vector3 startPosition;
        public float startDelay;
        
        [Header("랜덤 이동 범위 (경계선)")]
        public float minX, maxX, minY, maxY;
        
        [Header("이동 타이밍")]
        public float interval; // 이동 간격
        public float delay;    // 이동 전 대기
        public float duration; // 이동에 걸리는 시간(속도)
    }
}