using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitUI : MonoBehaviour
{
    public RectTransform container;
    public Image healthBar;
    public Vector3 offset = new Vector3(0, 2, 0);
    private IDamageable trackedDamageable;
    private Transform trackedTransform;

    // Curved Health Bar
    public AnimationCurve curve;
    public bool useCurve = false;

    private void Awake()
    {
        trackedDamageable = GetComponentInParent<IDamageable>();
        trackedTransform = transform.parent;
    }

    private void LateUpdate()
    {
        // Move health bar to the correct screen position
        Vector3 world = trackedTransform.position + offset;
        container.anchoredPosition = Camera.main.WorldToScreenPoint(world);

        // Update amount of visible red health bar

        if (useCurve)
        {
            healthBar.fillAmount = curve.Evaluate(trackedDamageable.GetCurrentHealthPercent());
        }
        else
        {
            healthBar.fillAmount = trackedDamageable.GetCurrentHealthPercent();
        }
    }
}
