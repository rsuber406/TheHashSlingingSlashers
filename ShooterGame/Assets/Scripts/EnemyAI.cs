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

    Vector3 playerDirection;
   
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
        if (playerInRange)
        {
            playerDirection = GameManager.instance.player.transform.position - transform.position;
            agent.SetDestination(GameManager.instance.player.transform.position);

            if(agent.remainingDistance < agent.stoppingDistance)
            {
                FaceTarget();
            }
            if (!isShooting)
            {
                // Implement shoot
            }
        }
    }
    public void TakeDamage(int amount)
    {
        health -= amount;
        if(health <= 0)
        {

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
}
