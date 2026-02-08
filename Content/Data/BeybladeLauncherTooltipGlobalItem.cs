using System;
using System.Collections.Generic;
using Gearstorm.Content.Items.Beyblades;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
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
                Item topItem = launcher.BeybladeParts[0];
                Item bladeItem = launcher.BeybladeParts[1];
                Item baseItem = launcher.BeybladeParts[2];

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
            Color header = Color.Gold;
            Color primary = Color.OrangeRed; // ofensivo
            Color second = Color.Cyan; // ritmo / spin
            Color tert = Color.LightGray; // físico / controle

            tooltips.Add(new TooltipLine(Mod, "BeyHeader", "--- BEYBLADE STATS ---")
            {
                OverrideColor = header
            });

            bool showAdvanced =
                Main.keyState.IsKeyDown(Keys.LeftShift) ||
                Main.keyState.IsKeyDown(Keys.RightShift);

// ==============================
// COMBAT OUTPUT (ALWAYS VISIBLE)
// ==============================
            Add(tooltips, "Base Damage", s.DamageBase, primary);
            Add(tooltips, "Knockback", s.KnockbackPower, primary);

// ==============================
// CRITICAL SYSTEM (Spin-based, legacy compatible)
// ==============================

// Spin base da build
            float effectiveSpin = s.BaseSpinSpeed * s.Balance;

// Crit chance máximo (0–100%) derivado da build
            float critChance = MathF.Min(100f, effectiveSpin * 20f);

            tooltips.Add(new TooltipLine(
                Mod,
                "BeyCritChance",
                $"Critical Chance: {critChance:F0}%"
            )
            {
                OverrideColor = primary
            });

            tooltips.Add(new TooltipLine(
                Mod,
                "BeyCritMult",
                $"Critical Multiplier: ×{s.CritMultiplier:F2}"
            )
            {
                OverrideColor = primary
            });

// ==============================
// CORE SPIN (Always visible)
// ==============================
            Add(tooltips, "Effective Spin Speed", effectiveSpin, second);

// ==============================
// ADVANCED STATS (SHIFT)
// ==============================
            if (!showAdvanced)
            {
                tooltips.Add(new TooltipLine(
                    Mod,
                    "BeyHint",
                    "[Hold SHIFT for advanced physics stats]"
                )
                {
                    OverrideColor = Color.Gray
                });

                return;
            }

// ---- ADVANCED HEADER ----
            tooltips.Add(new TooltipLine(
                Mod,
                "BeyAdvancedHeader",
                "--- ADVANCED PHYSICS ---"
            )
            {
                OverrideColor = Color.LightGray
            });

// ==============================
// SPIN / FLOW (Advanced)
// ==============================
            Add(tooltips, "Spin Decay Rate", s.SpinDecay, second);
            Add(tooltips, "Move Speed", s.MoveSpeed, second);

// ==============================
// PHYSICS / CONTROL (Advanced)
// ==============================
            Add(tooltips, "Mass", s.Mass, tert);
            Add(tooltips, "Density", s.Density, tert);
            Add(tooltips, "Moment of Inertia", s.MomentOfInertia, tert);

            Add(tooltips, "Radius", s.Radius, tert);
            Add(tooltips, "Height", s.Height, tert);

            Add(tooltips, "Balance", s.Balance, tert);
            Add(tooltips, "Tip Friction", s.TipFriction, tert);
            Add(tooltips, "Knockback Resistance", s.KnockbackResistance, tert);
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
