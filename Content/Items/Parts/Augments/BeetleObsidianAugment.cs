using Gearstorm.Content.Projectiles.Beyblades;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace Gearstorm.Content.Items.Parts.Augments
{
    public class BeetleObsidianAugment : BeybladeAugment
    {
        public override string Texture => "Gearstorm/Assets/Items/Parts/Augment";
        public override Color AugmentColor => Color.DarkSlateBlue;

        public override string ExtraDescription =>
            "[c/4B0082:Shell-Hardened Core]\n" +
            "Each hit increases [c/FFFFFF:Density and Knockback] for this launch.\n" +
            "Resets when the Beyblade stops.\n" +
            "'The more it fights, the harder it becomes.'";

        public override void ApplyAugmentEffect(BaseBeybladeProjectile beybladeProj, NPC target, bool wasCrit)
        {
            // Crescimento progressivo (sem afetar Mass)
            beybladeProj.Stats.Density += 0.02f;
            beybladeProj.Stats.KnockbackPower += 0.1f;

            // Feedback visual
            if (Main.rand.NextBool(5))
            {
                Dust.NewDust(
                    beybladeProj.Projectile.position,
                    beybladeProj.Projectile.width,
                    beybladeProj.Projectile.height,
                    DustID.Obsidian,
                    0, 0, 0,
                    default,
                    0.7f
                );
            }
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.BeetleHusk, 5);
            recipe.AddIngredient(ItemID.Obsidian, 20);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.Register();
        }
    }
}