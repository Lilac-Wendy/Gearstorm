using System;
using System.Collections.Generic;
using Gearstorm.Content.Projectiles;
using Gearstorm.Content.Projectiles.Beyblades;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Gearstorm.Content.Items.Parts.Augments;

public class PoisonAugment : BeybladeAugment
{
    public override string Texture => "Gearstorm/Assets/Items/Parts/Augment";
    public override Color AugmentColor => new Color(50, 205, 50);
    
    public static int CurrentVenomStacks;
    public static float CurrentDefenseReduction;

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        Color poisonColor = new Color(50, 205, 50);
        Color venomColor = new Color(148, 0, 211);
    
        tooltips.Add(new TooltipLine(Mod, "AugHeader", "[c/32CD32:Septic Tip]"));

        if (CurrentVenomStacks > 0)
        {
            tooltips.Add(new TooltipLine(Mod, "CurrentStats",
                $"Current Venom Stacks: [c/00FF00:{CurrentVenomStacks}]\n" +
                $"Defense Reduction: [c/FF0000:{CurrentDefenseReduction:F1}%]"
            ));
        }

        tooltips.Add(new TooltipLine(Mod, "AugmentDescription",
            "[c/00FF00:Base Effect]\n" +
            "Applies [c/00FF00:5 seconds] of Poison on hit\n" +
            "Striking non-boss poisoned foes reduces defense by [c/FF0000:5% + 1] per hit\n" +
            "Damage numbers appear in [c/32CD32:toxic green]"
        ));

        if (Main.hardMode)
        {
            tooltips.Add(new TooltipLine(Mod, "HardmodeBonus",
                "[c/FFD700:Hardmode: Venom Protocol]\n" +
                "Also applies [c/9400D3:1 second of Venom]\n" +
                "[c/9400D3:10%] chance to create a toxic gas cloud\n" +
                "Gas cloud deals [c/00FF00:25%] of damage as the initial explosion\n" +
                "Gas accumulates [c/00FF00:Venom Stacks] on enemies inside\n" +
                "Max stacks: [c/00FF00:10 base + 30 per 100% excess crit]\n" +
                "[c/FFA500:Overcrit Stacks] (>10) gain [c/00FF00:+3%] damage per stack\n" +
                "Stacks decay after [c/00FF00:2 seconds] out of gas"
            ));
        }

        tooltips.Add(new TooltipLine(Mod, "AugmentFlavor",
            "[c/AAAAAA:'In Terratown, hope is the first thing to die.']"));
    }

    public override void ApplyAugmentEffect(BaseBeybladeProjectile beybladeProj, NPC target, bool wasCrit)
    {
        if (target == null)
            return;

        // 1. Aplica buffs base
        target.AddBuff(BuffID.Poisoned, 300);
        
        // Atualiza stats para tooltips
        var venomNpc = target.GetGlobalNPC<VenomGasGlobalNpc>();
        CurrentVenomStacks = venomNpc.VenomStacks;
        CurrentDefenseReduction = (1f - (target.defense / (float)target.defDefense)) * 100f;

        if (Main.hardMode)
        {
            target.AddBuff(BuffID.Venom, 60);
            
            // Explosão tóxica (10% chance)
            if (Main.rand.NextBool(10))
            {
                // Calcula dano do gás (250% do dano do beyblade)
                int venomDamage = (int)(beybladeProj.Projectile.damage * 0.25f);
                
                // Aplica crítico se o hit original foi crítico
                if (wasCrit)
                {
                    venomDamage = (int)(venomDamage * beybladeProj.CritMultiplier);
                }

                // 🔥 CORREÇÃO: Spawna o VenomGas PASSANDO O CRITMULTIPLIER
                Projectile.NewProjectile(
                    beybladeProj.Projectile.GetSource_FromThis(),
                    target.Center,
                    Vector2.Zero,
                    ModContent.ProjectileType<VenomGas>(),
                    venomDamage,
                    0f,
                    beybladeProj.Projectile.owner,
                    0f,                    // ai[0] = 0 para ser o projétil principal
                    venomDamage,           // ai[1] = dano para cálculos de DPS
                    beybladeProj.CritMultiplier // 🔥 ai[2] = CritMultiplier TRANSFERIDO
                );
                

            }
        }

        // 2. Redução de defesa para alvos envenenados
        if ((target.HasBuff(BuffID.Poisoned) || target.HasBuff(BuffID.Venom)) && !target.boss)
        {
            int flatReduction = 1;
            float percentReduction = 0.95f;
            
            int newDefense = (int)(target.defense * percentReduction) - flatReduction;
            target.defense = Math.Max(0, newDefense);
            
            for (int i = 0; i < 5; i++)
            {
                Dust.NewDustPerfect(
                    target.Center + new Vector2(Main.rand.Next(-20, 20), Main.rand.Next(-20, 20)),
                    DustID.GreenBlood,
                    new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-2f, -1f)),
                    0,
                    new Color(255, 50, 50)
                );
            }
        }

        // 3. DANO POR STACKS DE VENOM
        var venomGlobalNpc = target.GetGlobalNPC<VenomGasGlobalNpc>();
        if (venomGlobalNpc.VenomStacks > 0)
        {
            // 🔥 AGORA usa CritMultiplier para calcular bônus de overcrit
            int baseStackDamage = (int)(beybladeProj.Projectile.damage * 0.15f * venomGlobalNpc.VenomStacks);
            
            // 🔥 BÔNUS DE OVERCRIT: stacks acima de 10 ganham dano extra
            if (venomGlobalNpc.VenomStacks > 10)
            {
                float overcritBonus = 1.0f + ((venomGlobalNpc.VenomStacks - 10) * 0.05f);
                baseStackDamage = (int)(baseStackDamage * overcritBonus);
            }
            
            if (baseStackDamage > 0)
            {
                bool stackCrit = wasCrit && Main.rand.NextFloat() < 0.3f;
                if (stackCrit)
                {
                    baseStackDamage = (int)(baseStackDamage * beybladeProj.CritMultiplier);
                }

                NPC.HitInfo stackHitInfo = new NPC.HitInfo
                {
                    Damage = baseStackDamage,
                    SourceDamage = baseStackDamage,
                    Crit = stackCrit,
                    Knockback = 0f,
                    HitDirection = Math.Sign(target.Center.X - beybladeProj.Projectile.Center.X),
                    DamageType = beybladeProj.Projectile.DamageType,
                    InstantKill = false,
                    HideCombatText = true
                };

                target.StrikeNPC(stackHitInfo);

                // 🔥 COR DINÂMICA BASEADA EM STACKS E CRITMULTIPLIER
                Color stackColor;
                if (venomGlobalNpc.VenomStacks > 10)
                {
                    // Gradiente de verde para dourado baseado em overcrit
                    float overcritRatio = Math.Min((venomGlobalNpc.VenomStacks - 10) / 40f, 1f);
                    stackColor = Color.Lerp(Color.Lime, Color.Gold, overcritRatio);
                }
                else if (venomGlobalNpc.VenomStacks >= 8)
                    stackColor = new Color(255, 50, 50);
                else if (venomGlobalNpc.VenomStacks >= 5)
                    stackColor = new Color(255, 165, 0);
                else
                    stackColor = new Color(50, 205, 50);

                string stackText = $"{stackHitInfo.Damage}";
                if (venomGlobalNpc.VenomStacks > 1)
                    stackText += $" ({venomGlobalNpc.VenomStacks})";
                
                if (stackCrit)
                    stackText += "!";

                CombatText.NewText(
                    target.Hitbox,
                    stackColor,
                    stackText,
                    dramatic: stackCrit,
                    dot: false
                );

            }
        }
    }

    public override void AddRecipes()
    {
        Recipe recipe = CreateRecipe();
        recipe.AddIngredient(ItemID.PoisonDart, 50);
        recipe.AddIngredient(ItemID.VialofVenom, 5);
        recipe.AddTile(TileID.MythrilAnvil);
        recipe.Register();
    }
}