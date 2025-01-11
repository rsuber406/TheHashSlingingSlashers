using UnityEngine;
using UnityEngine.UI;

public class ScoreSys : MonoBehaviour
{
    [SerializeField] private int score;

    private float startTime;
    GameTimer currGameTime;

    void Start()
    {
       currGameTime = FindFirstObjectByType<GameTimer>();
        startTime = Time.time;
        
    }


    public void AddFlatScore(int basePoints)
    {
        score += basePoints; 
        //for kills
    }

    public void AddFinalScore(int basePoints)
    {
        float timeTaken = Time.time - startTime;
        float gameTimer = currGameTime.GetTime();
        float scoreMultiplier = Mathf.Max(1f, gameTimer - timeTaken);
        int finalScore = Mathf.RoundToInt(scoreMultiplier * basePoints);
        score += finalScore;
       //This is for end of game
    }

    public int GetScore()
    {
        return score;
    }

    
}
