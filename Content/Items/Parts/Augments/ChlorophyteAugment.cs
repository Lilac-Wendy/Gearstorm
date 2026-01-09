using System;
using Gearstorm.Content.Items.Parts;
using Gearstorm.Content.Projectiles.Beyblades;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Gearstorm.Content.Items.Parts.Augments
{
    public class ChlorophyteAugment : BeybladeAugment
    {
        public override string Texture => "Gearstorm/Assets/Items/Parts/Augment";
        public override Color AugmentColor => Color.LimeGreen;

        // Tooltip puxada do localization
        public override string AugmentDescriptionKey => "Mods.Gearstorm.Augments.Chlorophyte.Description";

        public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Color glowColor = Color.LimeGreen * 0.8f;
            spriteBatch.Draw(texture, position, null, glowColor, 0f, origin, scale, SpriteEffects.None, 0f);
        }

        public override void ApplyAugmentEffect(BaseBeybladeProjectile beybladeProj, NPC target)
        {
            float searchDistance = 600;
            bool foundTarget = false;
            Vector2 targetCenter = Vector2.Zero;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.CanBeChasedBy() && !npc.friendly)
                {
                    float between = Vector2.Distance(npc.Center, beybladeProj.Projectile.Center);
                    if (between < searchDistance)
                    {
                        searchDistance = between;
                        targetCenter = npc.Center;
                        foundTarget = true;
                    }
                }
            }

            if (foundTarget)
            {
                Vector2 direction = targetCenter - beybladeProj.Projectile.Center;
                if (direction.Length() > 0)
                {
                    direction.Normalize();
                    float homingStrength = 0.12f;

                    Vector2 desiredVelocity = direction * beybladeProj.Projectile.velocity.Length();
                    beybladeProj.Projectile.velocity = Vector2.Lerp(
                        beybladeProj.Projectile.velocity,
                        desiredVelocity,
                        homingStrength
                    );
                }
            }
            
            beybladeProj.Projectile.damage = Math.Max(1, beybladeProj.Projectile.damage - 3);

            if (!beybladeProj.bonusesApplied)
            {
                beybladeProj.Projectile.timeLeft += 600;
                beybladeProj.Projectile.penetrate += 3;
                beybladeProj.bonusesApplied = true;
            }
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.ChlorophyteBullet, 100);
            recipe.AddIngredient(ModContent.ItemType<BasicBladeItem>(), 5);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.Register();
        }
    }
}
