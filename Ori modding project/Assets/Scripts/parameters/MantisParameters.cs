using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static DamageDealerParameters;

[ExecuteInEditMode]
public class MantisParameters : ElementParameter
{
    [InfoBox("if any of these values are -1 they will be replaced with the default")]

    public mantisType MantisType;

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
    [ShowIf(EConditionOperator.And,"ShouldSpawnLoot", "SpawnsExpOrbs")]
    public int ExpOrbsNumber;



    [Foldout("Editor Sprites")]
    public SpriteRenderer MantisSpriteRenderer;
    [Foldout("Editor Sprites")]
    public Sprite BaseMantisSprite;
    [Foldout("Editor Sprites")]
    public Sprite GreenMantisSprite;
    [Foldout("Editor Sprites")]
    public Sprite ElectricMantisSprite;

    void Update()
    {
        SetString("MantisType", Enum.GetName(typeof(mantisType), MantisType));

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
        switch (MantisType)
        {
            case mantisType.Base:
                MantisSpriteRenderer.sprite = BaseMantisSprite;
                break;

            case mantisType.Green:
                MantisSpriteRenderer.sprite = GreenMantisSprite;
                break;

            case mantisType.Electric:
                MantisSpriteRenderer.sprite = ElectricMantisSprite;
                break;
        }
    }

    public enum mantisType
    {
        Base,
        Green,
        Electric
    }
}
