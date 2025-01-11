using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField] GameObject menuActive;
    [SerializeField] GameObject menuPause;
    [SerializeField] GameObject menuWin;
    [SerializeField] GameObject menuLose;

    public Image playerHPBar;
    public GameObject damagePanel;
    public ScoreSys scoreSys;

    public PlayerController playerscript;
    public GameObject player;

    public bool isPaused;

    int goalCount;
    void Awake()
    {
        instance = this;
        player = GameObject.FindWithTag("Player");
        playerscript = player.GetComponent<PlayerController>();
        scoreSys = FindFirstObjectByType<ScoreSys>();
    }

    // Update is called once per frame
    void Update()
    {
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

    public void updateGameGoal(int amount)
    {
        goalCount += amount;

        if (goalCount <= 0)
        {
            statePause();
            menuActive = menuWin;
            menuActive.SetActive(true);
        }

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




}

