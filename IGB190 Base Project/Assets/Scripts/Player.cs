using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Player : MonoBehaviour
{
    // Player Stats
    public float health = 500f;
    public float maxHealth = 500f;
    public float movementSpeed = 3.5f;
    public float attacksPerSecond = 1.5f;
    public float attackRange = 2.0f;
    public float attackDamage = 10.0f;

    // Cache references to important components for easy access later
    private NavMeshAgent agentNavigation;
    private Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        agentNavigation = gameObject.GetComponent<NavMeshAgent>();
        animator = gameObject.GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateMovement();
    }

    private void UpdateMovement()
    {
        animator.SetFloat("Speed", agentNavigation.velocity.magnitude);
        if (Input.GetMouseButton(0))
        {
            agentNavigation.SetDestination(Utilities.GetMouseWorldPosition());
        }
    }
}
