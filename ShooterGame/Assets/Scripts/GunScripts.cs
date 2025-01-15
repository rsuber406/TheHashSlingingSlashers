using System.Collections;
using UnityEngine;

public class GunScripts : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] float fireRate;
    [SerializeField] Transform shootPos;
    [SerializeField] GameObject bullet;
    [SerializeField] int shotsPerMagazine;
    [SerializeField] float reloadTime;
    [SerializeField] bool isAIControlled;
    // Update is called once per frame
    int referenceToOriginalShots;

    private void Awake()
    {
        referenceToOriginalShots = shotsPerMagazine;
    }


    public void AIShoot(Quaternion rotation)
    {
        if (shotsPerMagazine <= 0) return;
        Instantiate(bullet, shootPos.position, rotation);
        shotsPerMagazine--;
    }

    public float GetFireRate() { return fireRate; }

    public void PlayerShoot()
    {
        if (shotsPerMagazine <= 0) return;
        shootPos.rotation = Camera.main.transform.rotation;
        Instantiate(bullet, shootPos.position, shootPos.transform.rotation);
        shotsPerMagazine--;
    }

    public IEnumerator Reload()
    {
        shotsPerMagazine = referenceToOriginalShots;
        yield return new WaitForSeconds(reloadTime);
    }
    public int GetBulletsRemaining() { return shotsPerMagazine; }
}
