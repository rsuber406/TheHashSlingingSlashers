using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour, IDamage
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] int movementSpeed;
    [SerializeField] int sprintMod;
    [SerializeField] int jumpSpeed;
    [SerializeField] int jumpMax;
    [SerializeField] int gravity;
    [SerializeField] CharacterController controller;
    [SerializeField] LayerMask ignoreLayer;
    [SerializeField] int health;
    [SerializeField] GameObject bullet;
    [SerializeField] Transform shootPos;
    [SerializeField] GameObject firearm;
   



    private int previousHealth;
    Vector3 playerVel;
    Vector3 moveDir;

    int jumpCount;
    bool isSprinting;
    int maxHealth;

    GunScripts firearmScript;

    int numBulletsReserve = 60;
    int numBulletsinMag;

    float Timesincereload = Time.time + 10000;
    //this is silly, but now if you sit in the level for 10 minutes, you will be told to reload.

    void Start()
    {
        maxHealth = health;
        firearmScript = firearm.GetComponent<GunScripts>();
        numBulletsinMag = firearmScript.GetBulletsRemaining();
    }

    // Update is called once per frame
    void Update()
    {
       
        Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * 50, Color.red);
        Movement();
        Sprint();
        Shoot();
        PerformReload();
        CheckTimeSinceReload();
    }

    void Movement()
    {
        if (controller.isGrounded)
        {
            jumpCount = 0;
            playerVel = Vector3.zero;

        }
        moveDir = (Input.GetAxis("Horizontal") * transform.right) + (Input.GetAxis("Vertical") * transform.forward);
        controller.Move(moveDir * movementSpeed * Time.deltaTime);
        Jump();
        controller.Move(playerVel * Time.deltaTime);
        playerVel.y -= gravity * Time.deltaTime;
        shootPos.transform.rotation = Camera.main.transform.rotation;

    }
    void Sprint()
    {
        if (Input.GetButtonDown("Sprint") && !isSprinting)
        {
            isSprinting = true;
            movementSpeed = movementSpeed * sprintMod;
        }
        else if (Input.GetButtonUp("Sprint"))
        {
            isSprinting = false;
            movementSpeed = movementSpeed / sprintMod;
        }
    }

    void Jump()
    {
        if (Input.GetButtonDown("Jump") && jumpCount < jumpMax)
        {
            jumpCount++;
            playerVel.y = jumpSpeed;
        }
    }

    public void TakeDamage(int amount)
    {
        previousHealth = health;

        health -= amount;
        StartCoroutine(FlashDmgScreen());
        UpdatePlayerUI();
    }

    void UpdatePlayerUI()
    {
        GameManager.instance.playerHPBar.fillAmount = (float)health / maxHealth;
        GameManager.instance.PubcurrentHPText.SetText(health.ToString());

        if (health < 0)
        {
            GameManager.instance.PubcurrentHPText.SetText("0");
            GameManager.instance.PublowHealthScreen.SetActive(false);
        }
        else if (health < 50)
        {
            GameManager.instance.PublowHealthScreen.SetActive(true);
        }
        else
            GameManager.instance.PublowHealthScreen.SetActive(false);

    }

    void Shoot()
    {
        if (Input.GetButtonDown("Shoot") && ((numBulletsReserve > 0) || (numBulletsinMag > 0)))
        {
            firearmScript.PlayerShoot();
            //count bullets set new bullet count on UI
            numBulletsinMag--;
            GameManager.instance.PubcurrentBulletsMagText.SetText(numBulletsinMag.ToString());
            if(numBulletsReserve == 0)
            {
                Timesincereload = Time.time + 3;
            }
        }
    }
    IEnumerator FlashDmgScreen()
    {
        if (previousHealth > health)
        {
            GameManager.instance.FlashDamageScreenOn();
            yield return new WaitForSeconds(0.1f);
            GameManager.instance.FlashDamageScreenOff();
        } else
        {
            GameManager.instance.FlashHealthScreenOn();
            yield return new WaitForSeconds(0.1f);
            GameManager.instance.FlashDamageScreenOff();
        }
        if(health <= 0)
        {
            GameManager.instance.Lose();
        }
    }
    void PerformReload()
    {
        if (Input.GetButtonDown("Reload"))
        {
            if (numBulletsReserve > 0)
            {
                StartCoroutine(firearmScript.Reload());
                GameManager.instance.PubReloadText.enabled = false;
                numBulletsReserve -= 15;
                numBulletsinMag = 15;
                GameManager.instance.PubcurrentBulletsMagText.SetText("15");
                GameManager.instance.PubcurrentBulletsReserveText.SetText(numBulletsReserve.ToString());
            }
            else
            {
                GameManager.instance.PubReloadText.enabled = true;
                GameManager.instance.PubReloadText.SetText("Out Of Ammo!");
            }
        }
    }

    void CheckTimeSinceReload()//TO DO: I DONT WORK!
    {
        if(numBulletsinMag ==0 && Timesincereload >= Time.time)
        {
            GameManager.instance.PubReloadText.enabled = true;
        }
    }
}
