using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PigPowerUpAI : MonoBehaviour
{
    [Header("Configuration")]
    protected Rigidbody rigidbody;
    protected AudioSource audioSource;
    [SerializeField] public float healthBuff;
    [SerializeField] public float staminaBuff;
    [SerializeField] protected float pigMoveSpeed = 15f;
    [Tooltip("Minimum time between pig move state changes")][SerializeField] protected float pigMoveChangeFrequency = 3f;

    protected bool usable = true;
    [SerializeField]protected bool dying = false;

    [SerializeField] protected SphereCollider trigger;
    [SerializeField] protected List<GameObject> enemiesPresentInTrigger = new List<GameObject>();

    [SerializeField] protected GameObject characterModel;
    [SerializeField] protected Transform boundsRing;

    protected enum moveStates { Standing, Walking, Evading };
    [SerializeField] protected moveStates moveState = moveStates.Standing;
    protected IEnumerator MovementHandler;

    [Space(10)]
    [Header("Sound")]
    [SerializeField] protected AudioClip takeDamageSound;
    [SerializeField] protected AudioClip deathSound;

    private void Start()
    {
        //Assign constants
        rigidbody = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();

        //Setup the bounds radius and detectionRadius
        boundsRing.localScale = boundsRing.localScale * GameController.gameController.gameSettings.boundsRadius;
        GetComponent<CapsuleCollider>().radius *= GameController.gameController.gameSettings.boundsRadius;
        trigger.radius *= GameController.gameController.gameSettings.pigDetectionRadius * GameController.gameController.gameDifficulty.pigDetectionRadiusMultiplier;

        //Face forward
        FaceCamera();

        //Initiate movement
        MovementHandler = Move();
        StartCoroutine(MovementHandler);
    }

    private void FaceCamera()
    {
        transform.LookAt(new Vector3(Camera.main.gameObject.transform.position.x, transform.position.y, -10000f));
    }

    private void OnCollisionEnter(Collision collision)
    {
        Bouncer enemyBouncer;
        //Check if it's a bouncer you're colliding with, if it is, give them boosted stats
        if (collision.gameObject.CompareTag("Bouncer") && collision.gameObject.GetComponent<Bouncer>() != null && dying == false)
        {
            enemyBouncer = collision.gameObject.GetComponent<Bouncer>();
            //ensure the other person was attacking the pig purposefully
            if (enemyBouncer.attacking && Vector3.Angle(enemyBouncer.transform.forward, transform.position - enemyBouncer.transform.position) <= GameController.gameController.gameSettings.attackAngle)
            {
                dying = true;
                //Add to the player's score
                if (enemyBouncer.isControllable) GameController.gameController.bounties[1]++;
                //Apply Power up
                enemyBouncer.PowerUp(healthBuff, staminaBuff, transform.position);
                //Kill off the pig and prevent it from being used again
                usable = false;
                StopCoroutine(MovementHandler);
                //Die
                StartCoroutine(Die());
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //If an enemy enters the trigger, add it to the list of the ones present and freak the pig out
        if (other.CompareTag("Bouncer"))
        {
            enemiesPresentInTrigger.Add(other.gameObject);
            moveState = moveStates.Evading;
        }
    }

    IEnumerator Move()
    {
        //The time since the pig last turned or stood
        float moveChangeTimer = 0f;

        while (true)
        {
            //get the current state of the pig's movement
            switch (moveState)
            {
                case moveStates.Walking:
                    moveChangeTimer += 0.3f;
                    rigidbody.velocity = transform.forward * pigMoveSpeed * GameController.gameController.gameDifficulty.pigMoveSpeedMultiplier;
                    //if it hits a wall, apply a turn
                    if (Physics.Raycast(transform.position, transform.forward, GameController.gameController.gameSettings.boundsRadius*1.25f))
                    {
                        moveChangeTimer = 0f;
                        Debug.Log("Turned due to collision");
                        transform.Rotate(new Vector3(0f, Random.Range(60f, 180f), 0f));
                    }
                    //if it walks straight for too long, turn or stand still
                    if(moveChangeTimer > pigMoveChangeFrequency)
                    {
                        float ranNum = Random.Range(0f, 10f);
                        if (ranNum > 6f)
                        {
                            Debug.Log("Turned after " + moveChangeTimer.ToString() + " seconds");
                            moveChangeTimer = 0f;
                            transform.Rotate(new Vector3(0f, Random.Range(60f, 180f), 0f));
                        }else if (ranNum < 2f)
                        {
                            moveState = moveStates.Standing;
                            moveChangeTimer = 0f;
                        }
                    }
                    break;

                case moveStates.Evading:
                    Debug.Log("Freaking " + enemiesPresentInTrigger.Count + " enemies in the trigger!");
                    Vector3 averageEnemyPosition = new Vector3(0f, 1f, 0f);
                    List<GameObject> enemiesToRemove = new List<GameObject>();
                    //Assign enemies from trigger to cull (as they will not trigger the on exit code)
                    for(int i = 0; i < enemiesPresentInTrigger.Count; i++)
                    {
                        //Assign Cull
                        if (enemiesPresentInTrigger[i] == null)
                        {
                            break;
                        }
                        else if (Vector3.Distance(enemiesPresentInTrigger[i].transform.position, transform.position) > GameController.gameController.gameSettings.pigDetectionRadius * 1.8f * GameController.gameController.gameDifficulty.pigDetectionRadiusMultiplier)
                        {
                            enemiesToRemove.Add(enemiesPresentInTrigger[i]);
                            //average position for all so the pig can run away
                            averageEnemyPosition += enemiesPresentInTrigger[i].transform.position;
                        }
                    }
                    //Cull loop
                    if(enemiesToRemove.Count != 0)
                    {
                        for (int i = 0; i < enemiesToRemove.Count; i++)
                        {
                            enemiesPresentInTrigger.Remove(enemiesToRemove[i]);
                        }
                    }
                    if (enemiesPresentInTrigger.Count == 0)
                    {
                        moveChangeTimer = 0f;
                        if (Random.Range(0f, 10f) > 5f) moveState = moveStates.Walking;
                        else moveState = moveStates.Standing;
                    }
                    //Generate the average position of all enemies in the trigger
                    averageEnemyPosition /= enemiesPresentInTrigger.Count + enemiesToRemove.Count;
                    averageEnemyPosition.y = 1f;
                    //turn the pig away from the enemy and have them run
                    transform.LookAt(transform.position - (averageEnemyPosition - transform.position));
                    Debug.Log(transform.position - (averageEnemyPosition - transform.position));
                    rigidbody.velocity = transform.forward * pigMoveSpeed * GameController.gameController.gameDifficulty.pigMoveSpeedMultiplier;
                    break;

                case moveStates.Standing:
                    moveChangeTimer += 0.3f;
                    rigidbody.velocity = Vector3.zero;
                    //if it stands still too long, have it move
                    if (moveChangeTimer > pigMoveChangeFrequency)
                    {
                        float ranNum = Random.Range(0f, 10f);
                        if (ranNum > 3f)
                        {
                            moveChangeTimer = 0f;
                            moveState = moveStates.Walking;
                        }
                    }
                    break;
            }
            yield return new WaitForSeconds(0.3f);
        }
    }

    IEnumerator Die()
    {
        //Spawn particles to show force of attack
        Instantiate(GameController.gameController.gameSettings.trailParticlesPrefab, transform);

        //Play pig hurt noise
        audioSource.PlayOneShot(takeDamageSound);

        //Do a little visual effect blackout and then remove the pig
        Renderer[] rends = characterModel.GetComponentsInChildren<Renderer>();
        foreach (Renderer rend in rends)
        {
            rend.material.SetFloat("Boolean_8B593A8", 1);
        }
        yield return new WaitForSeconds(0.3f);

        foreach (Renderer rend in rends)
        {
            rend.material.SetFloat("Boolean_8B593A8", 0);
        }
        for (int i = 0; i < 3; i++)
        {
            yield return new WaitForSeconds(0.07f);
            foreach (Renderer rend in rends)
            {
                rend.material.SetFloat("Boolean_8B593A8", 1);
            }
            yield return new WaitForSeconds(0.04f);
            foreach (Renderer rend in rends)
            {
                rend.material.SetFloat("Boolean_8B593A8", 0);
            }
        }

        GameController.gameController.pigs.Remove(this);

        //Explode
        Instantiate(GameController.gameController.gameSettings.explosionParticlesPrefab, transform.position, transform.rotation);
        GameController.gameController.universalAudioSource.PlayOneShot(deathSound);

        Destroy(this.gameObject);
    }
}
