using Gearstorm.Content.Data;
using Gearstorm.Content.Items;
using Gearstorm.Content.Items.Parts;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace Gearstorm.Content.Projectiles.Beyblades
{
    public class StarterBeyProjectile : BaseBeybladeProjectile
    {
        public override string Texture => "Gearstorm/Assets/Items/StarterBey";  //NotReallyUsed

        public override void SetDefaults()
        {
            if (stats.Mass > 0 || stats.DamageBase > 0)
                base.SetDefaults();

            Projectile.scale = 1.0f;
        }


        public static BeybladeStats GetCombinedStats(Item item)
        {
            if (item.ModItem is StarterBeyItem beyItem)
            {
                var basePart = new BasicBaseItem();
                var bladePart = new BasicBladeItem();
                var topPart = new BasicTopItem();
                
                return BeybladeCombiner.CombineStats(basePart, bladePart, topPart);
            }
            return new BeybladeStats(
                mass: 2.0f,
                density: 1.00f,
                Radius: 0.5f,
                Height: 0.5f,
                tipFriction: 0.020f,
                spinSpeed: 1.5f,
                SpinDecay: 0.010f,
                balance: 0.85f,
                knockbackPower: 3.0f,
                knockbackResistance: 1.8f,
                moveSpeed: 1.0f,
                damageBase: 66f
            );
        }
        public override bool PreDraw(ref Color lightColor)
        {
            // Get references to the textures from the items
            Texture2D baseTexture = ModContent.Request<Texture2D>("Gearstorm/Assets/Items/Parts/Base_Default").Value;
            Texture2D bladeTexture = ModContent.Request<Texture2D>("Gearstorm/Assets/Items/Parts/Blade_Default").Value;
            Texture2D topTexture = ModContent.Request<Texture2D>("Gearstorm/Assets/Items/Parts/Top_Default").Value;

            // Calculate the origin for each part (center of the texture)
            Vector2 baseOrigin = new Vector2(baseTexture.Width / 2f, baseTexture.Height / 2f);
            Vector2 bladeOrigin = new Vector2(bladeTexture.Width / 2f, bladeTexture.Height / 2f);
            Vector2 topOrigin = new Vector2(topTexture.Width / 2f, topTexture.Height / 2f);

            // Get the sprite effects based on direction
            SpriteEffects spriteEffects = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            // Draw each component with the same rotation and position
            float rotation = Projectile.rotation;
            Vector2 position = Projectile.Center - Main.screenPosition;

            // Draw base (bottom layer)
            Main.EntitySpriteDraw(
                baseTexture,
                position,
                null,
                lightColor,
                rotation,
                baseOrigin,
                Projectile.scale,
                spriteEffects,
                0);

            // Draw blade (middle layer)
            Main.EntitySpriteDraw(
                bladeTexture,
                position,
                null,
                lightColor,
                rotation,
                bladeOrigin,
                Projectile.scale,
                spriteEffects,
                0);

            // Draw top (top layer)
            Main.EntitySpriteDraw(
                topTexture,
                position,
                null,
                lightColor,
                rotation,
                topOrigin,
                Projectile.scale,
                spriteEffects,
                0);

            return false; // Return false so the original draw doesn't happen
        }
        public void InitializeStats(BeybladeStats newStats)
        {
            stats = newStats;
            base.SetDefaults();
        }

    }
}