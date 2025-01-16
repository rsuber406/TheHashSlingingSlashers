using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour, IDamage
{

    // Public Variables
    public bool IsDebugMode;
    
    
    // Serialized fields
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
    [SerializeField] float wallRunSpeed = 20f;
    [SerializeField] float wallRunDuration = 5f;
    [SerializeField] float wallRunGroundCheckThreshhold = 3f;
    [SerializeField] float groundCheckRay = 1.2f;
    
    
    // Private fields
    private CollisionInfo collisionInfo;
    private Vector3 playerVel;
    private Vector3 moveDir;
    private int jumpCount;
    private bool isSprinting;
    private bool isGrounded;
    private bool isWallRunning;
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




    int maxHealth = 100;

    GunScripts firearmScript;

    int numBulletsReserve = 60;
    int numBulletsInMag;

    float Timesincereload;
    //this is silly, but now if you sit in the level for 10 minutes, you will be told to reload.

    void Start()
    {
        health = maxHealth;
        firearmScript = firearm.GetComponent<GunScripts>();
        origHeight = controller.height;
        origMovementSpeed = movementSpeed;
        numBulletsInMag = firearmScript.GetBulletsRemaining();
        Timesincereload = Time.time + 10000;
        GameManager.instance.UpdatePlayerHeathUI(health);
    }


    void Update()
    {
       
        Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * 50, Color.red);
        Movement();
        Sprint();
        Crouch();
        Slide();
        CheckWallRun();
        Shoot();
        PerformReload();
        
        if (IsDebugMode)
            DrawDebugLines();
        CheckTimeSinceReload();
    }

    void Movement()
    {
        isGrounded = IsGrounded();
        
        if (isGrounded)
        {
            jumpCount = 0;
            playerVel = Vector3.zero;
        }

        else
            // cannot crouch while player in air
            if (playerVel.y > 0)
                isCrouching = false;

        // If wall running, handle movement against the wall
        if (isWallRunning)
        {
            moveDir = transform.forward * Input.GetAxis("Vertical");
            WallRunMovement(moveDir);
        }
        else
        {
            moveDir = (Input.GetAxis("Horizontal") * transform.right) + (Input.GetAxis("Vertical") * transform.forward);
            GroundedMovement(moveDir);
        }

        moveDir = (Input.GetAxis("Horizontal") * transform.right) + (Input.GetAxis("Vertical") * transform.forward);
        controller.Move(moveDir * movementSpeed * Time.deltaTime);

        Jump();

        controller.Move(playerVel * Time.deltaTime);
        playerVel.y -= gravity * Time.deltaTime;
        
        shootPos.transform.rotation = Camera.main.transform.rotation;
    }

    /// <summary>
    /// Evaluates if a player is touching the ground using a raycast and is colliding with a surface with the "Ground" Tag
    /// </summary>
    /// <returns>book</returns>
    private bool IsGrounded()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out var hit, groundCheckRay ))
        {
            if (hit.collider.CompareTag("Ground")) return true;
        }

        return false;
    }
    
    /// <summary>
    /// Check If a player is far enough off the ground to start wall running
    /// </summary>
    /// <returns>bool</returns>
    private bool HasGroundClearance()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out var hit, wallRunGroundCheckThreshhold ))
        {
            if (hit.collider.CompareTag("Ground"))
            {
                return false;
            }
        }

        return true;
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
        if(health > maxHealth)
        {
            health = maxHealth;
        }
        GameManager.instance.UpdatePlayerHeathUI(health);
        if(health > previousHealth)
        {
            // Input healing screen
        }
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


         Camera camRef = Camera.main;
        if (Input.GetButtonDown("Shoot"))
        if (Input.GetButtonDown("Shoot") && ((numBulletsReserve > 0) || (numBulletsInMag > 0)))
        {
                if (numBulletsInMag > 0)
                {
                    firearmScript.PlayerShoot();
                    //count bullets set new bullet count on UI
                    numBulletsInMag--;
                    GameManager.instance.pubCurrentBulletsMagText.SetText(numBulletsInMag.ToString());
                    if (numBulletsReserve == 0)
                    {
                        Timesincereload = Time.time + 3;

                    }
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


    void CheckWallRun()
    {
        if (!isGrounded && IsAgainstWall())
        {
            // Maybe dont need this after the refactor
            Vector3 wallNormal = GetWallNormal();
            StartWallRun(wallNormal);
        }
        else
        {
            EndWallRun();
        }
    }

    private bool IsAgainstWall()
    {
        collisionInfo.Reset();

        // Raycast to the left
        bool isCollidingOnLeftWall = Physics.Raycast(transform.position, -transform.right, out var leftHit, 1f);
        if (isCollidingOnLeftWall && leftHit.collider.CompareTag("Wall"))
            collisionInfo.left = true;

        // Raycast to the right
        bool isCollidingOnRightWall = Physics.Raycast(transform.position, transform.right, out var rightHit, 1f);
        if (isCollidingOnRightWall && rightHit.collider.CompareTag("Wall"))
            collisionInfo.right = true;

        return collisionInfo.left || collisionInfo.right;
    }

    private Vector3 GetWallNormal()
    {
        // Check for left wall collision
        RaycastHit leftHit;
        if (Physics.Raycast(transform.position, -transform.right, out leftHit, 1f))
            return leftHit.normal;

        // Check for right wall collision
        RaycastHit rightHit;
        if (Physics.Raycast(transform.position, transform.right, out rightHit, 1f))
            return rightHit.normal;

        return Vector3.zero;
    }

    void DrawDebugLines()
    {
        // Raycast to the left
        RaycastHit leftHit;
        bool isCollidingOnLeftWall = Physics.Raycast(transform.position, -transform.right, out leftHit, 1f);
        Debug.DrawRay(transform.position, -transform.right, isCollidingOnLeftWall ? Color.red : Color.green);

        // Raycast to the right
        RaycastHit rightHit;
        bool isCollidingOnRightWall = Physics.Raycast(transform.position, transform.right, out rightHit, 1f);
        Debug.DrawRay(transform.position, transform.right, isCollidingOnRightWall ? Color.red : Color.green);

        // Draw a raycast to the ground
        Debug.DrawRay(transform.position, Vector3.down * groundCheckRay, Color.blue);
    }
    
    public void StartWallRun(Vector3 wallNormal)
    {
        if (!isWallRunning)
        {
            isWallRunning = true;

            // Lock the player's rotation to the wall, slerp makes the transition smoother
            // Dissabling this because some snaping happens that maked this unbearable
            // Vector3 wallDirection = Vector3.Cross(wallNormal, Vector3.up);
            // Quaternion targetRotation = Quaternion.LookRotation(wallDirection);
            // controller.transform.rotation = Quaternion.Slerp(controller.transform.rotation, targetRotation, Time.deltaTime * 10f);

            StartCoroutine(EndWallRun_Internal());
        }
    }

    private IEnumerator EndWallRun_Internal()
    {
        yield return new WaitForSeconds(wallRunDuration);
        EndWallRun();
    }

    public void EndWallRun()
    {
        isWallRunning = false;
    }

    void WallRunMovement(Vector3 direction)
    {
        Vector3 moveDirection = new Vector3(direction.x, 0, direction.z);
        playerVel.y = 0;
        controller.Move(moveDirection * (wallRunSpeed * Time.deltaTime));
    }
    
    void GroundedMovement(Vector3 direction)
    {
        controller.Move(direction * (movementSpeed * Time.deltaTime));
    }

    
    // Expand this to store character collision data in all directions and adata about what is being collided with.
    public struct CollisionInfo
    {
        public bool above, below;
        public bool left, right;
        public bool backward, forward;

        public void Reset()
        {
            above = below = false;
            left = right = false;
            backward = forward = false;
        }
     }

    void PerformReload()
    {

        if (Input.GetButtonDown("Reload"))
        {
            if (numBulletsReserve > 0)
            {
               int bulletsToLoad = 15 - numBulletsInMag;

                bulletsToLoad = Mathf.Min(bulletsToLoad, numBulletsReserve);
                StartCoroutine(firearmScript.Reload());
                GameManager.instance.PubReloadText.enabled = false;
                numBulletsReserve -= bulletsToLoad;
                numBulletsInMag += bulletsToLoad;

                GameManager.instance.pubCurrentBulletsMagText.SetText("15");
                GameManager.instance.pubCurrentBulletsReserveText.SetText(numBulletsReserve.ToString());
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
        if(numBulletsInMag ==0 && Timesincereload >= Time.time)
        {
            GameManager.instance.PubReloadText.enabled = true;
        }
    }
    public void AddAmmo(int amount)
    {
        int maxReserveAmount = 50;
        numBulletsReserve = Mathf.Min(numBulletsReserve + amount, maxReserveAmount);

        GameManager.instance.pubCurrentBulletsReserveText.SetText(numBulletsReserve.ToString());
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

