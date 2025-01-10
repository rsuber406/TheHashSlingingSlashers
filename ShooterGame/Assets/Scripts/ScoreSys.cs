using UnityEngine;
using UnityEngine.UI;

public class ScoreSys : MonoBehaviour
{
    [SerializeField] private int score;

    private float startTime;

    void Start()
    {
       
        startTime = Time.time;
        
    }


    public void AddScore(int basePoints)
    {
        float timeTaken = Time.time - startTime; 
        float scoreMultiplier = Mathf.Max(1f, 10f - timeTaken);
        int finalScore = Mathf.RoundToInt(scoreMultiplier * basePoints);
        score += finalScore;
       
    }

    public int GetScore()
    {
        return score;
    }

    
}
