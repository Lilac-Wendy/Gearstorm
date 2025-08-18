using System;
using System.Collections.Generic;
using Gearstorm.Content.DamageClasses;
using Gearstorm.Content.Data;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
// Bazinga //
namespace Gearstorm.Content.Projectiles.Beyblades;



public abstract class BaseBeybladeProjectile : ModProjectile 
{
    public float currentSpinSpeed;
    protected int hitCooldown = 0;
    protected float spinSpeed = 1f;
    protected bool onGround = false;
    protected const float SpinFrameSpeed = 0.5f;
    protected virtual bool CollideWithMinecartTracks => true;

    protected int continuousHitCounter = 0;
    protected const float BaseHitsPerSecond = 4f; 
    protected List<NPC> recentlyHitNPCs = new List<NPC>();

    public BeybladeStats stats;

    public float DamageBase => stats.DamageBase;
    public float Mass => stats.Mass;
    public float Balance => stats.Balance;
    public float SpinSpeedProp => stats.SpinSpeed; 
    public float TipFriction => stats.TipFriction;

    public override void SetStaticDefaults()
    {
        Main.projFrames[Projectile.type] = 2;
    }

    public override void SetDefaults()
    {
        Projectile.width = (stats.Radius > 0) ? (int)(stats.Radius * 2 * 16) : 32;
        Projectile.height = (stats.Height > 0) ? (int)(stats.Height * 32) : 32;

        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 600;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = true;
        Projectile.DamageType = ModContent.GetInstance<Spinner>();

        Projectile.knockBack = (stats.KnockbackPower > 0) ? stats.KnockbackPower : 5f;

        if (stats.DamageBase > 0)
        {
            float minimumDamage = stats.DamageBase * 1.0f;
            float calculatedDamage = stats.DamageBase * (1f + stats.Mass * 0.1f) * stats.Balance;
            Projectile.damage = (int)Math.Max(minimumDamage, calculatedDamage);
        }

        Projectile.aiStyle = 0;
        spinSpeed = (stats.SpinSpeed > 0) ? stats.SpinSpeed : 1f;
        currentSpinSpeed = spinSpeed;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D texture = ModContent.Request<Texture2D>(Projectile.ModProjectile.Texture).Value;
        Rectangle sourceRect = texture.Frame(1, Main.projFrames[Projectile.type], 0, Projectile.frame);
        Vector2 origin = sourceRect.Size() / 2f;
        Vector2 drawPosition = Projectile.Center - Main.screenPosition;
        drawPosition.Y += 4f; 

        if (stats.Density < 0.7f)
        {
            float wobbleScale = (0.7f - stats.Density) * 0.3f;
            float wobble = (float)Math.Sin(Main.GameUpdateCount * 0.2f) * wobbleScale;

            Vector2 wobbleScaleVec = new Vector2(1f + wobble, 1f - wobble);
            Main.EntitySpriteDraw(
                texture,
                drawPosition,
                sourceRect,
                lightColor,
                Projectile.rotation,
                origin,
                Projectile.scale * wobbleScaleVec, 
                SpriteEffects.None,
                0
            );

            return false; 
        }
        Main.EntitySpriteDraw(
            texture,
            drawPosition,
            sourceRect,
            lightColor,
            Projectile.rotation,
            origin,
            Projectile.scale,
            SpriteEffects.None,
            0
        );

        return false;
    }

    protected void CheckMinecartTrackCollision(ref bool onTrack)
    {
        Vector2[] detectionPoints = {
            Projectile.Bottom,                         
            Projectile.Bottom + new Vector2(-8, 0),    
            Projectile.Bottom + new Vector2(8, 0)      
        };

        bool isOnTrack = false;
        Vector2 trackPosition = Vector2.Zero;
        Vector2 trackNormal = Vector2.Zero;
        bool foundValidTrack = false;

        foreach (Vector2 point in detectionPoints)
        {
            Point tilePos = point.ToTileCoordinates();
            if (!WorldGen.InWorld(tilePos.X, tilePos.Y)) continue;

            Tile tile = Main.tile[tilePos.X, tilePos.Y];
            if (tile != null && tile.HasTile && tile.TileType == TileID.MinecartTrack)
            {
                isOnTrack = true;
                trackPosition = new Vector2(tilePos.X * 16, tilePos.Y * 16);

                switch (tile.Slope)
                {
                    case SlopeType.SlopeDownLeft:  
                        trackNormal = new Vector2(-1, 1);
                        foundValidTrack = true;
                        break;
                    case SlopeType.SlopeDownRight: 
                        trackNormal = new Vector2(1, 1);
                        foundValidTrack = true;
                        break;
                    case SlopeType.SlopeUpLeft:    
                        trackNormal = new Vector2(-1, -1);
                        foundValidTrack = true;
                        break;
                    case SlopeType.SlopeUpRight:   
                        trackNormal = new Vector2(1, -1);
                        foundValidTrack = true;
                        break;
                    case SlopeType.Solid:          
                        trackNormal = Vector2.UnitY;
                        foundValidTrack = true;
                        break;
                }

                if (foundValidTrack) break;
            }
        }

        if (isOnTrack && foundValidTrack)
        {
            onTrack = true;
            SnapToTrack(trackPosition, trackNormal);
            ApplyTrackPhysics(trackNormal);
            PlayTrackEffects();
        }
        else
        {
            onTrack = false;
        }
    }

    private void SnapToTrack(Vector2 trackPosition, Vector2 trackNormal)
    {
        float baseHeight = 2f;
        float heightAdjust = 0f;

        if (trackNormal.Y > 0) 
        {
            heightAdjust = Math.Abs(trackNormal.X) * 4f;
        }
        else if (trackNormal.Y < 0) 
        {
            heightAdjust = -Math.Abs(trackNormal.X) * 4f;
        }

        float targetY = trackPosition.Y - Projectile.height + baseHeight + heightAdjust;
        float yDiff = targetY - Projectile.position.Y;
        Projectile.position.Y += yDiff * 0.5f; 

        if (Math.Abs(trackNormal.X) > 0.1f)
        {
            float xDiff = trackPosition.X + 8 - Projectile.Center.X;
            Projectile.position.X += xDiff * 0.2f;
        }
    }

    private void ApplyTrackPhysics(Vector2 trackNormal)
    {
        if (trackNormal != Vector2.Zero) 
            trackNormal.Normalize();

        float acceleration = 0.15f;
        float maxSpeed = 15f;

        if (Projectile.velocity.X > 0)
        {
            Projectile.velocity.X = Math.Min(Projectile.velocity.X + acceleration, maxSpeed);
        }
        else if (Projectile.velocity.X < 0)
        {
            Projectile.velocity.X = Math.Max(Projectile.velocity.X - acceleration, -maxSpeed);
        }
        else
        {
            Projectile.velocity.X = acceleration * Math.Sign(Main.player[Projectile.owner].Center.X - Projectile.Center.X);
        }

        if (Math.Abs(trackNormal.Y) > 0.1f)
        {
            Projectile.velocity.Y = -trackNormal.Y * Math.Abs(Projectile.velocity.X) * 0.4f;
        }
        else
        {
            Projectile.velocity.Y = 0;
        }

        stats.TipFriction *= 0.3f;
    }

    private void PlayTrackEffects()
    {
        if (Main.rand.NextBool(30))
        {
            var soundStyle = new SoundStyle("Terraria/Sounds/Item_55")
            {
                Volume = 0.3f,
                Pitch = Main.rand.NextFloat(-0.2f, 0.2f),
                PitchVariance = 0.15f
            };
            SoundEngine.PlaySound(soundStyle, Projectile.position);
        }

        if (Main.rand.NextBool(5))
        {
            int dustType = DustID.Smoke;
            Vector2 dustVelocity = new Vector2(-Math.Sign(Projectile.velocity.X) * 0.8f, -0.5f);
            Dust.NewDustPerfect(Projectile.Bottom, dustType, dustVelocity, 100, default, 0.7f);
        }
    }

    private void ApplyContinuousDamage()
    {

        float hitsPerSecond = BaseHitsPerSecond * spinSpeed;
        int hitInterval = (int)(60 / hitsPerSecond);

        continuousHitCounter++;
        if (continuousHitCounter < hitInterval) return;
        continuousHitCounter = 0;

        recentlyHitNPCs.RemoveAll(npc => !npc.active || !npc.GetGlobalNPC<BeybladeNPC>().IsInContact(Projectile));

        float damageRadius = Projectile.width / 2f;
        foreach (NPC npc in Main.npc)
        {
            if (!npc.active || npc.friendly || npc.life <= 0) continue;

            float distance = Vector2.Distance(Projectile.Center, npc.Center);
            if (distance > damageRadius) continue;

            if (recentlyHitNPCs.Contains(npc)) continue;

            Vector2 knockbackDir = npc.Center - Projectile.Center;
            if (knockbackDir != Vector2.Zero) knockbackDir.Normalize();

            float damageMultiplier = 0.3f + (spinSpeed * 0.1f);
            int continuousDamage = (int)(Projectile.damage * damageMultiplier);

            NPC.HitInfo hitInfo = new NPC.HitInfo()
            {
                Damage = continuousDamage,
                Knockback = stats.KnockbackPower * 0.5f * spinSpeed,
                HitDirection = knockbackDir.X > 0 ? 1 : -1
            };

            npc.StrikeNPC(hitInfo);

            for (int i = 0; i < 3; i++)
            {
                Dust.NewDust(npc.position, npc.width, npc.height, 
                    DustID.Blood, Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f));
            }

            recentlyHitNPCs.Add(npc);
            npc.GetGlobalNPC<BeybladeNPC>().SetContact(Projectile);
        }
    }

    public override void AI()
    {

        Projectile.velocity.Y += 0.3f;

        float velX = Projectile.velocity.X;
        float tiltAmount = MathHelper.Clamp(velX * 0.05f, -0.3f, 0.3f);
        Projectile.rotation = tiltAmount;

        onGround = Collision.SolidCollision(Projectile.position + new Vector2(0, 1), Projectile.width, Projectile.height);

        bool onTrack = false;
        CheckMinecartTrackCollision(ref onTrack);

        float inertiaEffect = MathHelper.Clamp(stats.MomentOfInertia * 0.01f, 0.5f, 2f);
        if (onGround && currentSpinSpeed > 0)
        {
            float spinDecay = 0.02f / inertiaEffect;
            currentSpinSpeed = Math.Max(currentSpinSpeed - spinDecay, 0);
        }

        if (Math.Abs(Projectile.velocity.Y) > 2f && stats.Density < 0.7f)
        {
            float bounceFactor = 1.5f - stats.Density;
            Projectile.velocity.Y *= -0.3f * bounceFactor;
        }

        if (currentSpinSpeed < 0.5f && stats.Density < 0.8f)
        {
            float wobbleIntensity = (0.8f - stats.Density) * 0.1f;
            Projectile.rotation += (float)Math.Sin(Main.GameUpdateCount * 0.1f) * wobbleIntensity;
        }

        spinSpeed = MathHelper.Clamp(currentSpinSpeed, 0.1f, 3f);

        if (spinSpeed > 0.05f)
        {
            if (++Projectile.frameCounter >= (int)(5 / spinSpeed))
            {
                Projectile.frameCounter = 0;
                Projectile.frame = (Projectile.frame + 1) % Main.projFrames[Projectile.type];
            }
        }
        else
        {
            Projectile.frame = 0;
        }

        if (onGround)
        {
            float friction = (stats.TipFriction > 0) ? stats.TipFriction : 0.04f;
            Projectile.velocity.X *= (1f - friction);
            if (Math.Abs(Projectile.velocity.X) < 0.1f)
                Projectile.velocity.X = 0f;
        }

        if (hitCooldown > 0)
            hitCooldown--;

        if (spinSpeed > 0.5f) 
        {
            ApplyContinuousDamage();
        }

        if (hitCooldown == 9) 
        {
            if (stats.Density < 0.6f) 
            {
                for (int i = 0; i < 5; i++)
                {
                    Vector2 sparkVel = new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-3f, -1f));
                    Dust.NewDustPerfect(Projectile.Center, DustID.YellowTorch, sparkVel, 100, default, 1.2f);
                }
            }
            else if (stats.Density > 0.9f) 
            {
                for (int i = 0; i < 3; i++)
                {
                    Vector2 impactVel = new Vector2(Main.rand.NextFloat(-1f, 1f), 0);
                    Dust.NewDustPerfect(Projectile.Bottom, DustID.Iron, impactVel, 150, default, 0.8f);
                }
            }
        }

        for (int i = 0; i < Main.maxProjectiles; i++)
        {
            Projectile other = Main.projectile[i];
            if (other.active && other.type == Projectile.type && other.whoAmI != Projectile.whoAmI)
            {
                if (Projectile.Hitbox.Intersects(other.Hitbox))
                {
                    Vector2 tempVel = Projectile.velocity;
                    Projectile.velocity = other.velocity;
                    other.velocity = tempVel;

                    Projectile.timeLeft -= 10;
                    other.timeLeft -= 10;

                    Vector2 pushDir = Projectile.Center - other.Center;
                    if (pushDir != Vector2.Zero)
                    {
                        pushDir.Normalize();

                        BaseBeybladeProjectile otherBeyblade = other.ModProjectile as BaseBeybladeProjectile;
                        if (otherBeyblade != null)
                        {
                            BeybladeStats otherStats = otherBeyblade.stats;

                            float ourDensityFactor = MathHelper.Clamp(stats.Density * 0.5f, 0.5f, 1.5f);
                            float theirDensityFactor = MathHelper.Clamp(otherStats.Density * 0.5f, 0.5f, 1.5f);

                            float knockbackPower = (stats.KnockbackPower > 0) ? 
                                stats.KnockbackPower * ourDensityFactor : 2f;
                            float otherKnockbackPower = (otherStats.KnockbackPower > 0) ? 
                                otherStats.KnockbackPower * theirDensityFactor : 2f;
                            Projectile.velocity += pushDir * knockbackPower;
                            other.velocity -= pushDir * otherKnockbackPower;
                        }
                    }

                    for (int d = 0; d < 8; d++)
                    {
                        Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Torch,
                            Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f), 150, default, 1.2f);
                    }

                    Terraria.Audio.SoundEngine.PlaySound(SoundID.NPCHit4, Projectile.position);
                }
            }
        }
    }

    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        if (Projectile.velocity.X != oldVelocity.X)
            Projectile.velocity.X = -oldVelocity.X * 0.8f;
        if (Projectile.velocity.Y != oldVelocity.Y)
            Projectile.velocity.Y = -oldVelocity.Y * 0.5f;

        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (hitCooldown <= 0)
        {
            float spinFactor = MathHelper.Clamp(spinSpeed, 0.5f, 2f);

            float velocityBonus = Projectile.velocity.Length() * 0.8f * spinFactor;
            int finalDamage = (int)(Projectile.damage * spinFactor) + (int)velocityBonus;

            float critChance = (stats.SpinSpeed * 0.05f) + (stats.Balance * 0.1f);
            if (Main.rand.NextFloat() < critChance)
            {
                hit.Crit = true;
            }

            hit.SourceDamage = finalDamage;

            target.velocity.X += Math.Sign(Projectile.velocity.X) * 4f * spinFactor;

            float densityFactor = 1f - MathHelper.Clamp(stats.Density * 0.3f, 0f, 0.7f);
            Projectile.velocity.X *= densityFactor;

            hitCooldown = 10;
        }
    }
}
public class BeybladeNPC : GlobalNPC
{
    private Projectile lastContactProjectile;
    private int contactTimer;

    public override bool InstancePerEntity => true;

    public bool IsInContact(Projectile projectile)
    {
        return lastContactProjectile == projectile && contactTimer > 0;
    }

    public void SetContact(Projectile projectile)
    {
        lastContactProjectile = projectile;
        contactTimer = 10; 
    }

    public override void PostAI(NPC npc)
    {
        if (contactTimer > 0) contactTimer--;
    }
}