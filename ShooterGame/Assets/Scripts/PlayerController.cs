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
    [SerializeField] float wallRunSpeed = 10f;
    [SerializeField] float wallRunDuration = 2f;
    [SerializeField] float groundCheckDistance = 1.2f;

    [SerializeField] private CollisionInfo collisionInfo;

    private WallRunController wallRunHandler;
    Vector3 playerVel;
    Vector3 moveDir;
    int jumpCount;
    bool isSprinting;

    void Start()
    {
        wallRunHandler = new WallRunController(controller, wallRunSpeed, wallRunDuration);
    }

    void Update()
    {
        Movement();
        Sprint();
        Shoot();
        CheckWallRun();
        DrawDebugLines();

        Debug.Log("IsGrounded: " + IsGrounded());

        if (wallRunHandler.IsWallRunning() && !IsAgainstWall())
        {
            wallRunHandler.EndWallRun();
        }
    }

    void Movement()
    {
        if (IsGrounded())
        {
            jumpCount = 0;
            playerVel = Vector3.zero;
        }

        // If wall running, handle movement against the wall
        if (wallRunHandler.IsWallRunning())
        {
            Vector3 forwardMovement = transform.forward * Input.GetAxis("Vertical");
            wallRunHandler.Move(forwardMovement);
        }
        else
        {
            moveDir = (Input.GetAxis("Horizontal") * transform.right) + (Input.GetAxis("Vertical") * transform.forward);
            controller.Move(moveDir * movementSpeed * Time.deltaTime);
        }

        Jump();
        controller.Move(playerVel * Time.deltaTime);
        if (!wallRunHandler.IsWallRunning())
        {
            playerVel.y -= gravity * Time.deltaTime;
        }
    }

    private bool IsGrounded()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, groundCheckDistance))
        {
            if (hit.collider.CompareTag("Ground"))
            {
                return true;
            }
        }

        return false;
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
            Instantiate(bullet, Camera.main.transform.position, Camera.main.transform.rotation);
        }
    }

    IEnumerator FlashDmgScreen()
    {
        GameManager.instance.FlashDamageScreenOn();
        yield return new WaitForSeconds(0.1f);
        GameManager.instance.FlashDamageScreenOff();
        if (health <= 0)
        {
            GameManager.instance.Lose();
        }
    }

    void CheckWallRun()
    {
        if (!IsGrounded() && IsAgainstWall())
        {
            Debug.Log("WallRun");
            Vector3 wallNormal = GetWallNormal();
            wallRunHandler.StartWallRun(wallNormal);
        }
    }

    private bool IsAgainstWall()
    {
        collisionInfo.Reset();

        // Raycast to the left
        RaycastHit leftHit;
        bool isCollidingOnLeftWall = Physics.Raycast(transform.position, -transform.right, out leftHit, 1f);
        if (isCollidingOnLeftWall && leftHit.collider.CompareTag("Wall"))
        {
            collisionInfo.left = true;
        }

        // Raycast to the right
        RaycastHit rightHit;
        bool isCollidingOnRightWall = Physics.Raycast(transform.position, transform.right, out rightHit, 1f);
        if (isCollidingOnRightWall && rightHit.collider.CompareTag("Wall"))
        {
            collisionInfo.right = true;
        }

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
        Debug.DrawRay(transform.position, Vector3.down * groundCheckDistance, Color.blue);
    }

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
}
