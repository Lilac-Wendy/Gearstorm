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
        public float CritChance { get; set; } = 0f;
        public float CritMultiplier { get; set; } = 2f;
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
            float critChance = 0f, // ADICIONE ESTE
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
            CritChance = critChance; // INICIALIZE
            CritMultiplier = critMultiplier; // INICIALIZE
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

            float finalBaseSpin = (b.BaseSpinSpeed + l.BaseSpinSpeed) * t.Balance;

            float totalMass = b.Mass + l.Mass + t.Mass;
            float finalRadius = MathF.Max(b.Radius, MathF.Max(l.Radius, t.Radius));

            // CALCULA CHANCE CRÍTICA: Blade fornece base, Top melhora
            float critChance = l.CritChance * (1f + t.Balance * 0.5f);

            // CALCULA MULTIPLICADOR: Base fornece base, Blade e Top melhoram
            float critMultiplier = 2f + (t.CritMultiplier - 2f) * 0.3f;

            return new BeybladeStats(
                damageBase: l.DamageBase + (t.DamageBase * 0.2f),
                knockbackPower: l.KnockbackPower + (totalMass * 0.5f),
                knockbackResistance: b.KnockbackResistance + t.KnockbackResistance + (totalMass * 0.2f),
                radius: finalRadius,
                height: b.Height + t.Height,
                baseSpinSpeed: finalBaseSpin,
                spinDecay: l.SpinDecay * (1.1f - t.Balance),
                mass: totalMass,
                balance: t.Balance,
                density: (b.Density + l.Density + t.Density) / 3f,
                tipFriction: b.TipFriction,
                moveSpeed: b.MoveSpeed,
                momentOfInertia: totalMass * finalRadius,
                critChance: critChance, // ADICIONADO
                critMultiplier: critMultiplier // ADICIONADO
            );
        }
    }
}