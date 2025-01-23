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
    Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
        if (damageType == DamageType.Moving)
        {
            originPosition = rigidBody.position;
            rigidBody.linearVelocity = transform.forward * bulletSpeed * Time.deltaTime;

        }
        Destroy(gameObject, timeToDespawn);

    }

    private void Update()
    {
        if (damageType == DamageType.Moving)
        {
            if (Vector3.Distance(startPosition, transform.position) > travelDistance)
            {
                Destroy(gameObject);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Assuming "Player" and "Enemy" are tags on your player and enemy GameObjects
        if (collision.gameObject.CompareTag("Enemy") && !collision.gameObject.CompareTag(sourceTag))
        {
            // Get enemy health component and apply damage
            IDamage enemyHealth = collision.gameObject.GetComponent<IDamage>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage, originPosition);
            }
        }

        // Destroy bullet on any collision
        Destroy(gameObject);
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

            else if (other.gameObject.CompareTag("Enemy"))
            {
                AINetwork aiNetwork = other.GetComponent<AINetwork>();
                if (aiNetwork != null)
                {
                    aiNetwork.ActivateCollider();

                }
                DamageAI(ref dmg);
            }
            else
            {

                DamagePlayer(ref dmg);

            }

        }
        DestroyItems();


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
            Destroy(this.gameObject);
        }
        if (damageType == DamageType.HealthPack)
        {
            Destroy(this.gameObject);
        }
        if (damageType == DamageType.GroundTrap)
        {
            Destroy(this.gameObject);
        }

    }
}
