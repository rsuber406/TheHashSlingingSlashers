using UnityEngine;

public class BulletTime : MonoBehaviour
{
    [SerializeField] private float slowMotionFactor = 0.2f;
    [SerializeField] private float maxSlowMotionDuration = 7f;
    private float rechargeRate = 0.5f;
    private float normalTimeScale = 1f;
    private bool isBulletTimeActive = false;
    private float bulletTimeTimer = 0f;
    private float currentSlowMotionDuration;

    private void Start()
    {
        currentSlowMotionDuration = maxSlowMotionDuration;
    }

    void Update()
    {
        if (Input.GetButtonDown("BulletTime") && currentSlowMotionDuration > 0)
        {
            ToggleBulletTime();
        }

        if (isBulletTimeActive)
        {
            bulletTimeTimer += Time.unscaledDeltaTime;
            currentSlowMotionDuration -= Time.unscaledDeltaTime;

            if (bulletTimeTimer >= maxSlowMotionDuration || currentSlowMotionDuration <= 0)
            {
                ToggleBulletTime();
            }
        }
        else
        {
            RechargeBulletTime();
        }

        UpdateBulletTimerUI();
    }
    private void RechargeBulletTime()
    {
        if (currentSlowMotionDuration < maxSlowMotionDuration)
        {
            currentSlowMotionDuration += rechargeRate * Time.deltaTime;
            currentSlowMotionDuration = Mathf.Min(currentSlowMotionDuration, maxSlowMotionDuration);
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
    public float GetBulletTimeRemaining()
    {
        return currentSlowMotionDuration;
    }

  private void UpdateBulletTimerUI()
    {
        GameManager.instance.playerBulletTimeBar.fillAmount = currentSlowMotionDuration / maxSlowMotionDuration;
    }
    public void IncreaseMaxSlowMotionDuration(float amount)
    {
        maxSlowMotionDuration += amount;
        currentSlowMotionDuration = Mathf.Min(currentSlowMotionDuration + amount, maxSlowMotionDuration);
        if (currentSlowMotionDuration > maxSlowMotionDuration)
        {
            currentSlowMotionDuration = maxSlowMotionDuration;
        }
    }
}
