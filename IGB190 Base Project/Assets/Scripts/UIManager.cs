using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    // Dodge UI Elements
    [Header("Dodge UI")]
    public Image dodgeImage;
    public TMP_Text dodgeTimer;
    public TMP_Text dodgeKey;

    // Buff UI Elements
    [Header("Buff UI")]
    public Image buffImage;
    public Image activeBuffImage;
    public TMP_Text buffTimer;
    public TMP_Text buffKey;

    // Reference to player
    private Player player;

    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<Player>();
    }

    // Update is called once per frame
    void Update()
    {
        // Make sure player exists before running following code
        if (player == null) return;

        // Handle Buff UI
        if (player.canBuffAt > Time.time)
        {
            buffImage.enabled = false;
            buffTimer.enabled = false;

            // Show buff is currently active in UI
            if (player.isBuffed) activeBuffImage.enabled = true;
            else
            {
                // Start showing cooldown timer when buff is finished
                buffTimer.enabled = true;
                activeBuffImage.enabled = false;

                // Buff Cooldown timer in UI
                buffTimer.text = Mathf.RoundToInt(player.canBuffAt - Time.time).ToString();
            }
        }
        else
        {
            buffImage.enabled = true;
            buffTimer.enabled = false;
            activeBuffImage.enabled = false;
        }

        // Handle Dodge UI
        if (player.canDodgeAt > Time.time)
        {
            dodgeImage.enabled = false;
            dodgeTimer.enabled = true;

            // Dodge cooldown timer in UI
            dodgeTimer.text = Mathf.CeilToInt(player.canDodgeAt - Time.time).ToString();
        }
        else
        {
            dodgeImage.enabled = true;
            dodgeTimer.enabled = false;
        }
    }

}
