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
    [SerializeField] float shootRate;
    [SerializeField] int facePlayerSpeed;
    [SerializeField] bool chasePlayer;
    [SerializeField] GameObject bullet;
    [SerializeField] Transform shootPos;

    Vector3 playerDirection;
    Vector3 playerPosition;
    Vector3 playerPreviousPosition;
    bool isShooting;
    bool playerInRange;
    Color originalColor;
    void Start()
    {
        originalColor = model.material.color;
        

    }

    // Update is called once per frame
    void Update()
    {
        PlayerDetection();
    }
    void PlayerDetection()
    {
        if (playerInRange)
        {
            playerPreviousPosition = GameManager.instance.player.transform.position;
            playerDirection = GameManager.instance.player.transform.position - transform.position;

            if(chasePlayer)
            agent.SetDestination(GameManager.instance.player.transform.position);

            if (agent.remainingDistance < agent.stoppingDistance)
            {
                FaceTarget();
            }
            if (!isShooting)
            {
                StartCoroutine(Shoot());
            }
        }
    }
    public void TakeDamage(int amount)
    {
        health -= amount;
       //TODO FLASH RED ~Dakota
        if (health <= 0)
        {
            // Without the proper reference, this will cause issues and not despawn the gameobject
            GameManager.instance.scoreSys.AddFlatScore(100);
            Destroy(gameObject);
        }
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
        if (playerStationary)
        {
            // Implement shooting at player
            // Implement random number offsets so the AI does not laser beam the player
            Instantiate(bullet, shootPos.position, transform.rotation);
            yield return new WaitForSeconds(shootRate);
            isShooting = false;
        }

        else
        {
            // Implement prediction of player movement with random offset to the player is not being laser beamed
            // 
            Instantiate(bullet, shootPos.position, transform.rotation);
            yield return new WaitForSeconds(shootRate);
            isShooting = false;
        }
    }
  
}
