using UnityEngine;
using UnityEngine.UI;

public class ScoreSys : MonoBehaviour
{
    [SerializeField] private int score;
    [SerializeField] private Text scoreText;

    private float startTime;
    void Start()
    {
       
        startTime = Time.time; // records start time
        UpdateScoreUI();
        
    }

    public void AddScore(int basePoints)
    {
        float timeTaken = Time.time - startTime; 
        float scoreMultiplier = Mathf.Max(1f, 10f - timeTaken);
        int finalScore = Mathf.RoundToInt(scoreMultiplier * basePoints);
        score += finalScore;
        UpdateScoreUI();
    }
    private void UpdateScoreUI()
    {

    }
    
}
