using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    public static SceneChanger instance {  get; private set; }

    public int activeScene;
    public int numMaps = 2;
    public void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StageManager(int ScoreIn)
    {
        ScoreHandler scoreHandler = new ScoreHandler();
        SceneManager.LoadScene("ScoreScene");
        scoreHandler.CalculateScore(activeScene, ScoreIn);
        activeScene++;
        if (activeScene < numMaps)
        {
            SceneManager.LoadScene(activeScene + 1);
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        else
        {
            SceneManager.LoadScene("Main Menu");
        }
    }
}
