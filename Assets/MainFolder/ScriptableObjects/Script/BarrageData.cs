using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Danmaku.Data
{
    /// <summary>
    /// 탄막의 각도가 시간의 흐름에 따라 어떻게 변화할지 계산 방식을 정의합니다.
    /// </summary>
    public enum OffsetType
    {
        None,   // 변화 없음
        Linear, // 일정한 속도로 회전
        Sin     // 사인 파동에 따라 흔들리며 발사
    }

    /// <summary>
    /// 탄막 발사의 단일 웨이브(Barrage) 데이터를 관리하는 ScriptableObject입니다.
    /// 보스의 각 페이즈 내에서 여러 번 실행되는 탄막의 기본 발사 설정을 담습니다.
    /// </summary>
    [CreateAssetMenu(fileName = "Barrage Data", menuName = "DanmakuData/BarrageData", order = 0)]
    public class BarrageData : ScriptableObject
    {
        [Tooltip("사용할 총알의 속성(속도, lifetime 등) 데이터")]
        public ShotData shotData;

        [Tooltip("스펠카드 시작 후 첫 탄막 발사 대기 시간")]
        public float startDelay;

        [Tooltip("연속적인 웨이브 발사 간의 휴식 시간")]
        public float interval;

        [Tooltip("발사 모양(원형, 부채꼴 등) 데이터")]
        public FireData fireData;

        [Tooltip("발사 각도가 시간 흐름에 따라 변화하는 기믹 설정")]
        public FireOffset fireOffset;

        public BarrageData(ShotData shotData, FireData fd, float interval, float delay = 0f)
        {
            this.shotData = shotData;
            this.fireData = fd;
            this.interval = interval;
            this.startDelay = delay;
        }
    }

    /// <summary>
    /// 발사 각도를 주기적으로 변화시켜 탄막에 기하학적 무늬(예: 소용돌이)를 만드는 설정 클래스.
    /// </summary>
    [System.Serializable]
    public class FireOffset
    {
        [Tooltip("각도 변화의 1 사이클 길이 (총알 발사 횟수 기준)")]
        public int cycle;

        [Tooltip("사이클을 시작할 인덱스 위치")]
        public int startCycleIndex;

        [Tooltip("각도가 변화하는 최대 범위")]
        public float range = 360f;

        [Tooltip("각도 변화 방식: 선형(Linear) 또는 파동(Sin)")]
        public OffsetType type = OffsetType.Linear;

        [Tooltip("왕복 운동(와이퍼 형태) 여부: True면 좌우로 흔들림")]
        public bool reciprocate = false;
    }
}