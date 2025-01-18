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
    [SerializeField] float enemyBulletSpread;
    [SerializeField] Animator animatorController;
    [SerializeField] int animSpeedTrans;
    [SerializeField] int distanceRunTowardPlayerOnDmg;
    [SerializeField] Transform locateWallPos;
    [SerializeField] int searchForWall;
    [SerializeField] Rigidbody rb;
    float fireRate;
    float angleOfPlayer;
    Vector3 playerDirection;
    Vector3 playerPosition;
    Vector3 playerPreviousPosition;
    Vector3 coverTransitionVector;
    Vector3 firstTransitionClear, coverPointPos;
    bool isShooting;
    bool canMeleeAttack;
    bool playerInRange;
    float inSideDistance;
    bool coverSetDirectionFinished = false;
    bool visibleToPlayer;
    bool checkVisibilityToPlayer = false;
    Color originalColor;

    void Start()
    {
        coverTransitionVector = Vector3.zero;
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
        //if ((visibleToPlayer || checkVisibilityToPlayer) && agent.remainingDistance < 2f)
        //{
        //    checkVisibilityToPlayer = false;
        //    if (AmIVisibleToPlayer())
        //    {
        //        visibleToPlayer = true;
        //    }
        //    else visibleToPlayer = false;
        //}
        if (coverSetDirectionFinished)
        {
            if (!agent.pathPending && agent.remainingDistance < 2f)
            {
                coverSetDirectionFinished = false;
                checkVisibilityToPlayer = true;
            }
        }

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
            if (!isShooting)
                HandleRangedCombatOnDmg(playerDirection);


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
                randomRotation = Quaternion.Euler(Random.Range(-enemyBulletSpread, enemyBulletSpread), Random.Range(-enemyBulletSpread, enemyBulletSpread), 1);

            }

            firearmScript.AIShoot(randomRotation, transform.position);


        }

        else
        {
            // Implement prediction of player movement with random offset to the player is not being laser beamed
            if (inaccuracyChance > applyInaccuracy)
            {
                randomRotation = Quaternion.Euler(Random.Range(-enemyBulletSpread, enemyBulletSpread), Random.Range(-enemyBulletSpread, enemyBulletSpread), 1);

            }
            Vector3 rotateDir = PredictPlayerMovement(transform.position, GameManager.instance.player.transform.position, GameManager.instance.player.transform.position, 150);
            RotateToPlayer(rotateDir);
            firearmScript.AIShoot(randomRotation, transform.position);
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
        angleOfPlayer = Vector3.Angle(playerDirection, transform.forward);
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
            Quaternion rotateAi = Quaternion.LookRotation(playerDirection);
            transform.rotation = Quaternion.Lerp(transform.rotation, rotateAi, facePlayerSpeed * Time.deltaTime);
            agent.SetDestination(GameManager.instance.player.transform.position);

        }
    }
    void HandleRangedCombatOnDmg(Vector3 playerDirection)
    {
        Quaternion rotateAi = Quaternion.LookRotation(playerDirection);
        transform.rotation = rotateAi;
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
        Vector3 wallPoint = Vector3.zero;
        float offset = -90;
        Vector3 foundWall = Vector3.zero;
        Quaternion rotationToApply = Quaternion.Euler(1, offset, 1);
        Vector3 directionForCast = rotationToApply * locateWallPos.transform.forward;
        Vector3 hitNorm = Vector3.zero;
       
        Transform originalWallPos = locateWallPos;
        bool isWallLocated = false;
        int layerMask = LayerMask.GetMask("Wall");
        for (int i = 0; i < 9; i++)
        {
            Debug.DrawRay(locateWallPos.position, directionForCast, Color.blue, Mathf.Infinity);
            if (Physics.Raycast(locateWallPos.position, directionForCast, out hit, Mathf.Infinity, layerMask))
            {
                Debug.Log("Hit a wall");
                Vector3 wallLocation = hit.collider.gameObject.transform.position.normalized;
                isWallLocated = true;
                if (i != 0)
                {
                    float distanceFirst = Vector3.Distance(transform.position, wallLocation);
                    float distanceSecond = Vector3.Distance(transform.position, foundWall);
                    if (distanceFirst < distanceSecond)
                    {
                        foundWall = wallLocation;
                        wallPoint = hit.point;
                        hitNorm = hit.normal;
                    }

                }
                else
                {
                    foundWall = wallLocation;
                    wallPoint = hit.point;
                    hitNorm = hit.normal;
                }
            }


            // check at 20 degree intervals 
            offset += 20;
            rotationToApply = Quaternion.Euler(1, offset, 1);
            directionForCast = rotationToApply * locateWallPos.transform.forward;
        }
        if (isWallLocated)
        {
            Vector3 directPlayerToWall = wallPoint - GameManager.instance.player.transform.position;
            Vector3 changeXAxis = new Vector3(0, directPlayerToWall.y, directPlayerToWall.z);
            Vector3 oppositeSide =(changeXAxis.normalized - wallPoint + hitNorm) * 10f;
            NavMeshHit navHit;
            if (NavMesh.SamplePosition(oppositeSide, out navHit, 10f, NavMesh.AllAreas))
            {
                agent.SetDestination(oppositeSide);
                Debug.Log("Navigating to opposite wall");
            }
            else Debug.Log("Warning, not valid position");

        }


        locateWallPos = originalWallPos;

    }
    bool AmIVisibleToPlayer()
    {

        Vector3 playerDirection = GameManager.instance.player.transform.position - transform.position;
        float angleBetweenAIToPlayer = Vector3.Angle(playerDirection, transform.forward);
        Quaternion rotateAI = Quaternion.LookRotation(playerDirection);
        float reflectedAngle = 360 - angleBetweenAIToPlayer;
        float redirectAngle = reflectedAngle / 1.5f;
        transform.rotation = Quaternion.Lerp(transform.rotation, rotateAI, facePlayerSpeed * Time.deltaTime);
        
        RaycastHit hit;
        if(Physics.Raycast(transform.position, playerDirection, out hit, 10000))
        {
            Debug.DrawRay(transform.position, transform.forward);
            if (hit.collider.gameObject.CompareTag("Player"))
            {
                Vector3 coverDirection = coverPointPos - transform.position;
                rotateAI = Quaternion.LookRotation(coverDirection);
                transform.rotation = Quaternion.Lerp(transform.rotation, rotateAI, facePlayerSpeed * Time.deltaTime);
                agent.SetDestination(coverDirection);
                return true;
            }
        }

        return false;
    }
}
