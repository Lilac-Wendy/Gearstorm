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
            "Each hit increases [c/FFFFFF:Mass, Density and Knockback] for this launch.\n" +
            "Resets when the Beyblade stops.\n" +
            "'The more it fights, the heavier it becomes.'";

        public override void ApplyAugmentEffect(BaseBeybladeProjectile beybladeProj, NPC target)
        {
            // Crescimento progressivo
            beybladeProj.stats.Mass += 0.05f;
            beybladeProj.stats.Density += 0.02f;
            beybladeProj.stats.KnockbackPower += 0.1f;
            
            // Feedback visual: a Beyblade fica levemente maior ou brilha
            if (Main.rand.NextBool(5))
                Dust.NewDust(beybladeProj.Projectile.position, beybladeProj.Projectile.width, beybladeProj.Projectile.height, DustID.Obsidian, 0, 0, 0, default, 0.7f);
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