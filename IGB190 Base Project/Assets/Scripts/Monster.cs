using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Monster : MonoBehaviour, IDamageable
{
    // Monster Stats
    [Header("Basic Stats")]
    public float health = 40f;
    public float maxHealth = 40f;
    public float movementSpeed = 1.0f;
    public float attacksPerSecond = 1.0f;
    public float attackRange = 2.0f;
    public float attackDamage = 10.0f;

    public bool isArcher = false;

    // Hit Properties
    [Header("Hit Properties")]
    public Material defaultMaterial;
    public Material hitMaterial;
    public float hitEffectDuration = 0.1f;

    // Reference to monster's mesh renderer
    private SkinnedMeshRenderer skinRenderer;

    // Specific child object containing mesh renderer component
    public GameObject meshObject;

    [Header("Loot Drops")]
    public GameObject[] drops;
    public float[] dropChances;
    public float increasedDropAtPlayerHealthPercent = 0.1f;
    public float dropChanceIncrease = 2.0f;

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
    [HideInInspector] public MonsterSpawner spawner;

    // Variables to control ability casting.
    private enum Ability { Slash, Shoot, /* Add more abilities in here! */ }
    private Ability? abilityBeingCast = null;
    private float finishAbilityCastAt;

    [Header("Ability Casting")]
    [Range(0.0f, 1.0f)] public float slashActivationPoint = 0.4f;
    [Range(0.0f, 1.0f)] public float shootActivationPoint = 0.8f;

    // Start is called before the first frame update
    void Start()
    {
        agentNavigation = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        player = GameObject.FindObjectOfType<Player>();

        // Specifically finds skinned mesh renderer in mesh object if specified (fixes skeleton archer material not changing initially)
        if (meshObject != null) skinRenderer = meshObject.GetComponentInChildren<SkinnedMeshRenderer>();        
        else skinRenderer = GetComponentInChildren<SkinnedMeshRenderer>();

        skinRenderer.material = defaultMaterial;

        //spawner = GameObject.FindObjectOfType<MonsterSpawner>();
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
            if (isArcher) StartCastingShoot(); else StartCastingSlash();

        // If the current ability has reached the end of its cast, run the appropriate actions for the ability
        if (abilityBeingCast != null && Time.time > finishAbilityCastAt)
            switch (abilityBeingCast)
            {
                case Ability.Slash:
                    FinishCastingSlash();
                    break;

                case Ability.Shoot:
                    FinishCastingShoot();
                    break;
            }

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
        finishAbilityCastAt = Time.time + slashActivationPoint * castTime;
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

    private void StartCastingShoot()
    {
        // Stop the character from moving while they attack
        agentNavigation.SetDestination(transform.position);

        // Set the ability being cast to the slash ability
        abilityBeingCast = Ability.Shoot;

        // Play the appropriate ability animation at the correct speed
        animator.CrossFadeInFixedTime("Attack", 0.2f);
        animator.SetFloat("AttackSpeed", attacksPerSecond);

        // Calculate when the ability will finish casting, and when the monster can next cast and move
        float castTime = (1.0f / attacksPerSecond);
        canCastAt = Time.time + castTime;
        finishAbilityCastAt = Time.time + shootActivationPoint * castTime;
        canMoveAt = finishAbilityCastAt + MOVEMENT_DELAY_AFTER_CASTING;
    }

    private void FinishCastingShoot()
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
        StartCoroutine(HitEffect());
        if (health <= 0)
            Kill();
    }

    public void Kill()
    {
        if (animator != null)
        {
            if (defaultMaterial != null) skinRenderer.material = defaultMaterial;

            animator.SetTrigger("Die");
            animator.transform.SetParent(null);
            Destroy(animator.gameObject, TIME_BEFORE_CORPSE_DESTROYED);
        }

        Drop();
        spawner.skeletonCount--;
        spawner.monstersKilled++;
        Destroy(gameObject);
    }

    public void Drop()
    {        
        if (drops == null || dropChances == null) return;
        // Make sure there is a corresponding spawn chance for each item drop
        if (drops.Length != dropChances.Length) return;

        float spawnChance = 0;
        for (int i = 0; i < drops.Length; i++)
        {
            // Increases drop rates if player health is below a certain threshold
            if (player.GetCurrentHealthPercent() < increasedDropAtPlayerHealthPercent)
                spawnChance = dropChances[i] * dropChanceIncrease;

            else spawnChance = dropChances[i];

            if (UnityEngine.Random.value <= spawnChance)
            {
                Instantiate(drops[i], transform.position, Quaternion.identity);
            }
        }
        
    }

    public float GetCurrentHealthPercent()
    {
        return health / maxHealth;
    }

    public IEnumerator HitEffect()
    {
        if (hitMaterial != null) skinRenderer.material = hitMaterial;
        yield return new WaitForSeconds(hitEffectDuration);
        skinRenderer.material = defaultMaterial;
    }
}
