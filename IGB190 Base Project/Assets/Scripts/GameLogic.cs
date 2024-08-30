using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLogic : MonoBehaviour
{
    public int monsterKillObjective = 30;
    public int totalMonstersKilled = 0;

    public TMP_Text objectiveDisplay;
    public TMP_Text objectiveCounter;
    
    public MonsterSpawner[] spawner;


    // Start is called before the first frame update
    void Start()
    {
        spawner = FindObjectsOfType<MonsterSpawner>();

        objectiveDisplay.text = $"- Kill {monsterKillObjective} Monsters";
    }

    // Update is called once per frame
    void Update()
    {
        if (totalMonstersKilled >= monsterKillObjective)
        {
            // Game WON code here
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            return;
        }

        objectiveCounter.text = $"{totalMonstersKilled} / {monsterKillObjective}";

        totalMonstersKilled = CheckTotalKills();
    }

    int CheckTotalKills()
    {
        int totalKills = 0;

        foreach (MonsterSpawner spawner in spawner)
        {
            totalKills += spawner.monstersKilled;
        }

        return totalKills;
    }
}
