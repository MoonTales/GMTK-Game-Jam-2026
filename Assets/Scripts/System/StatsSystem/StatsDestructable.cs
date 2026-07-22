using System;
using System.Collections.Generic;
using UnityEngine;

public class StatsDestructable : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] float HealthValue = 10f;
    [SerializeField] List<float> Values = new List<float>();
    void Start()
    {
        StatsInitializer.InitializeDestructible(gameObject, HealthValue, OnDeath);
    }

    protected virtual void OnDeath()
    {
        DebugUtils.Log("Destructible Object Destroyed");
        Destroy(gameObject);
    }

}
