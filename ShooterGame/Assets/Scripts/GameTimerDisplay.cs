using TMPro;
using UnityEngine;

public class GameTimerDisplay : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public TextMeshProUGUI timerText;
    private GameTimer gameTimer;
    void Start()
    {
        gameTimer = FindFirstObjectByType<GameTimer>();
        UpdateTimerUI();

    }

    // Update is called once per frame
    void Update()
    {
        UpdateTimerUI();
    }

    private void UpdateTimerUI()
    {
        int timeInSeconds = Mathf.FloorToInt(gameTimer.GetTime());
        timerText.text = "Time left: " + timeInSeconds.ToString();
    }
}
