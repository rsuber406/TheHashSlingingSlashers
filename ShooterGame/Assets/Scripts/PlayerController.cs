using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] int movementSpeed;
    [SerializeField] int sprintMod;
    [SerializeField] int jumpSpeed;
    [SerializeField] int jumpMax;
    [SerializeField] int gravity;
    [SerializeField] CharacterController controller;
    [SerializeField] LayerMask ignoreLayer;

    

    Vector3 playerVel;
    Vector3 moveDir;
    int jumpCount;
    bool isSprinting;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
       
        Movement();
        Sprint();
      

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

    }
    void Sprint()
    {
        if (Input.GetButtonDown("Sprint") && !isSprinting)
        {
            isSprinting = true;
            movementSpeed = movementSpeed * sprintMod;
        }
        else if(Input.GetButtonUp("Sprint") && controller.isGrounded)
        {
            isSprinting= false;
            movementSpeed = movementSpeed / sprintMod;
        }
    }

    void Jump()
    {
        if(Input.GetButtonDown("Jump") && jumpCount < jumpMax)
        {
            jumpCount++;
            playerVel.y = jumpSpeed;
        }
    }
}
