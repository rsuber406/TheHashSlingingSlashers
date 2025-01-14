using System.Collections;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour, IDamage
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] int movementSpeed;
    [SerializeField] int sprintMod;
    [SerializeField] NavMeshAgent agent;
    [SerializeField] int health;
    [SerializeField] int maxHealth;
    [SerializeField] Renderer model;
    [SerializeField] int facePlayerSpeed;
    [SerializeField] bool chasePlayer;
    [SerializeField] float distanceToActivateSprint;
    [SerializeField] bool isMelee;
    [SerializeField] bool isTurrent;
    [SerializeField] GameObject firearm;
    [SerializeField] GunScripts firearmScript;
    float fireRate;
    
    Vector3 playerDirection;
    Vector3 playerPosition;
    Vector3 playerPreviousPosition;
    bool isShooting;
    bool playerInRange;
    Color originalColor;
    void Start()
    {
        originalColor = model.material.color;
       
        if(firearm != null)
        {
            firearmScript = firearm.GetComponent<GunScripts>();
            fireRate = firearmScript.GetFireRate();
        }
        else
        {
            fireRate = firearmScript.GetFireRate();
        }
    }

    // Update is called once per frame
    void Update()
    {
        PlayerDetection();
        PerformReload();
    }
    void PlayerDetection()
    {
        if (playerInRange)
        {
            playerPreviousPosition = GameManager.instance.player.transform.position;
            playerDirection = GameManager.instance.player.transform.position - transform.position;
            float distance = playerDirection.magnitude;


            if (chasePlayer)
            {
                agent.SetDestination(GameManager.instance.player.transform.position);
                if (distance > distanceToActivateSprint)
                {
                    movementSpeed *= sprintMod;
                }
                else
                {
                    movementSpeed = movementSpeed / sprintMod;
                }
            }

            if (agent.remainingDistance < agent.stoppingDistance)
            {
                FaceTarget();
            }
            if (!isShooting && !isMelee)
            {
                StartCoroutine(Shoot());
            }

            else
            {
                // Handle melee
            }
        }
    }
    public void TakeDamage(int amount)
    {
        health -= amount;
        StartCoroutine(RegisterHit());

    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;

        }

    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }

    void FaceTarget()
    {
        Quaternion rotation = Quaternion.LookRotation(playerDirection);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, facePlayerSpeed * Time.deltaTime);
    }

    IEnumerator Shoot()
    {

            isShooting = true;
            playerPosition = GameManager.instance.player.transform.position;
            bool playerStationary = playerPosition == playerPreviousPosition ? true : false;
            // This will allow us to decrease the accuracy so out player is not being laser beamed
            int applyInaccuracy = 20;
            int inaccuracyChance = Random.Range(0, 100);
            Quaternion randomRotation = Quaternion.Euler(1, 1, 1);
            if (playerStationary)
            {
                // Implement shooting at player
                // Implement random number offsets so the AI does not laser beam the player
                if (inaccuracyChance > applyInaccuracy)
                {
                    randomRotation = Quaternion.Euler(Random.Range(-1.5f, 1.5f), Random.Range(-1.5f, 1.5f), 1);

                }

                firearmScript.AIShoot(transform.rotation * randomRotation);
              

            }

            else
            {
                // Implement prediction of player movement with random offset to the player is not being laser beamed
                if (inaccuracyChance > applyInaccuracy)
                {
                    randomRotation = Quaternion.Euler(Random.Range(-1.5f, 1.5f), Random.Range(-1.5f, 1.5f), 1);

                }
                Vector3 rotateDir = PredictPlayerMovement(transform.position, GameManager.instance.player.transform.position, GameManager.instance.player.transform.position, 150);
                RotateToPlayer(rotateDir);
                firearmScript.AIShoot(transform.rotation * randomRotation);
            }
                yield return new WaitForSeconds(fireRate);
                isShooting = false;
        
    }

    Vector3 PredictPlayerMovement(Vector3 aiPosition, Vector3 playerPosition, Vector3 playerVelocity, float projectileSpeed)
    {
        Vector3 toPlayer = playerPosition - aiPosition;
        float distance = toPlayer.magnitude;
        float timeToHit = distance / projectileSpeed;
        return playerPosition + playerVelocity * timeToHit;
    }

    void RotateToPlayer(Vector3 direction)
    {
        Quaternion rotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * facePlayerSpeed);
    }

    IEnumerator RegisterHit()
    {
        model.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        model.material.color = originalColor;
        if (health <= 0)
        {
            // Without the proper reference, this will cause issues and not despawn the gameobject
            GameManager.instance.scoreSys.AddFlatScore(100);
            Destroy(gameObject);
        }
    }

    void PerformReload()
    {
        if(firearmScript.GetBulletsRemaining() <= 0)
        {
            StartCoroutine(firearmScript.Reload());
        }
    }

   



}
