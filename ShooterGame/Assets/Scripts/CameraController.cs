using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] int sensitivity;
    [SerializeField] int lockVertMin, lockVertMax;
    [SerializeField] bool invYAxis;
    [SerializeField] int leftLean;
    [SerializeField] int rightLean;
    [SerializeField] int leanSpeed;
    [SerializeField] int leftCameraLeanMovement;
    

    private Quaternion initialAngle;
    private Vector3 cameraInitPos;
    float rotX;
    float rotZ;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        initialAngle = transform.localRotation;
        cameraInitPos = transform.parent.localPosition;

    }

    // Update is called once per frame
    void Update()
    {


        CameraMovement();
    }
    void LeftLeanPosition()
    {
        rotZ = leftLean * leanSpeed * Time.deltaTime;
        transform.localRotation = Quaternion.Euler(0, 0, rotZ);
        Vector3 targetPosition = new Vector3(leftCameraLeanMovement, 0, 0);
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, leanSpeed * Time.deltaTime);

    }
    void RightLeanPosition()
    {
        rotZ = rightLean * leanSpeed * Time.deltaTime;
        Quaternion targetRotation = Quaternion.Euler(0, 0, rotZ);
        transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, leanSpeed * Time.deltaTime);
        Vector3 targetPosition = new Vector3(-1 * leftCameraLeanMovement, 0, 0);
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, leanSpeed * Time.deltaTime);
    }

    void ResetLeftLean()
    {
        rotZ = 0;
        Vector3 targetPosition = new Vector3(-1 * leftCameraLeanMovement, 0, 0);
        transform.localRotation = Quaternion.Lerp(transform.localRotation, initialAngle, leanSpeed * Time.deltaTime);
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, leanSpeed * Time.deltaTime);
    }
    void ResetRightLean()
    {
        rotZ = 0;
        Quaternion targetRotation = Quaternion.Euler(0, 0, rotZ);
        transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, leanSpeed * Time.deltaTime);
        Vector3 targetPosition = new Vector3(leftCameraLeanMovement, 0, 0);
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, leanSpeed * Time.deltaTime);
    }
  

    void CameraMovement()
    {
        if (Input.GetButtonDown("Left Lean"))
        {
            LeftLeanPosition();
        }
        else if (Input.GetButtonUp("Left Lean"))
        {
            ResetLeftLean();
        }
        if(Input.GetButtonDown("Right Lean"))
        {
            RightLeanPosition();
        }
        else if(Input.GetButtonUp("Right Lean"))
        {
            ResetRightLean();
        }
        ChangeLookView();
    }

    void ChangeLookView()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;
        if (invYAxis)
        {
            rotX += mouseY;
        }
        else rotX -= mouseY;

        rotX = Mathf.Clamp(rotX, lockVertMin, lockVertMax);
        transform.localRotation = Quaternion.Euler(rotX, 0, rotZ);
        transform.parent.Rotate(Vector3.up * mouseX);
    }
}
