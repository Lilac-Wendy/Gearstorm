using System.Collections.Generic;
using Gearstorm.Content.DamageClasses;
using Gearstorm.Content.Projectiles.Beyblades;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

// Bazinga //
namespace Gearstorm.Content.Data
{
    public class BeybladeTooltipGlobalItem : GlobalItem
    {
        public override bool AppliesToEntity(Item item, bool lateInstantiation)
        {
            return item.DamageType == ModContent.GetInstance<Spinner>();
        }
        
        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            if (item.shoot <= 0) return;

            var proj = ModContent.GetModProjectile(item.shoot);
    
            // Verifique diretamente por BaseBeybladeProjectile
            if (proj is not BaseBeybladeProjectile bey) return;
    
            tooltips.Add(new TooltipLine(Mod, "BeyHeader", Language.GetTextValue("Mods.Gearstorm.Items.BeybladeStatsHeader")) { 
                OverrideColor = Color.Gold 
            });

            // Adicione todas as estatísticas usando o método auxiliar
            AddBeybladeStatTooltip(tooltips, "Damage", bey.DamageBase.ToString("F1"), Color.Red);
            AddBeybladeStatTooltip(tooltips, "Mass", bey.Mass.ToString("F2"), Color.Yellow);
            AddBeybladeStatTooltip(tooltips, "Density", bey.stats.Density.ToString("F2"), Color.Orange);
            AddBeybladeStatTooltip(tooltips, "SpinSpeed", bey.SpinSpeedProp.ToString("F2"), Color.LightGreen);
            AddBeybladeStatTooltip(tooltips, "Balance", bey.Balance.ToString("F2"), Color.Cyan);
            AddBeybladeStatTooltip(tooltips, "TipFriction", bey.TipFriction.ToString("F3"), Color.Gray);
            AddBeybladeStatTooltip(tooltips, "KnockbackPower", bey.stats.KnockbackPower.ToString("F1"), Color.Pink);
            AddBeybladeStatTooltip(tooltips, "KnockbackResistance", bey.stats.KnockbackResistance.ToString("F1"), Color.Violet);
            AddBeybladeStatTooltip(tooltips, "MoveSpeed", bey.stats.MoveSpeed.ToString("F1"), Color.Lime);
        }
        
        private void AddBeybladeStatTooltip(List<TooltipLine> tooltips, string statKey, string value, Color color)
        {
            string fullKey = $"Mods.Gearstorm.Items.BeybladeStats.{statKey}";
    
            string text;
            try
            {
                text = Language.GetTextValue(fullKey, value);
            }
            catch
            {
                // Fallback se a tradução não existir
                text = $"{statKey}: {value}";
            }

            tooltips.Add(new TooltipLine(Mod, $"Beyblade{statKey}", text)
            {
                OverrideColor = color
            });
        }
    }
}