using System;
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
        // Fator de conversão do Projétil (Tem que bater com o SpinToDpsFactor do BaseBeybladeProjectile)
        private const float VISUAL_DPS_FACTOR = 4f; 

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
                // Se todas as partes estiverem equipadas
                if (!launcher.BeybladeParts[0].IsAir && 
                    !launcher.BeybladeParts[1].IsAir && 
                    !launcher.BeybladeParts[2].IsAir)
                {
                    var basePart = launcher.BeybladeParts[0].ModItem as IHasBeybladeStats;
                    var bladePart = launcher.BeybladeParts[1].ModItem as IHasBeybladeStats;
                    var topPart = launcher.BeybladeParts[2].ModItem as IHasBeybladeStats;

                    if (basePart != null && bladePart != null && topPart != null)
                    {
                        var combinedStats = BeybladeCombiner.CombineStats(basePart, bladePart, topPart);
                        
                        // ==== CORREÇÃO VISUAL PARA EVITAR ZERO ====
                        // Se o Balance vier 0 (porque a peça não definiu), usamos 1.0f apenas para mostrar o numero na tela.
                        float displayBalance = combinedStats.Balance <= 0.01f ? 1f : combinedStats.Balance;
                        
                        // Recalcula o Spin visualmente caso o combinador tenha zerado devido ao Balance 0
                        float rawBaseSpin = (basePart.Stats.BaseSpinSpeed + bladePart.Stats.BaseSpinSpeed);
                        float displaySpin = rawBaseSpin * displayBalance;

                        // Calcula Attacks Per Second (APS)
                        float aps = displaySpin * VISUAL_DPS_FACTOR;

                        tooltips.Add(new TooltipLine(Mod, "BeyHeader", 
                            "--- STATUS DO BEYBLADE ---") // Pode usar Language.GetTextValue aqui se preferir
                        {
                            OverrideColor = Color.Gold
                        });

                        // 1. Dano Real
                        AddBeybladeStatTooltip(tooltips, "Damage", combinedStats.DamageBase.ToString("F0"), Color.Red);
                        
                        // 2. Spin Speed e APS (A INFORMAÇÃO MAIS IMPORTANTE)
                        string spinText = $"{displaySpin:F1} (APS: {aps:F1}/s)";
                        AddBeybladeStatTooltip(tooltips, "Spin Speed", spinText, Color.Cyan);

                        // 3. Outros Stats
                        AddBeybladeStatTooltip(tooltips, "Mass", combinedStats.Mass.ToString("F2"), Color.Yellow);
                        AddBeybladeStatTooltip(tooltips, "Density", combinedStats.Density.ToString("F2"), Color.Orange);
                        AddBeybladeStatTooltip(tooltips, "Balance", displayBalance.ToString("F2"), Color.Lerp(Color.White, Color.Green, displayBalance));
                        AddBeybladeStatTooltip(tooltips, "Knockback", combinedStats.KnockbackPower.ToString("F1"), Color.Pink);
                        AddBeybladeStatTooltip(tooltips, "Move Speed", combinedStats.MoveSpeed.ToString("F1"), Color.Lime);
                        
                        return; 
                    }
                }
            }
            
            // Caso não esteja montado
            tooltips.Add(new TooltipLine(Mod, "BeyHeader", 
                "--- BEYBLADE INCOMPLETO ---")
            {
                OverrideColor = Color.Gray
            });
            
            tooltips.Add(new TooltipLine(Mod, "NoParts", 
                "Adicione Base, Lâmina e Topo para ver os status.")
            {
                OverrideColor = Color.LightGray
            });
        }
        
        private void AddBeybladeStatTooltip(List<TooltipLine> tooltips, string statKey, string value, Color color)
        {
            // Adiciona direto formatado para garantir que apareça
            string text = $"{statKey}: {value}";

            // Tenta buscar tradução se existir, senão usa o padrão acima
            string translationKey = $"Mods.Gearstorm.Items.BeybladeStats.{statKey}";
            if (Language.Exists(translationKey))
            {
                text = Language.GetTextValue(translationKey, value);
            }

            tooltips.Add(new TooltipLine(Mod, $"Beyblade{statKey}", text)
            {
                OverrideColor = color
            });
        }
    }
}