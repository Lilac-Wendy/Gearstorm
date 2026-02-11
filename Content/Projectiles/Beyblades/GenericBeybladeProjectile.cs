using Gearstorm.Content.Data;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace Gearstorm.Content.Projectiles.Beyblades
{
    public class GenericBeybladeProjectile : BaseBeybladeProjectile
    {
        private Texture2D topTexture;
        private Texture2D baseTexture;
        private Texture2D bladeTexture;

        private string topTexturePath;
        private string baseTexturePath;
        private string bladeTexturePath;

        private bool texturesLoaded;
        private float visualRailOffset;

        public override string Texture => "Terraria/Images/Item_0";

        public void InitializeWithParts(
            BeybladeStats combinedStats,
            string topPath,
            string basePath,
            string bladePath)
        {
            Stats = combinedStats;
            topTexturePath = topPath;
            baseTexturePath = basePath;
            bladeTexturePath = bladePath;
        }

        public override void SetDefaults()
        {
            base.SetDefaults();

            Vector2 center = Projectile.Center;

            Projectile.Resize(Projectile.width, Projectile.height);

            Projectile.Center = center;
        }


        public override bool PreDraw(ref Color lightColor)
        {
            // ================== VISUAL RAIL OFFSET ==================
            if (OnTrack)
            {
                visualRailOffset = MathHelper.Lerp(visualRailOffset, 6f, 0.35f);
            }
            else
            {
                visualRailOffset = MathHelper.Lerp(visualRailOffset, 0f, 0.35f);
            }

            if (!texturesLoaded)
            {
                topTexture = ModContent.Request<Texture2D>(topTexturePath).Value;
                baseTexture = ModContent.Request<Texture2D>(baseTexturePath).Value;
                bladeTexture = ModContent.Request<Texture2D>(bladeTexturePath).Value;
                texturesLoaded = true;
            }

            int frame = Projectile.frame;

            // >>> AQUI está a correção <<<
            Vector2 pos =
                Projectile.Center
                - Main.screenPosition
                + new Vector2(0f, visualRailOffset);

            Rectangle baseFrame  = baseTexture.Frame(1, 2, 0, frame % 2);
            Rectangle bladeFrame = bladeTexture.Frame(1, 4, 0, frame % 4);
            Rectangle topFrame   = topTexture.Frame(1, 2, 0, frame % 2);

            SpriteEffects flip = SpriteEffects.FlipVertically;
            SpriteEffects noflip = SpriteEffects.None;

            // ================== TOP ==================
            Main.EntitySpriteDraw(
                topTexture,
                pos,
                topFrame,
                Projectile.GetAlpha(lightColor),
                Projectile.rotation,
                topFrame.Size() / 2f,
                1f,
                flip
            );

            // ================== BLADE ==================
            Main.EntitySpriteDraw(
                bladeTexture,
                pos,
                bladeFrame,
                Projectile.GetAlpha(lightColor),
                Projectile.rotation,
                bladeFrame.Size() / 2f,
                1f,
                noflip
            );

            // ================== BASE ==================
            Main.EntitySpriteDraw(
                baseTexture,
                pos,
                baseFrame,
                Projectile.GetAlpha(lightColor),
                Projectile.rotation,
                baseFrame.Size() / 2f,
                1f,
                flip
            );

            return false; // cancela o draw vanilla
        }
    }
}
