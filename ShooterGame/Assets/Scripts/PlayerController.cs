using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

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
    
    // crouch
    [SerializeField] float crouchHeight;
    [SerializeField] float crouchMovementSpeed;
    [SerializeField] float crouchSpeed;

    // slide
    [SerializeField] float slideMod;
    [SerializeField] float slideMomentum;   // lower the number further you travel
    [SerializeField] float slideDuration;
    [SerializeField] float slideThreshold;
    
    // Private fields
    private CollisionInfo collisionInfo;
    private Vector3 playerVel;
    private Vector3 moveDir;
    private int jumpCount;
    private bool isSprinting;
    private bool isGrounded;
    private bool isWallRunning;
    private bool hasTakenDmg = false;
    // jump


    private int previousHealth;
    float origMovementSpeed;
    float origHeight;
    float slideTimer;
    bool isCrouching, isSliding;




    int maxHealth = 100;

    float bulletTimeLeft;
    GunScripts firearmScript;
    BulletTime bt;
    GameManager gameManager;
    private int maxMagCapacity;
    int numBulletsReserve;
    int numBulletsInMag;

    float Timesincereload;


    //[SerializeField] Transform orientation;

    //this is silly, but now if you sit in the level for 10 minutes, you will be told to reload.

    void Start()
    {
        // w/e this shit is
        health = maxHealth;
        firearmScript = firearm.GetComponent<GunScripts>();
        origHeight = controller.height;
        origMovementSpeed = movementSpeed;

        //ammo
        SetAllAmmoCount(30, 30, 30);
        numBulletsReserve = maxMagCapacity * 4;

        //other shit
        Timesincereload = Time.time + 10000;
        GameManager.instance.UpdatePlayerHeathUI(health);



    }


    void Update()
    {
       
      
        Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * 50, Color.red);

        PlayerMovement();

        CheckWallRun();
        Shoot();
        PerformReload();
        UpdateAmmoUI();
        if (IsDebugMode)
            DrawDebugLines();
        CheckTimeSinceReload();
    }

    void PlayerMovement()
    {
        Movement();
        Sprint();
        Crouch();
        Slide();
    }

    void Movement()
    {
        isGrounded = IsGrounded();
        
        if (isGrounded)
        {
            jumpCount = 0;
            playerVel = Vector3.zero;
        }

        // cannot crouch while player in air
        else
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


        //rb.AddForce(moveDir.normalized * movementSpeed * 10f, ForceMode.Force);

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
        if (Physics.Raycast(transform.position, Vector3.down, out var hit, groundCheckRay))
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
                    movementSpeed = (int) origMovementSpeed;
                }

                isSprinting = true;
                movementSpeed *= sprintMod;
            }

            if (Input.GetButtonUp("Sprint"))
            {
                isSprinting = false;
                movementSpeed = (int) origMovementSpeed;
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
        if(!hasTakenDmg)
        StartCoroutine(FlashDmgScreen());

        UpdatePlayerUI();
    }

    void UpdatePlayerUI()
    {
        float btLeft = GameManager.instance.GetPlayerBulletTimeLeft();

        GameManager.instance.playerBulletTimeBar.fillAmount = btLeft;
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
        hasTakenDmg = true;
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
        hasTakenDmg = false;
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
               int bulletsToLoad = maxMagCapacity - numBulletsInMag;

                bulletsToLoad = Mathf.Min(bulletsToLoad, numBulletsReserve);
                StartCoroutine(firearmScript.Reload());
                GameManager.instance.PubReloadText.enabled = false;
                numBulletsReserve -= bulletsToLoad;
                numBulletsInMag += bulletsToLoad;

                UpdateAmmoUI();
            }
            else
            {
                GameManager.instance.PubReloadText.enabled = true;
                GameManager.instance.PubReloadText.SetText("Out Of Ammo!");
            }
        }
    }

    void UpdateAmmoUI()
    {
        GameManager.instance.pubCurrentBulletsMagText.SetText(numBulletsInMag.ToString());
        GameManager.instance.pubCurrentBulletsReserveText.SetText(numBulletsReserve.ToString());
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
       numBulletsReserve += amount;
        UpdateAmmoUI();
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
                    movementSpeed = (int) crouchMovementSpeed;
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
                        movementSpeed = (int) origMovementSpeed;

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

            movementSpeed = (int)origMovementSpeed;
            movementSpeed *= (int) slideMod;
        }

        else if (isSliding)
        {
            // slide cancel
            if (Input.GetButtonDown("Crouch"))
            {
                isCrouching = false;
                isSliding = false;
                movementSpeed = (int) origMovementSpeed;

            }

            // Continue sliding
            else
            {
                controller.Move(moveDir * slideMod * Time.deltaTime);

                slideTimer += Time.deltaTime;
                movementSpeed -= (int) slideMod * (int) slideMomentum * (int) Time.deltaTime;

            }

            // End slide if timer exceeds duration or player's speed drops below threshold
            if ((slideTimer >= slideDuration || movementSpeed < slideThreshold) && isSliding)
            {
                movementSpeed = (int) origMovementSpeed;
                isSliding = false;
                isCrouching = true;

            }
        }
    }
    public void TakeDamage(int amount, Vector3 origin)
    {
        // There should be no implementation here. This is only because of the interface class and AI needing special override
    }
    public void SetMaxMagCapacity(int maxMagCapacity) {
        this.maxMagCapacity = maxMagCapacity;
    }
    public void SetMaxAmmo(int maxAmmo)
    {
        numBulletsReserve = maxAmmo;
    }
    public void SetCurrentAmmo(int ammo)
    {
        numBulletsInMag = ammo;
    }
    public void SetAllAmmoCount(int maxMagCapacity, int maxAmmo, int currentAmmo)
    {
        this.maxMagCapacity = maxMagCapacity;
        numBulletsReserve = maxAmmo;
        numBulletsInMag = currentAmmo;
    }

    public void OnTriggerEnter(Collider other)
    {
        if(other.tag == "DeathBox")
        {
            //this is a deathbox trigger to kill the player. Use Deathbox Prefabs on Death pits - A
            TakeDamage(500);
            //why not just set the players health to zero?
        }

        if(other.tag == "SpeedBox")
        {
            //This is a Speedbox Trigger. Use for speedup door/platform - A
            StartCoroutine(Speedup());          
        }
    }
    

    private IEnumerator Speedup()
    {
        Debug.Log("Speedup is triggered");
        movementSpeed = movementSpeed *2;
        yield return new WaitForSeconds(2f);
        movementSpeed = (int)origMovementSpeed;
    }
    public int GetHealth() { return health; }
}



