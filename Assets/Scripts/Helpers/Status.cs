using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Status
{
    public float maxValue;
    public float minValue;
    public float currentValue;
    public float interpolatedValue;
    public float regenRate; //Applied per second to currentValue

    public bool allowRegen;
    public bool allowChange;

    protected float interpolateSpeed = 4f;

    protected IEnumerator RegenTicker;
    protected IEnumerator Interpolator;

    protected Bouncer bouncer;

    public Status(Bouncer _bouncer)
    {
        bouncer = _bouncer;
        maxValue = 10f;
        minValue = 0f;
        currentValue = maxValue;
        interpolatedValue = currentValue;
        regenRate = 0f;
        allowRegen = false;

        StartRegen();
    }

    public Status(Bouncer _bouncer, float _maxValue, float _regenRate)
    {
        bouncer = _bouncer;
        maxValue = _maxValue;
        minValue = 0f;
        currentValue = maxValue;
        interpolatedValue = currentValue;
        regenRate = _regenRate;
        if (regenRate != 0) allowRegen = true;

        StartRegen();
    }

    public Status(Bouncer _bouncer, float _maxValue, float _minValue, float _regenRate, bool _allowRegen, bool _allowChange)
    {
        bouncer = _bouncer;
        maxValue = _maxValue;
        minValue = _minValue;
        currentValue = maxValue;
        interpolatedValue = currentValue;
        regenRate = _regenRate;
        allowRegen = _allowRegen;
        allowChange = _allowChange;
        
        StartRegen();
    }

    public void StartRegen()
    {
        RegenTicker = RegenTick();
        bouncer.StartCoroutine(RegenTicker);
    }


    public void AdjustValue(float amount)
    {
        if (allowChange)
        {
            currentValue = Mathf.Clamp(currentValue + amount, minValue, maxValue);
            InterValue();
        }
    }

    public void SetValue(float amount)
    {
        if (allowChange)
        {
            currentValue = Mathf.Clamp(amount, minValue, maxValue);
            InterValue();
        }
    }


    private void InterValue()
    {
        if (Interpolator != null) bouncer.StopCoroutine(Interpolator);
        Interpolator = InterpolateValue();
        bouncer.StartCoroutine(Interpolator);
    }

    IEnumerator InterpolateValue()
    {
        while (true)
        {
            interpolatedValue = Mathf.Lerp(interpolatedValue, currentValue, Time.deltaTime * interpolateSpeed);
            interpolatedValue = Mathf.Clamp(interpolatedValue, minValue, maxValue);

            if (Mathf.Abs(interpolatedValue - currentValue) < 0.25f)
            {
                interpolatedValue = currentValue;
                break;
            }
            yield return null;
        }
    }

    IEnumerator RegenTick()
    {
        while (true)
        {
            if(allowRegen && regenRate != 0f)
            {
                AdjustValue(regenRate*0.1f*GameController.gameController.gameSpeedSettings.statusRegenMultiplier);
            }
            yield return new WaitForSeconds(0.1f);
        }
    }
}