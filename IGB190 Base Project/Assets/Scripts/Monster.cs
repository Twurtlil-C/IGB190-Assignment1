using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Monster : MonoBehaviour, IDamageable
{
    // Monster Stats
    public float health = 40f;
    public float maxHealth = 40f;
    public float movementSpeed = 1.0f;
    public float attacksPerSecond = 1.0f;
    public float attackRange = 2.0f;
    public float attackDamage = 10.0f;

    // Store a reference to the player for easy access
    private Player player;

    // Variables to control when the unit can attack and move.
    private float canCastAt;
    private float canMoveAt;

    // Constants to prevent magic numbers in the code. Makes it easier to edit later.
    private const float MOVEMENT_DELAY_AFTER_CASTING = 1.5f;
    private const float TURNING_SPEED = 10.0f;
    private const float TIME_BEFORE_CORPSE_DESTROYED = 5.0f;

    // Cache references to important components for easy access later
    private NavMeshAgent agentNavigation;
    private Animator animator;

    // Variables to control ability casting.
    private enum Ability { Slash, /* Add more abilities in here! */ }
    private Ability? abilityBeingCast = null;
    private float finishAbilityCastAt;

    // Start is called before the first frame update
    void Start()
    {
        agentNavigation = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        player = GameObject.FindObjectOfType<Player>();
        canMoveAt = Time.time + 1.0f;
        transform.LookAt(player.transform);
    }

    // Update is called once per frame
    void Update()
    {
        // If the player is dead, don't do anything
        if (player.isDead) return;
        UpdateMovement();
        UpdateAbilityCasting();
    }

    private void UpdateMovement()
    {
        animator.SetFloat("Speed", agentNavigation.velocity.magnitude);
        if (Time.time > canMoveAt)
            agentNavigation.SetDestination(player.transform.position);
    }

    // Handle all update logic associated with ability casting
    private void UpdateAbilityCasting()
    {
        // If the the player is within range, start a basic attack cast
        if (Vector3.Distance(transform.position, player.transform.position) < attackRange && Time.time > canCastAt)
            StartCastingSlash();

        // If the current ability has reached the end of its cast, run the appropriate actions for the ability
        if (abilityBeingCast != null && Time.time > finishAbilityCastAt)
            if (abilityBeingCast == Ability.Slash)
                FinishCastingSlash();

        // If a cast is in progress, face towards the player
        if (abilityBeingCast != null)
        {
            Quaternion look = Quaternion.LookRotation(player.transform.position - transform.position);
            transform.rotation = Quaternion.Lerp(transform.rotation, look, Time.deltaTime * TURNING_SPEED);
        }
    }

    private void StartCastingSlash()
    {
        // Stop the character from moving while they attack
        agentNavigation.SetDestination(transform.position);

        // Set the ability being cast to the slash ability
        abilityBeingCast = Ability.Slash;

        // Play the appropriate ability animation at the correct speed
        animator.CrossFadeInFixedTime("Attack", 0.2f);
        animator.SetFloat("AttackSpeed", attacksPerSecond);

        // Calculate when the ability will finish casting, and when the monster can next cast and move
        float castTime = (1.0f / attacksPerSecond);
        canCastAt = Time.time + castTime;
        finishAbilityCastAt = Time.time + 0.4f * castTime;
        canMoveAt = finishAbilityCastAt + MOVEMENT_DELAY_AFTER_CASTING;        
    }

    // Perform all logic for when the monster *finishes* casting the cleave ability
    private void FinishCastingSlash()
    {
        // Clear the ability currently being cast
        abilityBeingCast = null;

        // Find all the targets that should be hit by the attack and damage them
        Vector3 hitPoint = transform.position + transform.forward * attackRange;
        List<Player> targets = Utilities.GetAllWithinRange<Player>(hitPoint, attackRange);
        foreach (Player target in targets)
            target.TakeDamage(attackDamage);       

    }

    // Remove the specified amount of health from this unit, killing it if needed
    public void TakeDamage(float amount)
    {
        health -= amount;
        if (health <= 0)
            Kill();
    }

    public void Kill()
    {
        if (animator != null)
        {
            animator.SetTrigger("Die");
            animator.transform.SetParent(null);
            Destroy(animator.gameObject, TIME_BEFORE_CORPSE_DESTROYED);
        }
        Destroy(gameObject);
    }

    public float GetCurrentHealthPercent()
    {
        return health / maxHealth;
    }
}
