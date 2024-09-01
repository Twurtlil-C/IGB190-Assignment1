using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomiseSFXPitch : MonoBehaviour
{
    public AudioSource audioSFX;
    public float defaultPitch = 1.0f;
    [Range(-3f, 3f)] public float minPitch;
    [Range(-3f, 3f)] public float maxPitch;

    // Applied to instantiating objects
    private void Awake()
    {
        if (audioSFX != null) audioSFX.pitch = Random.Range(minPitch, maxPitch);
    }

    // Manually call from other scripts
    public void RandomisePitch()
    {
        if (audioSFX != null) audioSFX.pitch = Random.Range(minPitch, maxPitch);
    }

    public void ResetPitch()
    {
        if (audioSFX != null) audioSFX.pitch = defaultPitch;
    }
}
