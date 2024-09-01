using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealingOrb : MonoBehaviour, ICollectible
{
    [Header("Healing Properties")]
    public float healAmount = 15.0f;
           

    public void Collect(Player collector)
    {        
        collector.health += healAmount;
        Destroy(this.gameObject);
    }
}
    
