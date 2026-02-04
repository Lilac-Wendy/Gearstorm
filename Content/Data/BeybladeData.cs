using System;

namespace Gearstorm.Content.Data
{
    // ==========================================
    // 1. A ESTRUTURA DE DADOS (BeybladeStats)
    // ==========================================
    public struct BeybladeStats
    {
        public float DamageBase;
        public float KnockbackPower;
        public float KnockbackResistance;
        public float Radius;
        public float Height;

        // 🔥 IMPORTANTE
        public float BaseSpinSpeed;   // stat puro (UI / balance)
        public float SpinDecay;

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
            float momentOfInertia = 0f)
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
        }
    }


    // ==========================================
    // 2. O ENUM (Tipos de Peças)
    // ==========================================
    public enum BeybladePartType
    {
        Base,
        Blade,
        Top
    }

    // ==========================================
    // 3. A INTERFACE QUE ESTAVA FALTANDO
    // ==========================================
    // Todo item que for uma peça de Beyblade PRECISA implementar isso.
    public interface IHasBeybladeStats
    {
        BeybladeStats Stats { get; }
        BeybladePartType PartType { get; }
    }

    // ==========================================
    // 4. O COMBINADOR (Lógica Matemática)
    // ==========================================
    public static class BeybladeCombiner
    {
        public static BeybladeStats CombineStats(
            IHasBeybladeStats basePart,
            IHasBeybladeStats bladePart,
            IHasBeybladeStats topPart)
        {
            var baseStats = basePart.Stats;
            var bladeStats = bladePart.Stats;
            var topStats = topPart.Stats;
            float finalBaseSpinSpeed =
                baseStats.BaseSpinSpeed +
                bladeStats.BaseSpinSpeed;

            float balancedSpin =
                finalBaseSpinSpeed * topStats.Balance;

            // Massa total
            float totalMass =
                baseStats.Mass +
                bladeStats.Mass +
                topStats.Mass;

            // Dano: lâmina é a fonte principal
            float finalDamage =
                bladeStats.DamageBase +
                topStats.DamageBase * 0.2f;

            // Spin: SOMENTE A BASE, modulada pelo BALANCE do topo
            float combinedBaseSpin =
                baseStats.BaseSpinSpeed * topStats.Balance;

            // Raio: maior peça
            float finalRadius = MathF.Max(
                baseStats.Radius,
                MathF.Max(bladeStats.Radius, topStats.Radius)
            );

            // Inércia: massa × raio (modelo simples e honesto)
            float momentOfInertia =
                totalMass * finalRadius;

            return new BeybladeStats(
                damageBase: finalDamage,
                knockbackPower: bladeStats.KnockbackPower + (totalMass * 0.5f),
                knockbackResistance: baseStats.KnockbackResistance + topStats.KnockbackResistance + (totalMass * 0.2f),
                radius: finalRadius,
                height: baseStats.Height + topStats.Height,

                baseSpinSpeed: balancedSpin, // 🔥 AGORA EXISTE

                spinDecay: bladeStats.SpinDecay * topStats.Balance,
                mass: totalMass,
                balance: topStats.Balance,
                density: (baseStats.Density + bladeStats.Density + topStats.Density) / 3f,
                tipFriction: baseStats.TipFriction,
                moveSpeed: baseStats.MoveSpeed,
                momentOfInertia: totalMass * finalRadius
            );

        }
    }

}