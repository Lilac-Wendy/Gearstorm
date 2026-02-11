using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Gearstorm.Content.DamageClasses;
using Gearstorm.Content.Data;
using Gearstorm.Content.Items.Parts;

namespace Gearstorm.Content.Projectiles.Beyblades
{
    public abstract class BaseBeybladeProjectile : ModProjectile
    {
        #region Variables
        //
        public bool LastHitWasCrit;
        public float CurrentSpinSpeed;
        protected float SpinSpeed = 1f;
        //
        protected bool OnGround;


        //
        protected int HitCooldown;
        private int hitRateTimer;
        protected const int AMMO_SLOT_START = 54;
        protected const int AMMO_SLOT_END = 57;
        //
        public float CritMultiplier => Stats.CritMultiplier;
        protected static bool CanCrit(Player player, NPC target) => true;
        //
        public Color AugmentColor { get; set; } = Color.Transparent;

        public BeybladeStats Stats;
        //
        #endregion
        #region Defaults and ModifyHitNPC

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            // ================== HITBOX FÍSICA ==================
            int diameter = (Stats.Radius > 0f)
                ? (int)(Stats.Radius * 2f * 16f)
                : 16;

            Projectile.Resize(diameter,diameter);
            // ================== FLAGS BÁSICAS ==================
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 600;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.DamageType = ModContent.GetInstance<Spinner>();
            

            // ================== COMBATE ==================
            Projectile.knockBack = Stats.KnockbackPower > 0f
                ? Stats.KnockbackPower
                : 5f;

            Projectile.damage = (int)(
                Stats.DamageBase *
                (1f + Stats.Mass * 0.1f) *
                Stats.Balance
            );

            // ================== SPIN ==================
            SpinSpeed = Stats.BaseSpinSpeed;
            CurrentSpinSpeed = SpinSpeed;

        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            // Só Spinner usa o sistema custom
            if (Projectile.DamageType is not Spinner)
                return;

            Player player = Main.player[Projectile.owner];

            // Crit chance BASE da build (0–1)
            float critChance = MathHelper.Clamp(Stats.CritChance, 0f, 1f);

            if (critChance <= 0f || !CanCrit(player, target))
            {
                LastHitWasCrit = false;
                return;
            }

            // Roll custom sem SpinEfficiency
            if (Main.rand.NextFloat() < critChance)
            {
                modifiers.SetCrit();

                // Multiplicador custom pós-defesa
                modifiers.FinalDamage *= Stats.CritMultiplier;

                LastHitWasCrit = true;
            }
            else
            {
                LastHitWasCrit = false;
            }

            // DEBUG
            Main.NewText(
                $"CritChance={(critChance*100f):F0}% | Mult={Stats.CritMultiplier:F2}",
                Color.Orange
            );
        }



       
    
    


        #endregion
        #region Helper Color Code
private Color MixAugmentColors(List<Color> colors)
{
    if (colors == null || colors.Count == 0)
        return Color.Transparent;

    if (colors.Count == 1)
        return colors[0];

    float sumX = 0f, sumY = 0f, sumS = 0f, sumV = 0f;
    int validCount = 0;
    
    foreach (Color c in colors)
    {
        if (c.A == 0) continue;


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
#endregion
        #region AI

        public override void AI()
        {
            HandleBeybladeVsBeyblade();
            UpdateRailGrind();
            
// ================== RAIL LEVITATION BEAM ==================
if (OnTrack && Main.rand.NextBool(2))
{
    Vector2 beamPos = Projectile.Bottom + new Vector2(Main.rand.NextFloat(-4f, 4f), 2f);
    
    // Cores do beam com variação
    Color beamColor = AugmentColor == Color.Transparent ? 
        Color.Lerp(Color.LightBlue, Color.Cyan, Main.rand.NextFloat(0.3f)) : 
        Color.Lerp(AugmentColor, Color.White, 0.3f);
    
    // Beam principal (mais intenso)
    Dust beam = Dust.NewDustPerfect(
        beamPos,
        DustID.GolfPaticle,
        new Vector2(Main.rand.NextFloat(-0.2f, 0.2f), Main.rand.NextFloat(1.2f, 2f)),
        100,
        beamColor,
        Main.rand.NextFloat(1.1f, 1.6f)
    );
    
    beam.noGravity = true;
    beam.rotation = MathHelper.PiOver2; // Rotação vertical
    beam.fadeIn = 1.2f; // Suaviza o aparecimento
    
    // Partículas secundárias (para efeito de "fumaça" ou resíduo)
    if (Main.rand.NextBool(3))
    {
        Dust secondary = Dust.NewDustPerfect(
            beamPos + new Vector2(Main.rand.NextFloat(-2f, 2f), 0),
            DustID.Smoke,
            new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), Main.rand.NextFloat(0.5f, 1.2f)),
            150,
            beamColor.MultiplyRGB(Color.Gray) * 0.7f,
            Main.rand.NextFloat(0.7f, 1.1f)
        );
        
        secondary.noGravity = true;
        secondary.velocity *= 0.5f;
    }
    
    // Efeito de "ponta" do beam (parte mais brilhante no início)
    if (Main.rand.NextBool(4))
    {
        Dust tip = Dust.NewDustPerfect(
            beamPos,
            DustID.Electric,
            new Vector2(0, Main.rand.NextFloat(0.8f, 1.5f)),
            80,
            beamColor.MultiplyRGB(Color.White) * 1.2f,
            Main.rand.NextFloat(0.5f, 0.9f)
        );
        
        tip.noGravity = true;
        tip.velocity *= 0.3f;
    }
}

// Adicional: Para fazer os beams diminuírem ao longo do tempo,
// você precisaria de um ModDust personalizado, mas aqui está uma alternativa


if (OnTrack && Main.rand.NextBool(3)) // Frequência reduzida para beams mais longos
{
    Vector2 beamPos = Projectile.Bottom + new Vector2(Main.rand.NextFloat(-3f, 3f), 1f);
    
    // Cria uma sequência de dusts que diminuem em escala
    for (int i = 0; i < 3; i++) // 3 partículas em sequência
    {
        float offsetY = i * 4f; // Espaçamento vertical
        float scale = 1.4f - (i * 0.3f); // Diminui a escala
        
        Dust sequentialBeam = Dust.NewDustPerfect(
            beamPos + new Vector2(0, offsetY),
            DustID.GolfPaticle,
            new Vector2(Main.rand.NextFloat(-0.1f, 0.1f), 0.2f), // Movimento mais lento
            120 + (i * 40), // Aumenta transparência
            AugmentColor == Color.Transparent ? 
                Color.Lerp(Color.LightBlue, Color.White, i * 0.2f) : 
                Color.Lerp(AugmentColor, Color.White, i * 0.15f),
            scale
        );
        
        sequentialBeam.noGravity = true;
        sequentialBeam.rotation = MathHelper.PiOver2;
        sequentialBeam.velocity *= 0.7f;
    }
}

            /* ================== AUGMENT VISUAL (MIXED) ================== */

            if (Projectile.localAI[0] == 0)
            {
                Player player = Main.player[Projectile.owner];

                List<Color> augmentColors = new();

                for (int i = AMMO_SLOT_START; i <= AMMO_SLOT_END; i++)
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
            


            /* ================== GROUND CHECK ================== */
            float groundProbe = 1f;

            OnGround =
                Projectile.velocity.Y >= 0f &&
                Collision.SolidCollision(
                    Projectile.BottomLeft + new Vector2(0, groundProbe),
                    Projectile.width,
                    1
                );

            if (OnGround)
            {
                // snap único, sem interpolação
                Projectile.velocity.Y = 0f;
            }


            /* ================== SPIN DECAY ================== */

            float inertia = MathHelper.Clamp(Stats.MomentOfInertia * 0.01f, 0.5f, 2f);
            if (OnGround && CurrentSpinSpeed > 0f)
            {
                CurrentSpinSpeed = Math.Max(CurrentSpinSpeed - (0.02f / inertia), 0f);
            }

            SpinSpeed = Math.Max(CurrentSpinSpeed, 0.1f);


            /* ================== BOUNCE (FIXED) ================== */

            if (OnGround && Stats.Density < 0.7f && Projectile.velocity.Y > 1f)
            {
                Projectile.velocity.Y *= -0.2f;
            }

            /* ================== FRICTION ================== */

            if (OnGround)
            {
                float friction = Stats.TipFriction > 0 ? Stats.TipFriction : 0.04f;
                Projectile.velocity.X *= (1f - friction);

                if (Math.Abs(Projectile.velocity.X) < 0.1f)
                    Projectile.velocity.X = 0f;
                if (Math.Abs(Projectile.velocity.X) > 1f && SpinSpeed > 0.4f && Main.rand.NextBool(4))
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

            if (++Projectile.frameCounter >= (int)(5 / SpinSpeed))
            {
                Projectile.frameCounter = 0;
                Projectile.frame = (Projectile.frame + 1) % Main.projFrames[Projectile.type];
            }

            /* ================== DAMAGE ================== */

            if (SpinSpeed > 0.5f)
                ApplyContinuousDamage();

            if (HitCooldown > 0)
                HitCooldown--;
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
        float radiusA = Stats.Radius * 16f;
        float radiusB = otherBey.Stats.Radius * 16f;
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
        float avgDensity = (Stats.Density + otherBey.Stats.Density) * 0.5f;
        float e = MathHelper.Clamp(1.0f - avgDensity, 0.1f, 0.9f);

        // Massa e Inércia
        float mA = Stats.Mass;
        float mB = otherBey.Stats.Mass;

        // Cálculo do Impulso Escalar (Fórmula de Colisão com Restituição)
        // j = -(1 + e) * v_rel / (1/mA + 1/mB)
        float j = -(1f + e) * velAlongNormal;
        j /= (1f / mA) + (1f / mB);

        // Modificador de Knockback (Potência de Ataque vs Resistência de Defesa)
        // Usamos a diferença de potências para escalar o impulso
        float attackBonus = (Stats.KnockbackPower + otherBey.Stats.KnockbackPower) * 0.5f;
        float impulseMag = j * attackBonus;

        Vector2 impulseVec = impulseMag * normal;

        // ==== APLICAÇÃO DE VELOCIDADE ====
        // A resistência reduz a aceleração final de forma inversamente proporcional (a = F / m * resist)
        float resA = 1f / (1f + Stats.KnockbackResistance);
        float resB = 1f / (1f + otherBey.Stats.KnockbackResistance);

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
        HandleImpact(normal, impulseMag, other);
        otherBey.HandleImpact(-normal, impulseMag, Projectile);
    }
}



private void HandleImpact(
    Vector2 normal,
    float impactStrength,
    Projectile otherProj = null,
    NPC targetNpc = null
)
{
    if (normal == Vector2.Zero) normal = -Vector2.UnitY;

    // Normalização da força de impacto baseada na inércia do objeto
    // Impactos mais fortes em objetos com pouca inércia causam mais perda de spin
    float effectiveImpact = impactStrength / (1f + Stats.Density);
    
    // ==== PERDA DE SPIN (ENERGIA CINÉTICA) ====
    // O SpinDecay atua como um multiplicador de eficiência
    float spinLoss = (effectiveImpact / Stats.MomentOfInertia) * Stats.SpinDecay;
    CurrentSpinSpeed = Math.Max(CurrentSpinSpeed - spinLoss, 0f);

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
    if (targetNpc != null)
    {
        float npcResist = 1f - targetNpc.knockBackResist;
        targetNpc.velocity += normal * (impactStrength / Stats.Mass) * Stats.KnockbackPower * npcResist;
    }

    ApplyAugmentOnHit(normal, effectiveImpact, otherProj, targetNpc);
}



private void ApplyAugmentOnHit(Vector2 normal, float impactStrength, Projectile otherProj, NPC targetNpc)
        {
            Player player = Main.player[Projectile.owner];

            for (int i = AMMO_SLOT_START; i <= AMMO_SLOT_END; i++)
            {
                Item item = player.inventory[i];
                if (item.ModItem is not BeybladeAugment aug)
                    continue;

                aug.OnBeybladeHit(
                    Projectile,
                    normal,
                    impactStrength,
                    otherProj,
                    targetNpc,
                    LastHitWasCrit
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
                                   * MathF.Sqrt(MathHelper.Clamp(Stats.Mass, 0.5f, 5f))
                                   * CurrentSpinSpeed
                                   / (target.knockBackResist + 1f);

            impactStrength = MathHelper.Clamp(impactStrength, 0f, 8f);

            HandleImpact(normal, impactStrength, null, target);
        }


        #endregion
       #region Minecart Logic (tML 1.4.4+ — CRASH FIXED)

       public bool OnTrack;
private Vector2 lastBoost;
private int fallStart;

/// <summary>
/// Atualiza o comportamento de grind nas tracks usando o sistema vanilla.
/// </summary>
private void UpdateRailGrind()
{
    int owner = Projectile.owner;
    if (owner < 0 || owner >= Main.maxPlayers)
    {
        ApplyNormalGravity();
        OnTrack = false;
        return;
    }

    Player player = Main.player[owner];
    if (player == null || !player.active)
    {
        ApplyNormalGravity();
        OnTrack = false;
        return;
    }

    // Mount ou delegates ainda não inicializados → NÃO PODE chamar TrackCollision
    if (player.mount == null ||
        player.mount._data == null ||
        player.mount._data.delegations == null)
    {
        ApplyNormalGravity();
        OnTrack = false;
        return;
    }

    // Necessário para rampas
    if (Projectile.velocity.Y == 0f)
        fallStart = (int)(Projectile.position.Y / 16f);

    BitsByte trackFlags = Minecart.TrackCollision(
        player,
        ref Projectile.position,
        ref Projectile.velocity,
        ref lastBoost,
        Projectile.width,
        Projectile.height,
        followDown: false,
        followUp: false,
        fallStart: fallStart,
        trackOnly: false,
        delegatesData: player.mount._data.delegations
    );

    OnTrack = trackFlags[Minecart.Flag_OnTrack];

    if (OnTrack)
        ApplyRailPhysics(trackFlags);
    else
        ApplyNormalGravity();
}

private void ApplyRailPhysics(BitsByte trackFlags)
{
    Projectile.gfxOffY = -16f;
    
    Projectile.velocity.Y = 0f;
    if (trackFlags[Minecart.Flag_BoostLeft])
        Projectile.velocity.X -= Minecart.BoosterSpeed;

    if (trackFlags[Minecart.Flag_BoostRight])
        Projectile.velocity.X += Minecart.BoosterSpeed;

    // Bumpers elásticos
    if (trackFlags[Minecart.Flag_BouncyBumper])
        Projectile.velocity *= 1.1f;

    // Hammer / switch
    if (trackFlags[Minecart.Flag_HitSwitch])
    {
        Minecart.HitTrackSwitch(
            Projectile.position,
            Projectile.width,
            Projectile.height
        );
    }

    // Atrito leve de grind
    Projectile.velocity *= 0.995f;

    // Integração com seus stats
    Stats.TipFriction *= 0.3f;
}

private void ApplyNormalGravity()
{
    Projectile.velocity.Y += 0.35f;
}

#endregion


        #region Tile Collision

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            
            float bounceFactor = (Stats.Density < 0.5f) ? 1.0f : 0.5f; 
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
            // ================== CALCULA INTERVALO DE HITS ==================
            float aps = SpinSpeed * 0.25f;
            int hitDelay = (int)MathHelper.Clamp(60f / Math.Max(aps, 0.1f), 3, 60);
    
            hitRateTimer++;

            if (hitRateTimer < hitDelay)
                return;

            hitRateTimer = 0;
            float radius = Projectile.width * 0.5f;

            // ================== VERIFICA TODOS OS NPCs ==================
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];

                // Filtros básicos
                if (!npc.active || npc.friendly || npc.dontTakeDamage || npc.immune[Projectile.owner] > 0)
                    continue;

                // Distância
                if (Vector2.DistanceSquared(Projectile.Center, npc.Center) > (radius + 24f) * (radius + 24f))
                    continue;

       

                // ================== APLICA O DANO ==================
                // Cria HitInfo com a flag de crítico correta
                NPC.HitInfo hitInfo = new NPC.HitInfo
                {
                    Damage = Projectile.damage,
                    Knockback = Projectile.knockBack * 0.5f,
                    HitDirection = Math.Sign(npc.Center.X - Projectile.Center.X),
                    DamageType = Projectile.DamageType
                };

                // Aplica o dano
                int damageDealt = npc.StrikeNPC(hitInfo);
        
                // Cooldown de imunidade
                npc.immune[Projectile.owner] = hitDelay;
        
                // ================== FÍSICA DO IMPACTO ==================
                // Chama OnHitNPC para aplicar knockback e física
                OnHitNPC(npc, hitInfo, damageDealt);
            }
        }







        #endregion
    }

    #region Global NPC
    public class BeybladeNpc : GlobalNPC
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