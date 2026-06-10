using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 인게임 상단에 표시되는 점수(Score) UI를 관리하는 가벼운 싱글톤 매니저 클래스입니다.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    [SerializeField] private Text scoreText;

    private void Awake() => Instance = this;

    /// <summary>
    /// 플레이어가 적을 파괴하거나 아이템을 먹을 때 호출되어 점수 텍스트를 갱신합니다.
    /// 문자열 보간($"")과 0채우기(D6) 포맷을 활용하여 깔끔한 표기 형태(예: 001250)를 만듭니다.
    /// </summary>
    public void UpdateScore(int score)
    {
        scoreText.text = $"Score: {score:D6}";
    }
}