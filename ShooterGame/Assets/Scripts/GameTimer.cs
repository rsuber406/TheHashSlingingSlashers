using UnityEngine;

public class GameTimer : MonoBehaviour
{

    [SerializeField] float gameTime = 120f;
    private float timer;
    void Start()
    {
        timer = gameTime;
        
    }

    // Update is called once per frame
    void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            timer = 0f;
            GameManager.instance.Lose();
        }
        
    }
   public float GetTime()
    {
        return timer;
    }
    public float GetInitialTime()
    {
        return gameTime;
    }
    public float GetTimeRatio()
    {
        return timer / gameTime;
    }
 

}
