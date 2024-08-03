using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class HornBugParameters : ElementParameter
{
    [InfoBox("if any of these values are -1 they will be replaced with the default")]

    public hornBugType HornBugType;

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

    [Foldout("Editor Sprites")]
    public SpriteRenderer HornBugSpriteRenderer;
    [Foldout("Editor Sprites")]
    public Sprite HornbugShelledSprite;
    [Foldout("Editor Sprites")]
    public Sprite HornbugNonShelledSprite;

    void Update()
    {
        SetString("HornBugType", Enum.GetName(typeof(hornBugType), HornBugType));

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

        HandleEditorSprites();
    }

    void HandleEditorSprites()
    {
        switch (HornBugType)
        {
            case hornBugType.Shelled:
                HornBugSpriteRenderer.sprite = HornbugShelledSprite;
                break;

            case hornBugType.NonShelled:
                HornBugSpriteRenderer.sprite = HornbugNonShelledSprite;
                break;
        }
    }

    public enum hornBugType
    {
        Shelled,
        NonShelled
    }
}
