using UnityEngine;

public class GunPickup : MonoBehaviour
{
    [SerializeField] FirearmScriptable gunStats;
    [SerializeField] Transform shootPos;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnTriggerEnter(Collider other)
    {
        IPickup pickup = other.GetComponent<IPickup>();
        if (pickup != null)
        {
            // transfer to the object
            pickup.GrabGun(gunStats, shootPos);
            Destroy(gameObject);
        }
    }
}
