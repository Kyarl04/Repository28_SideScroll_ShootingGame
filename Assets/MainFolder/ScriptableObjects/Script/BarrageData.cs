using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Danmaku.Data
{
    /// <summary>
    /// 발사 각도 변화의 계산 방식을 정의합니다.
    /// </summary>
    public enum OffsetType
    {
        None,   // 변함 없음
        Linear, // 선형적(일정하게) 변화
        Sin     // 사인 곡선(물결치듯) 변화
    }

    [CreateAssetMenu(fileName = "Barrage Data", menuName = "DanmakuData/BarrageData", order = 0)]
    public class BarrageData : ScriptableObject
    {
        [Tooltip("사용할 총알의 기본 데이터")]
        public ShotData shotData;

        /// <summary>스펠카드 시작 후 첫 번째 탄막을 발사하기까지 대기하는 시간</summary>
        [Tooltip("스펠카드 시작 후 첫 탄막 발사 대기 시간")]
        public float startDelay;

        /// <summary>발사 패턴(웨이브) 간의 휴식 간격</summary>
        [Tooltip("두 발사 패턴 사이의 시간 간격")]
        public float interval;

        /// <summary>어떻게 발사할 것인지 정의한 데이터</summary>
        [Tooltip("발사 형태 및 방식 데이터")]
        public FireData fireData;

        /// <summary>발사 각도의 주기적인 변화 설정</summary>
        [Tooltip("발사 각도가 변하는 주기 설정 (예: 좌우로 흔들리는 탄막)")]
        public FireOffset fireOffset;

        public BarrageData(ShotData shotData, FireData fd, float interval, float delay = 0f)
        {
            this.shotData = shotData;
            this.fireData = fd;
            this.interval = interval;
            this.startDelay = delay;
        }
    }

    [System.Serializable]
    public class FireOffset
    {
        [Tooltip("각도 변화의 1 사이클 길이")]
        public int cycle;

        [Tooltip("사이클을 시작할 인덱스 위치")]
        public int startCycleIndex;

        [Tooltip("각도가 변화하는 최대 범위")]
        public float range = 360f;

        /// <summary>발사 각도 변화 계산 방법</summary>
        [Tooltip("각도가 어떻게 변할지 설정 (선형, 사인 파동 등)")]
        public OffsetType type = OffsetType.Linear;

        /// <summary>각도가 한계에 다다랐을 때 왕복할지(True), 초기 위치로 튕겨 돌아갈지(False) 여부</summary>
        [Tooltip("왕복 운동(와이퍼 형태) 여부")]
        public bool reciprocate = false;
    }
}