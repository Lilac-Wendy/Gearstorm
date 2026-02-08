using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Gearstorm.Content.DamageClasses;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.Graphics.Shaders;

namespace Gearstorm.Content.Projectiles
{
    public class VortexOrbitalProjectile : ModProjectile
    {
        public override string Texture =>
            "Terraria/Images/Projectile_" + ProjectileID.HallowSpray;

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
            Texture2D texture =
                Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;

            Rectangle frame = texture.Frame();
            Vector2 origin = frame.Size() / 2f;

            // =========================
            // PEGAR SHADER DO DYE
            // =========================
            int dyeId = ItemID.VortexDye;

            ArmorShaderData shader =
                GameShaders.Armor.GetShaderFromItemId(dyeId);

            // =========================
            // APLICAR SHADER
            // =========================

            shader.UseOpacity(1f);
            shader.UseColor(Color.White);
            shader.UseSecondaryColor(Color.White);
            shader.UseSaturation(1f);
            shader.UseTargetPosition(Projectile.Center);

            // =========================
            // ATIVAR SHADER
            // =========================
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(
                SpriteSortMode.Immediate,
                BlendState.AlphaBlend,
                SamplerState.LinearClamp,
                DepthStencilState.None,
                RasterizerState.CullNone,
                shader.Shader,
                Main.GameViewMatrix.TransformationMatrix
            );

            Main.EntitySpriteDraw(
                texture,
                Projectile.Center - Main.screenPosition,
                frame,
                Color.White,
                Projectile.rotation,
                origin,
                Projectile.scale,
                SpriteEffects.None,
                0
            );

            // =========================
            // RESTAURAR SPRITEBATCH
            // =========================
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.LinearClamp,
                DepthStencilState.None,
                RasterizerState.CullNone,
                null,
                Main.GameViewMatrix.TransformationMatrix
            );

            return false;
        }


        public override void AI()
        {
            int parentId = (int)Projectile.ai[2];
            if (parentId < 0 || parentId >= Main.maxProjectiles)
            {
                Projectile.Kill();
                return;
            }

            Projectile parent = Main.projectile[parentId];
            if (!parent.active)
            {
                Projectile.Kill();
                return;
            }

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
                Projectile.Kill();
                return;
            }

            // ==========================
            // ROTAÇÃO GLOBAL DA ESPIRAL
            // ==========================
            float globalSpin =
                Main.GlobalTimeWrappedHourly * 0.4f;

            float t = Main.GlobalTimeWrappedHourly * 8f;

            float dnaA = (float)Math.Sin(t) * 12f;
            float dnaB = (float)Math.Sin(t * 1.08f + MathHelper.Pi) * 12f;

            Vector2 spiralDir =
                (angle + globalSpin).ToRotationVector2();

            float combinedRadius =
                radius + (dnaA + dnaB) * 0.5f;

            Projectile.Center =
                parent.Center +
                spiralDir * combinedRadius;

            Projectile.velocity =
                spiralDir * -inwardSpeed;

            Projectile.rotation += 0.35f * spinDir;

            // ==========================
            // RASTRO DNA (DUST)
            // ==========================
            Vector2 normal =
                spiralDir.RotatedBy(MathHelper.PiOver2);

            Color vortex = Color.Lerp(
                new Color(0, 255, 255),
                new Color(80, 255, 160),
                (float)Math.Sin(Main.GlobalTimeWrappedHourly * 4f) * 0.5f + 0.5f
            );
            Color vortex2 = Color.Lerp(                
                new Color(80, 255, 160),
                new Color(0, 255, 255),
                (float)Math.Sin(Main.GlobalTimeWrappedHourly * 4f) * 0.5f + 0.5f
            );

            Dust da = Dust.NewDustPerfect(
                Projectile.Center + normal * dnaA * 0.4f,
                DustID.Vortex,
                Vector2.Zero,
                140,
                vortex,
                0.9f
            );
            da.noGravity = true;

            Dust db = Dust.NewDustPerfect(
                Projectile.Center + normal * dnaB * 0.4f,
                DustID.Vortex,
                Vector2.Zero,
                140,
                vortex2,
                0.9f
            );
            db.noGravity = true;

            Lighting.AddLight(
                Projectile.Center,
                0.0f,
                0.8f,
                0.7f
            );
        }
    }
}
