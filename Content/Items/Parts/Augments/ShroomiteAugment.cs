using Gearstorm.Content.Items.Parts;
using Gearstorm.Content.Projectiles.Beyblades;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Gearstorm.Content.Items.Augments
{
    public class ShroomiteAugment : BeybladeAugment
    {
        public override string Texture => "Gearstorm/Assets/Items/Parts/Augment";
        public override Color AugmentColor => Color.DeepSkyBlue;

        public override string ExtraDescription => 
            "[c/00BFFF:Fungal Stealth]\n" +
            "Damage increases the [c/00BFFF:further away] you are from the Beyblade.\n" +
            "Increases damage by [c/00FF00:+50% every 25 blocks] of distance.\n" +
            "There is [c/FF0000:no limit] to how much this can scale.\n" +
            "Just a totally unsuspicious beyblade minding its own business.";

        public override void OnBeybladeHit(
            Projectile projectile,
            Vector2 normal,
            float impactStrength,
            Projectile otherProj,
            NPC targetNPC)
        {
            if (targetNPC == null)
                return;

            Player player = Main.player[projectile.owner];

            // Calcula a distância entre o jogador e a Beyblade em pixels
            float dist = Vector2.Distance(player.Center, projectile.Center);

            // Nova Proporção: 25 blocos = 400 pixels (25 * 16).
            // A cada 400 pixels, o bónus aumenta 0.5f (50%).
            // Exemplo: a 50 blocos (800px), o bónus será de +100%.
            float bonus = 1f + (dist / 400f) * 0.5f;

            // VFX de cogumelo brilhante para feedback visual
            if (bonus > 1.1f)
            {
                for (int i = 0; i < 3; i++)
                {
                    Dust d = Dust.NewDustDirect(targetNPC.position, targetNPC.width, targetNPC.height, DustID.GlowingMushroom, 0f, 0f, 100, default, 1.2f);
                    d.velocity *= 0.5f;
                    d.noGravity = true;
                }
            }

            // O bónus deve ser aplicado no cálculo de dano final do seu sistema de Spinner
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.ShroomiteBar, 10);
            recipe.AddTile(TileID.Autohammer);
            recipe.Register();
        }
    }
}