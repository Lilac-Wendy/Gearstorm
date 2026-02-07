using System;
using Gearstorm.Content.Items.Parts;
using Gearstorm.Content.Projectiles.Beyblades;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Gearstorm.Content.Items.Augments;

public class PoisonAugment : BeybladeAugment
{
    public override string Texture => "Gearstorm/Assets/Items/Parts/Augment";

    public override string ExtraDescription => 
        "[c/32CD32:Septic Tip]\n" +
        "Injects [c/00FF00:Neurotoxins] that deal damage over time.\n" +
        "Striking poisoned foes [c/32CD32:permanently worsens] their condition.\n" +
        "Each hit reduces defense by [c/32CD32:10% + 1].\n" +
        (Main.hardMode ? "[c/FFD700:Hardmode Bonus:] Injects a small [c/9400D3:Venom] and causes toxic explosions." : "") + "\n" +
        "'Nature's cruelest defense, refined for battle.'";

    public override Color AugmentColor => Color.MediumVioletRed;

    public override void ApplyAugmentEffect(BaseBeybladeProjectile beybladeProj, NPC target)
    {
        target.AddBuff(BuffID.Poisoned, 300);

        if (Main.hardMode)
        {
            target.AddBuff(BuffID.Venom, 60);
            if (Main.rand.NextBool(10)) // Explosão tóxica
            {
                Projectile.NewProjectile(beybladeProj.Projectile.GetSource_FromThis(), target.Center, Vector2.Zero, ProjectileID.GasTrap, 40, 0, beybladeProj.Projectile.owner);
            }
        }

        if (target.HasBuff(BuffID.Poisoned) || target.HasBuff(BuffID.Venom))
        {
            int flatReduction = 1;
            float percentReduction = 0.90f;
            int newDefense = (int)(target.defense * percentReduction) - flatReduction;
            target.defense = Math.Max(0, newDefense);
            
            Dust.NewDust(target.position, target.width, target.height, DustID.GreenBlood, 0, 0, 150, default, 0.8f);
        }
    }

    public override void AddRecipes()
    {
        Recipe recipe = CreateRecipe();
        recipe.AddIngredient(ItemID.PoisonDart, 50);
        recipe.AddIngredient(ItemID.VialofVenom, 5); // Hardmode requirement
        recipe.AddTile(TileID.MythrilAnvil);
        recipe.Register();
    }
}