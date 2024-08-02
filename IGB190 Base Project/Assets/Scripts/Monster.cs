using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Monster : MonoBehaviour
{
    // Monster Stats
    public float health = 40f;
    public float maxHealth = 40f;
    public float movementSpeed = 1.0f;
    public float attacksPerSecond = 1.0f;
    public float attackRange = 2.0f;
    public float attackDamage = 10.0f;

    private NavMeshAgent agentNavigation;
    private Animator animator;
    private Player player;

    // Start is called before the first frame update
    void Start()
    {
        agentNavigation = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        player = GameObject.FindObjectOfType<Player>();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateMovement();
    }

    private void UpdateMovement()
    {
        animator.SetFloat("Speed", agentNavigation.velocity.magnitude);
        agentNavigation.SetDestination(player.transform.position);
    }
}
