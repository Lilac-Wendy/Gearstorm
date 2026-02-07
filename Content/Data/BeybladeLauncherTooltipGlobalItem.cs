using System;
using System.Collections.Generic;
using Gearstorm.Content.Items.Beyblades;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace Gearstorm.Content.Data
{
    public class BeybladeLauncherGlobalItem : GlobalItem
    {

        
        // ==================================================
        // CONFIG
        // ==================================================
        public override bool InstancePerEntity => true;


        // ==================================================
        // APLICAÇÃO
        // ==================================================
        public override bool AppliesToEntity(Item item, bool lateInstantiation)
        {
            return item.ModItem is BeybladeLauncherItem;
        }

        // ==================================================
        // API PÚBLICA (CHAMADA PELO UI SYSTEM)
        // ==================================================
        public void RecalculateStats(BeybladeLauncherItem launcher)
        {
            if (launcher.BeybladeParts == null || launcher.BeybladeParts.Length < 3)
                return;

            Item topItem   = launcher.BeybladeParts[0];
            Item bladeItem = launcher.BeybladeParts[1];
            Item baseItem  = launcher.BeybladeParts[2];

            if (topItem.IsAir || bladeItem.IsAir || baseItem.IsAir)
                return;

            if (topItem.ModItem   is not BeybladeStats.IHasBeybladeStats top)   return;
            if (bladeItem.ModItem is not BeybladeStats.IHasBeybladeStats blade) return;
            if (baseItem.ModItem  is not BeybladeStats.IHasBeybladeStats @base) return;

            BeybladeStats.CombineStats(
                basePart: @base,
                bladePart: blade,
                topPart: top
            );
        }

        // ==================================================
        // TOOLTIP (SÓ LEITURA)
        // ==================================================
        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            if (item.ModItem is not BeybladeLauncherItem launcher)
                return;

            BeybladeStats? stats = null;

            if (launcher.BeybladeParts != null && launcher.BeybladeParts.Length >= 3)
            {
                Item topItem   = launcher.BeybladeParts[0];
                Item bladeItem = launcher.BeybladeParts[1];
                Item baseItem  = launcher.BeybladeParts[2];

                if (!topItem.IsAir && !bladeItem.IsAir && !baseItem.IsAir &&
                    topItem.ModItem is BeybladeStats.IHasBeybladeStats top &&
                    bladeItem.ModItem is BeybladeStats.IHasBeybladeStats blade &&
                    baseItem.ModItem is BeybladeStats.IHasBeybladeStats @base)
                {
                    stats = BeybladeStats.CombineStats(
                        basePart: @base,
                        bladePart: blade,
                        topPart: top
                    );
                }
            }

            if (!stats.HasValue)
            {
                tooltips.Add(new TooltipLine(Mod, "BeyIncomplete", "--- MONTAGEM INCOMPLETA ---")
                {
                    OverrideColor = Color.Gray
                });
                return;
            }

            BeybladeStats s = stats.Value;

    // ==============================
    // CORES PADRÃO
    // ==============================
    Color HEADER   = Color.Gold;
    Color PRIMARY = Color.OrangeRed;   // ofensivo
    Color SECOND  = Color.Cyan;        // ritmo / spin
    Color TERT    = Color.LightGray;   // físico / controle

    tooltips.Add(new TooltipLine(Mod, "BeyHeader", "--- BEYBLADE STATS ---")
    {
        OverrideColor = HEADER
    });

    Add(tooltips, "Base Damage", s.DamageBase, PRIMARY);
    Add(tooltips, "Impact Power", s.KnockbackPower, PRIMARY);

// Critical Chance (Spin × 20%)
    float critChance = MathF.Min(100f, s.BaseSpinSpeed * 20f);
    tooltips.Add(new TooltipLine(
        Mod,
        "BeyCritChance",
        $"Critical Chance: {critChance:F0}%"
    )
    {
        OverrideColor = PRIMARY
    });

// Critical Multiplier (Over-Spin)
    tooltips.Add(new TooltipLine(
        Mod,
        "BeyCritMult",
        $"Critical Multiplier: ×{s.CritMultiplier:F2}"
    )
    {
        OverrideColor = PRIMARY
    });

// SPIN / FLOW
    Add(tooltips, "Spin Speed", s.BaseSpinSpeed, SECOND);
    Add(tooltips, "Spin Decay", s.SpinDecay, SECOND);
    Add(tooltips, "Move Speed", s.MoveSpeed, SECOND);

    // ==============================
    // PHYSICS / CONTROL
    // ==============================
    Add(tooltips, "Mass", s.Mass, TERT);
    Add(tooltips, "Density", s.Density, TERT);
    Add(tooltips, "Moment of Inertia", s.MomentOfInertia, TERT);

    Add(tooltips, "Radius", s.Radius, TERT);
    Add(tooltips, "Height", s.Height, TERT);

    Add(tooltips, "Balance", s.Balance, TERT);
    Add(tooltips, "Tip Friction", s.TipFriction, TERT);
    Add(tooltips, "Knockback Resistance", s.KnockbackResistance, TERT);

}



        // ==================================================
        // UTIL
        // ==================================================
        private void Add(
            List<TooltipLine> tooltips,
            string label,
            float value,
            Color color
        )
        {
            if (value == 0f) return;

            tooltips.Add(new TooltipLine(
                Mod,
                $"BeyStat_{label.Replace(" ", "")}",
                $"{label}: {value:F2}"
            )
            {
                OverrideColor = color
            });
        }
    }
}
