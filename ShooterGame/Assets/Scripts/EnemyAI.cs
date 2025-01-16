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
    [SerializeField] Transform headPos;
    [SerializeField] int fov;
    [SerializeField] bool isMelee;
    [SerializeField] bool isTurrent;
    [SerializeField] GameObject firearm;
    [SerializeField] GunScripts firearmScript;
    [SerializeField] Animator animatorController;
    [SerializeField] int animSpeedTrans;
    [SerializeField] int distanceRunTowardPlayerOnDmg;
    [SerializeField] Transform locateWallPos;
    [SerializeField] int searchForWall;
    float fireRate;
    float angleOfPlayer;
    Vector3 playerDirection;
    Vector3 playerPosition;
    Vector3 playerPreviousPosition;
    bool isShooting;
    bool canMeleeAttack;
    bool playerInRange;
    float inSideDistance;
    Color originalColor;
    void Start()
    {
        originalColor = model.material.color;
        canMeleeAttack = isMelee;
        if (firearm != null)
        {
            firearmScript = firearm.GetComponent<GunScripts>();
            fireRate = firearmScript.GetFireRate();
        }
        else if (!isMelee)
        {
            fireRate = firearmScript.GetFireRate();
        }
    }

    // Update is called once per frame
    void Update()
    {

        float characterSpeed = agent.velocity.normalized.magnitude;
        float animSpeed = animatorController.GetFloat("Speed");
        animatorController.SetFloat("Speed", Mathf.MoveTowards(animSpeed, characterSpeed, Time.deltaTime * animSpeedTrans));
        PlayerDetection();
        if (!isMelee)
            PerformReload();
    }
    void PlayerDetection()
    {

        if (playerInRange && CanSeePlayer())
        {



        }
    }
    public void TakeDamage(int amount, Vector3 origin)
    {
        Vector3 playerDirection = origin - transform.position;
        if (isMelee)
        {
            HandleMeleeAIMoveOnDmg(ref playerDirection);
        }
        else
        {

            HandleRangedCombatOnDmg(ref playerDirection);


        }
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
        Quaternion rotation = Quaternion.LookRotation(new Vector3(playerDirection.x, 0, playerDirection.z));
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
        if (firearmScript.GetBulletsRemaining() <= 0)
        {
            StartCoroutine(firearmScript.Reload());
        }
    }

    bool CanSeePlayer()
    {
        RaycastHit hit;
        playerPreviousPosition = GameManager.instance.player.transform.position;
        playerDirection = GameManager.instance.player.transform.position - headPos.position;
        float distance = playerDirection.magnitude;
        if (Physics.Raycast(headPos.position, playerDirection, out hit))
        {
            if (hit.collider.CompareTag("Player") && angleOfPlayer <= fov)
            {
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

                else if (isMelee && distance < 5)
                {
                    // Handle melee
                    MeleeAttack();
                }
                return true;
            }

        }
        return false;
    }

    void MeleeAttack()
    {
        if (canMeleeAttack)
        {
            StartCoroutine(WeaponAttack());
        }
        else return;

    }

    IEnumerator WeaponAttack()
    {
        canMeleeAttack = false;
        animatorController.SetTrigger("Attack");
        yield return new WaitForSeconds(0.1f);
        canMeleeAttack = true;
    }
    void HandleMeleeAIMoveOnDmg(ref Vector3 playerDirection)
    {
        inSideDistance = playerDirection.magnitude;
        movementSpeed = movementSpeed * sprintMod;
        // give back a quaternion and feed a vector
        if (inSideDistance >= distanceRunTowardPlayerOnDmg)
        {
            Debug.Log("I am running away");
            // rotate 90 degrees and run
            //Vector3 rotateCalc = new Vector3(playerDirection.x, -45, playerDirection.z);
            //rotateCalc = rotateCalc + playerDirection;
            //Quaternion rotatePlayer = Quaternion.LookRotation(rotateCalc);
            //transform.rotation = Quaternion.Lerp(transform.rotation, rotatePlayer, facePlayerSpeed * Time.deltaTime);
            //agent.SetDestination(new Vector3(-1 * playerDirection.x, transform.position.y, -1 * playerDirection.z));
            FindNearestWall();
        }
        else
        {
            Debug.Log("I am running towards");
            agent.SetDestination(playerDirection);

        }
    }
    void HandleRangedCombatOnDmg(ref Vector3 playerDirection)
    {
        Debug.Log("Hit ranged enemy");
        Quaternion lookRotation = Quaternion.LookRotation(playerDirection);
        transform.rotation = Quaternion.Lerp(transform.rotation, lookRotation, facePlayerSpeed * Time.deltaTime);
        agent.SetDestination(playerDirection);
        PerformReload();
        StartCoroutine(Shoot());

    }

    public void TakeDamage(int amount)
    {

    }

    void FindNearestWall()
    {
        RaycastHit hit;

        float offset = -90;
        Vector3 foundWall = Vector3.zero;
        Quaternion rotationToApply = Quaternion.Euler(1, offset, 1);
        Vector3 directionForCast = rotationToApply * locateWallPos.transform.forward;
        Transform originalWallPos = locateWallPos;
        for (int i = 0; i < 5; i++)
        {
            Debug.DrawRay(locateWallPos.position, directionForCast, Color.blue, searchForWall * 5f);

            if (Physics.Raycast(locateWallPos.position, directionForCast, out hit)) 
            if (hit.collider.gameObject.CompareTag("Wall"))
            {
                Vector3 wallLocation = hit.collider.transform.position;

                if (i != 0)
                {
                    float distanceFirst = Vector3.Distance(transform.position, wallLocation);
                    float distanceSecond = Vector3.Distance(transform.position, foundWall);
                    if (distanceFirst < distanceSecond)
                    {
                        foundWall = wallLocation;

                    }

                }
                else
                {
                    foundWall = wallLocation;
                }
            }


            // check at 45 degree intervals 
            offset += 45;
            rotationToApply = Quaternion.Euler(1, offset, 1);
            directionForCast = rotationToApply * locateWallPos.transform.forward;
        }
        if (foundWall != Vector3.zero)
        {
            Debug.Log("Found a wall");
            Vector3 direction = foundWall - transform.position;
            Quaternion rotateAiToWall = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Lerp(transform.rotation, rotateAiToWall, facePlayerSpeed * Time.deltaTime);
            agent.SetDestination(direction);


        }
        else //Debug.Log("Did not find a wall");

        locateWallPos = originalWallPos;

    }
}
