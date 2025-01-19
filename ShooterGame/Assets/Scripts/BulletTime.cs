using UnityEngine;

public class BulletTime : MonoBehaviour
{
    [SerializeField] private float slowMotionFactor = 0.2f;
    [SerializeField] private float slowMotionDuration = 5f;
    private float normalTimeScale = 1f;
    private bool isBulletTimeActive = false;
    private float bulletTimeTimer = 0f;

    void Update()
    {
        if (Input.GetButtonDown("BulletTime"))
        {
            ToggleBulletTime(); 
        }
        if (isBulletTimeActive) {
            bulletTimeTimer += Time.unscaledDeltaTime;
            if (bulletTimeTimer > slowMotionDuration) {
                ToggleBulletTime();
            }



        }


    }

    private void ToggleBulletTime()
    {
        if (isBulletTimeActive)
        {
            Time.timeScale = normalTimeScale;
            isBulletTimeActive = false;
        }
        else
        {
            Time.timeScale = slowMotionFactor;
            isBulletTimeActive = true;
            bulletTimeTimer = 0f;
        }
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
    }
}
