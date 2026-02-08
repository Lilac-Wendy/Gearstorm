using System;
using Gearstorm.Content.Projectiles.Beyblades;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace Gearstorm.Content.Items.Parts.Augments
{
    public class EvilAugment : BeybladeAugment
    {
        public override string Texture => "Gearstorm/Assets/Items/Parts/Augment";
        public override Color AugmentColor => WorldGen.crimson ? Color.Crimson : Color.Purple;

        public override string ExtraDescription => 
            "[c/A330C9:Vampiric Residue]\n" +
            "Deals damage based on World Evil type.\n" +
            "Converts [c/FF0000:20% of Damage] into Beyblade Life (TimeLeft).\n" +
            (Main.hardMode ? "[c/FFD700:Hardmode Bonus:] Hits reduce enemy damage and knockback resistance." : "") + "\n" +
            "'An unholy thirst that fuels the spin.'";

        public override void ApplyAugmentEffect(BaseBeybladeProjectile beybladeProj, NPC target, bool wasCrit)
        {
            // Lógica de Cura de Tempo de Vida (Stamina)
            int lifeSteal = (int)(beybladeProj.Projectile.damage * 0.20f);
            beybladeProj.Projectile.timeLeft = Math.Min(beybladeProj.Projectile.timeLeft + lifeSteal, 3600);
            target.AddBuff(BuffID.Weak, 120);

            // Bônus de Hardmode
            if (Main.hardMode)
            {
                if (WorldGen.crimson)
                    target.AddBuff(BuffID.Ichor, 120);
                else
                    target.AddBuff(BuffID.CursedInferno, 120);

            }
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            
            if (WorldGen.crimson) {
                recipe.AddIngredient(ItemID.TissueSample, 15);
            } else {
                recipe.AddIngredient(ItemID.ShadowScale, 15);
            }
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
    }
}