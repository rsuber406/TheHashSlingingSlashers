using UnityEngine;

public class PlayerAimPosition : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] int sensitivity;
    [SerializeField] int lockVertMin, lockVertMax;
    [SerializeField] bool invYAxis;
    float rotX;
    float rotZ;
  
    void Start()
    {
      
    }

    // Update is called once per frame
    void Update()
    {
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

       
        transform.localRotation = transform.parent.localRotation;
        
       
        
    }
}
