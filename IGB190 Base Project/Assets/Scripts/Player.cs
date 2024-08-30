using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour, IDamageable
{
    // Player Stats
    [Header("Player Stats")]
    public float health = 500f;
    public float maxHealth = 500f;

    public float movementSpeed = 3.5f;

    public float attacksPerSecond = 1.5f;
    public float attackRange = 2.0f;
    public float attackDamage = 10.0f;

    public bool isImmune = false;
    
    private float damageReduction = 0.0f;
    
    [HideInInspector] public bool isDead;

    // Player Buff Stats
    [Header("Buff Stats")]
    public float buffCooldown = 30.0f;
    public float buffDuration = 10.0f;
    public float buffedAttackMultiplyer = 2.0f;
    public float buffedDamageReduction = 0.2f;

    public AudioSource buffSFX;

    public bool isBuffed = false;
    private float attackDamageCache;
    private float buffedAttackDamage;
    private float buffDurationOver;

    // Player Dodge Stats
    [Header("Dodge Stats")]
    public float dodgeCooldown = 1.5f;
    public float dodgeStartup = 0.2f;
    public float dodgeSpeed = 7.0f;
    public float dodgeAcceleration = 15.0f;
    public float dodgeLength = 3.0f;
    public float dodgeDuration = 1.0f;

    public static float dodgeBufferTime = 0.1f;

    public AudioSource dodgeSFX;

    private float accelerationCache;

    // Visual Effects
    [Header("Visual Effects")]
    public GameObject slashEffect;
    public GameObject startBuffEffect;
    public GameObject buffEffect;
    public GameObject dodgeEffect;

    public float initialScreenSaturation = 40f;
    public float deathScreenSaturation = -90f;

    // Input Variables
    private float bufferedDodgeAt = -dodgeBufferTime;
    private bool hasPressedMove;
    private bool hasPressedBuff;
    private bool hasPressedAttack;

    // Variables to control when the unit can attack and move
    private float canCastAt;
    private float canMoveAt;
    [HideInInspector] public float canBuffAt;
    [HideInInspector] public float canDodgeAt;

    // Constants to prevent magic numbers in the code. Makes it easier to edit later
    private const float MOVEMENT_DELAY_AFTER_CASTING = 0.2f;
    private const float MOVEMENT_DELAY_AFTER_DODGING = 0.5f;
    private const float TURNING_SPEED = 10.0f;

    // Cache references to important components for easy access later
    private NavMeshAgent agentNavigation;
    private Animator animator;

    public PostProcessProfile postProcessing;
    private ColorGrading colorGrading;

    private RandomiseSFXPitch randomSFXPitch;

    // Variables to control ability casting
    private enum Ability { Cleave, Buff, Dodge, /* Add more abilities here */}
    private Ability? abilityBeingCast = null;
    private float finishAbilityCastAt;
    private Vector3 abilityTargetLocation;
    [Range(0.0f, 1.0f)] public float cleaveActivationPoint = 0.4f;

    // Start is called before the first frame update
    void Start()
    {
        agentNavigation = gameObject.GetComponent<NavMeshAgent>();
        animator = gameObject.GetComponentInChildren<Animator>();
        randomSFXPitch = gameObject.GetComponent <RandomiseSFXPitch>();

        // Cache initial player stats
        attackDamageCache = attackDamage;
        accelerationCache = agentNavigation.acceleration;

        postProcessing.TryGetSettings<ColorGrading>(out colorGrading);
        colorGrading.saturation.value = initialScreenSaturation;
    }

    // Update is called once per frame
    void Update()
    {
        if (isDead) return;        
        HandleInput();
        UpdateMovement();
        UpdateAbilityCasting();
        UpdateBuffState();
        
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Q)) bufferedDodgeAt = Time.time;
        
        if (Input.GetMouseButton(0)) hasPressedMove = true; else hasPressedMove = false;

        if (Input.GetKeyDown(KeyCode.W)) hasPressedBuff = true; else hasPressedBuff = false;
        
        if (Input.GetMouseButton(1)) hasPressedAttack = true; else hasPressedAttack = false;

    }

    private void UpdateMovement()
    {        
        if (Time.time > canMoveAt) isImmune = false;

        animator.SetFloat("Speed", agentNavigation.velocity.magnitude);
        if (hasPressedMove && Time.time > canMoveAt)
        {
            agentNavigation.speed = movementSpeed;
            agentNavigation.acceleration = accelerationCache;
            agentNavigation.SetDestination(Utilities.GetMouseWorldPosition());
        }
    }

    // Handle all update logic associated with ability casting
    private void UpdateAbilityCasting()
    {
        // If the right click button is held and the player can cast, start a basic attack cast
        if (hasPressedAttack && Time.time > canCastAt)
        {
            StartCastingCleave();
        }

        if (hasPressedBuff && Time.time > canBuffAt)
        {
            StartBuff();
        }

        if (Time.time < bufferedDodgeAt + dodgeBufferTime && Time.time > canDodgeAt)
        {
            StartDodge();
        }

        // If the current ability has reached the end of its cast, run the appropriate actions for the ability
        if (abilityBeingCast != null && Time.time > finishAbilityCastAt)
        {
            switch (abilityBeingCast)
            {
                case Ability.Cleave:
                    FinishCastingCleave();
                    break;

                case Ability.Buff:
                    FinishBuff();
                    break;

                case Ability.Dodge:
                    FinishDodge();
                    break;

                // Add additional cases for other abilities
            }
        }

        // If a cast is in progress, have the player face towards the target location
        if (abilityBeingCast != null)
        {
            Quaternion look = Quaternion.LookRotation((abilityTargetLocation - transform.position).normalized);
            transform.rotation = Quaternion.Lerp(transform.rotation, look, Time.deltaTime * TURNING_SPEED);
        }
    }

    private void UpdateBuffState()
    {
        if (isBuffed)
        {            
            attackDamage = buffedAttackDamage;
            damageReduction = buffedDamageReduction;
        }
        else
        {
            // Reset attack damage value to previous
            attackDamage = attackDamageCache;
            damageReduction = 0.0f;
        }

        if (Time.time > buffDurationOver) isBuffed = false;
    }

    private void StartCastingCleave()
    {
        // Stop the character from moving while they attack
        agentNavigation.SetDestination(transform.position);

        // Set the ability being cast to the cleave ability
        abilityBeingCast = Ability.Cleave;

        // Play the appropriate ability animation at the correct speed
        animator.CrossFadeInFixedTime("Attack", 0.2f);
        animator.SetFloat("AttackSpeed", attacksPerSecond);

        // Calculate when the ability will finish casting, and when the player can next cast and move
        float castTime = (1.0f / attacksPerSecond);
        canCastAt = Time.time + castTime;
        finishAbilityCastAt = Time.time + cleaveActivationPoint * castTime;
        canMoveAt = finishAbilityCastAt + MOVEMENT_DELAY_AFTER_CASTING;
        abilityTargetLocation = Utilities.GetMouseWorldPosition();
    }

    // Perform all logic for when the player *finishes* casting the cleave ability
    private void FinishCastingCleave()
    {
        // Clear the ability currently being cast
        abilityBeingCast = null;

        // Create the slash visual and destroy it after it plays
        if (slashEffect != null)
        {
            GameObject slashVisual = Instantiate(slashEffect, transform.position, transform.rotation);
            Destroy(slashVisual, 1.0f);
        }

        // Find all the targets that should be hit by the attack and damage them
        Vector3 hitPoint = transform.position + transform.forward * attackRange;
        List<Monster> targets = Utilities.GetAllWithinRange<Monster>(hitPoint, attackRange);
        foreach (Monster target in targets )
        {
            target.TakeDamage(attackDamage);
        }

    }

    private void StartBuff()
    {
        // Stop character from moving any further
        agentNavigation.SetDestination(transform.position);
                
        abilityBeingCast = Ability.Buff;

        // Play buff animation
        animator.Play("Buff");
                
        canBuffAt = Time.time + buffCooldown;
        buffDurationOver = Time.time + buffDuration;
    }

    private void FinishBuff()
    {
        abilityBeingCast = null;


        // Visual effects for buff
        if (startBuffEffect != null)
        {
            GameObject startBuffVisual = Instantiate(startBuffEffect, transform.position, transform.rotation);
            Destroy(startBuffVisual, 1.0f);
        }

        if (buffEffect != null)
        {
            GameObject buffVisual = Instantiate(buffEffect, transform.position, transform.rotation, transform);
            Destroy(buffVisual, buffDuration);
        }

        if (buffSFX != null) buffSFX.Play();
        
        // Strengthen player while buffed
        buffedAttackDamage = attackDamage * buffedAttackMultiplyer;

        // Activate buffed state
        isBuffed = true;
    }

    private void StartDodge()
    {
        agentNavigation.SetDestination(transform.position);

        abilityBeingCast = Ability.Dodge;

        if (randomSFXPitch != null) randomSFXPitch.RandomisePitch();
        if (dodgeSFX != null) dodgeSFX.Play();

        float castTime = dodgeDuration;
        canDodgeAt = Time.time + dodgeCooldown;
        finishAbilityCastAt = Time.time + dodgeStartup * castTime;
        canMoveAt = finishAbilityCastAt + dodgeDuration;
        abilityTargetLocation = Utilities.GetMouseWorldPosition();
    }

    private void FinishDodge()
    {
        abilityBeingCast = null;

        if (dodgeEffect != null)
        {
            GameObject dodgeVisual = Instantiate(dodgeEffect, transform.position, transform.rotation, transform);
            
            Destroy(dodgeVisual, dodgeLength);
        }

        agentNavigation.speed = dodgeSpeed;
        agentNavigation.acceleration = dodgeAcceleration;
        isImmune = true;
        
        Vector3 dodgeLocation = transform.position + (abilityTargetLocation - transform.position).normalized * dodgeLength;
        agentNavigation.SetDestination(dodgeLocation);
    }


    // IDamageable Methods

    // Remove the specified amount of health from this unit, killing it if needed
    public virtual void TakeDamage(float amount)
    {
        if (isImmune) return;

        health -= amount * (1 - damageReduction);
        if (health <= 0)
            Kill();
    }

    // Destroy the player, but briefly keeping the corpse visible to play the death animation
    public virtual void Kill()
    {
        isDead = true;
        agentNavigation.SetDestination(transform.position);
        animator.SetTrigger("Die");
        colorGrading.saturation.value = deathScreenSaturation;
        StartCoroutine(RestartLevel());
    }

    // Returns the current health percent of the character (a value between 0.0 and 1.0)
    public float GetCurrentHealthPercent()
    {
        return health / maxHealth;
    }

    // Handles restarting the level when the player dies
    public IEnumerator RestartLevel()
    {
        yield return new WaitForSeconds(5.0f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
