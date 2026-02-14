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
            trailA = new PrimitiveTrail(); // agora é a única trail usada

            Color vortexWhiteCyan = new(190, 255, 210);
            Color vortexCyan      = new(0, 255, 170);
            Color vortexGreen     = new(0, 200, 90);
            Color tealBright      = new(120, 255, 140);
            Color voidDark        = new(5, 40, 25);

            // ==========================
            // WIDTH
            // ==========================
            trailA.WidthFunction = progress =>
            {
                progress = MathHelper.Clamp(progress, 0f, 1f);
                float t = MathHelper.SmoothStep(0f, 1f, progress);

                // mais fino perto do centro
                return 4f + 24f * t;
            };

            // ==========================
            // COLOR (gradiente forte)
            // ==========================
            trailA.ColorFunction = progress =>
            {
                progress = MathHelper.Clamp(progress, 0f, 1f);

                // ponta externa clara
                float t = 1f - progress;

                // curva mais agressiva para mudar mais rápido
                t = (float)Math.Pow(t, 0.65f);

                Color color;

                if (t < 0.4f)
                {
                    color = Color.Lerp(vortexWhiteCyan, vortexCyan, t / 0.4f);
                }
                else if (t < 0.75f)
                {
                    color = Color.Lerp(
                        vortexCyan,
                        vortexGreen,
                        (t - 0.4f) / 0.35f
                    );
                }
                else
                {
                    color = Color.Lerp(
                        vortexGreen,
                        voidDark,
                        (t - 0.75f) / 0.25f
                    );
                }

                float alpha = MathHelper.SmoothStep(0f, 1f, progress);

                return color * alpha;
            };
        }







       







    }
}
