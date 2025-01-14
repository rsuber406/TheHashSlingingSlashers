using System.Collections;
using UnityEngine;

public class GunScripts : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] float fireRate;
    [SerializeField] Transform shootPos;
    [SerializeField] GameObject bullet;
   

    // Update is called once per frame
  

    public void AIShoot(Quaternion rotation)
    {
        Instantiate(bullet, shootPos.position, rotation);
        
    }

    public float GetFireRate() { return fireRate; }

    public void PlayerShoot()
    {
        shootPos.rotation = Camera.main.transform.rotation;
        Instantiate(bullet, shootPos.position, shootPos.transform.rotation);
    }
}
