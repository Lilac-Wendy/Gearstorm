using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
// Bazinga //
namespace Gearstorm.Content.Data;

public class BeybladePartTooltipGlobalItem : GlobalItem
{
    public override bool AppliesToEntity(Item item, bool lateInstantiation)
    {
        // Só aplica se o item tiver BeybladeStats
        return item.ModItem is IHasBeybladeStats;
    }

    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
    {
        if (item.ModItem is not IHasBeybladeStats part) return;

        BeybladeStats stats = part.Stats;

        tooltips.Add(new TooltipLine(Mod, "BeyHeader", Language.GetTextValue("Mods.Gearstorm.Items.BeybladeStatsHeader"))
        {
            OverrideColor = Color.Gold
        });

        void AddStat(string key, float value, Color color)
        {
            if (value == 0f) return; // ignora atributos zerados
            string text = Language.GetTextValue($"Mods.Gearstorm.Items.BeybladeStats.{key}", value);
            tooltips.Add(new TooltipLine(Mod, $"Beyblade{key}", text) { OverrideColor = color });
        }

        AddStat("Damage", stats.DamageBase, Color.Red);
        AddStat("Mass", stats.Mass, Color.Yellow);
        AddStat("Density", stats.Density, Color.Orange);
        AddStat("SpinSpeed", stats.SpinSpeed, Color.LightGreen);
        AddStat("Balance", stats.Balance, Color.Cyan);
        AddStat("TipFriction", stats.TipFriction, Color.Gray);
        AddStat("KnockbackPower", stats.KnockbackPower, Color.Pink);
        AddStat("KnockbackResistance", stats.KnockbackResistance, Color.Violet);
        AddStat("MoveSpeed", stats.MoveSpeed, Color.Lime);
        AddStat("Height", stats.Height, Color.LightBlue);
        AddStat("Radius", stats.Radius, Color.White);
        AddStat("SpinDecay", stats.SpinDecay, Color.Brown);
    }
}

// Interface para marcar os itens que têm stats
public interface IHasBeybladeStats
{
    public BeybladeStats Stats { get; }
}