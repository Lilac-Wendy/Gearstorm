using System.Collections.Generic;
using Gearstorm.Content.DamageClasses;
using Gearstorm.Content.Items.Beyblades;
using Gearstorm.Content.Items.Parts;
using Gearstorm.Content.Projectiles.Beyblades;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

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
            if (item.DamageType != ModContent.GetInstance<Spinner>())
                return;

            if (item.shoot <= ProjectileID.None)
                return;

            // Verifica se é um BeybladeLauncherItem com partes montadas
            if (item.ModItem is BeybladeLauncherItem launcher)
            {
                // Se todas as partes estiverem equipadas, mostra os stats combinados
                if (!launcher.BeybladeParts[0].IsAir && 
                    !launcher.BeybladeParts[1].IsAir && 
                    !launcher.BeybladeParts[2].IsAir)
                {
                    // 🔥 CORREÇÃO: Usa IHasBeybladeStats. Se a peça implementa a interface, ela funciona.
                    var basePart = launcher.BeybladeParts[0].ModItem as IHasBeybladeStats;
                    var bladePart = launcher.BeybladeParts[1].ModItem as IHasBeybladeStats;
                    var topPart = launcher.BeybladeParts[2].ModItem as IHasBeybladeStats;

                    if (basePart != null && bladePart != null && topPart != null)
                    {
                        var combinedStats = BeybladeCombiner.CombineStats(basePart, bladePart, topPart);
                        
                        tooltips.Add(new TooltipLine(Mod, "BeyHeader", 
                            Language.GetTextValue("Mods.Gearstorm.Items.BeybladeStatsHeader"))
                        {
                            OverrideColor = Color.Gold
                        });

                        AddBeybladeStatTooltip(tooltips, "Damage", combinedStats.DamageBase.ToString("F0"), Color.Red);
                        AddBeybladeStatTooltip(tooltips, "Mass", combinedStats.Mass.ToString("F2"), Color.Yellow);
                        AddBeybladeStatTooltip(tooltips, "Density", combinedStats.Density.ToString("F2"), Color.Orange);
                        AddBeybladeStatTooltip(tooltips, "SpinSpeed", combinedStats.SpinSpeed.ToString("F2"), Color.LightGreen);
                        AddBeybladeStatTooltip(tooltips, "Balance", combinedStats.Balance.ToString("F2"), Color.Cyan);
                        AddBeybladeStatTooltip(tooltips, "TipFriction", combinedStats.TipFriction.ToString("F2"), Color.Gray);
                        AddBeybladeStatTooltip(tooltips, "KnockbackPower", combinedStats.KnockbackPower.ToString("F2"), Color.Pink);
                        AddBeybladeStatTooltip(tooltips, "KnockbackResistance", combinedStats.KnockbackResistance.ToString("F2"), Color.Violet);
                        AddBeybladeStatTooltip(tooltips, "MoveSpeed", combinedStats.MoveSpeed.ToString("F2"), Color.Lime);
                        
                        return; 
                    }
                }
            }
            
            tooltips.Add(new TooltipLine(Mod, "BeyHeader", 
                Language.GetTextValue("Mods.Gearstorm.Items.BeybladeStatsHeader"))
            {
                OverrideColor = Color.Gray
            });
            
            tooltips.Add(new TooltipLine(Mod, "NoParts", 
                Language.GetTextValue("Mods.Gearstorm.Items.BeybladeNoParts"))
            {
                OverrideColor = Color.LightGray
            });
        }
        
        private void AddBeybladeStatTooltip(List<TooltipLine> tooltips, string statKey, string value, Color color)
        {
            string fullKey = $"Mods.Gearstorm.Items.BeybladeStats.{statKey}";
    
            string text;
            try
            {
                // Tenta pegar do HJSON formatado "Dano: {0}"
                text = Language.GetTextValue(fullKey, value);
                // Fallback se a chave retornar ela mesma
                if (text == fullKey) text = $"{statKey}: {value}";
            }
            catch
            {
                text = $"{statKey}: {value}";
            }

            tooltips.Add(new TooltipLine(Mod, $"Beyblade{statKey}", text)
            {
                OverrideColor = color
            });
        }
    }
}