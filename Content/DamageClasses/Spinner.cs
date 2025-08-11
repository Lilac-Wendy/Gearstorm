    using Terraria.ModLoader;

    namespace Gearstorm.Content.DamageClasses
    {
        public class Spinner : DamageClass
        {
            public override StatInheritanceData GetModifierInheritance(DamageClass damageClass)
            {
                return StatInheritanceData.None; // não herda nada
            }

            public override bool GetEffectInheritance(DamageClass damageClass)
            {
                return false;
            }
        }
    }