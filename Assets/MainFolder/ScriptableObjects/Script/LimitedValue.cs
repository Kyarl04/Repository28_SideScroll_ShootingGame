using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 특정 수치가 가질 수 있는 최소(min)와 최대(max) 범위를 정의하는 데이터 유틸리티 클래스.
/// 탄막의 난이도 범위를 직관적으로 설정할 때 사용합니다.
/// </summary>
[System.Serializable]
public class LimitedValue
{
    public float value = 0; // 현재 값
    public float min = 0, max = 0; // 변동 가능한 최소/최대 범위

    public LimitedValue(float v, float min, float max){
        value = v;
        this.min = min;
        this.max = max;
    }
}