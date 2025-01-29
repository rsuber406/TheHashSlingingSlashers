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

    [Header("Bullet Info")] [SerializeField]
    int bulletSpeed;

    [SerializeField] int travelDistance;
    [SerializeField] int timeToDespawn;

    [Header("Physical Properites")] [SerializeField]
    Rigidbody rigidBody;

    [SerializeField] DamageType damageType;

    [Header("Projectile Source Info")] [SerializeField]
    string sourceTag;

    [Header("Particle Effects")] [SerializeField]
    private ParticleSystem hitEffect;

    [Header("Sound Effects")] [SerializeField]
    private AudioClip[] hitSounds;

    [SerializeField] AudioSource audioSource;
    public int damage;
    GameObject player;
    Vector3 originPosition;
    Vector3 startPosition;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.enabled = true;
        if (damageType == DamageType.Moving)
        {
            originPosition = rigidBody.position;
            Collider collider = rigidBody.GetComponent<Collider>();
            collider.enabled = false;
            rigidBody.linearVelocity = transform.forward * bulletSpeed * Time.deltaTime;
            StartCoroutine(ActivateCollider(collider));
            Destroy(gameObject, timeToDespawn);
        }
    }

    IEnumerator ActivateCollider(Collider collider)
    {
        yield return new WaitForSeconds(0.0001f);
        collider.enabled = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger) return;

        IDamage dmg = other.GetComponent<IDamage>();
        if (dmg != null)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                if (damageType == DamageType.HealthPack)
                {
                    damage = damage * 3;
                    dmg.TakeDamage(damage);
                    Destroy(gameObject);
                }
                else
                {
                    dmg.TakeDamage(damage);
                    Destroy(gameObject);
                }
            }
        }
    }


    private void OnCollisionEnter(Collision collision)
    {
        IDamage dmg = collision.gameObject.GetComponent<IDamage>();
        if (dmg != null)
        {
            if (collision.gameObject.CompareTag(sourceTag))
            {
                return;
            }

            else if (collision.gameObject.CompareTag("Enemy"))
            {
                AINetwork aiNetwork = collision.gameObject.GetComponent<AINetwork>();
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

        if (hitSounds.Length > 0)
            audioSource.PlayOneShot(hitSounds[Random.Range(0, hitSounds.Length)], 5f);
        if (collision.collider.gameObject.CompareTag("Bullet"))
        {
            return;
        }
        else
        {
            Instantiate(hitEffect, collision.contacts[0].point, Quaternion.identity);
            
            
        }

        StartCoroutine((DestroyBullets()));
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

    IEnumerator DestroyBullets()
    {
        yield return new WaitForSeconds(0.5f);
        Destroy(this.gameObject);
    }
}