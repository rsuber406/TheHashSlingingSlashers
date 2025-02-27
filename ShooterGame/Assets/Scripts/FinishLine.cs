using UnityEngine;
using UnityEngine.SceneManagement;

public class FinishLine : MonoBehaviour
{
   
    private bool finishPlane = false;

    // Use this for initialization
    void Start()
    {
        ;
        finishPlane = false;

    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            finishPlane = true;
            int finalScore = GameManager.instance.scoreSys.GetScore();
            GameManager.instance.scoreSys.AddFinalScore(finalScore);

            SceneChanger.instance.StageManager(finalScore);
        }
    }

}
