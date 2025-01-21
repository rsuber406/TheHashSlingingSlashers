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
        if (shotsPerMagazine <= 0) return;
        
        Vector3 direction = GameManager.instance.player.transform.position - shootPos.position;
        Quaternion rotateDir = Quaternion.LookRotation(direction);
        Instantiate(bullet, shootPos.position, rotateDir  * rotation);
        shotsPerMagazine--;
    }

    public float GetFireRate() { return fireRate; }

    public void PlayerShoot()
    {
        if (shotsPerMagazine <= 0) return;
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0)); // ray going thru middle of screen
        RaycastHit hit;
        Vector3 targetPoint = Vector3.zero;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
           
                targetPoint = hit.point;
        } else
        {
            targetPoint = ray.GetPoint(1000);
        }

        Vector3 direction = (targetPoint - shootPos.transform.position).normalized;

        var _bullet = Instantiate(bullet, shootPos.transform.position, Quaternion.LookRotation(direction));


        shotsPerMagazine--;
    }

    public IEnumerator Reload()
    {
        shotsPerMagazine = referenceToOriginalShots;
        yield return new WaitForSeconds(reloadTime);
    }
    public int GetBulletsRemaining() { return shotsPerMagazine; }
}
