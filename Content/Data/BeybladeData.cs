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
        public float SpinSpeed;
        public float SpinDecay;
        public float Mass;
        public float Balance;
        public float Density;
        public float TipFriction;
        public float MoveSpeed;
        public float MomentOfInertia;

        // Construtor utilitário
        public BeybladeStats(
            float damageBase = 0f, 
            float knockbackPower = 0f, 
            float knockbackResistance = 0f, 
            float radius = 0f, 
            float height = 0f,
            float spinSpeed = 0f, 
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
            SpinSpeed = spinSpeed;
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
        // Agora aceita a interface, permitindo GoldItem, IronItem, etc.
        public static BeybladeStats CombineStats(IHasBeybladeStats basePart, IHasBeybladeStats bladePart, IHasBeybladeStats topPart)
        {
            var baseStats = basePart.Stats;
            var bladeStats = bladePart.Stats;
            var topStats = topPart.Stats;

            // --- FÓRMULAS DE COMBINAÇÃO ---
            
            // Massa total afeta o empurrão e a resistência
            float totalMass = baseStats.Mass + bladeStats.Mass + topStats.Mass;
            
            // Dano base vem primariamente da lâmina, mas massa aumenta o impacto
            float finalDamage = bladeStats.DamageBase + (topStats.DamageBase * 0.2f);
            
            // Velocidade de giro: média das peças * balanço do topo
            // (Um topo desbalanceado faz girar menos suave)
            float finalSpin = ((bladeStats.SpinSpeed + baseStats.SpinSpeed) / 2f) * topStats.Balance;

            // Raio de colisão é ditado pela maior peça (geralmente a lâmina)
            float finalRadius = Math.Max(bladeStats.Radius, Math.Max(baseStats.Radius, topStats.Radius));

            return new BeybladeStats(
                damageBase: finalDamage,
                knockbackPower: bladeStats.KnockbackPower + (totalMass * 0.5f),
                knockbackResistance: baseStats.KnockbackResistance + topStats.KnockbackResistance + (totalMass * 0.2f),
                radius: finalRadius,
                height: baseStats.Height + topStats.Height, // Altura somada
                spinSpeed: finalSpin,
                spinDecay: bladeStats.SpinDecay * topStats.Balance, // Menos decay se balanceado
                mass: totalMass,
                balance: topStats.Balance,
                density: (baseStats.Density + bladeStats.Density + topStats.Density) / 3f,
                tipFriction: baseStats.TipFriction, // Fricção depende 100% da base (ponta)
                moveSpeed: baseStats.MoveSpeed, // Velocidade de movimento depende da base
                momentOfInertia: totalMass * finalRadius // Física simples de inércia
            );
        }
    }
}