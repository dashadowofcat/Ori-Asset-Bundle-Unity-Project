using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SlimeParameters : ElementParameter
{
    [InfoBox("if any of these values are -1 they will be replaced with the default")]

    [Header("Spawner Settings")]
    public bool RespawnOnScreen;
    public float RespawnTime;

    [Header("Health")]
    public float MaxHealth;

    [Header("Sight")]
    public float MaxSensorRadius;
    public float LoseSightRadius;


    [Header("Loot")]
    public bool ShouldSpawnLoot;

    [ShowIf("ShouldSpawnLoot")]
    public int EnergyOrbsNumber;
    [ShowIf("ShouldSpawnLoot")]
    public int HealthOrbsNumber;

    [ShowIf("ShouldSpawnLoot")]
    public bool SpawnsExpOrbs;
    [ShowIf(EConditionOperator.And, "ShouldSpawnLoot", "SpawnsExpOrbs")]
    public int ExpOrbsNumber;

    void Update()
    {
        // health
        SetFloat("MaxHealth", MaxHealth);

        // sight
        SetFloat("MaxSensorRadius", MaxSensorRadius);
        SetFloat("LoseSightRadius", LoseSightRadius);

        // loot
        SetBool("ShouldSpawnLoot", ShouldSpawnLoot);

        SetInt("EnergyOrbsNumber", EnergyOrbsNumber);
        SetInt("HealthOrbsNumber", HealthOrbsNumber);

        SetBool("SpawnsExpOrbs", SpawnsExpOrbs);

        SetInt("ExpOrbNumber", ExpOrbsNumber);

        // spawner

        SetBool("RespawnOnScreen", RespawnOnScreen);
        SetFloat("RespawnTime", RespawnTime);
    }
}
