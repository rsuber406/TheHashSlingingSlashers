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
    [SerializeField] int travelDistance;
    [SerializeField] int timeToDespawn;
    [SerializeField] Rigidbody rigidBody;
    [SerializeField] DamageType damageType;
    [SerializeField] string sourceTag;
    [SerializeField] ParticleSystem hitEffect;
    public int damage;
    GameObject player;
    Vector3 originPosition;
    Vector3 startPosition;

    void Start()
    {
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
        yield return new WaitForSeconds(0.01f);
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

        if (collision.collider.gameObject.CompareTag("Bullet"))
        {
            return;
        }
        else
        {
            Instantiate(hitEffect, collision.contacts[0].point, Quaternion.identity);
            Debug.Log(collision.collider.name);
            DestroyItems();
        }
    }
    //private void OnTriggerEnter(Collider other)
    //{
    //    if (other.isTrigger)
    //    {
    //        return;
    //    }


    //    IDamage dmg = other.GetComponent<IDamage>();
    //    if (dmg != null)
    //    {
    //        if (other.CompareTag(sourceTag))
    //        {
    //            return;
    //        }

    //       else if (other.gameObject.CompareTag("Enemy"))
    //        {
    //            Debug.Log("Enemy hit");
    //            AINetwork aiNetwork = other.GetComponent<AINetwork>();
    //            if (aiNetwork != null)
    //            {
    //                aiNetwork.ActivateCollider();

    //            }
    //            DamageAI(ref dmg);
    //        }
    //        else
    //        {

    //            DamagePlayer(ref dmg);

    //        }

    //    }
    //    DestroyItems();


    //}
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