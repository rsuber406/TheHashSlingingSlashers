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
    public int damage;
    GameObject player;
    Vector3 originPosition;
    Vector3 startPosition;

    void Start()
    {

        if (damageType == DamageType.Moving)
        {
            originPosition = rigidBody.position;
            rigidBody.linearVelocity = transform.forward * bulletSpeed * Time.deltaTime;

            Destroy(gameObject, timeToDespawn);
        }

    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger) return;

        IDamage dmg = other.GetComponent<IDamage>();
        if (dmg != null)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                dmg.TakeDamage(damage);
                Destroy(gameObject);
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
        DestroyItems();
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

