using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Gearstorm.Content.DamageClasses;
using Microsoft.Xna.Framework.Graphics;
using System;
using Gearstorm.Content.Systems.Primitives;
using Terraria.DataStructures;

namespace Gearstorm.Content.Projectiles
{
    public class VortexOrbitalProjectile : ModProjectile
    {
        public override string Texture =>
            "Terraria/Images/Projectile_" + ProjectileID.MoonBoulder;
        private PrimitiveTrail trailA;
        private PrimitiveTrail trailB;
        

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.friendly = true;
            Projectile.timeLeft = 160;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;

            Projectile.DamageType =
                ModContent.GetInstance<Spinner>();
        } 
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Rectangle frame = texture.Frame();
            Vector2 origin = frame.Size() / 2f;

            Main.EntitySpriteDraw(
                texture,
                Projectile.Center - Main.screenPosition,
                frame,
                Color.White,
                Projectile.rotation,
                origin,
                Projectile.scale,
                SpriteEffects.None
            );

            return false;
        }


        

        public override void OnSpawn(IEntitySource source)
        {
            trailA = new PrimitiveTrail();
            trailB = new PrimitiveTrail();
            Color vortexWhiteCyan = new(190, 255, 210);   // branco esverdeado etéreo
            Color vortexCyan = new(0, 255, 170);          // ciano puxado pro verde
            Color vortexGreen = new(0, 200, 90);          // verde místico intenso
            Color tealBright = new(120, 255, 140);        // verde-luz vibrante
            Color voidDark = new(5, 40, 25);              // fundo verde profundo

            trailA.WidthFunction = progress =>
            {
                float t = progress * progress * (3f - 2f * progress); // SmoothStep inline
                return 4f + (26f - 4f) * t;
            };
            trailB.WidthFunction = progress =>
            {
                float t = progress * progress * (3f - 2f * progress);
                return 3f + (20f - 3f) * t;
            };
            trailA.ColorFunction = progress =>
            {
                float t = progress * progress * (3f - 2f * progress);
                Color baseColor;
                if (progress < 0.3f)
                {
                    float local = progress / 0.3f;
                    baseColor = Color.Lerp(vortexWhiteCyan, vortexCyan, local);
                }
                else
                {
                    float local = (progress - 0.3f) / 0.7f;
                    baseColor = Color.Lerp(vortexCyan, vortexGreen, local);
                }

                return baseColor * t;
            };
            trailB.ColorFunction = progress =>
            {
                float t = progress * progress * (3f - 2f * progress);
                return Color.Lerp(tealBright, voidDark, progress) * t;
            };
        }





public override void AI()
{
    int parentId = (int)Projectile.ai[2];

    bool parentValid =
        parentId >= 0 &&
        parentId < Main.maxProjectiles &&
        Main.projectile[parentId].active;

    if (!parentValid)
    {
        Projectile.velocity = Vector2.Zero;
        Projectile.rotation += 0.1f;
        trailA?.Update();
        trailB?.Update();
        if ((trailA == null || !trailA.IsAlive) &&
            (trailB == null || !trailB.IsAlive))
        {
            Projectile.Kill();
        }

        return;
    }

    Projectile parent = Main.projectile[parentId];

    float angle = Projectile.ai[0];
    float radius = Projectile.ai[1];
    float angularSpeed = 0.25f;
    float inwardSpeed = 4f;
    float spinDir = 1f;
    angle += angularSpeed * spinDir;
    radius -= inwardSpeed;

    Projectile.ai[0] = angle;
    Projectile.ai[1] = radius;
    if (radius <= 10f)
    {
        Projectile.ai[2] = -1;
        return;
    }

    float globalSpin = Main.GlobalTimeWrappedHourly * 0.4f;
    float t = Main.GlobalTimeWrappedHourly * 8f;

    float dnaA = (float)Math.Sin(t) * 12f;
    float dnaB = (float)Math.Sin(t * 1.08f + MathHelper.Pi) * 12f;

    Vector2 spiralDir = (angle + globalSpin).ToRotationVector2();

    float combinedRadius =
        radius + (dnaA + dnaB) * 0.5f;

    Projectile.Center =
        parent.Center +
        spiralDir * combinedRadius;

    Projectile.velocity =
        spiralDir * -inwardSpeed;

    Projectile.rotation += 0.35f * spinDir;

    Vector2 normal = spiralDir.RotatedBy(MathHelper.PiOver2);

    Vector2 posA = Projectile.Center + normal * dnaA * 0.4f;
    Vector2 posB = Projectile.Center + normal * dnaB * 0.4f;

    // 🔥 Novo sistema — adiciona ponto com lifetime
    trailA?.AddPoint(posA, 18f);
    trailB?.AddPoint(posB, 18f);

    trailA?.Update();
    trailB?.Update();

    Lighting.AddLight(
        Projectile.Center,
        0.0f,
        0.8f,
        0.7f
    );
}

    }
}
