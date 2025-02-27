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


    public void AIShoot(Quaternion rotation, Vector3 aiPosition)
    {
      
        float dotProduct = Vector3.Dot(transform.forward, (GameManager.instance.player.transform.position - transform.position).normalized);
        if (dotProduct < 0) return;
        Vector3 direction = GameManager.instance.player.transform.position - shootPos.position;
        Quaternion rotateDir = Quaternion.LookRotation(direction);
        Instantiate(bullet, shootPos.position, rotateDir * rotation);
        shotsPerMagazine--;
    }

    public float GetFireRate() { return fireRate; }

    public void PlayerShoot(int damageRef ,bool isShotgun = false)
    {
        Ray cameraRay = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        Vector3 targetPoint;
        RaycastHit hit;
        if (Physics.Raycast(cameraRay, out hit))
        {
            targetPoint = hit.point;
            Vector3 directionFromShootToCam = (targetPoint - shootPos.position).normalized;
            Quaternion shootRot = Quaternion.LookRotation(directionFromShootToCam);
            Quaternion adjustShotgunRot = Quaternion.Euler(Random.Range(-3.0f, 3.0f), Random.Range(-3.0f, 3.0f), 1);
            if (isShotgun)
            {
                Instantiate(bullet, shootPos.position, shootPos.rotation * adjustShotgunRot);
            }
           else Instantiate(bullet, shootPos.position,  shootPos.rotation);
        }


    }

    public IEnumerator Reload()
    {
        shotsPerMagazine = referenceToOriginalShots;
        yield return new WaitForSeconds(reloadTime);
    }
    public int GetBulletsRemaining() { return shotsPerMagazine; }
}
