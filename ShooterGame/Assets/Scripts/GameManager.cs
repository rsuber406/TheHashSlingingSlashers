using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField] GameObject PlayerHUD;
    [SerializeField] GameObject menuActive;
    [SerializeField] GameObject menuPause;
    [SerializeField] GameObject menuWin;
    [SerializeField] GameObject menuLose;
    [SerializeField] GameObject playerDmgScreen;
    [SerializeField] GameObject playerGainHealth;

    [SerializeField] GameObject lowHealthScreen;
    [SerializeField] TMP_Text CurrentBulletsMagText;
    [SerializeField] TMP_Text CurrentBulletsReserveText;
    [SerializeField] TMP_Text ReloadText;

    //Score Stuff. This is badly done, but whatever, its still UI technically.
    [SerializeField] TMP_Text LevelCompleteText;
    [SerializeField] TMP_Text topScore;
    [SerializeField] TMP_Text curScore;
    [SerializeField] TMP_Text timeToComplete;
    [SerializeField] TMP_Text enemiesKilled;

    public TMP_Text PubReloadText => ReloadText;
    public TMP_Text pubCurrentBulletsMagText => CurrentBulletsMagText;
    public TMP_Text pubCurrentBulletsReserveText => CurrentBulletsReserveText;
    public GameObject PublowHealthScreen => lowHealthScreen;

    public Image playerHPBar;
    public Image playerBulletTimeBar;

    public ScoreSys scoreSys;

    public PlayerController playerscript;
    public GameObject player;
    public GameTimer gameTimer;
    public BulletTime bt;
    public DynamicTextManager dynamicTextManager;

    public bool isPaused;


    float scoreTimer;
    int goalCount;
    int playerCurrentHealth;
    bool loadedConfigs = false;
    void Awake()
    {

        instance = this;
        player = GameObject.FindWithTag("Player");
        playerscript = player.GetComponent<PlayerController>();
        scoreSys = FindFirstObjectByType<ScoreSys>();


    }

    void LoadConfigs()
    {
        
        DynamicTextManager.mainCamera = Camera.main.transform;
        loadedConfigs = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!loadedConfigs)
            LoadConfigs();

        if (Input.GetButtonDown("Cancel"))
        {
            if (menuActive == null)
            {
                statePause();
                menuActive = menuPause;
                menuActive.SetActive(true);
            }
            else if (menuActive != null)
            {
                stateUnpause();
            }
        }
    }

    public void statePause()
    {
        isPaused = true;
        Time.timeScale = 0;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
    }
    public void stateUnpause()
    {
        isPaused = false;
        Time.timeScale = 1;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        menuActive.SetActive(false);
        menuActive = null;

    }

    public void Lose()
    {
        statePause();
        menuActive = menuLose;
        menuActive.SetActive(true);
    }

    public void Win()
    {
        statePause();
        menuActive = menuWin;
        menuActive.SetActive(true);
    }

    public void FlashDamageScreenOn()
    {
        menuActive = playerDmgScreen;
        menuActive.SetActive(true);
    }
    public void FlashDamageScreenOff()
    {
        menuActive.SetActive(false);
        menuActive = null;
    }
    public void FlashHealthScreenOn()
    {
        menuActive = playerGainHealth;
        menuActive.SetActive(true);
    }

    public void UpdatePlayerHeathUI(int currentHealth)
    {
        playerCurrentHealth = currentHealth;

    }
    public float GetPlayerBulletTimeLeft()
    {
        float bulletTimeRemaining = bt.GetBulletTimeRemaining();
        return bulletTimeRemaining;


    }
    public int GetPlayerHealth() { return playerCurrentHealth; }


    public void disablePlayerHUD()
    {
        PlayerHUD.SetActive(false);

    }

    public void enablePlayerHUD()
    {
        PlayerHUD.SetActive(true);
    }


    public IEnumerator CalculateScore(int completedlvl, int ScoreIn)
    {
        float timePassed = 0f;
        StartCoroutine(DisplayScores(completedlvl, ScoreIn));
        float timeToWait = 8f;
        while (timePassed < timeToWait)
        {
            timePassed += Time.deltaTime;
            yield return null;
        }
        // DisableScoreDetails();
        SceneChanger.instance.loadNewScene();

        //TODO: Add time and Enemies Killed to the score screen as parameters
    }
    IEnumerator DisplayScores(int completedlvl, int ScoreIn)
    {
        Time.timeScale = 1;
        disablePlayerHUD();
        LevelCompleteText.text = "F0 Level " + completedlvl + " Completed!";
        timeToComplete.text = "F0 Time: To be Determined";
        enemiesKilled.text = "F0 Enemies Killed: To be Determined";
        curScore.text = "F0 Total Score: " + curScore;
        topScore.text = "F0 Top Score: " + curScore; //TODO: need code to read from a txt file.


        LevelCompleteText.enabled = true;
        // yield return new WaitForSeconds(1f);
        timeToComplete.enabled = true;
        // yield return new WaitForSeconds(1f);
        enemiesKilled.enabled = true;
        // yield return new WaitForSeconds(1f);
        curScore.enabled = true;
        // yield return new WaitForSeconds(1f);
        topScore.enabled = true;
        yield return new WaitForSeconds(2f);


        //Then turn them all off again


    }

    void DisableScoreDetails()
    {
        LevelCompleteText.enabled = false;
        timeToComplete.enabled = false;
        enemiesKilled.enabled = false;
        curScore.enabled = false;
        topScore.enabled = false;
        scoreTimer = 0;
    }

}

