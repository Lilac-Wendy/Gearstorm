using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Gearstorm.Content.Data
{
    public class BeybladePartTooltipGlobalItem : GlobalItem
    {
        public override bool AppliesToEntity(Item item, bool lateInstantiation)
        {
            // Só aplica se o item tiver BeybladeStats
            return item.ModItem is BeybladeStats.IHasBeybladeStats;
        }

        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            if (item.ModItem is not BeybladeStats.IHasBeybladeStats part) return;

            BeybladeStats stats = part.Stats;

            tooltips.Add(new TooltipLine(Mod, "BeyHeader", Language.GetTextValue("Mods.Gearstorm.Items.BeybladeStatsHeader"))
            {
                OverrideColor = Color.Gold
            });

            // Adiciona tipo da parte
            string partTypeText = part.PartType switch
            {
                BeybladeStats.BeybladePartType.Base => "Base",
                BeybladeStats.BeybladePartType.Blade => "Lâmina", 
                BeybladeStats.BeybladePartType.Top => "Topo",
                _ => "Parte"
            };
            
            tooltips.Add(new TooltipLine(Mod, "PartType", $"Tipo: {partTypeText}")
            {
                OverrideColor = GetPartTypeColor(part.PartType)
            });

            void AddStat(string key, float value, Color color)
            {
                if (value == 0f) return;
                string text = Language.GetTextValue($"Mods.Gearstorm.Items.BeybladeStats.{key}", value);
                tooltips.Add(new TooltipLine(Mod, $"Beyblade{key}", text) { OverrideColor = color });
            }

            AddStat("Damage", stats.DamageBase, Color.Red);
            AddStat("Mass", stats.Mass, Color.Yellow);
            AddStat("Density", stats.Density, Color.Orange);
            AddStat("SpinSpeed", stats.BaseSpinSpeed, Color.LightGreen);
            AddStat("Balance", stats.Balance, Color.Cyan);
            AddStat("TipFriction", stats.TipFriction, Color.Gray);
            AddStat("KnockbackPower", stats.KnockbackPower, Color.Pink);
            AddStat("KnockbackResistance", stats.KnockbackResistance, Color.Violet);
            AddStat("MoveSpeed", stats.MoveSpeed, Color.Lime);
            AddStat("Height", stats.Height, Color.LightBlue);
            AddStat("Radius", stats.Radius, Color.White);
            AddStat("SpinDecay", stats.SpinDecay, Color.Brown);
        }
        
        private Color GetPartTypeColor(BeybladeStats.BeybladePartType partType)
        {
            return partType switch
            {
                BeybladeStats.BeybladePartType.Base => Color.LightBlue,
                BeybladeStats.BeybladePartType.Blade => Color.LightGreen,
                BeybladeStats.BeybladePartType.Top => Color.LightGoldenrodYellow,
                _ => Color.White
            };
        }
    }
}