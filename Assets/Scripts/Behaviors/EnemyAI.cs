using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : Bouncer
{
    protected IEnumerator TelegraphedAttackHandler;

    [Space(10)]
    [Header("Attack Stats")]
    [SerializeField] protected Vector2 attackPowerRange = new Vector2(0.5f, 1f);
    [SerializeField] protected float attackMaxDevience = 7f;
    [SerializeField] protected float attackWindUpTime = 0.5f;
    [SerializeField] protected Vector2 attackCoolDownRange = new Vector2(3f, 6f);

    protected IEnumerator AttackSequencer;

    public override void Start()
    {
        //Assign constants
        gameController = GameController.gameController;
        rigidbody = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();

        //Set trail particles
        trailParticles = Instantiate(gameController.gameSettings.trailParticlesPrefab, transform);
        trailParticles.gameObject.SetActive(false);

        //Assign Health and Stamina
        health = new Status(this, Mathf.RoundToInt(Random.Range(stats.h_MaxValue.x, stats.h_MaxValue.y)), stats.h_MinValue, stats.h_RegenRate, stats.h_AllowRegen, stats.h_AllowChange);
        stamina = new Status(this, Mathf.RoundToInt(Random.Range(stats.s_MaxValue.x, stats.s_MaxValue.y)), stats.s_MinValue, stats.s_RegenRate, stats.s_AllowRegen, stats.s_AllowChange);

        //Setup the bounds radius and aim reticules
        boundsRing.localScale = boundsRing.localScale * gameController.gameSettings.boundsRadius;
        GetComponent<CapsuleCollider>().radius *= gameController.gameSettings.boundsRadius;
        foreach (Transform aimRet in aimReticule)
        {
            aimRet.localPosition = new Vector3(gameController.gameSettings.boundsRadius * aimRet.localPosition.x, aimRet.localPosition.y, gameController.gameSettings.boundsRadius * aimRet.localPosition.z);
        }

        //Start attack sequencing
        AttackSequencer = attackHandler();
        StartCoroutine(AttackSequencer);

        //Face forward
        FaceCamera();
    }

    public void AttackToward(Vector3 target)
    {
        //Get random attack power allowed by mana reserves
        float maxPullAllowByStamina = Mathf.Clamp(stamina.currentValue / gameController.gameSettings.maxAttackPower, 0f, 1f);
        float power = Mathf.Clamp(Random.Range(attackPowerRange.x, attackPowerRange.y), stamina.minValue, maxPullAllowByStamina);

        //Get random attack position close to the player
        target = new Vector3(target.x + Random.Range(-attackMaxDevience, attackMaxDevience), 0f, target.z + Random.Range(-attackMaxDevience, attackMaxDevience));

        //Reset attack coroutine and trigger again
        if (TelegraphedAttackHandler != null) StopCoroutine(TelegraphedAttackHandler);
        TelegraphedAttackHandler = TelegraphedAttack(target - transform.position, power, attackWindUpTime);
        StartCoroutine(TelegraphedAttackHandler);
    }

    IEnumerator TelegraphedAttack(Vector3 target, float power, float windUp)
    {
        //Face AI toward target
        transform.LookAt(new Vector3(target.x * 100f, transform.position.y, target.z * 100f));

        //Wind up the attack to show player the direction they'll be going
        EnableAimReticule(true);
        for (float i = 0f; i < windUp; i += Time.deltaTime)
        {
            AdjustAimReticuleSizeAndColor((i / windUp) * power);
            yield return null;
        }
        EnableAimReticule(false);

        //Launch Attack
        LaunchBouncer(power);
    }

    public override void ReceiveAttack(float attackDamage)
    {
        if (alive)
        {
            //Play hurt noise, particle effects, etc. here
            if (TakeDamageSequencer != null) StopCoroutine(TakeDamageSequencer);
            TakeDamageSequencer = TakeDamageSequence();
            StartCoroutine(TakeDamageSequencer);

            //Interrupt the planned attack if it was busy winding up
            if (TelegraphedAttackHandler != null)
            {
                StopCoroutine(TelegraphedAttackHandler);
                EnableAimReticule(false);
            }

            //Apply damage
            health.AdjustValue(-attackDamage);
            if (health.currentValue == 0f) Die();
        }
    }

    public override void Die()
    {
        alive = false;
        if(TelegraphedAttackHandler != null)StopCoroutine(TelegraphedAttackHandler);
        if (AttackSequencer != null) StopCoroutine(AttackSequencer);

        //Remove the gameobject
        GameController.gameController.wolves.Remove(this);
        if (GameController.gameController.wolves.Count == 0) gameController.GameOver(true);

        StartCoroutine(deathSequence());
    }

    public IEnumerator deathSequence()
    {
        //Play death effects
        Debug.Log(gameObject.name + " has been killed");
        //Add to the player's score
        GameController.gameController.bounties[0]++;

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


        GetComponent<Collider>().enabled = false;

        //Explode
        Instantiate(gameController.gameSettings.explosionParticlesPrefab, transform.position, transform.rotation);
        gameController.universalAudioSource.PlayOneShot(deathSound);

        //Remove object from scene
        Destroy(this.gameObject);
    }

    IEnumerator attackHandler()
    {
        yield return new WaitForSeconds((Random.Range(attackCoolDownRange.x, attackCoolDownRange.y))*gameController.gameDifficulty.wolfAttackFrequencyMultiplier*gameController.gameSpeedSettings.wolfAttackFrequencyMultiplier);
        while (gameController.player != null && !gameController.gameOver)
        {
            //every random amount of seconds, attack a random enemy or direction, if health is below 3, move away from player
            Vector3 target;

            //Evade if the player is too close
            if (health.currentValue < 3f && Vector3.Distance(gameController.player.transform.position, transform.position) < gameController.gameSettings.boundsRadius * 4f)
            {
                target = transform.position - (gameController.player.transform.position - transform.position);
            }
            //Otherwise, perform the random move
            else
            {
                int ranNum = Random.Range(0, 10);
                if (ranNum >= 5) target = GameController.gameController.player.gameObject.transform.position;
                else if (ranNum <= 2 && GameController.gameController.pigs.Count != 0) target = GameController.gameController.pigs[Random.Range(0, GameController.gameController.pigs.Count - 1)].gameObject.transform.position;
                else
                {
                    Vector2 circlePoint = Random.insideUnitCircle * 5f;
                    target = new Vector3(circlePoint.x, transform.position.y, circlePoint.y);
                }
            }
            //attack the target
            AttackToward(target);
            yield return new WaitForSeconds((Random.Range(attackCoolDownRange.x, attackCoolDownRange.y)) * gameController.gameDifficulty.wolfAttackFrequencyMultiplier * gameController.gameSpeedSettings.wolfAttackFrequencyMultiplier);
        }
    }

    public override IEnumerator Launch(float powerLevel)
    {
        //stamina handling
        stamina.AdjustValue(-GetAttackDamage(powerLevel));

        //Spawn particles
        trailParticles.SetActive(true);

        //Play sound effect
        audioSource.PlayOneShot(launchSound);

        //apply force for launch and set attacking to true until the bouncer has slowed down sufficiently
        rigidbody.velocity = transform.forward * gameController.gameSettings.attackPowerScaler * powerLevel * gameController.gameDifficulty.wolfPowerMultiplier;
        attacking = true;
        while (rigidbody.velocity.magnitude != 0f)
        {
            if (rigidbody.velocity.magnitude < 0.3f) rigidbody.velocity = Vector3.zero;
            yield return null;
        }
        currentAttackPower = 0f;
        attacking = false;

        //Despawn particles
        trailParticles.SetActive(false);

        //face the thing toward the camera again
        FaceCamera();
    }
}
