    using Terraria.Localization;
    using Terraria.ModLoader;

    namespace Gearstorm.Content.DamageClasses
    {
        public class Spinner : DamageClass
        {
            public override StatInheritanceData GetModifierInheritance(DamageClass damageClass) {
                if (damageClass == Generic) return StatInheritanceData.Full;
                return StatInheritanceData.None;
            }
            public override bool UseStandardCritCalcs => false;
            
            public override LocalizedText DisplayName => Language.GetText("Mods.Gearstorm.DamageClasses.Spinner.DisplayName");
            public override bool GetEffectInheritance(DamageClass damageClass)
            {
                return false;
            }
        }
    }