using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Collider))][RequireComponent(typeof(Rigidbody))][RequireComponent(typeof(AudioSource))]
public class Bouncer : MonoBehaviour
{
    [Header("Configuration")]
    protected GameController gameController;
    protected Rigidbody rigidbody;
    public AudioSource audioSource;
    public  GameObject trailParticles;
    [SerializeField] public Transform characterModel;
    [SerializeField] protected Transform boundsRing;
    [SerializeField] protected Transform[] aimReticule = { null, null };
    [SerializeField] protected Color[] aimReticuleStageGradient;
    [SerializeField] public bool isControllable = false;
    [SerializeField] public StatusProfile_SO stats;
    [SerializeField] public int team;

    [Space(10)]
    [Header("Sound")]
    [SerializeField] public AudioClip launchSound;
    [SerializeField] public AudioClip takeDamageSound;
    [SerializeField] public AudioClip deathSound;

    [Space(10)]
    [Header("Status")]
    [SerializeField] public bool aiming = false;
    [SerializeField] public bool attacking = false;
    [SerializeField] public bool alive = true;
    [SerializeField] protected float currentAttackPower = 0f;
    [SerializeField] public Status health;
    [SerializeField] public Status stamina;
    protected IEnumerator AttackHandler;
    protected IEnumerator TakeDamageSequencer;

    public virtual void Start()
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
        foreach(Transform aimRet in aimReticule)
        {
            aimRet.localPosition = new Vector3(gameController.gameSettings.boundsRadius* aimRet.localPosition.x, aimRet.localPosition.y , gameController.gameSettings.boundsRadius * aimRet.localPosition.z);
        }

        //Face forward
        FaceCamera();
    }

    public void FaceCamera()
    {
        if(!aiming)transform.LookAt(new Vector3(Camera.main.gameObject.transform.position.x, transform.position.y, -10000f));
    }

    #region Launching
    public void LaunchBouncer(float powerLevel)
    {
        currentAttackPower = powerLevel;

        //Start the attack coroutine when launch is called from another class
        if (AttackHandler != null) StopCoroutine(AttackHandler);
        AttackHandler = Launch(currentAttackPower);
        StartCoroutine(AttackHandler);
    }

    public virtual IEnumerator Launch(float powerLevel)
    {
        //stamina handling
        stamina.AdjustValue(-GetAttackDamage(powerLevel));

        //Play sound
        audioSource.Play();

        //Spawn particles
        trailParticles.SetActive(true);

        //apply force for launch and set attacking to true until the bouncer has slowed down sufficiently
        rigidbody.velocity = transform.forward * gameController.gameSettings.attackPowerScaler * powerLevel;
        attacking = true;
        while(rigidbody.velocity.magnitude != 0f)
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
    #endregion

    #region Aiming Visuals
    public void EnableAimReticule(bool state)
    {
        foreach(Transform aimRet in aimReticule)
        {
            aimRet.gameObject.SetActive(state);
        }
    }

    public void AdjustAimReticuleSizeAndColor(float size)
    {
        //get the current power bracket and assign it the right color
        Color sizeChosenColor;
        if (size <= 0.2f) sizeChosenColor = aimReticuleStageGradient[0]; 
        else if (size > 0.2f && size <= 0.4f) sizeChosenColor = aimReticuleStageGradient[1];
        else if (size > 0.4f && size <= 0.6f) sizeChosenColor = aimReticuleStageGradient[2];
        else if (size > 0.6f && size <= 0.8f) sizeChosenColor = aimReticuleStageGradient[3];
        else sizeChosenColor = aimReticuleStageGradient[4];

        //Change the size of the reticules according to game settings
        size *= gameController.gameSettings.aimReticuleClamps.y;
        size = Mathf.Clamp(size, gameController.gameSettings.aimReticuleClamps.x, gameController.gameSettings.aimReticuleClamps.y);
        foreach(Transform aimRet in aimReticule)
        {
            aimRet.localScale = Vector3.one * size;
            //Get each renderer, go through it and make sure all materials are changed
            Renderer[] rendersToChange = aimRet.gameObject.GetComponentsInChildren<Renderer>();
            foreach(Renderer rend in rendersToChange)
            {
                rend.material.SetColor("_BaseColor", sizeChosenColor);
            }
        }
    }
    #endregion

    private void OnCollisionEnter(Collision collision)
    {
        Bouncer enemyBouncer;
        //See if the object collided with is indeed an emeny, and only if so, go through to applying an attack
        if (collision.gameObject.CompareTag("Bouncer") && collision.gameObject.GetComponent<Bouncer>() != null)
        {
            if(collision.gameObject.GetComponent<Bouncer>().team != team)
            {
                enemyBouncer = collision.gameObject.GetComponent<Bouncer>();
                //Ensuring the enemy is within the attack angle
                if (attacking && Vector3.Angle(transform.forward, enemyBouncer.gameObject.transform.position - transform.position) <= gameController.gameSettings.attackAngle)
                {
                    DealAttack(enemyBouncer);
                }
            }
        }
    }

    public float GetAttackDamage(float power)
    {
        float attackDamage = Mathf.Clamp(power, 0f, 1f);
        return Mathf.RoundToInt((attackDamage) * gameController.gameSettings.maxAttackPower);
    }

    private void DealAttack(Bouncer enemy)
    {
        //Check if the attack is parried or lands
        //Parried
        if (enemy.attacking && Vector3.Angle(enemy.gameObject.transform.forward, transform.position - enemy.gameObject.transform.position) <= gameController.gameSettings.attackAngle)
        {
            //IF statement to ensure only a single bouncer plays the parry effects
            if(currentAttackPower > enemy.currentAttackPower)
            {
                //Play parry effects
                Debug.Log("Parried");
                audioSource.PlayOneShot(gameController.gameSettings.parrySound);
                Vector3 connectionPoint = (transform.position + enemy.gameObject.transform.position) / 2f;
                connectionPoint.y += gameController.gameSettings.boundsRadius;
                Instantiate(gameController.gameSettings.parryParticlesPrefab, connectionPoint, gameController.gameSettings.parryParticlesPrefab.transform.rotation);
            }
        }
        //Lands
        else
        {
            enemy.ReceiveAttack(GetAttackDamage(currentAttackPower));
            Debug.Log(gameObject.name + " landed a hit on " + enemy.gameObject.name);
        }
    }

    public virtual void ReceiveAttack(float attackDamage)
    {
        //Play hurt noise, particle effects, etc. here
        if (TakeDamageSequencer != null) StopCoroutine(TakeDamageSequencer);
        TakeDamageSequencer = TakeDamageSequence();
        StartCoroutine(TakeDamageSequencer);

        //Apply damage
        health.AdjustValue(-attackDamage*gameController.gameDifficulty.wolfAttackDamageMultiplier);
        if (health.currentValue == 0f && alive) Die();
    }

    public IEnumerator TakeDamageSequence()
    {
        //Play hurt noise
        audioSource.PlayOneShot(takeDamageSound);

        //Add the classic white out effect when a bouncer is attacked
        Renderer[] rends = characterModel.GetComponentsInChildren<Renderer>();
        foreach(Renderer rend in rends)
        {
            rend.material.SetFloat("Boolean_8B593A8", 1);
        }
        yield return new WaitForSeconds(0.3f);
        foreach (Renderer rend in rends)
        {
            rend.material.SetFloat("Boolean_8B593A8", 0);
        }
        for(int i = 0; i < 3; i++)
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
    }

    public virtual void Die()
    {
        //play death stuff
        alive = false;
        health.allowRegen = false;
        stamina.allowRegen = false;
        gameController.GameOver(false);
        //Explode
        Instantiate(gameController.gameSettings.explosionParticlesPrefab, transform.position, transform.rotation);
        gameController.universalAudioSource.PlayOneShot(deathSound);

        Destroy(this.gameObject);
    }

    public void PowerUp(float healthBuff, float staminaBuff, Vector3 powerUpperPosition)
    {
        //Apply powerup buffs
        health.maxValue += healthBuff;
        health.currentValue = health.maxValue;
        stamina.maxValue += staminaBuff;
        stamina.currentValue = stamina.maxValue;

        //Spawn power up particle
        Vector3 connectionPoint = (transform.position + powerUpperPosition) / 2f;
        connectionPoint.y += GameController.gameController.gameSettings.boundsRadius;
        Instantiate(GameController.gameController.gameSettings.powerUpParticlePrefab, connectionPoint, GameController.gameController.gameSettings.powerUpParticlePrefab.transform.rotation);

        //Start Power Up Sequence
        StartCoroutine(PlayPowerUp());
    }

    IEnumerator PlayPowerUp()
    {
        yield return new WaitForSeconds(0.3f);
        //Play power up noise 
        GameController.gameController.universalAudioSource.PlayOneShot(GameController.gameController.gameSettings.powerUpSound);
        Renderer[] rends = characterModel.GetComponentsInChildren<Renderer>();
        for (int i = 0; i < 3; i++)
        {
            yield return new WaitForSeconds(0.1f);
            foreach (Renderer rend in rends)
            {
                rend.material.SetFloat("PoweredUp", 1);
            }
            yield return new WaitForSeconds(0.1f);
            foreach (Renderer rend in rends)
            {
                rend.material.SetFloat("PoweredUp", 0);
            }
        }

    }
}
