using System.Collections.Generic;
using Gearstorm.Content.DamageClasses;
using Gearstorm.Content.Items;
using Gearstorm.Content.Items.Beyblades;
using Gearstorm.Content.Items.Parts;
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
    // Só aplica em itens que sejam do tipo Spinner
    if (item.DamageType != ModContent.GetInstance<Spinner>())
        return;

    // Garante que tem um projectile associado
    if (item.shoot <= 0)
        return;

    // Cria instância temporária do projectile
    Projectile dummyProj = new Projectile();
    dummyProj.SetDefaults(item.shoot);

    if (dummyProj.ModProjectile is not BaseBeybladeProjectile beyProj)
        return;

    // 🔹 Inicializa os stats manualmente
    // Aqui você pode injetar a mesma lógica que usaria em Shoot()
    if (item.ModItem is StarterBeyItem)
    {
        var basePart = new BasicBaseItem();
        var bladePart = new BasicBladeItem();
        var topPart = new BasicTopItem();

        beyProj.stats = BeybladeCombiner.CombineStats(basePart, bladePart, topPart);
    }

    // Adiciona o cabeçalho
    tooltips.Add(new TooltipLine(Mod, "BeyHeader", 
        Language.GetTextValue("Mods.Gearstorm.Items.BeybladeStatsHeader"))
    {
        OverrideColor = Color.Gold
    });

    // Adiciona os atributos
    AddBeybladeStatTooltip(tooltips, "Damage", beyProj.stats.DamageBase.ToString("F0"), Color.Red);
    AddBeybladeStatTooltip(tooltips, "Mass", beyProj.stats.Mass.ToString("F2"), Color.Yellow);
    AddBeybladeStatTooltip(tooltips, "Density", beyProj.stats.Density.ToString("F2"), Color.Orange);
    AddBeybladeStatTooltip(tooltips, "SpinSpeed", beyProj.stats.SpinSpeed.ToString("F2"), Color.LightGreen);
    AddBeybladeStatTooltip(tooltips, "Balance", beyProj.stats.Balance.ToString("F2"), Color.Cyan);
    AddBeybladeStatTooltip(tooltips, "TipFriction", beyProj.stats.TipFriction.ToString("F2"), Color.Gray);
    AddBeybladeStatTooltip(tooltips, "KnockbackPower", beyProj.stats.KnockbackPower.ToString("F2"), Color.Pink);
    AddBeybladeStatTooltip(tooltips, "KnockbackResistance", beyProj.stats.KnockbackResistance.ToString("F2"), Color.Violet);
    AddBeybladeStatTooltip(tooltips, "MoveSpeed", beyProj.stats.MoveSpeed.ToString("F2"), Color.Lime);
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