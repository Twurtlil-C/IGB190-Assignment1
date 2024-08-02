using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    public float timeBetweenSpawns = 2.0f;
    public float spawnRadius = 10.0f;
    public Monster monsterToSpawn;
    public GameObject monsterSpawnEffect;
    public float nextSpawnAt;
        
    // Update is called once per frame
    void Update()
    {
        if (monsterToSpawn != null && Time.time > nextSpawnAt)
        {
            // Calculate the correct spanw location (given the set spawn radius)
            Vector3 spawnPosition = transform.position + Random.insideUnitSphere * spawnRadius;
            spawnPosition.y = transform.position.y;

            // Calculate when the next monster should be spawned
            nextSpawnAt = Time.time + timeBetweenSpawns;

            // Spawn the monster at the correct spawn location
            Instantiate(monsterToSpawn.gameObject, spawnPosition, transform.rotation);

            // If a spawn effect has been assigned, spawn it
            if (monsterSpawnEffect != null)
            {
                Instantiate(monsterSpawnEffect, spawnPosition, Quaternion.identity);
            }
        }
    }
}
