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


        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.friendly = true;
            Projectile.timeLeft = 1400;
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
        public override void AI()
        {
            int parentId = (int)Projectile.ai[2];

            if (parentId < 0 ||
                parentId >= Main.maxProjectiles ||
                !Main.projectile[parentId].active)
            {
                Projectile.Kill();
                return;
            }

            Projectile parent = Main.projectile[parentId];

            float phase = Projectile.ai[1]; 

            float angularSpeed = 0.07f;
            Projectile.ai[0] += angularSpeed;
            float theta = Projectile.ai[0];

            float R0 = 260f;
            float k = 16f;
            float power = 1.08f;

            float baseRadius =
                R0 - k * (float)Math.Pow(theta, power);

            if (baseRadius <= 12f)
            {
                Projectile.Kill();
                return;
            }

            float modulationStrength = 0.12f;
            float frequency = 10f;

            float finalRadius =
                baseRadius *
                (1f + modulationStrength *
                    (float)Math.Sin(frequency * theta + phase));

            Vector2 direction = theta.ToRotationVector2();

            Projectile.Center =
                parent.Center +
                direction * finalRadius;

            Projectile.velocity =
                direction.RotatedBy(MathHelper.PiOver2) * 1.4f;

            Projectile.rotation += 0.15f;

            trailA?.AddPoint(Projectile.Center, 18f);
            trailA?.Update();

            Lighting.AddLight(
                Projectile.Center,
                0.0f,
                0.9f,
                0.7f
            );
        }



        public override void OnSpawn(IEntitySource source)
        {
            trailA = new PrimitiveTrail();

            Color electricWhite = new(220, 255, 240);
            Color neonCyan      = new(0, 255, 255);
            Color vortexTeal    = new(0, 180, 120);
            Color deepVoid      = new(10, 30, 20);

            Color[] palette = new[] { electricWhite, neonCyan, vortexTeal, deepVoid };

            trailA.WidthFunction = progress =>
            {
                float pulse = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 12f);
                float ease = (float)Math.Sin(progress * MathHelper.PiOver2);
                return MathHelper.Lerp(10f, 20f, ease) + pulse * 2f;
            };

            trailA.ColorFunction = progress =>
            {
                float time = Main.GlobalTimeWrappedHourly;

                // Oscilação rápida ao longo da trail
                float wave = (float)Math.Sin(progress * 20f - time * 10f);

                // Mistura progress + tempo
                float t = progress * 3f + time * 2f + wave * 0.3f;

                // Índices dinâmicos
                int indexA = (int)Math.Abs(Math.Floor(t)) % palette.Length;
                int indexB = (indexA + 1 + (int)(time * 3f)) % palette.Length;

                float lerpFactor = t - (float)Math.Floor(t);

                Color result = Color.Lerp(palette[indexA], palette[indexB], lerpFactor);

                // Flash energético no início
                if (progress < 0.15f)
                {
                    float flash = 1.5f + (float)Math.Sin(time * 30f) * 0.5f;
                    result *= flash;
                }

                return result;
            };

        }







       







    }
}
