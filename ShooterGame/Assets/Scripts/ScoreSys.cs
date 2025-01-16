using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ScoreSys : MonoBehaviour
{
    [SerializeField] private int score;
    [SerializeField] private float currentMultiplier; 
    [SerializeField] private float currentTimeRatio;
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
    private void Update()
    {
        currentMultiplier = CalculatePotentialReward();
        if (currGameTime != null)
        {
            currentTimeRatio = currGameTime.GetTime() / currGameTime.GetInitialTime();
        }
    }


    public void AddFlatScore(int basePoints)
    {
        score += basePoints; 
        //for kills
    }

    public void AddFinalScore(int basePoints)
    {
        float multiplier = CalculatePotentialReward();
        int potentialReward = Mathf.RoundToInt(multiplier * basePoints);
        score += potentialReward;


    }

    public int GetScore()
    {
        return score;
    }
    public float CalculatePotentialReward()
    {

        if (currGameTime == null) return 1f;

        float totalTime = currGameTime.GetInitialTime();
        float timeRemaining = currGameTime.GetTime();
        float timeRatio = timeRemaining / totalTime;

        for (int i = 0; i < timeTiers.Count; i++)
        {
            if (timeRatio >= timeTiers[i].Key)
            {
                return timeTiers[i].Value; 
            }
        }
        return 1f;

     
    }
  


}
