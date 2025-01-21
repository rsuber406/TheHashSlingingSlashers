using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField] GameObject menuActive;
    [SerializeField] GameObject menuPause;
    [SerializeField] GameObject menuWin;
    [SerializeField] GameObject menuLose;
    [SerializeField] GameObject playerDmgScreen;
    [SerializeField] GameObject playerGainHealth;

    [SerializeField] TMP_Text CurrentHPText;
    [SerializeField] GameObject lowHealthScreen;
    [SerializeField] TMP_Text CurrentBulletsMagText;
    [SerializeField] TMP_Text CurrentBulletsReserveText;
    [SerializeField] TMP_Text ReloadText;

    public TMP_Text PubReloadText => ReloadText;
    public TMP_Text PubcurrentHPText => CurrentHPText; 
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

    public bool isPaused;
    int maxHealth = 100;

    int goalCount;
    int playerCurrentHealth;
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
        CurrentHPText.text = currentHealth.ToString();
        
        
    }
    public float GetPlayerBulletTimeLeft() 
    {
       float bulletTimeRemaining = bt.GetBulletTimeRemaining();
        return bulletTimeRemaining;
       

    }
    public int GetPlayerHealth() { return playerCurrentHealth; }


}

