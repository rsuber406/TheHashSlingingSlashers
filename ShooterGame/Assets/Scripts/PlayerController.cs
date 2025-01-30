using System.Collections;
using UnityEngine;
using System.Collections.Generic;


public struct CollisionInfo
{
    public bool left, right;

    public void Reset()
    {
        left = right = false;
    }
}

[RequireComponent(typeof(AudioSource))]
public class PlayerController : MonoBehaviour, IDamage, IPickup
{
    // Public Variables
    [Header("------- Debug -------")]
    public bool isDebugMode;

    [Header("------- Components -------")]
    [SerializeField] CharacterController controller;
    [SerializeField] LayerMask ignoreLayer;
    private AudioSource audioController;

    [Header("------- Player Movement -------")]
    [SerializeField] public float movementSpeed;
    [SerializeField][Range(15, 25)] int sprintMod;
    [SerializeField][Range(10, 20)] int sprintMovementSpeed;
    [SerializeField] int jumpSpeed;
    [SerializeField] int forwardJumpBoost; // Controls how much forward bias is applied to the player, 1 is a good default
    [SerializeField] int jumpMax;
    [SerializeField] int forwardJumpBoost; // Controls how much forward bias is applied to the player, 1 is a good default
    [SerializeField] int gravity; // Negative value indicating downward force
    
    [Header("------- Crouch -------")]
    [SerializeField][Range(1, 5)] float crouchMovementSpeed;
    [SerializeField][Range(1, 10)] float crouchSpeed;
    [SerializeField][Range(0.3f, 1)] float crouchHeight;
    [SerializeField][Range(2, 4)] float slideMod;
    [SerializeField][Range(1, 8)] float slideDis;   // lower the number further you travel

    [Header("------- Player Health -------")]
    [SerializeField] int health;
    [SerializeField] int healthRegen;
    [SerializeField] private float healthRegenDelay;
    public int maxHealth = 300;


    [Header("------- Wall Running ------")] 
    [SerializeField] [Range(10, 20)] float wallRunSpeed;
    [SerializeField] [Range(1, 3)] float wallRunDuration;
    [SerializeField] [Range(1, 10)] float wallRunDetachForce;
    [SerializeField] [Range(1, 3)] float wallRunGroundCheckDistance = 2f;
    [SerializeField] [Range(1, 2)] float groundCheckRay;

    [Header("------- Grappling ---------")] 
    [SerializeField] [Range(1, 200)] float grappleCheckRay;
    [SerializeField] float grappleLineDelay;
    [SerializeField] LineRenderer lineRenderer;
    private bool isGrappling;
    
    [SerializeField] [Range(1, 60)] float forwardGrappleForce;
    [SerializeField] [Range(1, 30)] float upwardGrappleArkForce;
    [SerializeField] [Range(1, 10)] float minGrappleDistance;
    [SerializeField] [Range(0.5f, 5)] float grappleCooldown;

    [Header("------- Crouching ---------")] 
    [SerializeField] float crouchHeight;
    [SerializeField] float crouchMovementSpeed;
    [SerializeField] float crouchSpeed;
    [Header("------- Sliding -----------")] 
    [SerializeField] float slideMod;
    [SerializeField] float slideMomentum; // lower number more further you go
    [SerializeField] float slideDuration;
    [SerializeField] float slideThreshold;
    [Header("------- Weapons -----------")] 
    [SerializeField] int projectileDmg;
    [SerializeField] int projectileDistance;
    [SerializeField] float fireRate;
    [SerializeField] GameObject gunModel;
    [SerializeField] GunScripts firearmScript;
    [SerializeField] GameObject muzzleFlash;
    [SerializeField] List<FirearmScriptable> gunList = new List<FirearmScriptable>();

    public int maxHealth = 300;
    
    [Header("------- Audio Config ----------")]
    [SerializeField] private AudioSource audioController;
    [SerializeField] private float hurtVolume;
    [SerializeField] private float jumpVolume;
    [SerializeField] private float footstepVolume;
    [SerializeField] private float wallrunVolume;
    [SerializeField] private AudioClip[] jumpSounds;

    [SerializeField] private AudioClip[] hurtSounds;
    [SerializeField][Range(0, 1)] float hurtVol; 
    [SerializeField] private AudioClip[] stepSounds;

    [SerializeField] private AudioClip[] wallRunSounds;
    private bool isPlayingFootsteps;


    // Private fields
    private Camera playerCamera;
    private CollisionInfo collisionInfo;
    private Vector3 playerVel;
    private Vector3 moveDir;
    private Vector3 grapplePoint;

    private bool isSprinting;
    private bool isGrounded;
    private bool isWallRunning;
    private bool isCrouching;
    private bool isSliding;
    private bool isShooting;
    private bool hasTakenDmg = false;


    private int jumpCount;
    private int previousHealth;

    int gunListPosition = 0;
    private float originalGrappleSpeed;
    private float originalWallRunSpeed;

    float origMovementSpeed;
    float origHeight;
    float slideTimer;
    float crouch;
    float bulletTimeLeft;
    private float maxSpeedClamp;
    private float minSpeedClamp;
    BulletTime bt;
    GameManager gameManager;
    private int maxMagCapacity;
    int numBulletsReserve;
    int numBulletsInMag;
    private Coroutine regenCo;

    //this is silly, but now if you sit in the level for 10 minutes, you will be told to reload.

    float Timesincereload;

    private readonly float GRAVITY_CORRECTION = -2.0f;


    void Start()
    {

        audioController = GetComponent<AudioSource>();
        playerCamera = Camera.main;
        // w/e this shit is
        health = maxHealth;
        origHeight = transform.localScale.y;
        origMovementSpeed = movementSpeed;
        originalGrappleSpeed = forwardGrappleForce;
        originalWallRunSpeed = wallRunSpeed;

        //other shit
        Timesincereload = Time.time + 10000;
    }

    void Update()
    {
        GameManager.instance.UpdatePlayerHeathUI(health);
        Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * 50, Color.red);

        PlayerMovement();
        HandleGrappleHook();

        CheckWallRun();
        if (Input.GetButton("Shoot") && !isShooting && !GameManager.instance.isPaused)
            StartCoroutine(Shoot());
        PerformReload();
        UpdateAmmoUI();
        CheckTimeSinceReload();
        SelectGun();

        if (isDebugMode)
        {
            DrawDebugLines();
        }
    }

    void PlayerMovement()
    {
        Movement();
        Sprint();
        Slide();
        Crouch();
    }

    void Movement()
    {
        isGrounded = IsGrounded();

        if (isGrounded && playerVel.y < 0)
        {
            jumpCount = 0;
            playerVel = Vector3.zero;
            // The player might appear to "float" briefly after landing because gravity isn't pulling them back down.
            // We make the player slightly stick to the floor
            playerVel.y = GRAVITY_CORRECTION;
        }

        else if (playerVel.y > 0)
        {
            // cannot crouch while player in air
            isCrouching = false;
        }


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
        
        if (moveDir.magnitude > 0.3f && !isPlayingFootsteps && isGrounded)
        {
            StartCoroutine(PlayFootsteps());
        }


        Jump();

        // Apply player gravity, the order matters!
        playerVel.y -= gravity * Time.deltaTime;
        controller.Move(playerVel * Time.deltaTime);
    }

    /// <summary>
    /// Evaluates if a player is touching the ground using a raycast and is colliding with a surface with the "Ground" Tag
    /// </summary>
    /// <returns>bool</returns>
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
        if (Physics.Raycast(transform.position, Vector3.down, out var hit, wallRunGroundCheckDistance))
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
                isSprinting = true;

            if (Input.GetButtonUp("Sprint"))
                isSprinting = false;

            if (isSprinting)
            {
                if (isCrouching)
                    isCrouching = false;

                movementSpeed += sprintMod * Time.deltaTime;

                if ((movementSpeed >= sprintMovementSpeed))
                {
                    movementSpeed = sprintMovementSpeed;
                }
            }

            if (!isSprinting)
            {
                movementSpeed -= sprintMod * Time.deltaTime;

                if ((movementSpeed <= origMovementSpeed))
                {
                    movementSpeed = origMovementSpeed;
                }
            }
           
        }
    }

    void Jump()
    {
        if (Input.GetButtonDown("Jump") && jumpCount < jumpMax)
        {
            jumpCount++;
            audioController.PlayOneShot(jumpSounds[Random.Range(0, jumpSounds.Length)], jumpVolume);
            if (isWallRunning)
            {
                Vector3 jumpDirection = GetWallNormal() + Vector3.up + transform.forward * forwardJumpBoost;
                playerVel = jumpDirection.normalized * jumpSpeed;
                playerVel.y = jumpSpeed;
            }
            else
            {
                playerVel.y = jumpSpeed;
            }
        }
    }

    public void TakeDamage(int amount)
    {
        previousHealth = health;

        health -= amount;
        audioController.PlayOneShot(hurtSounds[Random.Range(0, hurtSounds.Length)], hurtVolume);
        if (health > maxHealth)
        {
            health = maxHealth;
        }

        GameManager.instance.UpdatePlayerHeathUI(health);

        if (!hasTakenDmg)
            StartCoroutine(FlashDmgScreen());
        if (regenCo != null)
        {
            StopCoroutine(regenCo);
        }

        regenCo = StartCoroutine(RegenerateHealth());

        UpdatePlayerUI();
    }

    private IEnumerator RegenerateHealth()
    {
        yield return new WaitForSeconds(healthRegenDelay);

        while (health < maxHealth)
        {
            health += healthRegen;
            health = Mathf.Min(health, maxHealth);
            UpdatePlayerUI();
            yield return new WaitForSeconds(1f);
        }

        regenCo = null;
    }

    void UpdatePlayerUI()
    {
        float btLeft = GameManager.instance.GetPlayerBulletTimeLeft();

        GameManager.instance.playerBulletTimeBar.fillAmount = btLeft;
        GameManager.instance.playerHPBar.fillAmount = (float)health / maxHealth;

        if (health < 0)
        {
            GameManager.instance.PublowHealthScreen.SetActive(false);
        }
        else if (health < 50)
        {
            GameManager.instance.PublowHealthScreen.SetActive(true);
        }
        else
        {
            GameManager.instance.PublowHealthScreen.SetActive(false);
        }
    }

    IEnumerator Shoot()
    {
        if (numBulletsInMag > 0)
        {
            if (gunList[gunListPosition].isShotgun)
            {
                isShooting = true;
                numBulletsInMag--;
                audioController.PlayOneShot(gunList[gunListPosition].shootSound[Random.Range(0, gunList[gunListPosition].shootSound.Length)], 0.5f);
                for (int i = 0; i < 9; i++)
                {
                    firearmScript.PlayerShoot(projectileDmg, gunList[gunListPosition].isShotgun);
                    StartCoroutine(FlashMuzzle());
                }

                yield return new WaitForSeconds(fireRate);
                isShooting = false;
            }
            else
            {
                audioController.PlayOneShot(gunList[gunListPosition].shootSound[Random.Range(0, gunList[gunListPosition].shootSound.Length)], 0.5f);
                isShooting = true;
                audioController.PlayOneShot(gunList[gunListPosition].shootSound[Random.Range(0, gunList[gunListPosition].shootSound.Length)], 0.5f);
                numBulletsInMag--;
                firearmScript.PlayerShoot(projectileDmg);
                StartCoroutine(FlashMuzzle());
                yield return new WaitForSeconds(fireRate);
                isShooting = false;
                
            }
            
        }
    }

    IEnumerator FlashMuzzle()
    {
        muzzleFlash.SetActive(true);
        yield return new WaitForSeconds(0.01f);
        muzzleFlash.SetActive(false);
    }

    void HandleGrappleHook()
    {
        if (Input.GetButtonDown("Fire2") && !isGrappling)
        {
            if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit hit,
                    grappleCheckRay))
            {
                float distance = Vector3.Distance(transform.position, hit.point);
                if (distance > minGrappleDistance)
                {
                    isGrappling = true;
                    grapplePoint = hit.point;

                    Vector3 direction = (grapplePoint - transform.position).normalized;

                    playerVel = direction * forwardGrappleForce +
                                Vector3.up * Mathf.Clamp(distance * 0.5f, 0, upwardGrappleArkForce);

                    if (lineRenderer)
                    {
                        lineRenderer.enabled = true;
                        lineRenderer.positionCount = 2;
                        lineRenderer.SetPosition(0, transform.position);
                        lineRenderer.SetPosition(1, grapplePoint);
                    }
                    else
                    {
                        Debug.LogError("Missing Line Render For Grapple Rope");
                    }
                    
                    audioController.PlayOneShot(jumpSounds[Random.Range(0, jumpSounds.Length)], jumpVolume);
                    GameManager.instance.UseGrappleAbility();
                }
            }
        }

        if (isGrappling && lineRenderer)
        {
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, grapplePoint);
        }

        if (Input.GetButtonUp("Fire2"))
        {
            EndGrappleHook();
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
        }
        else
        {
            GameManager.instance.FlashHealthScreenOn();
            yield return new WaitForSeconds(0.1f);
            GameManager.instance.FlashDamageScreenOff();
        }

        hasTakenDmg = false;
        if (health <= 0)
        {
            GameManager.instance.Lose();
        }
    }


    void CheckWallRun()
    {
        if (!isGrounded && IsAgainstWall())
        {
            StartWallRun();
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
        if (Physics.Raycast(transform.position, -transform.right, out RaycastHit leftHit, 1f))
            return leftHit.normal;

        // Check for right wall collision
        if (Physics.Raycast(transform.position, transform.right, out RaycastHit rightHit, 1f))
            return rightHit.normal;

        return Vector3.zero;
    }

    void DrawDebugLines()
    {
        // Shoot Distance Ray
        Debug.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * 50, Color.red);

        // Grapple Check Ray
        Debug.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * grappleCheckRay, Color.yellow);

        // Raycast to the left
        bool isCollidingOnLeftWall = Physics.Raycast(transform.position, -transform.right, out RaycastHit leftHit, 1f);
        Debug.DrawRay(transform.position, -transform.right, isCollidingOnLeftWall ? Color.red : Color.green);

        // Raycast to the right
        bool isCollidingOnRightWall = Physics.Raycast(transform.position, transform.right, out RaycastHit rightHit, 1f);
        Debug.DrawRay(transform.position, transform.right, isCollidingOnRightWall ? Color.red : Color.green);

        // Draw a raycast to the ground
        Debug.DrawRay(transform.position, Vector3.down * groundCheckRay, Color.blue);
    }

    public void StartWallRun()
    {
        if (!isWallRunning)
        {
            isWallRunning = true;
            audioController.PlayOneShot(wallRunSounds[Random.Range(0, wallRunSounds.Length)], wallrunVolume);
            jumpCount = 0;
            StartCoroutine(EndWallRun_Internal());
        }
    }

    private IEnumerator EndWallRun_Internal()
    {
        yield return new WaitForSeconds(wallRunDuration);

        Vector3 jumpDirection = GetWallNormal();
        playerVel = jumpDirection.normalized * wallRunDetachForce;

        EndWallRun();
    }

    public void EndWallRun()
    {
        if (!isWallRunning) return;
        isWallRunning = false;
        audioController.Stop();
    }

    void WallRunMovement(Vector3 direction)
    {
        Vector3 moveDirection = new Vector3(direction.x, 0, direction.z);
        playerVel.y = 0; // This enables us to override player velocity
        // while running to prevent immediate gravity pull down
        controller.Move(moveDirection * (wallRunSpeed * Time.deltaTime));
    }

    void GroundedMovement(Vector3 direction)
    {
        controller.Move(direction * (movementSpeed * Time.deltaTime));
    }


    // Expand this to store character collision data in all directions and adata about what is being collided with.

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

    void CheckTimeSinceReload() //TO DO: I DONT WORK!
    {
        if (numBulletsInMag == 0 && Timesincereload >= Time.time)
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
        if (Input.GetButtonDown("Crouch") && !isSprinting && !isSliding)
            isCrouching = !isCrouching;

        if (isCrouching)
        {
            if (!isSliding)
                movementSpeed = crouchMovementSpeed;

            crouch = transform.localScale.y;
            crouch -= crouchSpeed * Time.deltaTime;

            transform.localScale = new Vector3(transform.localScale.x, crouch, transform.localScale.z);

            if (crouch <= crouchHeight)
            {
                transform.localScale = new Vector3(transform.localScale.x, crouchHeight, transform.localScale.z);
            }
        }

        else if (!isCrouching)
        {
            crouch = transform.localScale.y;
            crouch += crouchSpeed * Time.deltaTime;

            transform.localScale = new Vector3(transform.localScale.x, crouch, transform.localScale.z);

            if (crouch >= origHeight)
            {
                transform.localScale = new Vector3(transform.localScale.x, origHeight, transform.localScale.z);

                if (!isSprinting)
                    movementSpeed = origMovementSpeed;
            }
        }
    }


    void Slide()
    {
        if (Input.GetButtonDown("Crouch") && isSprinting && !isSliding)
        {
            // Start sliding
            isSliding = true;
            movementSpeed = origMovementSpeed * slideMod;
            audioController.PlayOneShot(slideSounds[Random.Range(0, slideSounds.Length)], slideVol);
            isCrouching = true;
        }

        else if (isSliding)
        {
            // slide cancel
            if (Input.GetButtonDown("Crouch"))
            {
                if (Input.GetButtonDown("Sprint"))
                {
                    isSliding = false;
                    isCrouching = false;
                    movementSpeed = origMovementSpeed * sprintMod;
                    audioController.Stop();
                }
                else
                {
                    isSliding = false;
                    isSprinting = false;
                    movementSpeed = origMovementSpeed;
                    audioController.Stop();
                }
            }

            else if (Input.GetButtonDown("Sprint"))
            {
                isSliding = false;
                isCrouching = false;
                movementSpeed = origMovementSpeed * sprintMod;
                audioController.Stop();
            }

            // Continue sliding
            else
            {
                controller.Move(moveDir * (slideMod * Time.deltaTime));
                movementSpeed -= slideMod * slideDis * Time.deltaTime;

                // End slide when slide speed reaches crouch speed
                if ((movementSpeed <= crouchMovementSpeed) || moveDir.magnitude <= 0)
                {
                    isCrouching = true;
                    isSliding = false;
                    isSprinting = false;
                    audioController.Stop();
                }
                else if (!isGrounded)
                {
                    movementSpeed = origMovementSpeed;
                    isSliding = false;
                    audioController.Stop();
                }
            }
        }
    }

    public void TakeDamage(int amount, Vector3 origin)
    {
        // There should be no implementation here. This is only because of the interface class and AI needing special override
    }

    public void SetMaxMagCapacity(int maxMagCapacity)
    {
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
        if (other.isTrigger) return;
        if (other.tag == "DeathBox")
        {
            //this is a deathbox trigger to kill the player. Use Deathbox Prefabs on Death pits - A
            TakeDamage(500);
            //why not just set the players health to zero?
        }

        if (other.tag == "SpeedBox")
        {
            //This is a Speedbox Trigger. Use for speedup door/platform - A
            StartCoroutine(Speedup());
        }
    }


    private IEnumerator Speedup()
    {
        Debug.Log("Speedup is triggered");
        movementSpeed = movementSpeed * 2;
        yield return new WaitForSeconds(2f);
        movementSpeed = (int)origMovementSpeed;
    }

    public void GrabGun(FirearmScriptable gun, Transform shootPos)
    {
        gunList.Add(gun);


        ChangeGun();
    }

    void SelectGun()
    {
        if (Input.GetAxis("Mouse ScrollWheel") > 0 && gunListPosition < gunList.Count - 1)
        {
            gunListPosition++;
            ChangeGun();
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0 && gunListPosition > 0)
        {
            gunListPosition--;
            ChangeGun();
        }
    }

    void ChangeGun()
    {
        FirearmScriptable gun = gunList[gunListPosition];
        projectileDistance = gun.range;
        fireRate = gun.fireRate;
        projectileDmg = gun.damage;
        gunModel.GetComponent<MeshFilter>().sharedMesh = gun.model.GetComponent<MeshFilter>().sharedMesh;
        gunModel.GetComponent<MeshRenderer>().sharedMaterial = gun.model.GetComponent<MeshRenderer>().sharedMaterial;
        SetAllAmmoCount(gun.ammoCurrent, gun.ammoMax, gun.ammoCurrent);
        numBulletsReserve = gun.ammoMax;
    }

    void EndGrappleHook()
    {
        StartCoroutine(DisableGrappleRope());
        StartCoroutine(GrappleCoolDown());
    }

    IEnumerator DisableGrappleRope()
    {
        yield return new WaitForSeconds(grappleLineDelay);
        if (lineRenderer)
        {
            lineRenderer.enabled = false;
        }
    }

    IEnumerator GrappleCoolDown()
    {
        yield return new WaitForSeconds(grappleCooldown);
        isGrappling = false;
        GameManager.instance.ReadyGrappleAbility();

        Debug.Log("Grapple Cooldown Ended");
    }

    public void DoubleGrappleSpeed()
    {
        forwardGrappleForce *= 2f;
        upwardGrappleArkForce *= 2f;
    }

    public void ResetGrappleSpeed()
    {
        forwardGrappleForce /= 2f;
        upwardGrappleArkForce /= 2f;
    }

    public void DoubleWallRunSpeed()
    {
        wallRunSpeed *= 2f;
    }

    public void ResetWallRunSpeed()
    {
        wallRunSpeed /= 2f;
    }
    
        IEnumerator PlayFootsteps()
    {
        isPlayingFootsteps = true;
        
        audioController.PlayOneShot(stepSounds[Random.Range(0, stepSounds.Length)], footstepVolume);

        if (isSprinting)
        {
            yield return new WaitForSeconds(0.3f);
        } 
        else if (isWallRunning)
        {
            yield return new WaitForSeconds(0.2f);
        }
        else if (isCrouching && !isSliding)
        {
            yield return new WaitForSeconds(0.7f);
        }
        else
        {
            yield return new WaitForSeconds(0.45f);
        }
        
        isPlayingFootsteps = false;
    }

}


    



