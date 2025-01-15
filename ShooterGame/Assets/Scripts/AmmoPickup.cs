using UnityEngine;

public class AmmoPickup : MonoBehaviour
{
    [SerializeField] int ammoAmount = 20; 

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = FindAnyObjectByType(typeof(PlayerController)) as PlayerController;
            if (player != null)
            {
                player.AddAmmo(ammoAmount);
                Destroy(gameObject); 
            }
        }
    }
}
