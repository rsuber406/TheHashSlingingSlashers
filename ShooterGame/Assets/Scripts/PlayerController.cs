using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour, IDamage
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] CharacterController controller;
    [SerializeField] GameObject bullet;
    [SerializeField] GameObject firearm;
    [SerializeField] Transform shootPos;
    [SerializeField] LayerMask ignoreLayer;

    [SerializeField] const int maxHealth = 0;
    [SerializeField] int health;

    // movement
    [SerializeField] float movementSpeed;
    [SerializeField] float sprintMod;

    // jump
    [SerializeField] int jumpSpeed;
    [SerializeField] int jumpMax;
    [SerializeField] int gravity;

    // crouch
    [SerializeField] float crouchHeight;
    [SerializeField] float crouchMovementSpeed;
    [SerializeField] float crouchSpeed;

    // slide
    [SerializeField] float slideMod;
    [SerializeField] float slideMomentum;   // lower number more further you go
    [SerializeField] float slideDuration;
    [SerializeField] float slideThreshold;

    private int previousHealth;

    Vector3 playerVel;
    Vector3 moveDir;

    float origMovementSpeed;
    float origHeight;
    float slideTimer;
    int jumpCount;
    bool isSprinting, isCrouching, isSliding;

    GunScripts firearmScript;
    void Start()
    {
        firearmScript = firearm.GetComponent<GunScripts>();
        origHeight = controller.height;
        origMovementSpeed = movementSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * 50, Color.red);
        Movement();
        Sprint();
        Crouch();
        Slide();
        Shoot();

    }

    void Movement()
    {
        if (controller.isGrounded)
        {
            jumpCount = 0;
            playerVel = Vector3.zero;
        }

        else
            // cannot crouch while player in air
            if (playerVel.y > 0)
                isCrouching = false;

        moveDir = (Input.GetAxis("Horizontal") * transform.right) + (Input.GetAxis("Vertical") * transform.forward);
        controller.Move(moveDir * movementSpeed * Time.deltaTime);

        Jump();

        controller.Move(playerVel * Time.deltaTime);
        playerVel.y -= gravity * Time.deltaTime;
        
        shootPos.transform.rotation = Camera.main.transform.rotation;

    }

    void Sprint()
    {
        if (!isSliding)
        {
            if (Input.GetButtonDown("Sprint"))
            {
                // toggles crouch
                if (isCrouching)
                {
                    isCrouching = false;
                    Crouch();
                    movementSpeed = origMovementSpeed;
                }

                isSprinting = true;
                movementSpeed *= sprintMod;
            }

            if (Input.GetButtonUp("Sprint"))
            {
                isSprinting = false;
                movementSpeed = origMovementSpeed;
            }
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

    void Crouch()
    {
        if (!isSliding)
        {
            if (Input.GetButtonDown("Crouch") && !isSprinting)
                isCrouching = !isCrouching;

            if (isCrouching)
            {
                controller.height -= crouchSpeed * Time.deltaTime;

                if (controller.height <= crouchHeight)
                {
                    movementSpeed = crouchMovementSpeed;
                    controller.height = crouchHeight;
                }
            }

            else if (!isCrouching && !isSliding)
            {
                controller.height += crouchSpeed * Time.deltaTime;

                // was Supposed to stop the jitter from un crouching
                /* if (controller.height < normalHeight)
                    player.position += offset * Time.deltaTime;*/

                if (controller.height >= origHeight)
                {
                    controller.height = origHeight;

                    if (!isSprinting)
                        movementSpeed = origMovementSpeed;

                }
            }
        }
    }


    void Slide()
    {
        if (isSprinting && Input.GetButtonDown("Crouch"))
        {
            // Start sliding
            isSliding = true;
            isCrouching = false;
            isSprinting = false;

            slideTimer = 0f;
            controller.height = crouchHeight;

            movementSpeed = origMovementSpeed;
            movementSpeed *= slideMod;
        }

        else if (isSliding)
        {
            // slide cancel
            if (Input.GetButtonDown("Crouch"))
            {
                isCrouching = false;
                isSliding = false;
                movementSpeed = origMovementSpeed;

            }

            // Continue sliding
            else
            {
                controller.Move(moveDir * slideMod * Time.deltaTime);

                slideTimer += Time.deltaTime;
                movementSpeed -= slideMod * slideMomentum * Time.deltaTime;

            }

            // End slide if timer exceeds duration or player's speed drops below threshold
            if ((slideTimer >= slideDuration || movementSpeed < slideThreshold) && isSliding)
            {
                movementSpeed = origMovementSpeed;
                isSliding = false;
                isCrouching = true;

            }
        }
    }
}

