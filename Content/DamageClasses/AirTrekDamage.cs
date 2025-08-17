using Terraria.ModLoader;
// Bazinga //
    namespace Gearstorm.Content.DamageClasses
    {
        public class AirTrekDamage : DamageClass
        {

            public override StatInheritanceData GetModifierInheritance(DamageClass damageClass)
            {
                if (damageClass == DamageClass.Melee)
                    return StatInheritanceData.Full;

                return new StatInheritanceData(
                    damageInheritance: 0.5f,
                    critChanceInheritance: 0.7f,
                    attackSpeedInheritance: 1f,
                    armorPenInheritance: 0f,
                    knockbackInheritance: 0.8f
                );
            }
        }
    }
