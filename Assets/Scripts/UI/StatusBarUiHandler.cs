using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(TrackObject))]
public class StatusBarUiHandler : MonoBehaviour
{
    [SerializeField] public Bouncer statusTarget;
    [SerializeField] protected Vector2 healthBarFillLimits = new Vector2(0.15f, 0.85f);
    [SerializeField] protected Vector2 staminaBarFillLimits = new Vector2(0.15f, 0.85f);
    [SerializeField] Image healthFill;
    [SerializeField] Image healthTrail;
    [SerializeField] Image staminaFill;
    [SerializeField] Image staminaTrail;

    private void Start()
    {
        GetComponent<TrackObject>().worldObjectToTrack = statusTarget.characterModel;
    }

    void Update()
    {
        if (statusTarget == null) Destroy(this.gameObject);
        healthFill.fillAmount = GetFillAmount(healthBarFillLimits, statusTarget.health, statusTarget.health.currentValue);
        healthTrail.fillAmount = GetFillAmount(healthBarFillLimits, statusTarget.health, statusTarget.health.interpolatedValue);
        staminaFill.fillAmount = GetFillAmount(staminaBarFillLimits, statusTarget.stamina, statusTarget.stamina.currentValue);
        staminaTrail.fillAmount = GetFillAmount(staminaBarFillLimits, statusTarget.stamina, statusTarget.stamina.interpolatedValue);
    }

    private float GetFillAmount(Vector2 limits, Status status, float valueToCheck)
    {
        float percentFilled = valueToCheck / (status.maxValue - status.minValue);
        return limits.x + percentFilled * Mathf.Abs(limits.y - limits.x);
    }
}
