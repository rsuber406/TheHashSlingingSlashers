using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] int sensitivity;
    [SerializeField] int lockVertMin, lockVertMax;
    [SerializeField] bool invYAxis;
    float rotX;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

    }

    // Update is called once per frame
    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;
        if (invYAxis)
        {
            rotX += mouseY;
        }
        else rotX -= mouseY;

        rotX = Mathf.Clamp(rotX, lockVertMin, lockVertMax);
        transform.localRotation = Quaternion.Euler(rotX, 0, 0);
        transform.parent.Rotate(Vector3.up * mouseX);
    }
}
