using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
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
    [HideInInspector] public bool isDead;

    // Player Buff Stats
    [Header("Buff Stats")]
    public float buffCooldown = 30.0f;
    public float buffDuration = 10.0f;
    public float buffedAttackMultiplyer = 2.0f;

    private bool isBuffed = false;
    private float attackDamageCache;
    private float buffedAttackDamage;
    private float buffDurationOver;

    // Visual Effects
    [Header("Visual Effects")]
    public GameObject slashEffect;
    public GameObject startBuffEffect;
    public GameObject buffEffect;

    // Variables to control when the unit can attack and move
    private float canCastAt;
    private float canMoveAt;
    private float canBuffAt;

    // Constants to prevent magic numbers in the code. Makes it easier to edit later
    private const float MOVEMENT_DELAY_AFTER_CASTING = 0.2f;
    private const float TURNING_SPEED = 10.0f;

    // Cache references to important components for easy access later
    private NavMeshAgent agentNavigation;
    private Animator animator;

    // Variables to control ability casting
    private enum Ability { Cleave, Buff, /* Add more abilities here */}
    private Ability? abilityBeingCast = null;
    private float finishAbilityCastAt;
    private Vector3 abilityTargetLocation;
    [Range(0.0f, 1.0f)] public float cleaveActivationPoint = 0.4f;

    // Start is called before the first frame update
    void Start()
    {
        agentNavigation = gameObject.GetComponent<NavMeshAgent>();
        animator = gameObject.GetComponentInChildren<Animator>();
        // Save initial attack damage for buff
        attackDamageCache = attackDamage;

    }

    // Update is called once per frame
    void Update()
    {
        if (isDead) return;
        UpdateMovement();
        UpdateAbilityCasting();
        UpdateBuffState();
    }

    private void UpdateMovement()
    {
        animator.SetFloat("Speed", agentNavigation.velocity.magnitude);
        if (Input.GetMouseButton(0) && Time.time > canMoveAt)
        {
            agentNavigation.SetDestination(Utilities.GetMouseWorldPosition());
        }
    }

    // Handle all update logic associated with ability casting
    private void UpdateAbilityCasting()
    {
        // If the right click button is held and the player can cast, start a basic attack cast
        if (Input.GetMouseButton(1) && Time.time > canCastAt)
        {
            StartCastingCleave();
        }

        if (Input.GetKeyDown(KeyCode.Q) && Time.time > canBuffAt)
        {
            StartBuff();
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
        }
        else
        {
            // Reset attack damage value to previous
            attackDamage = attackDamageCache;   
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

        
        buffedAttackDamage = attackDamage * buffedAttackMultiplyer;

        // Activate buffed state
        isBuffed = true;
    }

    // Remove the specified amount of health from this unit, killing it if needed
    public virtual void TakeDamage(float amount)
    {
        health -= amount;
        if (health <= 0)
            Kill();
    }

    // Destroy the player, but briefly keeping the corpse visible to play the death animation
    public virtual void Kill()
    {
        isDead = true;
        agentNavigation.SetDestination(transform.position);
        animator.SetTrigger("Die");
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
