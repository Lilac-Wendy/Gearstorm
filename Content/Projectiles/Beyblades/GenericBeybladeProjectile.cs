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
            Projectile.scale = 1.5f;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (!texturesLoaded)
            {
                topTexture = ModContent.Request<Texture2D>(topTexturePath).Value;
                baseTexture = ModContent.Request<Texture2D>(baseTexturePath).Value;
                bladeTexture = ModContent.Request<Texture2D>(bladeTexturePath).Value;
                texturesLoaded = true;
            }

            int frame = Projectile.frame;
            float scale = 0.5f * Projectile.scale;
    
            // Se ele está "afundado" ou de cabeça para baixo, 
            // talvez o offset precise ser negativo (-14f) para subir.
            // Ajuste este valor conforme necessário.
            Vector2 pos = Projectile.Center - Main.screenPosition + new Vector2(0f, 14f);

            Rectangle baseFrame = baseTexture.Frame(1, 2, 0, frame % 2);
            Rectangle bladeFrame = bladeTexture.Frame(1, 4, 0, frame % 4);
            Rectangle topFrame = topTexture.Frame(1, 2, 0, frame % 2);

            // Usamos FlipVertically se a sprite original estiver invertida no eixo Y
            SpriteEffects flip = SpriteEffects.FlipVertically; 
    
            // Se você quer que ele rode normalmente mas as imagens apareçam "certas",
            // desenhamos na ordem Base -> Blade -> Top:

            // 1. BASE
            Main.EntitySpriteDraw(
                baseTexture, pos, baseFrame, lightColor,
                Projectile.rotation, baseFrame.Size() / 2f,
                scale, flip, 0
            );
    
            // 2. BLADE
            Main.EntitySpriteDraw(
                bladeTexture, pos, bladeFrame, lightColor,
                Projectile.rotation, bladeFrame.Size() / 2f,
                scale, flip, 0
            );
    
            // 3. TOP (Agora realmente no topo visual e na ordem de desenho)
            Main.EntitySpriteDraw(
                topTexture, pos, topFrame, lightColor,
                Projectile.rotation, topFrame.Size() / 2f,
                scale, flip, 0
            );

            return false;
        }
    }
}
