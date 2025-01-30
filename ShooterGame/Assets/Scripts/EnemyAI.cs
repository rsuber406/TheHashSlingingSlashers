using System.Collections;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour, IDamage, AINetwork
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("AI Movement")]
   
    [SerializeField] int sprintMod;
    [SerializeField] int facePlayerSpeed;
    [SerializeField] float distanceToActivateSprint;
    [SerializeField] int fov;
    [SerializeField] int searchForWall;
    [SerializeField] int distanceRunTowardPlayerOnDmg;
    [Header("Health")]
    [SerializeField] int health;
    [SerializeField] int maxHealth;
    [Header("Enemy Type")]
    [SerializeField] bool chasePlayer;
    [SerializeField] bool isMelee;
    [SerializeField] bool isTurrent;
    [Header("Weapon Loadout")]
    [SerializeField] GameObject firearm;
    [SerializeField] GunScripts firearmScript;
    [SerializeField] GameObject meleeWeapon;
    [SerializeField] float enemyBulletSpread;
    [Header("AI Configuration")]
    [SerializeField] private ParticleSystem bloodEffect;
    [SerializeField] Animator animatorController;
    [SerializeField] int animSpeedTrans;
    [SerializeField] Transform locateWallPos;
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Renderer model;
    [SerializeField] Transform headPos;
    [SerializeField] SphereCollider playerSphere;
    [SerializeField] SphereCollider aiSphere;
    [SerializeField] int roamDistance;
    [SerializeField] int roamPauseTime;
    // [SerializeField] Rigidbody rb;
    float fireRate;
    float angleOfPlayer;
    Vector3 playerDirection;
    Vector3 playerPreviousPosition;
    Vector3  currentPos;
    Vector3 startingPos;
    bool isShooting;
    bool canMeleeAttack;
    bool playerInRange;
    bool assistingFriend = false;
    bool isRoaming = false;
    bool isAlive = true;
    bool isDead = false;
    float fovAsDecimal;
    float stoppingDistance;
    Color originalColor;
    Coroutine roamCo;
    private DynamicTextManager dynamicTextManager;
    
    void Start()
    {
        dynamicTextManager = GameManager.instance.dynamicTextManager;
        playerPreviousPosition = Vector3.zero;
        startingPos = this.transform.position;
        originalColor = model.material.color;
        canMeleeAttack = isMelee;
        stoppingDistance = agent.stoppingDistance;
        if (firearm != null)
        {
            firearmScript = firearm.GetComponent<GunScripts>();
            fireRate = firearmScript.GetFireRate();
        }
        else if (!isMelee)
        {
            fireRate = firearmScript.GetFireRate();
        }
        fovAsDecimal = 1f - ((float)fov / 100);
    }

    // Update is called once per frame
    void Update()
    {
        if (!isAlive) return;
        currentPos = transform.position;
        float characterSpeed = agent.velocity.normalized.magnitude;
        float animSpeed = animatorController.GetFloat("Speed");
        animatorController.SetFloat("Speed", Mathf.MoveTowards(animSpeed, characterSpeed, Time.deltaTime * animSpeedTrans));
        PlayerDetection();
        if (!isMelee)
        {
          //  PerformReload();

        }


        if (assistingFriend)
        {
            if (!isMelee && !isShooting)
                StartCoroutine(Shoot());
            if (GameManager.instance.GetPlayerHealth() <= 0)
            {
                assistingFriend = false;
            }
        }


    }
    void PlayerDetection()
    {
        if (!isAlive) return;
        if ((playerInRange && !CanSeePlayer()))
        {
            if (!isRoaming && agent.remainingDistance < 0.01f)
            {
               
                roamCo = StartCoroutine(Roam());
            }

        }
        else if (!playerInRange)
        {
            if (!isRoaming && agent.remainingDistance < 0.01f)
            {
                
                roamCo = StartCoroutine(Roam());
            }
        }

    }
    public void TakeDamage(int amount, Vector3 origin)
    {
        if (health <= 0) return;
        playerDirection = origin - transform.position;
        if (roamCo != null)
        {
            StopCoroutine(roamCo);
            isRoaming = false;
        }
        
        agent.stoppingDistance = stoppingDistance;
        if (isMelee && !playerInRange)
        {
            HandleMeleeAIMoveOnDmg(ref playerDirection);
        }
        else
        {
            if (!isShooting && !isMelee)
                HandleRangedCombatOnDmg(playerDirection);


        }
        health -= amount;
        StartCoroutine(RegisterHit());


        //blood
        Quaternion bloodRotation = Quaternion.LookRotation(playerDirection);
        Vector3 bloodSpawnPos = transform.position + (Vector3.up * 1f);
        float randomX = Random.Range(-0.1f, 0.2f);
        float randomY = Random.Range(0.1f, 0.1f);
        float randomZ = Random.Range(-0.1f, 0.1f);
        bloodSpawnPos += new Vector3(randomX, randomY, randomZ);
        ParticleSystem blood = Instantiate(bloodEffect, bloodSpawnPos, bloodRotation);
        blood.transform.SetParent(headPos.transform);

        //text
        Vector3 textPos = headPos.transform.position;
        float randomXText = Random.Range(-0.6f, 0.6f);
        float randomYText = Random.Range(0.1f, 0.6f);
        float randomZText = Random.Range(-0.6f, 0.6f);
        textPos += new Vector3(randomXText, randomYText, randomZText);
        string dmgText = amount.ToString();
        DynamicTextData damageTextData = Resources.Load<DynamicTextData>("EnemyDamageTextData");
        DynamicTextManager.CreateText(textPos, dmgText, damageTextData);

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger)
        {
            return;
        }
        if (other.CompareTag("Player"))
        {
            playerInRange = true;

        }
        // This uses the collision sphere to see which bots are nearby and call them to help
        if (other.CompareTag("Enemy") && aiSphere.enabled && !playerInRange)
        {


            AINetwork aiNet = other.GetComponent<AINetwork>();
            if (aiNet != null)
            {
                aiNet.HelpBots(currentPos);

            }

        }


    }
   
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            agent.stoppingDistance = 0;
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
       Vector3 playerPosition = GameManager.instance.player.transform.position;
        // This is how we should compare vectors, take a difference and then compare it against a small amount
        bool playerStationary = (playerPosition - playerPreviousPosition).sqrMagnitude < 0.0001f;
        // This will allow us to decrease the accuracy so out player is not being laser beamed
        int applyInaccuracy = 40;
        int inaccuracyChance = Random.Range(0, 100);
        Quaternion randomRotation = Quaternion.Euler(1, 1, 1);
        if (playerStationary)
        {
            // Implement shooting at player
            // Implement random number offsets so the AI does not laser beam the player
            if (inaccuracyChance > applyInaccuracy)
            {
                randomRotation = Quaternion.Euler(Random.Range(-enemyBulletSpread * 2, enemyBulletSpread * 2), Random.Range(-enemyBulletSpread * 2, enemyBulletSpread * 2), 1);

            }

            firearmScript.AIShoot( randomRotation, transform.position);


        }

        else
        {
            // Implement prediction of player movement with random offset to the player is not being laser beamed
            if (inaccuracyChance > applyInaccuracy)
            {
                randomRotation = Quaternion.Euler(Random.Range(-enemyBulletSpread * 2, enemyBulletSpread * 2), Random.Range(-enemyBulletSpread * 2, enemyBulletSpread * 2), 1);

            }
            Vector3 rotateDir = PredictPlayerMovement(transform.position, GameManager.instance.player.transform.position, GameManager.instance.player.transform.position, 45);
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
    IEnumerator OnDeath()
    {

        if (isDead)
        {
            yield break;
        }
        isDead = true;

        BulletTime bt = FindAnyObjectByType<BulletTime>();
        if (bt != null)
        {
            bt.IncreaseMaxSlowMotionDuration(1f);
        }
        GameManager.instance.scoreSys.AddFlatScore(100);
        isAlive = false;
        agent.isStopped = true;
       
       
        agent.velocity = Vector3.zero;
        animatorController.SetTrigger("Death");

        CapsuleCollider disableCollider = this.GetComponent<CapsuleCollider>();
        disableCollider.enabled = false;
        if (isMelee)
        {
            BoxCollider weaponCollider = meleeWeapon.GetComponent<BoxCollider>();
            weaponCollider.enabled = false;

        }
        yield return new WaitForSeconds(3f);
        Destroy(gameObject);
    }

    IEnumerator RegisterHit()
    {
        if (isAlive)
            model.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        model.material.color = originalColor;
        if (health <= 0)
        {
            StartCoroutine(OnDeath());
        }
    }

    void PerformReload()
    {
        if (isMelee) return;
        if (firearmScript.GetBulletsRemaining() <= 0)
        {
            StartCoroutine(firearmScript.Reload());
        }
    }
    bool CanSeePlayer()
    {
        float dotProduct = Vector3.Dot(transform.forward, (GameManager.instance.player.transform.position - transform.position).normalized);

        if (dotProduct > 0)
        {
            if (dotProduct >= fovAsDecimal)
            {
                RaycastHit hit;
                // This is for prediction code needed later
                playerPreviousPosition = GameManager.instance.player.transform.position;
                playerDirection = playerPreviousPosition - headPos.transform.position;
                float distance = playerDirection.magnitude;
                if (Physics.Raycast(headPos.position, playerDirection, out hit))
                {
                    if (hit.collider.CompareTag("Player"))
                    {
                        agent.stoppingDistance = stoppingDistance;
                        if (chasePlayer)
                        {
                            agent.SetDestination(GameManager.instance.player.transform.position);
                            if (distance > distanceToActivateSprint)
                            {
                               
                            }
                            else
                            {
                               

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

            }
        }
        return false;
    }
    // 


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
        float inSideDistance = playerDirection.magnitude;

        // give back a quaternion and feed a vector
        if (inSideDistance >= distanceRunTowardPlayerOnDmg)
        {

            // rotate 90 degrees and run
            FindNearestWall();
        }
        else
        {
            
            Quaternion rotateAi = Quaternion.LookRotation(playerDirection);
            // transform.rotation = Quaternion.Lerp(transform.rotation, rotateAi, facePlayerSpeed * Time.deltaTime);
            agent.SetDestination(GameManager.instance.player.transform.position);

        }
    }
    void HandleRangedCombatOnDmg(Vector3 playerDirection)
    {
        Quaternion rotateAi = Quaternion.LookRotation(playerDirection);
        transform.rotation = rotateAi;
        agent.SetDestination(playerDirection);
        if (isMelee) return;
       
        PerformReload();
        StartCoroutine(DelayFireBack());
        StartCoroutine(Shoot());

    }

    public void TakeDamage(int amount)
    {

    }
    IEnumerator DelayFireBack()
    {
        yield return new WaitForSeconds(0.5f);
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
          
            if (Physics.Raycast(locateWallPos.position, directionForCast, out hit, Mathf.Infinity, layerMask))
            {

                Vector3 wallLocation = hit.collider.gameObject.transform.position.normalized;
                isWallLocated = true;
                if(Vector3.Distance(wallPoint, Vector3.zero) <= 0)
                {
                    wallPoint = hit.point;
                    hitNorm = hit.normal;
                    
                }
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
            float dotProductToWall = Vector3.Dot(transform.forward, GameManager.instance.player.transform.position);
            if (dotProductToWall < 0)
            {
               
                hitNorm = hitNorm * -1;
            }
            else if (dotProductToWall > 0)
            {
              
            }
            Vector3 aiToWall = (hitNorm * Vector3.Distance(transform.position, wallPoint) * 1.5f) + transform.position;
           
            float dotProductNewPosToPlayer = Vector3.Dot(aiToWall, (GameManager.instance.player.transform.forward).normalized);
            if(dotProductNewPosToPlayer < 0)
            {
               if(Physics.Raycast(GameManager.instance.player.transform.position, aiToWall - GameManager.instance.player.transform.position))
                {
                    Vector3 directionFromPlayerToNewPos = aiToWall - GameManager.instance.player.transform.position;
                    Vector3 redirectPos = (aiToWall.normalized * 5) - (directionFromPlayerToNewPos* 0.75f);

                    agent.SetDestination(-redirectPos);
                }
            }
           else agent.SetDestination(aiToWall);


        }


        locateWallPos = originalWallPos;

    }


    public void HelpBots(Vector3 assistVector)
    {
        if (assistingFriend) return;

        agent.SetDestination(assistVector);
        assistingFriend = true;
     

    }

    public void ActivateCollider()
    {

        StartCoroutine(ToggleHelpField());
    }
    IEnumerator ToggleHelpField()
    {
        aiSphere.enabled = true;
        yield return new WaitForSeconds(0.5f);
        aiSphere.enabled = false;
    }

    IEnumerator Roam()
    {
        isRoaming = true;
        yield return new WaitForSeconds(roamPauseTime);
        agent.stoppingDistance = 0;
        Vector3 randomPosition = Random.insideUnitSphere * roamDistance;
        randomPosition += startingPos;
        NavMeshHit hit;
        NavMesh.SamplePosition(randomPosition, out hit, roamDistance,1);
        agent.SetDestination(hit.position);
        isRoaming = false;

    }

   


}
