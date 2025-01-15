using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ScoreSys : MonoBehaviour
{
    [SerializeField] private int score;

    private float startTime;
    GameTimer currGameTime;

    List<KeyValuePair<float, float>> timeTiers = new List<KeyValuePair<float, float>>()
        {
            new KeyValuePair<float, float>(0.75f, 10f),
            new KeyValuePair<float, float>(0.5f, 5f),
            new KeyValuePair<float, float>(0.25f, 2.5f),
            new KeyValuePair<float, float>(0f, 1f)
        };


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

        float timeRatio = gameTimer / timeTaken;

      
        float scoreMultiplier = 1f;
        int currentTier = 0; 

        for (int i = 0; i < timeTiers.Count; i++)
        {
            if (timeRatio >= timeTiers[i].Key)
            {
                scoreMultiplier = timeTiers[i].Value;
                currentTier = i + 1; 
                break;
            }
        }

        int potentialReward = Mathf.RoundToInt(scoreMultiplier * basePoints);
        score += potentialReward;


    }

    public int GetScore()
    {
        return score;
    }
    public float CalculatePotentialReward()
    {
        float timeTaken = Time.time - startTime;
        float gameTimer = currGameTime.GetTime();
        float timeRatio = gameTimer / timeTaken;

        float scoreMultiplier = 1f;

        foreach (KeyValuePair<float, float> tier in timeTiers)
        {
            if (timeRatio >= tier.Key)
            {
                scoreMultiplier = tier.Value;
                break;
            }
        }

        return scoreMultiplier;
    }
  


}
