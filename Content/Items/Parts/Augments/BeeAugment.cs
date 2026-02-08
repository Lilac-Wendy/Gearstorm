using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace Gearstorm.Content.Items.Parts.Augments
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

        public override void OnBeybladeHit(Projectile beyblade, Vector2 hitNormal, float impactStrength, Projectile otherBeyblade, NPC targetNpc, bool wasCrit)
        {
            if (Main.myPlayer != beyblade.owner) return;

            int count = impactStrength > 3f ? 2 : 1;
            for (int i = 0; i < count; i++)
            {
                int p = Projectile.NewProjectile(beyblade.GetSource_FromThis(), beyblade.Center, hitNormal.RotatedByRandom(0.5f) * 4f, 
                    Main.player[beyblade.owner].beeType(), Main.player[beyblade.owner].beeDamage(10), Main.player[beyblade.owner].beeKB(1f), beyblade.owner);
                Main.projectile[p].DamageType = beyblade.DamageType;
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