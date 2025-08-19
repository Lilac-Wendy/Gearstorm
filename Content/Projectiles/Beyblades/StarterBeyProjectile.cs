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

// só aplica se stats já foi inicializado pelo item

            if (stats.Mass > 0 || stats.DamageBase > 0)

                base.SetDefaults();


            Projectile.scale = 1.0f;

        }


public override bool PreDraw(ref Color lightColor)
{
    Texture2D baseTexture  = ModContent.Request<Texture2D>("Gearstorm/Assets/Items/Parts/Base_Default").Value;
    Texture2D bladeTexture = ModContent.Request<Texture2D>("Gearstorm/Assets/Items/Parts/Blade_Default").Value;
    Texture2D topTexture   = ModContent.Request<Texture2D>("Gearstorm/Assets/Items/Parts/Top_Default").Value;
    int frame = Projectile.frame;
    int frameHeightBase  = baseTexture.Height  / Main.projFrames[Projectile.type];
    int frameHeightBlade = bladeTexture.Height / Main.projFrames[Projectile.type];
    int frameHeightTop   = topTexture.Height   / Main.projFrames[Projectile.type];
    Rectangle baseFrame  = new Rectangle(0, frame * frameHeightBase,  baseTexture.Width,  frameHeightBase);
    Rectangle bladeFrame = new Rectangle(0, frame * frameHeightBlade, bladeTexture.Width, frameHeightBlade);
    Rectangle topFrame   = new Rectangle(0, frame * frameHeightTop,   topTexture.Width,   frameHeightTop);
    Vector2 baseOrigin  = new Vector2(baseFrame.Width / 2f,  baseFrame.Height / 2f);
    Vector2 bladeOrigin = new Vector2(bladeFrame.Width / 2f, bladeFrame.Height / 2f);
    Vector2 topOrigin   = new Vector2(topFrame.Width / 2f,   topFrame.Height / 2f);
    SpriteEffects spriteEffects = Projectile.spriteDirection == 1
        ? SpriteEffects.None
        : SpriteEffects.FlipHorizontally;
    float rotation = Projectile.rotation;
    Vector2 position = Projectile.Center - Main.screenPosition;
    Main.EntitySpriteDraw(baseTexture, position, baseFrame, lightColor, rotation, baseOrigin, Projectile.scale, spriteEffects, 0);
    Main.EntitySpriteDraw(bladeTexture, position, bladeFrame, lightColor, rotation, bladeOrigin, Projectile.scale, spriteEffects, 0);
    Main.EntitySpriteDraw(topTexture, position, topFrame, lightColor, rotation, topOrigin, Projectile.scale, spriteEffects, 0);
    return false; 
}

        public void InitializeStats(BeybladeStats newStats)

        {

            stats = newStats;

            base.SetDefaults(); 

        }

    }

}


