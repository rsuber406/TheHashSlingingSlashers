using UnityEngine;

public class Damage : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    enum DamageType
    {
        Moving,
        Stationary,
        HealthPack,
        GroundTrap
    }
    [SerializeField] int bulletSpeed;
    [SerializeField] int damage;
    [SerializeField] int travelDistance;
    [SerializeField] int timeToDespawn;
    [SerializeField] Rigidbody rigidBody;
    [SerializeField] DamageType damageType;
    [SerializeField] string sourceTag;


    void Start()
    {
        if (damageType == DamageType.Moving)
        {
            rigidBody.linearVelocity = transform.forward * bulletSpeed;
            Destroy(gameObject, timeToDespawn);
        }
       

    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger)
        {
            return;
        }
        if (other.CompareTag(sourceTag))
        {
            return; 
        }
        

        IDamage dmg = other.GetComponent<IDamage>();
        if (dmg != null)
        {
            dmg.TakeDamage(damage);
        }
        if (damageType == DamageType.Moving)
        {
            Destroy(gameObject);
        }
        if (damageType == DamageType.HealthPack)
        {
            Destroy(gameObject);
        }
        if (damageType == DamageType.GroundTrap)
        {
            Destroy(gameObject);
        }

    }
}

