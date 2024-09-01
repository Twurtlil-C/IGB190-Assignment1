using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameLogic : MonoBehaviour
{    
    [Header("Objectives")]
    public int monsterKillObjective = 30;
    public int totalMonstersKilled = 0;
        
    public TMP_Text objectiveDisplay;
    public TMP_Text objectiveCounter;

    public MonsterSpawner[] spawner;

    [Header("SceneFade")]
    public Image fadeImage;
    public float fadeTime = 1.0f;

    public bool gameWon = false;

    // Reference to persistent game music object for restarting when game restarts
    private GameObject gameMusic;
    private AudioSource gameMusicSource;

    // Start is called before the first frame update
    void Start()
    {
        // Reset game music on scene load
        gameMusic = GameObject.FindGameObjectWithTag("Music");
        gameMusicSource = gameMusic.GetComponent<AudioSource>();
        gameMusicSource.Play();

        spawner = FindObjectsOfType<MonsterSpawner>();

        // Fade out image initially
        fadeImage.CrossFadeAlpha(0, 0.0f, true);

        objectiveDisplay.text = $"- Kill {monsterKillObjective} Monsters";
    }

    // Update is called once per frame
    void Update()
    {

        if (totalMonstersKilled >= monsterKillObjective)
        {
            // Make sure objective shows correct monster kills displayed (kills may go over the total limit)
            objectiveCounter.text = $"{monsterKillObjective} / {monsterKillObjective}";
            // Game WON code here
            StartCoroutine(FadeToNextScene(SceneManager.GetActiveScene().buildIndex + 1, fadeTime));
            gameWon = true;
            return;
        }
        
        totalMonstersKilled = CheckTotalKills();

        objectiveCounter.text = $"{totalMonstersKilled} / {monsterKillObjective}";
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

    public IEnumerator FadeToNextScene(int sceneNum, float fadeTime)
    {
        // 2f instead of 1f for the alpha parameter ensures fade is full black before next scene loads
        fadeImage.CrossFadeAlpha(2.0f, fadeTime, true);
        yield return new WaitForSeconds(fadeTime); 
        SceneManager.LoadSceneAsync(sceneNum);
    }        
        
}
