using Gearstorm.Content.Items.Parts;
using Gearstorm.Content.Projectiles.Beyblades;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Gearstorm.Content.Items.Augments
{
    public class BeeAugment : BeybladeAugment
    {
        public override string Texture => "Gearstorm/Assets/Items/Parts/Augment";
        public override Color AugmentColor => Color.Gold;

        public override string ExtraDescription => 
            "[c/FFD700:Hivelord's Core]\n" +
            "Releases [c/FFFF00:Angry Bees] upon impact.\n" +
            "Impacts on tiles have a small chance to spawn bees too.\n" +
            "'AAAAAHHHHH! OH, THEY'RE IN MY EYES!'";

        public override void OnBeybladeHit(Projectile projectile, Vector2 normal, float impactStrength, Projectile otherProj, NPC targetNPC)
        {
            if (Main.myPlayer != projectile.owner) return;

            int count = impactStrength > 3f ? 2 : 1;
            for (int i = 0; i < count; i++)
            {
                int p = Projectile.NewProjectile(projectile.GetSource_FromThis(), projectile.Center, normal.RotatedByRandom(0.5f) * 4f, 
                    Main.player[projectile.owner].beeType(), Main.player[projectile.owner].beeDamage(10), Main.player[projectile.owner].beeKB(1f), projectile.owner);
                Main.projectile[p].DamageType = projectile.DamageType;
            }
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.BeeWax, 8);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
    }
}