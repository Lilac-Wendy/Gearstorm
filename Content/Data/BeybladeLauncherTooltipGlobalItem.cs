using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Gearstorm.Content.Data;
using Gearstorm.Content.Items.Beyblades;

namespace Gearstorm.Content.Globals
{
    public class BeybladeLauncherGlobalItem : GlobalItem
    {

        
        // ==================================================
        // CONFIG
        // ==================================================
        public override bool InstancePerEntity => true;

        // ==================================================
        // CACHE DE STATUS
        // ==================================================
        public BeybladeStats? CachedStats;

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
            CachedStats = null;

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

            CachedStats =  BeybladeStats.CombineStats(
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

    tooltips.Add(new TooltipLine(Mod, "BeyHeader", "--- STATUS DO BEYBLADE ---")
    {
        OverrideColor = HEADER
    });

    // ==============================
    // OFENSIVO
    // ==============================
    Add(tooltips, "Dano Base", s.DamageBase, PRIMARY);

    // Chance de crítico
    float critChanceTotal = s.BaseSpinSpeed * 20f;
    float critChance = MathHelper.Clamp(critChanceTotal, 0f, 100f);

    tooltips.Add(new TooltipLine(
        Mod,
        "BeyCritChance",
        $"Chance de Crítico: {critChance:F0}%"
    )
    {
        OverrideColor = PRIMARY
    });

    // Multiplicador crítico REAL
    float critMultiplier = 2f;
    if (s.BaseSpinSpeed > 5f)
        critMultiplier *= 1f + (s.BaseSpinSpeed - 5f) * 0.4f;

    tooltips.Add(new TooltipLine(
        Mod,
        "BeyCritMult",
        $"Multiplicador Crítico: ×{critMultiplier:F1}"
    )
    {
        OverrideColor = PRIMARY
    });

    // Armor Pen (Over-Spinning)
    float armorPen = s.BaseSpinSpeed * 10f;
    if (armorPen > 0f)
    {
        tooltips.Add(new TooltipLine(
            Mod,
            "BeyArmorPen",
            $"Perfuração de Armadura: +{armorPen:F0}"
        )
        {
            OverrideColor = PRIMARY
        });
    }

    Add(tooltips, "Impacto", s.KnockbackPower, PRIMARY);

    // ==============================
    // RITMO / SPIN
    // ==============================
    Add(tooltips, "Spin Base", s.BaseSpinSpeed, SECOND);
    Add(tooltips, "Decaimento", s.SpinDecay, SECOND);

    float aps = CalculateAttacksPerSecond(s);
    if (aps > 0f)
    {
        tooltips.Add(new TooltipLine(
            Mod,
            "BeyAPS",
            $"Ataques por Segundo: {aps:F2}"
        )
        {
            OverrideColor = SECOND
        });
    }

    Add(tooltips, "Velocidade", s.MoveSpeed, SECOND);

    // ==============================
    // FÍSICO / CONTROLE
    // ==============================
    Add(tooltips, "Massa", s.Mass, TERT);
    Add(tooltips, "Densidade", s.Density, TERT);
    Add(tooltips, "Inércia", s.MomentOfInertia, TERT);

    Add(tooltips, "Raio", s.Radius, TERT);
    Add(tooltips, "Altura", s.Height, TERT);

    Add(tooltips, "Equilíbrio", s.Balance, TERT);
    Add(tooltips, "Atrito", s.TipFriction, TERT);
    Add(tooltips, "Resistência", s.KnockbackResistance, TERT);
}



        // ==================================================
        // UTIL
        // ==================================================
        private float CalculateAttacksPerSecond(BeybladeStats s)
        {
            float effectiveSpin = s.BaseSpinSpeed * (1f - s.SpinDecay * 0.5f);

            if (effectiveSpin <= 0f)
                return 0f;

            const float hitFactor = 0.25f;

            return MathF.Max(0f, effectiveSpin * hitFactor);
        }

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
