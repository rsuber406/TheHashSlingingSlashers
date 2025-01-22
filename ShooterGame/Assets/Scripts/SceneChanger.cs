using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    public static SceneChanger instance {  get; private set; }
    public ScoreHandler scoreHandler;

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
        if (scoreHandler != null)
        {
            SceneManager.LoadScene("ScoreScene");
            scoreHandler.CalculateScore(activeScene, ScoreIn);

        }//Safety Step for if there is no SceneChanger in the scene
        else
        {
            Debug.LogWarning("SceneChanger reference is null. Skipping score calculation.");
        }

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
