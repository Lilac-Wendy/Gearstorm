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

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 8;
        }

        public void InitializeWithParts(BeybladeStats combinedStats, string topPath, string basePath, string bladePath)
        {
            Stats = combinedStats;
            topTexturePath = topPath;
            baseTexturePath = basePath;
            bladeTexturePath = bladePath;
            texturesLoaded = false; 
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
            int size = (Stats.Radius > 0) ? (int)(Stats.Radius * 32) : 32;
            Projectile.width = size;
            Projectile.height = size;
        }
 public override bool PreDraw(ref Color lightColor)
{
    // 1. VERIFICAÇÃO DE CARREGAMENTO
    if (!texturesLoaded)
    {
        if (!string.IsNullOrEmpty(topTexturePath))
        {
            topTexture = ModContent.Request<Texture2D>(topTexturePath).Value;
            bladeTexture = ModContent.Request<Texture2D>(bladeTexturePath).Value;
            baseTexture = ModContent.Request<Texture2D>(baseTexturePath).Value;

            texturesLoaded = (topTexture != null && baseTexture != null && bladeTexture != null);
        }
        return false; 
    }

    // CORREÇÃO DO SPRITEBATCH:
    // Fechamos o batch atual do Terraria para poder iniciar o nosso com as flags específicas
    Main.spriteBatch.End();
    Main.spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

    Vector2 drawPos = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
    Color drawColor = Projectile.GetAlpha(lightColor);

    int frameHeight = 20;
    int startY = frameHeight * Projectile.frame;
    Rectangle sourceRectangle = new Rectangle(0, startY, 88, frameHeight);

    Vector2 origin = new Vector2(44f, 10f);
    float stackGap = 6f; 
    Vector2 topOffset = new Vector2(0, stackGap).RotatedBy(Projectile.rotation);
    Vector2 baseOffset = new Vector2(0, -stackGap).RotatedBy(Projectile.rotation);

    // 1. BASE
    Main.EntitySpriteDraw(
        baseTexture, 
        drawPos + baseOffset, 
        sourceRectangle, 
        drawColor, 
        Projectile.rotation, 
        origin, 
        Projectile.scale, 
        SpriteEffects.None, 
        0.1f // Valor de profundidade para o BackToFront
    );
    
    // 2. BLADE (meio)
    Main.EntitySpriteDraw(
        bladeTexture, 
        drawPos, 
        sourceRectangle, 
        drawColor, 
        Projectile.rotation, 
        origin, 
        Projectile.scale, 
        SpriteEffects.None, 
        0.5f
    );
    
    // 3. TOP (Frente)
    Main.EntitySpriteDraw(
        topTexture, 
        drawPos + topOffset, 
        sourceRectangle, 
        drawColor, 
        Projectile.rotation, 
        origin, 
        Projectile.scale, 
        SpriteEffects.None, 
        0.9f
    );

    Main.spriteBatch.End();
    // Reabrimos o batch padrão para o Terraria continuar desenhando o resto sem crashar
    Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

    return false; 
}
    }
}