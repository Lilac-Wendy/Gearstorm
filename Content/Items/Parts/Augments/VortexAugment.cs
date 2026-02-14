using Gearstorm.Content.Projectiles;
using Gearstorm.Content.Projectiles.Beyblades;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Gearstorm.Content.Items.Parts.Augments
{
    public class VortexAugment : BeybladeAugment
    {
        public override string Texture => "Gearstorm/Assets/Items/Parts/Augment";
        public override Color AugmentColor => Color.Turquoise;

        public override string ExtraDescription =>
            "[c/00FFFF:Singularity Protocol]\n" +
            "Suctions nearby enemies toward the Beyblade.\n" +
            "Deals [c/00CED1:bonus damage] for each enemy caught in the vacuum.\n" +
            "Critical hits or rapid strikes manifest [c/00FFFF:Vortex Orbitals]\n" +
            "that spiral inward, scaling with the number of affected foes.\n" +
            "'Spaghetti(fication) is the only thing I offer, but it's not on a plate.'";
        public override void ApplyAugmentEffect(BaseBeybladeProjectile beybladeProj, NPC target, bool wasCrit) 
        {
            float orbitalSpawnRadius = 300f;
            float pullRange = orbitalSpawnRadius + 48f;
            if (target == null || !target.active) return;

            Projectile projectile = beybladeProj.Projectile;
            int affectedEnemies = 0;

            Lighting.AddLight(projectile.Center, 0f, 0.8f, 0.5f);

            // --- SUCÇÃO E CONTAGEM ---
            foreach (NPC npc in Main.ActiveNPCs) 
            {
                if (npc.friendly || npc.boss) continue;

                float distance = Vector2.Distance(npc.Center, projectile.Center);
                if (distance >= pullRange) continue;

                Vector2 toCenter = projectile.Center - npc.Center;
                float distFactor = MathHelper.Clamp(1f - (distance / pullRange), 0.15f, 1f);
        
                float suctionSpeed = ((18f + beybladeProj.CurrentSpinSpeed) * 2f) * distFactor;
                npc.velocity = Vector2.Lerp(npc.velocity, Vector2.Normalize(toCenter) * suctionSpeed, 0.1f * npc.knockBackResist);

                affectedEnemies++;
            }

            // --- DANO ESCALÁVEL (1 por inimigo) ---
            if (affectedEnemies > 0) 
            {
                NPC.HitInfo hit = new NPC.HitInfo {
                    Damage = affectedEnemies, // Quanto mais inimigos, mais dano
                    Knockback = 0f,
                    HitDirection = projectile.direction,
                    Crit = false
                };

                target.StrikeNPC(hit);
            }
        


    // =========================
    // VFX — ESPIRAL DE DUST
    // =========================
    float spawnRadius = orbitalSpawnRadius;


    for (int arm = 0; arm < 2; arm++)
    {
        float time = Main.GlobalTimeWrappedHourly;
        float spawnAngle = time * -10f + arm * MathHelper.Pi; // Rotação mais lenta
        Vector2 spawnPos = projectile.Center + spawnAngle.ToRotationVector2() * orbitalSpawnRadius;

        // Apenas 1 ou 2 dusts por braço por execução
        Dust d = Dust.NewDustPerfect(spawnPos, arm == 0 ? DustID.Vortex : 61, null, 100, Color.Cyan, 1.5f);
        d.noGravity = true;
        d.velocity *= 0.5f;
    }


    const int hitsPerSpawn = 4;

    projectile.localAI[1]++;
    bool spawnByHits = projectile.localAI[1] >= hitsPerSpawn;
    if (spawnByHits)
        projectile.localAI[1] = 0f;

    bool spawnByCrit = beybladeProj.LastHitWasCrit;

    if (spawnByCrit || spawnByHits)
    {
        int helixPairs = 1; 

        for (int i = 0; i < helixPairs; i++)
        {
            float initialTheta = 0f; 
            int parentId = projectile.whoAmI;

            //  A
            Projectile.NewProjectile(
                projectile.GetSource_FromThis(),
                projectile.Center,
                Vector2.Zero,
                ModContent.ProjectileType<VortexOrbitalProjectile>(),
                (projectile.damage / 2) * affectedEnemies,
                0f,
                projectile.owner,
                initialTheta,   // ai[0] = theta 
                0f,             // ai[1] = phase
                parentId        // ai[2]
            );

            //  B
            Projectile.NewProjectile(
                projectile.GetSource_FromThis(),
                projectile.Center,
                Vector2.Zero,
                ModContent.ProjectileType<VortexOrbitalProjectile>(),
                (projectile.damage / 2) * affectedEnemies,
                0f,
                projectile.owner,
                initialTheta,
                MathHelper.Pi,  //opposite phase
                parentId
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
