using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    [SerializeField] private Text scoreText;

    private void Awake() => Instance = this;

    public void UpdateScore(int score)
    {
        scoreText.text = $"Score: {score:D6}";
    }
}
