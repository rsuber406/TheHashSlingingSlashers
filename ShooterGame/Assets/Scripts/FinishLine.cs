using UnityEngine;
using UnityEngine.UI;

public class FinishLine : MonoBehaviour
{
    public ScoreSys scoreSys;
    private bool FinishPlane = false;

    // Use this for initialization
    void Start()
    {
        scoreSys = FindFirstObjectByType<ScoreSys>();
        FinishPlane = false;

    }

    void OnTriggerEnter(Collider other)
    {
        //game pause code goes here

            FinishPlane = true;
            scoreSys.AddScore(10);

    }

}
