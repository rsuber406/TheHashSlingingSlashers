using UnityEngine;

public class ScoreTester : MonoBehaviour
{

    public ScoreSys scoreSys;
    public int pointsToAdd = 10;
    private float timer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        scoreSys = FindFirstObjectByType<ScoreSys>();
        timer = 0f;
        
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;

        if(timer >= 5f)
        {
            scoreSys.AddFlatScore(pointsToAdd);
            timer = 0f;
        }
        
    }
}
