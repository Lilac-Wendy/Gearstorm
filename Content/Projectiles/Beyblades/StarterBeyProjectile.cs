using System;
using Gearstorm.Content.Data;

using Microsoft.Xna.Framework;

using Microsoft.Xna.Framework.Graphics;

using Terraria;

using Terraria.ModLoader;


namespace Gearstorm.Content.Projectiles.Beyblades

{

    public class StarterBeyProjectile : BaseBeybladeProjectile

    {

        public override string Texture => "Gearstorm/Assets/Items/StarterBey"; // NotReallyUsed


        public override void SetDefaults()

        {
            if (stats.Mass > 0 || stats.DamageBase > 0)
                base.SetDefaults();
            Projectile.scale = 1.5f;

        }


public override bool PreDraw(ref Color lightColor)
{
    Texture2D topTexture   = ModContent.Request<Texture2D>("Gearstorm/Assets/Items/Parts/Top_Default").Value;
    Texture2D baseTexture  = ModContent.Request<Texture2D>("Gearstorm/Assets/Items/Parts/Base_Default").Value;
    Texture2D bladeTexture = ModContent.Request<Texture2D>("Gearstorm/Assets/Items/Parts/Blade_Default").Value;

    
    int frame = Projectile.frame;
    int totalFramesBlade = 4;
    int totalFrames = 2;
    float scaleFactor = 0.5f;
    float uniformScale = scaleFactor * Projectile.scale;
    
    int frameHeightBase  = baseTexture.Height  / totalFrames;
    int frameHeightBlade = bladeTexture.Height / totalFramesBlade;
    int frameHeightTop   = topTexture.Height   / totalFrames;
    
    Rectangle baseFrame  = new Rectangle(0, frame * frameHeightBase,  baseTexture.Width,  frameHeightBase);
    Rectangle bladeFrame = new Rectangle(0, frame * frameHeightBlade, bladeTexture.Width, frameHeightBlade);
    Rectangle topFrame   = new Rectangle(0, frame * frameHeightTop,   topTexture.Width,   frameHeightTop);
    
    Vector2 baseOrigin  = new Vector2(baseFrame.Width / 2f,  frameHeightBase / 2f);
    Vector2 bladeOrigin = new Vector2(bladeFrame.Width / 2f, frameHeightBlade / 2f);
    Vector2 topOrigin   = new Vector2(topFrame.Width / 2f,   frameHeightTop / 2f);
    
    SpriteEffects spriteEffects = Projectile.spriteDirection == 1
        ? SpriteEffects.None
        : SpriteEffects.FlipHorizontally;
    
    float rotation = Projectile.rotation;
    Vector2 position = Projectile.Center - Main.screenPosition;
    
    Main.EntitySpriteDraw(topTexture, position, topFrame, lightColor, rotation, topOrigin, uniformScale, spriteEffects, 0);
    Main.EntitySpriteDraw(baseTexture, position, baseFrame, lightColor, rotation, baseOrigin, uniformScale, spriteEffects, 0);
    Main.EntitySpriteDraw(bladeTexture, position, bladeFrame, lightColor, rotation, bladeOrigin, uniformScale, spriteEffects, 0);

    if (spinSpeed > 1f)
    {
        for (int i = 0; i < 2; i++)
        {
            float blurRotation = rotation + (i * 0.2f);
            Color blurColor = lightColor * 0.3f;
            Main.EntitySpriteDraw(bladeTexture, position, bladeFrame, blurColor, 
                blurRotation, bladeOrigin, uniformScale, spriteEffects, 0);
        }
    }
    return false; 
}

        public void InitializeStats(BeybladeStats newStats)

        {

            stats = newStats;

            base.SetDefaults(); 

        }

    }

}


