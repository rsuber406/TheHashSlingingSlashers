using UnityEngine;

public class GunScripts : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    // This will be implemented at a later date, for now we will use diffent bullets to differentiate damage
    [SerializeField] float fireRate;
    [SerializeField] int ammoCount;
    [SerializeField] float reloadTime;
    [SerializeField] Transform shootPos;
    [SerializeField] GameObject bullet;

    bool isFiring = false;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
