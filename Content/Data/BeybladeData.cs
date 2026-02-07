using System;

namespace Gearstorm.Content.Data
{
    public struct BeybladeStats
    {
        public float DamageBase;
        public float KnockbackPower;
        public float KnockbackResistance;
        public float Radius;
        public float Height;
        public float BaseSpinSpeed;
        public float SpinDecay;
        public float CritChance;
        public float CritMultiplier;
        public float Mass;
        public float Balance;
        public float Density;
        public float TipFriction;
        public float MoveSpeed;
        public float MomentOfInertia;

        public BeybladeStats(
            float damageBase = 0f,
            float knockbackPower = 0f,
            float knockbackResistance = 0f,
            float radius = 0f,
            float height = 0f,
            float baseSpinSpeed = 0f,
            float spinDecay = 0f,
            float mass = 0f,
            float balance = 0f,
            float density = 0f,
            float tipFriction = 0f,
            float moveSpeed = 0f,
            float momentOfInertia = 0f,
            float critChance = 0f, 
            float critMultiplier = 2f) // E ESTE
        {
            DamageBase = damageBase;
            KnockbackPower = knockbackPower;
            KnockbackResistance = knockbackResistance;
            Radius = radius;
            Height = height;
            BaseSpinSpeed = baseSpinSpeed;
            SpinDecay = spinDecay;
            Mass = mass;
            Balance = balance;
            Density = density;
            TipFriction = tipFriction;
            MoveSpeed = moveSpeed;
            MomentOfInertia = momentOfInertia;
            CritChance = critChance; 
            CritMultiplier = critMultiplier; 
        }

        public enum BeybladePartType
        {
            Base,
            Blade,
            Top
        }

        public interface IHasBeybladeStats
        {
            BeybladeStats Stats { get; }
            BeybladePartType PartType { get; }
        }

public static BeybladeStats CombineStats(
    IHasBeybladeStats basePart,
    IHasBeybladeStats bladePart,
    IHasBeybladeStats topPart)
{
    var b = basePart.Stats;
    var l = bladePart.Stats;
    var t = topPart.Stats;

    // =========================
    // MASS / GEOMETRY
    // =========================
    float totalMass = b.Mass + l.Mass + t.Mass;
    float finalRadius = MathF.Max(b.Radius, MathF.Max(l.Radius, t.Radius));

    // =========================
    // SPIN
    // =========================
    float finalBaseSpin =
        (b.BaseSpinSpeed + l.BaseSpinSpeed)
        * MathF.Max(0.1f, t.Balance);

    // =========================
// CRITICAL SYSTEM (SPIN-BASED)
// =========================

/*
    TOOLTIP MATH:
    - Crit Chance = SpinSpeed × 20%
    - 5.0 Spin = 100% crit
    - Above 5.0 Spin:
        +40% Crit Damage per extra Spin
    - Base crit multiplier = 2.0
*/

    float spin = l.BaseSpinSpeed;

// Crit chance: 20% por 1.0 Spin
    float rawCritChance = spin * 0.20f;

// Cap em 100%
    float critChance = MathF.Min(rawCritChance, 1f);

// Over-Spinning começa após 5.0 Spin
    float overflowSpin = MathF.Max(0f, spin - 5f);

// Crit multiplier base + overflow
    float critMultiplier = 2f + overflowSpin * 0.40f;
    

    // =========================
    // FINAL STATS
    // =========================
    return new BeybladeStats(
        damageBase:
            l.DamageBase + t.DamageBase * 0.2f,

        knockbackPower:
            l.KnockbackPower + totalMass * 0.5f,

        knockbackResistance:
            b.KnockbackResistance
            + t.KnockbackResistance
            + totalMass * 0.2f,

        radius: finalRadius,

        height:
            b.Height + t.Height,

        baseSpinSpeed:
            finalBaseSpin,

        spinDecay:
            l.SpinDecay * (1.1f - t.Balance),

        mass:
            totalMass,

        balance:
            t.Balance,

        density:
            (b.Density + l.Density + t.Density) / 3f,

        tipFriction:
            b.TipFriction,

        moveSpeed:
            b.MoveSpeed,

        momentOfInertia:
            totalMass * finalRadius,

        critChance:
            critChance,

        critMultiplier:
            critMultiplier
    );
}

    }
}