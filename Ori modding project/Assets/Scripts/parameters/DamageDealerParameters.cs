using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class DamageDealerParameters : ElementParameter
{
    public float Damage;

    public damageType DamageType;

    void Update()
    {
        SetFloat("Damage", Damage);

        SetString("DamageType", Enum.GetName(typeof(damageType),DamageType));
    }

    public enum damageType
    {
        Water,
        Lava,
        Ice,
        Spikes,
        Laser,
        Projectile,
        Acid,
        SlugSpike,
        Explosion,
        Heat,
        Enemy,
        Crush,
        HitSurface,
        Drowning,
        Wind,
        Obstacle,
        Draining,
        Tar,
        Melee,
        DiggingDash,
        SwimmingDash,
        DebugDamage,
        InstantKill,
        DeadlyDarkness,
        Interaction
    }
}
