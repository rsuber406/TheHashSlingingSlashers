using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    public static SceneChanger instance {  get; private set; }
    public int activeScene;
    public int numMaps = 3;
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
        SceneManager.LoadScene("ScoreScene");
        GameManager.instance.CalculateScore(activeScene, ScoreIn);
    }

    public void loadNewScene()
    {
        if (activeScene < numMaps)
        {
            GameManager.instance.enablePlayerHUD();
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
