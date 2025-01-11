using UnityEngine;

public class FinishLine : MonoBehaviour
{
    public ScoreSys scoreSys;
    private bool finishPlane = false;

    // Use this for initialization
    void Start()
    {
        scoreSys = FindFirstObjectByType<ScoreSys>();
        finishPlane = false;

    }

    void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("Player"))
        {
            finishPlane = true;
          int finalScore = GameManager.instance.scoreSys.GetScore();
          GameManager.instance.scoreSys.AddFinalScore(finalScore);
            GameManager.instance.statePause();
            GameManager.instance.Win();
        }

        
    }

}
