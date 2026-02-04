using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Gearstorm.Content.DamageClasses;
using Gearstorm.Content.Data;
using Gearstorm.Content.Items.Parts;
using Gearstorm.Content.Items.Parts.Augments;

namespace Gearstorm.Content.Projectiles.Beyblades
{
    public abstract class BaseBeybladeProjectile : ModProjectile
    {
        #region Variables

        public float currentSpinSpeed;
        protected float spinSpeed = 1f;
        protected bool onGround;
        protected int hitCooldown;
        private float dpsAccumulator;
        private int hitRateTimer = 0;
        protected const int AmmoSlotStart = 54;
        protected const int AmmoSlotEnd = 57;

        protected const float SpinToDpsFactor = 4f;
        public int shimmerFlightTimer = 0;
        public Color AugmentColor { get; set; } = Color.Transparent;

        public BeybladeStats stats;

        #endregion

        #region Defaults

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            // ================== HITBOX FÍSICA ==================
            // Raio vem da Blade (em tiles)
            int diameter = (stats.Radius > 0f)
                ? (int)(stats.Radius * 2f * 16f)
                : 32;

            Projectile.width = diameter;
            Projectile.height = diameter;

            // ================== FLAGS BÁSICAS ==================
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 600;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.DamageType = ModContent.GetInstance<Spinner>();

            // ================== OFFSET VISUAL ==================
            // Height do Top influencia SOMENTE o visual
            // Tops mais altos levantam o centro visual
            MathHelper.Clamp(stats.Height, 0f, 1f);

            // Valor negativo puxa o sprite para baixo
            Projectile.gfxOffY = 0f;

            // ================== COMBATE ==================
            Projectile.knockBack = stats.KnockbackPower > 0f
                ? stats.KnockbackPower
                : 5f;

            Projectile.damage = (int)(
                stats.DamageBase *
                (1f + stats.Mass * 0.1f) *
                stats.Balance
            );

            // ================== SPIN ==================
            spinSpeed = stats.BaseSpinSpeed;
            currentSpinSpeed = spinSpeed;

        }


        #endregion
private Color MixAugmentColors(List<Color> colors)
{
    if (colors == null || colors.Count == 0)
        return Color.Transparent;

    if (colors.Count == 1)
        return colors[0];

    float sumX = 0f, sumY = 0f, sumS = 0f, sumV = 0f;
    int validCount = 0;

    // --- PARTE 1: RGB -> HSV E SOMA CIRCULAR ---
    foreach (Color c in colors)
    {
        if (c.A == 0) continue;

        // Conversão RGB para HSV embutida
        float r = c.R / 255f;
        float g = c.G / 255f;
        float b = c.B / 255f;

        float min = Math.Min(r, Math.Min(g, b));
        float max = Math.Max(r, Math.Max(g, b));
        float delta = max - min;

        float h = 0;
        if (delta > 0)
        {
            if (max == r) h = (g - b) / delta + (g < b ? 6 : 0);
            else if (max == g) h = (b - r) / delta + 2;
            else h = (r - g) / delta + 4;
            h /= 6;
        }

        float s = (max == 0) ? 0 : (delta / max);
        float v = max;

        // Lógica circular para evitar cores "mortas" no meio
        float hueRad = h * MathHelper.TwoPi;
        sumX += (float)Math.Cos(hueRad);
        sumY += (float)Math.Sin(hueRad);

        sumS += s;
        sumV += v;
        validCount++;
    }

    if (validCount == 0) return Color.Transparent;

    // --- PARTE 2: MÉDIAS E CLAMPS ---
    float avgHue = (float)Math.Atan2(sumY, sumX) / MathHelper.TwoPi;
    if (avgHue < 0f) avgHue += 1f;

    float avgSat = MathHelper.Clamp(sumS / validCount, 0.55f, 0.9f);
    float avgVal = MathHelper.Clamp(sumV / validCount, 0.6f, 0.9f);

    // --- PARTE 3: HSV -> RGB E RETORNO ---
    int i = (int)Math.Floor(avgHue * 6);
    float f = avgHue * 6 - i;
    float p = avgVal * (1 - avgSat);
    float q = avgVal * (1 - f * avgSat);
    float t = avgVal * (1 - (1 - f) * avgSat);

    float fr, fg, fb;
    switch (i % 6)
    {
        case 0: fr = avgVal; fg = t; fb = p; break;
        case 1: fr = q; fg = avgVal; fb = p; break;
        case 2: fr = p; fg = avgVal; fb = t; break;
        case 3: fr = p; fg = q; fb = avgVal; break;
        case 4: fr = t; fg = p; fb = avgVal; break;
        case 5: fr = avgVal; fg = p; fb = q; break;
        default: fr = fg = fb = 0; break;
    }

    return new Color((byte)(fr * 255), (byte)(fg * 255), (byte)(fb * 255));
}


        #region AI

        public override void AI()
        {
            HandleBeybladeVsBeyblade();

            /* ================== AUGMENT VISUAL (MIXED) ================== */

            if (Projectile.localAI[0] == 0)
            {
                Player player = Main.player[Projectile.owner];

                List<Color> augmentColors = new();

                for (int i = AmmoSlotStart; i <= AmmoSlotEnd; i++)
                {
                    Item item = player.inventory[i];
                    if (!item.IsAir && item.ModItem is BeybladeAugment aug)
                    {
                        if (aug.AugmentColor != Color.Transparent)
                            augmentColors.Add(aug.AugmentColor);
                    }
                }

                AugmentColor = MixAugmentColors(augmentColors);
                Projectile.localAI[0] = 1;
            }


            if (AugmentColor != Color.Transparent && Main.rand.NextBool())
            {
                Vector2 trailPos = Projectile.Bottom + new Vector2(0, -2f);

                Dust d = Dust.NewDustPerfect(
                    trailPos, 
                    DustID.TintableDustLighted,
                    Projectile.velocity * -0.2f, // Faz o rastro ir para trás
                    120,
                    AugmentColor,
                    2.4f
                );
                d.noGravity = true;
            }



            /* ================== TRACK + GRAVITY ================== */

            bool onTrack = false;
            CheckMinecartTrackCollision(ref onTrack);

            switch (onTrack)
            {
                case false:
                    Projectile.velocity.Y += 0.35f; // gravidade correta
                    break;
                case true:
                    Projectile.velocity.Y = Math.Min(Projectile.velocity.Y, 0.1f);
                    break;
            }

            /* ================== GROUND CHECK ================== */
            float groundProbe = 1f;

            onGround =
                Projectile.velocity.Y >= 0f &&
                Collision.SolidCollision(
                    Projectile.BottomLeft + new Vector2(0, groundProbe),
                    Projectile.width,
                    2
                );

            if (onGround)
            {
                // snap único, sem interpolação
                Projectile.velocity.Y = 0f;
            }


            /* ================== SPIN DECAY ================== */

            float inertia = MathHelper.Clamp(stats.MomentOfInertia * 0.01f, 0.5f, 2f);
            if (onGround && currentSpinSpeed > 0f)
            {
                currentSpinSpeed = Math.Max(currentSpinSpeed - (0.02f / inertia), 0f);
            }

            spinSpeed = Math.Max(currentSpinSpeed, 0.1f);


            /* ================== BOUNCE (FIXED) ================== */

            if (onGround && stats.Density < 0.7f && Projectile.velocity.Y > 1f)
            {
                Projectile.velocity.Y *= -0.2f;
            }

            /* ================== FRICTION ================== */

            if (onGround)
            {
                float friction = stats.TipFriction > 0 ? stats.TipFriction : 0.04f;
                Projectile.velocity.X *= (1f - friction);

                if (Math.Abs(Projectile.velocity.X) < 0.1f)
                    Projectile.velocity.X = 0f;
                if (Math.Abs(Projectile.velocity.X) > 1f && spinSpeed > 0.4f && Main.rand.NextBool(4))
                {
                    Dust d = Dust.NewDustPerfect(
                        Projectile.Bottom + new Vector2(Main.rand.NextFloat(-8f, 8f), -2f),
                        DustID.Torch,
                        new Vector2(
                            -Projectile.velocity.X * 0.3f,
                            -1f
                        ),
                        120,
                        AugmentColor == Color.Transparent ? Color.Orange : AugmentColor,
                        0.9f
                    );
                    d.noGravity = true;
                }
                if (Main.rand.NextBool(30))
                {
                    SoundEngine.PlaySound(
                        SoundID.Item10 with { Volume = 0.2f, PitchVariance = 0.1f },
                        Projectile.Center
                    );
                }

            }

            /* ================== ROTATION ================== */

            Projectile.rotation = MathHelper.Clamp(Projectile.velocity.X * 0.05f, -0.3f, 0.3f);

            if (++Projectile.frameCounter >= (int)(5 / spinSpeed))
            {
                Projectile.frameCounter = 0;
                Projectile.frame = (Projectile.frame + 1) % Main.projFrames[Projectile.type];
            }

            /* ================== DAMAGE ================== */

            if (spinSpeed > 0.5f)
                ApplyContinuousDamage();

            if (hitCooldown > 0)
                hitCooldown--;
        }
private void HandleBeybladeVsBeyblade()
{
    for (int i = 0; i < Main.maxProjectiles; i++)
    {
        Projectile other = Main.projectile[i];

        if (!other.active || other.whoAmI == Projectile.whoAmI || other.type != Projectile.type)
            continue;

        if (other.ModProjectile is not BaseBeybladeProjectile otherBey)
            continue;

        // ==== DETECÇÃO DE COLISÃO ====
        // Convertendo raio para pixels (escala 1:16 padrão Terraria)
        float radiusA = stats.Radius * 16f;
        float radiusB = otherBey.stats.Radius * 16f;
        Vector2 delta = Projectile.Center - other.Center;
        float distance = delta.Length();
        float minDist = radiusA + radiusB;

        if (distance > minDist)
            continue;

        // Prevenção de divisão por zero e sobreposição absoluta
        Vector2 normal = distance > 0f ? delta / distance : Vector2.UnitX;

        // ==== FÍSICA DE IMPACTO (COLISÃO ELÁSTICA) ====
        Vector2 relVel = Projectile.velocity - other.velocity;
        float velAlongNormal = Vector2.Dot(relVel, normal);

        // Se já estão se afastando, ignorar
        if (velAlongNormal > 0f)
            continue;

        // Coeficiente de Restituição baseado na densidade média
        // Quanto mais denso, mais o impacto é absorvido (menos "quique" seco)
        float avgDensity = (stats.Density + otherBey.stats.Density) * 0.5f;
        float e = MathHelper.Clamp(1.0f - avgDensity, 0.1f, 0.9f);

        // Massa e Inércia
        float mA = stats.Mass;
        float mB = otherBey.stats.Mass;

        // Cálculo do Impulso Escalar (Fórmula de Colisão com Restituição)
        // j = -(1 + e) * v_rel / (1/mA + 1/mB)
        float j = -(1f + e) * velAlongNormal;
        j /= (1f / mA) + (1f / mB);

        // Modificador de Knockback (Potência de Ataque vs Resistência de Defesa)
        // Usamos a diferença de potências para escalar o impulso
        float attackBonus = (stats.KnockbackPower + otherBey.stats.KnockbackPower) * 0.5f;
        float impulseMag = j * attackBonus;

        Vector2 impulseVec = impulseMag * normal;

        // ==== APLICAÇÃO DE VELOCIDADE ====
        // A resistência reduz a aceleração final de forma inversamente proporcional (a = F / m * resist)
        float resA = 1f / (1f + stats.KnockbackResistance);
        float resB = 1f / (1f + otherBey.stats.KnockbackResistance);

        Projectile.velocity += (impulseVec / mA) * resA;
        other.velocity -= (impulseVec / mB) * resB;

        // ==== RESOLUÇÃO DE PENETRAÇÃO (ANTI-STUCK) ====
        // Empurra levemente para fora para evitar que fiquem presos um dentro do outro
        float overlap = minDist - distance;
        Vector2 separation = normal * (overlap / (mA + mB));
        Projectile.position += separation * mB;
        other.position -= separation * mA;

        // ==== EFEITOS E CALLBACKS ====
        // Passamos o impulso escalar para o tratamento de efeitos
        HandleImpact(normal, impulseMag, other, null);
        otherBey.HandleImpact(-normal, impulseMag, Projectile, null);
    }
}



private void HandleImpact(
    Vector2 normal,
    float impactStrength,
    Projectile otherProj = null,
    NPC targetNPC = null
)
{
    if (normal == Vector2.Zero) normal = -Vector2.UnitY;

    // Normalização da força de impacto baseada na inércia do objeto
    // Impactos mais fortes em objetos com pouca inércia causam mais perda de spin
    float effectiveImpact = impactStrength / (1f + stats.Density);
    
    // ==== PERDA DE SPIN (ENERGIA CINÉTICA) ====
    // O SpinDecay atua como um multiplicador de eficiência
    float spinLoss = (effectiveImpact / stats.MomentOfInertia) * stats.SpinDecay;
    currentSpinSpeed = Math.Max(currentSpinSpeed - spinLoss, 0f);

    // ==== DESGASTE DE TEMPO DE VIDA (TIMELEFT) ====
    // Representa a perda de estabilidade/estamina da Beyblade
    int stabilityLoss = (int)(effectiveImpact * 2f);
    Projectile.timeLeft = Math.Max(Projectile.timeLeft - stabilityLoss, 0);

    // ==== FEEDBACK VISUAL E SONORO ====
    float visualIntensity = (float)Math.Sqrt(effectiveImpact);
    if (visualIntensity > 0.5f)
    {
        float intensity = MathHelper.Clamp(visualIntensity, 0f, 1f);
        float burst = MathF.Pow(intensity, 1.2f); // impacto mais agressivo no pico

        int dustCount = (int)MathHelper.Lerp(6, 18, burst);

        for (int i = 0; i < dustCount; i++)
        {
            float spread = MathHelper.Lerp(0.2f, 0.6f, burst);
            float speed = MathHelper.Lerp(4f, 8f, Main.rand.NextFloat()) * burst;

            Vector2 velocity =
                normal.RotatedByRandom(spread) * speed;

            Dust d = Dust.NewDustPerfect(
                Projectile.Center,
                DustID.FireworksRGB,
                velocity,
                180,
                AugmentColor == Color.Transparent ? Color.OrangeRed : AugmentColor,
                MathHelper.Lerp(0.5f, 0.70f, MathF.Sqrt(burst))
            

            );

            d.noGravity = true;
            d.velocity *= Main.rand.NextFloat(0.85f, 1.1f);
        }
        Dust core = Dust.NewDustPerfect(
            Projectile.Center,
            DustID.Torch,
            Vector2.Zero,
            0,
            AugmentColor == Color.Transparent ? Color.Yellow : AugmentColor,
            1.0f + burst
        );
        core.noGravity = true;
    

        
        SoundEngine.PlaySound(
            SoundID.Item37 with { 
                Volume = MathHelper.Clamp(visualIntensity * 0.3f, 0.2f, 0.8f), 
                PitchVariance = 0.3f,
                Pitch = MathHelper.Clamp(visualIntensity * 0.1f, -0.5f, 0.5f)
            },
            Projectile.Center
        );
    }

    // Se for um NPC, aplicamos o knockback residual aqui, pois NPCs não usam a física de Beyblade
    if (targetNPC != null)
    {
        float npcResist = 1f - targetNPC.knockBackResist;
        targetNPC.velocity += normal * (impactStrength / stats.Mass) * stats.KnockbackPower * npcResist;
    }

    ApplyAugmentOnHit(normal, effectiveImpact, otherProj, targetNPC);
}



private void ApplyAugmentOnHit(Vector2 normal, float impactStrength, Projectile otherProj, NPC targetNPC)
        {
            Player player = Main.player[Projectile.owner];

            for (int i = AmmoSlotStart; i <= AmmoSlotEnd; i++)
            {
                Item item = player.inventory[i];
                if (item.ModItem is not BeybladeAugment aug)
                    continue;

                aug.OnBeybladeHit(
                    Projectile,
                    normal,
                    impactStrength,
                    otherProj,
                    targetNPC
                );
            }
        }


public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Vector2 normal = target.Center - Projectile.Center;
            if (normal == Vector2.Zero)
                normal = Vector2.UnitX;

            normal.Normalize();

            float impactStrength = Projectile.velocity.Length()
                                   * MathF.Sqrt(MathHelper.Clamp(stats.Mass, 0.5f, 5f))
                                   * currentSpinSpeed
                                   / (target.knockBackResist + 1f);

            impactStrength = MathHelper.Clamp(impactStrength, 0f, 8f);

            HandleImpact(normal, impactStrength, null, target);
        }


        #endregion
        private void SnapToTrack(Vector2 trackPosition, Vector2 trackNormal)
        {
            float baseHeight = 2f;

            float targetY = trackPosition.Y - Projectile.height + baseHeight;
            float diff = targetY - Projectile.position.Y;

            Projectile.position.Y += MathHelper.Clamp(diff, -2f, 2f);


            if (Math.Abs(trackNormal.X) > 0.1f)
            {
                float xDiff = (trackPosition.X + 8f) - Projectile.Center.X;
                Projectile.position.X += xDiff * 0.3f;
            }
        }

        #region Minecart Logic (RESTORED + FIXED)

        protected void CheckMinecartTrackCollision(ref bool onTrack)
        {
            Vector2 checkPos = Projectile.Bottom + new Vector2(0, 2);
            Point tilePos = checkPos.ToTileCoordinates();

            if (!WorldGen.InWorld(tilePos.X, tilePos.Y))
                return;

            Tile tile = Main.tile[tilePos.X, tilePos.Y];
            if (tile == null || !tile.HasTile || tile.TileType != TileID.MinecartTrack)
                return;

            onTrack = true;

            Vector2 trackPos = new(tilePos.X * 16, tilePos.Y * 16);

            Vector2 normal = tile.Slope switch
            {
                SlopeType.SlopeDownLeft => new Vector2(-1, 1),
                SlopeType.SlopeDownRight => new Vector2(1, 1),
                SlopeType.SlopeUpLeft => new Vector2(-1, -1),
                SlopeType.SlopeUpRight => new Vector2(1, -1),
                _ => Vector2.UnitY
            };

            SnapToTrack(trackPos, normal);
            ApplyTrackPhysics(normal);
        }


        protected void ApplyTrackPhysics(Vector2 trackNormal)
        {
            if (trackNormal != Vector2.Zero)
                trackNormal.Normalize();

            Projectile.velocity.X += trackNormal.X * 0.15f;

            if (Math.Abs(trackNormal.Y) < 0.1f)
            {
                Projectile.velocity.Y = Math.Min(Projectile.velocity.Y, 0.2f);
            }
            else
            {
                Projectile.velocity.Y += trackNormal.Y * 0.15f;
            }

            stats.TipFriction *= 0.3f;
        }

        #endregion

        #region Tile Collision

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            
            float bounceFactor = (stats.Density < 0.5f) ? 1.0f : 0.5f; 
            if (Projectile.velocity.Y != oldVelocity.Y)
                Projectile.velocity.Y = -oldVelocity.Y * bounceFactor;
            
            if (Projectile.velocity.X != oldVelocity.X)
                Projectile.velocity.X = -oldVelocity.X * 0.8f;

            if (Projectile.velocity.Y != oldVelocity.Y)
                Projectile.velocity.Y = -oldVelocity.Y * 0.5f;
            return false;
        }

        #endregion

        #region Combat

private void ApplyContinuousDamage()
        {
            // 1. Acumula o DPS no "balde"
            float dps = Projectile.damage * SpinToDpsFactor * spinSpeed;
            dpsAccumulator += dps / 60f;
            
            // 2. CALCULA O RITMO DOS HITS (A Mágica)
            // Quanto maior o SpinSpeed, menor o intervalo.
            // Exemplo: 
            // Spin 1.0 -> delay 15 frames (4 hits/s)
            // Spin 2.0 -> delay 7 frames (8 hits/s)
            // Spin 5.0 -> delay 3 frames (20 hits/s)
            int currentDelay = (int)(15f / Math.Max(spinSpeed, 0.1f));

            // TRAVA DE SEGURANÇA:
            // Não deixamos bater mais rápido que a cada 3 frames.

            if (currentDelay < 3) currentDelay = 3;

            hitRateTimer++;

            // Se ainda não chegou a hora de bater, espera acumular mais força
            if (hitRateTimer < currentDelay) 
                return;

            // Hora do Show!
            hitRateTimer = 0;

            // Se mesmo acumulando, não deu nem 1 de dano, aborta (spin muito baixo ou acabou a energia)
            if (dpsAccumulator < 1f) return;

            int damageThisHit = (int)dpsAccumulator;
            dpsAccumulator = 0f; // Esvazia o balde

            float radius = Projectile.width * 0.5f;

            foreach (NPC npc in Main.npc)
            {
                if (!npc.active || npc.friendly) continue;
                
                // Checagem de distância otimizada (Squared evita raiz quadrada)
                if (Vector2.DistanceSquared(Projectile.Center, npc.Center) > radius * radius) continue;

                // Aplica o dano acumulado
                npc.StrikeNPC(new NPC.HitInfo
                {
                    Damage = damageThisHit,
                    Knockback = stats.KnockbackPower * 0.5f,
                    HitDirection = Math.Sign(npc.Center.X - Projectile.Center.X)
                });
                
                // Visual e Som
                // Reduzimos a quantidade de partículas se estiver batendo muito rápido para não lagar
                if (currentDelay > 5 || Main.rand.NextBool(3)) 
                {
                    Dust.NewDustPerfect(npc.Center, DustID.Blood, 
                        Vector2.Normalize(npc.Center - Projectile.Center) * 2f, 
                        80, AugmentColor == Color.Transparent ? Color.Red : AugmentColor, 0.8f);
                }

                // O som muda o pitch conforme a velocidade
                // Somente toca som se não estiver spamando rápido demais (para não estourar o ouvido)
                if (currentDelay > 8 || Main.rand.NextBool(3)) 
                {
                    SoundEngine.PlaySound(
                        SoundID.NPCHit4 with { 
                            Volume = 0.4f, 
                            Pitch = MathHelper.Clamp(spinSpeed * 0.15f, -0.6f, 0.6f),
                            MaxInstances = 3
                        },
                        npc.Center
                    );
                }
            }
        }

        #endregion
    }

    #region Global NPC

    public class BeybladeNPC : GlobalNPC
    {
        private Projectile lastContact;
        private int timer;

        public override bool InstancePerEntity => true;

        public bool IsInContact(Projectile proj)
            => lastContact == proj && timer > 0;

        public void SetContact(Projectile proj)
        {
            lastContact = proj;
            timer = 10;
        }

        public override void PostAI(NPC npc)
        {
            if (timer > 0) timer--;
        }
    }

    #endregion
}
