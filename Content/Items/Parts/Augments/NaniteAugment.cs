using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Gearstorm.Content.Projectiles.Beyblades;
using Gearstorm.Content.Items.Parts;
using Terraria.GameContent;

namespace Gearstorm.Content.Items.Parts.Augments;

public class NaniteAugment : BeybladeAugment
{
    // ====================== PROPERTIES & FIELDS ======================

    public override Color AugmentColor => Color.Cyan;
    public override string Texture => "Gearstorm/Assets/Items/Parts/Augment";
    public override string ExtraDescription => 
        "[c/00FFFF:Nanite Overdrive]\n" +
        "Blade impacts discharge [c/E1FFFF:Arc Lightning] that chains between up to 8 enemies\n" +
        "Striking enemies has a chance to generate or charge an special [c/00FFFF:Electrosphere]\n" +
        "Kills grant [c/00FFFF:15% extra charge] to the active sphere\n" +
        "At 100% charge, it evolves into a [c/00FFFF:Massive Magnet Sphere] dealing 400% damage per Beam";
    private static List<(Vector2 Start, Vector2 End, float Intensity)> _arcsToDraw = new();
    private static Dictionary<int, float> _projectileCharges = new();
    private static Dictionary<int, uint> _magnetExplosionSchedule = new();

    // ====================== CORE OVERRIDES (TML) ======================

    public override void Load()
    {
        if (!Main.dedServ)
            On_Main.DrawProjectiles += DrawArcsHook;
    }

    public override void Unload()
    {
        if (!Main.dedServ)
            On_Main.DrawProjectiles -= DrawArcsHook;

        _arcsToDraw.Clear();
        _projectileCharges.Clear();
        _magnetExplosionSchedule.Clear();

    }

    public override void UpdateAugment(BaseBeybladeProjectile beybladeProj)
{
    Projectile beyblade = beybladeProj.Projectile;
    Player player = Main.player[beyblade.owner];


    // ===================== GARANTIR 1 ELECTROSPHERE =====================
    Projectile mainSphere = null;

    for (int i = 0; i < Main.maxProjectiles; i++)
    {
        Projectile p = Main.projectile[i];
        if (p.active && p.owner == beyblade.owner && p.type == ProjectileID.Electrosphere)
        {
            mainSphere = p;
            break;
        }
    }

    // Spawn IMEDIATO se não existir
    if (mainSphere == null && beyblade.owner == Main.myPlayer)
    {
        int idx = Projectile.NewProjectile(
            beyblade.GetSource_FromAI(),
            beyblade.Center,
            Vector2.Zero,
            ProjectileID.Electrosphere,
            beyblade.damage / 2,
            0f,
            beyblade.owner
        );

        if (idx != Main.maxProjectiles)
        {
            mainSphere = Main.projectile[idx];
            _projectileCharges[mainSphere.whoAmI] = 0f;
        }
    }

    if (mainSphere == null)
        return;

    // ===================== ORBITA ESTÁVEL =====================
    float angle = Main.GlobalTimeWrappedHourly * 3.5f;
    Vector2 offset = new Vector2(0f, -48f).RotatedBy(angle);

    mainSphere.Center = beyblade.Center + offset;
    mainSphere.velocity = Vector2.Zero;
    mainSphere.timeLeft = 60; // nunca morre sozinha
    mainSphere.netUpdate = true;

    if (!_projectileCharges.ContainsKey(mainSphere.whoAmI))
        _projectileCharges[mainSphere.whoAmI] = 0f;

    // ===================== ABSORÇÃO DE OUTRAS ESFERAS =====================
    for (int i = 0; i < Main.maxProjectiles; i++)
    {
        Projectile p = Main.projectile[i];

        if (!p.active ||
            p.whoAmI == mainSphere.whoAmI ||
            p.owner != beyblade.owner ||
            p.type != ProjectileID.Electrosphere)
            continue;

        _projectileCharges[mainSphere.whoAmI] =
            Math.Min(1f, _projectileCharges[mainSphere.whoAmI] + 0.10f);

        NaniteAugment_AddArc(p.Center, mainSphere.Center, 1.2f);

        CombatText.NewText(
            mainSphere.getRect(),
            Color.Cyan,
            $"Charge: {(int)(_projectileCharges[mainSphere.whoAmI] * 100)}%",
            true
        );

        p.Kill();
        
    }
}
    


    public override void OnBeybladeHit(
    Projectile beyblade,
    Vector2 hitNormal,
    float impactStrength,
    Projectile otherBeyblade,
    NPC targetNPC)
{
    if (Main.myPlayer != beyblade.owner)
        return;

    Player player = Main.player[beyblade.owner];

    // ===================== CHAIN SEMPRE =====================
    if (targetNPC != null)
    {
        HashSet<int> hitNpcs = new();
        ApplyChainDamage(player, targetNPC, beyblade.damage, hitNpcs, 0, beyblade.Center);
    }

    // ===================== PROCURA ESFERA =====================
    Projectile sphere = Main.projectile.FirstOrDefault(p =>
        p.active &&
        p.owner == beyblade.owner &&
        p.type == ProjectileID.Electrosphere
    );

    // ===================== SPAWN / ABSORÇÃO =====================
    if (sphere == null)
    {
        if (Main.rand.NextBool(7)) // ~14%
        {
            int idx = Projectile.NewProjectile(
                beyblade.GetSource_OnHit(targetNPC),
                beyblade.Center,
                Vector2.Zero,
                ProjectileID.Electrosphere,
                beyblade.damage / 2,
                0f,
                beyblade.owner
            );

            if (idx != Main.maxProjectiles)
            {
                _projectileCharges[idx] = 0f;
                SoundEngine.PlaySound(SoundID.Item93, beyblade.Center);
            }
        }
    }
    else
    {
        // Absorção de qualquer tentativa de spawn
        if (Main.rand.NextBool(7))
        {
            _projectileCharges[sphere.whoAmI] =
                Math.Min(1f, _projectileCharges[sphere.whoAmI] + 0.10f);

            NaniteAugment_AddArc(beyblade.Center, sphere.Center, 1f);
            CombatText.NewText(
                sphere.getRect(),
                Color.Cyan,
                $"Charge {(int)(_projectileCharges[sphere.whoAmI] * 100)}%",
                true
            );
        }
    }

    // ===================== KILL → CARGA =====================
    if (targetNPC != null && (!targetNPC.active || targetNPC.life <= 0) && sphere != null)
    {
        _projectileCharges[sphere.whoAmI] =
            Math.Min(1f, _projectileCharges[sphere.whoAmI] + 0.15f);

        CombatText.NewText(
            sphere.getRect(),
            Color.Cyan,
            $"Charge {(int)(_projectileCharges[sphere.whoAmI] * 100)}%",
            true
        );
    }

    // ===================== EVOLUÇÃO =====================
    if (sphere != null &&
        _projectileCharges.TryGetValue(sphere.whoAmI, out float charge) &&
        charge >= 1f)
    {
        int idx = Projectile.NewProjectile(
            beyblade.GetSource_OnHit(targetNPC),
            sphere.Center,
            Vector2.Zero,
            ProjectileID.MagnetSphereBall,
            (int)(beyblade.damage * 4.0f),
            2f,
            beyblade.owner
        );

        if (idx != Main.maxProjectiles)
        {
            Projectile p = Main.projectile[idx];

            // Centro original (ANTES de mexer em size)
            Vector2 center = p.Center;
            
            p.scale = 2.0f;

            // Recalcula hitbox baseada na escala
            int newSize = (int)(32 * p.scale); // MagnetSphereBall base = 32

            p.width = newSize;
            p.height = newSize;

            // REALINHA corretamente
            p.Center = center;

            p.netUpdate = true;
        }

        _projectileCharges.Remove(sphere.whoAmI);
        sphere.Kill();

        SoundEngine.PlaySound(SoundID.Item94, sphere.Center);
    }
}
    

    private void ApplyChainDamage(
        Player player,
        NPC currentTarget,
        int baseDamage,
        HashSet<int> hitNpcs,
        int chainCount,
        Vector2 lastPos)
    {
        if (chainCount > 7 || !hitNpcs.Add(currentTarget.whoAmI))
            return;

        int damage = (int)(baseDamage * MathHelper.Lerp(1f, 0.35f, chainCount / 7f));
        if (damage < 1) damage = 1;

        currentTarget.StrikeNPC(
            currentTarget.CalculateHitInfo(
                damage,
                currentTarget.Center.X < lastPos.X ? -1 : 1
            )
        );

        currentTarget.AddBuff(BuffID.Electrified, 300 + chainCount * 60);

        // ===== VISUAL =====
        SpawnLightningArc(lastPos, currentTarget.Center, 1.1f - chainCount * 0.08f);
        NaniteAugment_AddArc(lastPos, currentTarget.Center, 1.2f - chainCount * 0.1f);
        
        // ===== PRÓXIMO ALVO =====
        const float range = 360f;
        NPC next = Main.npc
            .Where(n => n.active && !n.friendly && !hitNpcs.Contains(n.whoAmI))
            .OrderBy(n => Vector2.Distance(n.Center, currentTarget.Center))
            .FirstOrDefault(n => n.Distance(currentTarget.Center) <= range);

        if (next != null && Main.rand.NextFloat() < 0.9f)
        {
            ApplyChainDamage(
                player,
                next,
                baseDamage,
                hitNpcs,
                chainCount + 1,
                currentTarget.Center
            );
        }
        
    }
    private void SpawnLightningArc(Vector2 start, Vector2 end, float intensity)
    {
        Vector2 diff = end - start;
        float length = diff.Length();
        if (length < 8f) return;

        Vector2 dir = diff.SafeNormalize(Vector2.UnitY);
        Vector2 normal = new Vector2(-dir.Y, dir.X);

        int segments = Math.Max(6, (int)(length / 22f));
        Vector2 prev = start;

        for (int i = 1; i <= segments; i++)
        {
            float t = i / (float)segments;
            Vector2 point = start + diff * t;

            // jitter forte, mas controlado
            float jitter = Main.rand.NextFloat(-18f, 18f) * intensity;
            point += normal * jitter;

            // ===== Dust Branco (energia base) =====
            Dust core = Dust.NewDustPerfect(
                Vector2.Lerp(prev, point, 0.5f),
                DustID.RainCloud,
                Vector2.Zero,
                100,
                Color.White,
                0.8f * intensity
            );
            core.noGravity = true;

            // ===== Dust Elétrico colorido =====
            Color electricColor = Main.rand.Next(4) switch
            {
                0 => Color.Cyan,
                1 => Color.White,
                2 => Color.MediumPurple,
                _ => Color.LimeGreen
            };

            Dust elec = Dust.NewDustPerfect(
                point,
                DustID.Electric,
                Main.rand.NextVector2Circular(2f, 2f),
                100,
                electricColor,
                1.2f * intensity
            );
            elec.noGravity = true;

            prev = point;
        }
    }
    private void CreateArcEffect(Vector2 start, Vector2 end)
    {
        int segments = 8;
        Vector2 step = (end - start) / segments;

        for (int i = 0; i <= segments; i++)
        {
            Vector2 point = start + step * i;
            // Adiciona o jitter característico do arco original
            point += new Vector2(Main.rand.NextFloat(-6f, 6f), Main.rand.NextFloat(-6f, 6f));

            Dust dust = Dust.NewDustPerfect(point, DustID.Electric, Vector2.Zero, 100, Color.LightBlue, 1.4f);
            dust.noGravity = true;
            dust.velocity = Main.rand.NextVector2Circular(1f, 1f);
        }
    }

    // ====================== REMAINING HELPERS ======================
    


    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.Nanites, 30)
            .AddIngredient(ItemID.RainCloud, 10)
            .AddIngredient(ModContent.ItemType<BasicBladeItem>(), 5)
            .AddTile(TileID.Anvils)
            .Register();
    }

    // ====================== VISUALS & DRAWING ======================

    public static void NaniteAugment_AddArc(Vector2 start, Vector2 end, float intensity)
    {
        _arcsToDraw.Add((start, end, intensity));
    }

    private void DrawArcsHook(On_Main.orig_DrawProjectiles orig, Main self)
    {
        orig(self);
        if (_arcsToDraw.Count == 0) return;

        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        foreach (var arc in _arcsToDraw) DrawBeam(arc.Start, arc.End, arc.Intensity);
        Main.spriteBatch.End();
        _arcsToDraw.Clear();
    }

    private void DrawBeam(Vector2 start, Vector2 end, float intensity)
    {
        Texture2D texture = TextureAssets.Extra[98].Value;
        Vector2 dist = end - start;
        float fullLength = dist.Length();
        if (fullLength <= 0f) return;
        Vector2 unit = dist / fullLength;
        int segments = Math.Max(2, (int)(fullLength / 18f));
        Vector2 prevPos = start;
        Vector2 perp = new Vector2(-unit.Y, unit.X);
        float maxOffset = 14f * intensity;

        for (int i = 1; i <= segments; i++)
        {
            float progress = i / (float)segments;

            // posição base reta
            Vector2 basePos = start + dist * progress;

            // zigue-zague randômico (mudança brusca)
            float zigzagStrength = Main.rand.NextFloat(-1f, 1f);
            float attenuation = 1f - Math.Abs(progress - 0.5f) * 2f; // mais forte no meio

            Vector2 currentPos =
                basePos +
                perp * zigzagStrength * maxOffset * attenuation;

            Vector2 segDist = currentPos - prevPos;
            float rot = segDist.ToRotation() + MathHelper.PiOver2;

            float segmentIntensity = intensity * Main.rand.NextFloat(0.7f, 1.3f);

            // Raio principal
            Main.spriteBatch.Draw(
                texture,
                prevPos - Main.screenPosition,
                null,
                Color.Cyan * 0.45f * segmentIntensity,
                rot,
                new Vector2(texture.Width / 2f, 0f),
                new Vector2(0.7f * segmentIntensity, segDist.Length() / 64f),
                SpriteEffects.None,
                0f
            );

            // Glow interno
            Main.spriteBatch.Draw(
                texture,
                prevPos - Main.screenPosition,
                null,
                Color.White * 0.6f * segmentIntensity,
                rot,
                new Vector2(texture.Width / 2f, 0f),
                new Vector2(0.25f * segmentIntensity, segDist.Length() / 64f),
                SpriteEffects.None,
                0f
            );

            prevPos = currentPos;
        }

    }

    private void SpawnDustArcLine(Vector2 start, Vector2 end, float intensity)
    {
        Vector2 dist = end - start;
        float length = dist.Length();
        if (length < 5f) return;
        int segments = Math.Max(3, (int)(length / 15f));
        for (int i = 0; i <= segments; i++)
        {
            Vector2 pos = Vector2.Lerp(start, end, i / (float)segments);
            Dust d = Dust.NewDustPerfect(pos, DustID.Electric, Main.rand.NextVector2Circular(1.5f, 1.5f), 100, Color.Cyan, 0.8f * intensity);
            d.noGravity = true;
        }
    }
}