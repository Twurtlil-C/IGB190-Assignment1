using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour, IDamageable
{
    // Player Stats
    public float health = 500f;
    public float maxHealth = 500f;
    public float movementSpeed = 3.5f;
    public float attacksPerSecond = 1.5f;
    public float attackRange = 2.0f;
    public float attackDamage = 10.0f;
    [HideInInspector] public bool isDead;

    // Visual Effects
    public GameObject slashEffect;

    // Variables to control when the unit can attack and move
    private float canCastAt;
    private float canMoveAt;

    // Constants to prevent magic numbers in the code. Makes it easier to edit later
    private const float MOVEMENT_DELAY_AFTER_CASTING = 0.2f;
    private const float TURNING_SPEED = 10.0f;

    // Cache references to important components for easy access later
    private NavMeshAgent agentNavigation;
    private Animator animator;

    // Variables to control ability casting
    private enum Ability { Cleave, /* Add more abilities here */}
    private Ability? abilityBeingCast = null;
    private float finishAbilityCastAt;
    private Vector3 abilityTargetLocation;

    // Start is called before the first frame update
    void Start()
    {
        agentNavigation = gameObject.GetComponent<NavMeshAgent>();
        animator = gameObject.GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isDead) return;
        UpdateMovement();
        UpdateAbilityCasting();
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

        // If the current ability has reached the end of its cast, run the appropriate actions for the ability
        if (abilityBeingCast != null && Time.time > finishAbilityCastAt)
        {
            switch (abilityBeingCast)
            {
                case Ability.Cleave:
                    FinishCastingCleave();
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
        finishAbilityCastAt = Time.time + 0.4f * castTime;
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
