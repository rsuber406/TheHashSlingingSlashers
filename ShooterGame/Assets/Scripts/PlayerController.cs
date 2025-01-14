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




    Vector3 playerVel;
    Vector3 moveDir;
    int jumpCount;
    bool isSprinting;
    GunScripts firearmScript;
    void Start()
    {
        firearmScript = firearm.GetComponent<GunScripts>();
    }

    // Update is called once per frame
    void Update()
    {
        Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * 50, Color.red);
        Movement();
        Sprint();
        Shoot();
        PerformReload();
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
        health -= amount;
        StartCoroutine(FlashDmgScreen());
       
    }

    void Shoot()
    {
        
        if (Input.GetButtonDown("Shoot"))
        {
            firearmScript.PlayerShoot();

        }
    }
    IEnumerator FlashDmgScreen()
    {
        GameManager.instance.FlashDamageScreenOn();
        yield return new WaitForSeconds(0.1f);
        GameManager.instance.FlashDamageScreenOff();
        if(health <= 0)
        {
            GameManager.instance.Lose();
        }
    }
    void PerformReload()
    {
        if (Input.GetButtonDown("Reload"))
        {
            StartCoroutine(firearmScript.Reload());
        }
    }
}
