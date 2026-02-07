using Gearstorm.Content.Items.Parts;
using Gearstorm.Content.Projectiles.Beyblades;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using Gearstorm.Content.Projectiles;

namespace Gearstorm.Content.Items.Augments
{
    public class VortexAugment : BeybladeAugment
    {
        public override string Texture => "Gearstorm/Assets/Items/Parts/Augment";
        public override Color AugmentColor => Color.Turquoise;

        public override string ExtraDescription =>
            "[c/00FFFF:Vacuum Pulse]\n" +
            "Creates an [c/00CED1:implosion] on heavy impacts.\n" +
            "Pulls nearby enemies toward the Beyblade.\n" +
            "'Spaghetti(fication) is the only thing I offer, but it's not on a plate.'";

        public override void ApplyAugmentEffect(BaseBeybladeProjectile beybladeProj, NPC target)
{
    float orbitalSpawnRadius = 180f;
    float pullRange = orbitalSpawnRadius + 48f;

    if (target == null || !target.active)
        return;

    Projectile projectile = beybladeProj.Projectile;

    Lighting.AddLight(projectile.Center, 0f, 0.8f, 0.5f);

    int affectedEnemies = 0;

    foreach (NPC npc in Main.npc)
    {
        if (!npc.active || npc.friendly || npc.boss || npc.knockBackResist <= 0f)
            continue;

        float distance = Vector2.Distance(npc.Center, projectile.Center);
        if (distance >= pullRange)
            continue;

        Vector2 toCenter = projectile.Center - npc.Center;
        if (toCenter.LengthSquared() < 0.01f)
            continue;

        toCenter.Normalize();

        float distanceFactor = 1f - (distance / pullRange);
        distanceFactor = MathHelper.Clamp(distanceFactor, 0.15f, 1f);

        float suctionSpeed =
            (18f + beybladeProj.currentSpinSpeed * 3.5f) * distanceFactor;

        float pullFactor =
            0.14f * npc.knockBackResist * distanceFactor;

        Vector2 targetVelocity = toCenter * suctionSpeed;
        npc.velocity = Vector2.Lerp(npc.velocity, targetVelocity, pullFactor);

        if (!npc.noGravity && distance > 48f)
            npc.velocity.Y -= 0.45f * distanceFactor;

        affectedEnemies++;
    }

    // =========================
    // DANO ON-HIT ESCALÁVEL
    // =========================
    if (affectedEnemies > 0)
    {
        int bonusDamage = 1 + (affectedEnemies - 1);

        NPC.HitInfo hit = new NPC.HitInfo
        {
            Damage = bonusDamage,
            Knockback = 0f,
            HitDirection = projectile.direction,
            Crit = false
        };

        target.StrikeNPC(hit);
    }


    // =========================
    // VFX — ESPIRAL DE DUST
    // =========================
    int arms = 2;
    float spawnRadius = orbitalSpawnRadius;
    float inwardVelocity = 18f;
    float tangentVelocity = 12f;
    float dnaDanceSpeed = 18f;
    float dnaAmplitude = 30f;

    float time = Main.GlobalTimeWrappedHourly;

    for (int arm = 0; arm < arms; arm++)
    {
        float spawnAngle = time * -60f + arm * MathHelper.Pi;

        float dnaSin = (float)Math.Sin(time * dnaDanceSpeed + arm * MathHelper.Pi);
        float currentAmplitude = dnaSin * dnaAmplitude;

        Vector2 offsetDir = spawnAngle.ToRotationVector2();
        Vector2 sideOffset = offsetDir.RotatedBy(MathHelper.PiOver2) * currentAmplitude;

        Vector2 spawnPos =
            projectile.Center +
            offsetDir * spawnRadius +
            sideOffset;

        Vector2 toCenter =
            (projectile.Center - spawnPos).SafeNormalize(Vector2.Zero);

        Vector2 tangential =
            toCenter.RotatedBy(MathHelper.PiOver2) * tangentVelocity;

        Vector2 finalVelocity =
            toCenter * inwardVelocity + tangential;

        Color dustColor = arm == 0 ? Color.Cyan : Color.Lime;
        int dustID = arm == 0 ? DustID.Vortex : 61;

        for (int r = 0; r < 32; r++)
        {
            Vector2 trailPos = spawnPos - finalVelocity * (r * 0.4f);
            float scale = 2.3f - r * 0.15f;

            Dust d = Dust.NewDustPerfect(
                trailPos,
                dustID,
                finalVelocity,
                0,
                dustColor,
                scale
            );

            d.noGravity = true;
            d.alpha = 50 + r * 20;
        }
    }

    // =========================
    // ORBITAIS — CRIT OU CONTADOR
    // =========================
    const int HitsPerSpawn = 4;

    projectile.localAI[1]++;
    bool spawnByHits = projectile.localAI[1] >= HitsPerSpawn;
    if (spawnByHits)
        projectile.localAI[1] = 0f;

    bool spawnByCrit = beybladeProj.LastHitWasCrit;

    if (spawnByCrit || spawnByHits)
    {
        int orbitalCount = 2;

        for (int i = 0; i < orbitalCount; i++)
        {
            float angle = MathHelper.TwoPi * i / orbitalCount;

            Projectile.NewProjectile(
                projectile.GetSource_FromThis(),
                projectile.Center + angle.ToRotationVector2() * spawnRadius,
                Vector2.Zero,
                ModContent.ProjectileType<VortexOrbitalProjectile>(),
                projectile.damage / 2,
                0f,
                projectile.owner,
                angle,
                spawnRadius,
                projectile.whoAmI
            );
        }
    }

    Terraria.Audio.SoundEngine.PlaySound(
        SoundID.Item62 with { Pitch = 0.5f, Volume = 0.6f },
        projectile.Center
    );
}


        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.FragmentVortex, 12);
            recipe.AddTile(TileID.LunarCraftingStation);
            recipe.Register();
        }
    }
}
