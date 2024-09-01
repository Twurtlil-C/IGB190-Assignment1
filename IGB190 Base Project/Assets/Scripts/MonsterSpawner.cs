using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    [Header("Main Variables")]
    public bool isActive = true;

    public float timeBetweenSpawns = 2.0f;
    public float spawnRadius = 10.0f;
    public Monster[] monstersToSpawn;
    public Monster monsterToSpawn;
    public GameObject monsterSpawnEffect;
    [HideInInspector] public float nextSpawnAt;
    [HideInInspector] public int skeletonCount;
    public int maxSpawnCount = 5;

    [Header("Destruction Variables")]
    public int destroyAfter = 10;
    public int monstersKilled = 0;
    public GameObject destroyEffect;

    private GameLogic gameLogic;

    private void Start()
    {
        gameLogic = FindObjectOfType<GameLogic>();
    }
    // Update is called once per frame
    void Update()
    {
        if (monstersKilled >= destroyAfter || gameLogic.gameWon)
        {
            if (destroyEffect != null)
            {
                GameObject obeliskDestroyEffect = Instantiate(destroyEffect, transform.position, Quaternion.identity);
                Destroy(obeliskDestroyEffect, 5.0f);
            }
            Destroy(this.gameObject);
        }

        // Cap spawns when max reached or when player wins
        if (skeletonCount >= maxSpawnCount) return;


        if (isActive && monsterToSpawn != null && Time.time > nextSpawnAt)
        {
            // Calculate the correct spawn location (given the set spawn radius)
            Vector3 spawnPosition = transform.position + Random.insideUnitSphere * spawnRadius;
            spawnPosition.y = transform.position.y;

            // Calculate when the next monster should be spawned
            nextSpawnAt = Time.time + timeBetweenSpawns;

            // Pick which monster to spawn (randomised)
            monsterToSpawn = monstersToSpawn[Random.Range(0, monstersToSpawn.Length)];
            
            // Spawn the monster at the correct spawn location
            GameObject monster = Instantiate(monsterToSpawn.gameObject, spawnPosition, transform.rotation);
            monster.GetComponent<Monster>().spawner = this;

            skeletonCount++;

            // If a spawn effect has been assigned, spawn it
            if (monsterSpawnEffect != null)
            {
                GameObject spawnEffect = Instantiate(monsterSpawnEffect, spawnPosition, Quaternion.identity);
                Destroy(spawnEffect.gameObject, 5.0f);
            }
        }
    }
}
