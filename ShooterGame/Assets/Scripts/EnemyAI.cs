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
    PlayerController playerController;

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
        
    }
    public void TakeDamage(int amount)
    {
        health -= amount;
        if(health <= 0)
        {

        }
    }
}
