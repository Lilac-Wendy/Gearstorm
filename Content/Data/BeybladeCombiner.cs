using Gearstorm.Content.Items.Parts;

namespace Gearstorm.Content.Data
{
    public static class BeybladeCombiner
    {
        public static BeybladeStats CombineStats(BasicBaseItem basePart, BasicBladeItem bladePart, BasicTopItem topPart)
        {
            // Obtém os stats individuais
            var baseStats = basePart.Stats;
            var bladeStats = bladePart.Stats;
            var topStats = topPart.Stats;

            // Combina os stats (soma simples)
            return new BeybladeStats(
                mass: baseStats.Mass + topStats.Mass,
                density: baseStats.Density,
                Radius: bladeStats.Radius,
                Height: topStats.Height,
                tipFriction: baseStats.TipFriction,
                spinSpeed: bladeStats.SpinSpeed,
                SpinDecay: bladeStats.SpinDecay,
                balance: topStats.Balance,
                knockbackPower: bladeStats.KnockbackPower,
                knockbackResistance: bladeStats.KnockbackResistance,
                moveSpeed: baseStats.MoveSpeed,
                damageBase: bladeStats.DamageBase
            );
        }
    }
}