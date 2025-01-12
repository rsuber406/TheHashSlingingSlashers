using UnityEngine;

public class Damage : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    enum DamageType
    {
        Moving,
        Stationary
    }
    [SerializeField] int bulletSpeed;
    [SerializeField] int damage;
    [SerializeField] int travelDistance;
    [SerializeField] int timeToDespawn;
    [SerializeField] Rigidbody rigidBody;
    [SerializeField] DamageType damageType;

    
    void Start()
    {
        if(damageType == DamageType.Moving)
        {
            rigidBody.linearVelocity = transform.forward * bulletSpeed;
            Destroy(gameObject, timeToDespawn);
        }
    }

    // Update is called once per frame
    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger) return;
        IDamage dmg = other.GetComponent<IDamage>();
        if(dmg != null)
        {
            dmg.TakeDamage(damage);
        }
        if(damageType == DamageType.Moving)
        {
            Destroy(gameObject);
        }
    }
}
