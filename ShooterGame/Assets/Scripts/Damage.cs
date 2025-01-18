using System.Collections;
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
    GameObject player;
    bool hasGivenDmg = false;
    Vector3 originPosition;

    void Start()
    {
        if (damageType == DamageType.Moving)
        {
            rigidBody.linearVelocity = transform.forward * bulletSpeed;
            originPosition = rigidBody.position;
            Destroy(gameObject, timeToDespawn);
        }


    }
   
   
    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger)
        {
            return;
        }


        IDamage dmg = other.GetComponent<IDamage>();
        if (dmg != null)
        {
            if (other.CompareTag(sourceTag))
            {
                return;
            }

            if (other.gameObject.CompareTag("Enemy"))
            {
                DamageAI(ref dmg);
            }
            else
            {
               
                DamagePlayer(ref dmg);
                
            }

        }
        else
        {
            DestroyItems();
        }


    }
    void DamagePlayer(ref IDamage dmg)
    {
        
        dmg.TakeDamage(damage);
        DestroyItems();

    }

    void DamageAI(ref IDamage dmg)
    {

        dmg.TakeDamage(damage, originPosition);
        DestroyItems();

    }

    void DestroyItems()
    {
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

