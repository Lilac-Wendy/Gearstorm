using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Gearstorm.Content.Items.Parts.Augments;

public class BoneAugment : BeybladeAugment
{
    public override string Texture => "Gearstorm/Assets/Items/Parts/Augment";
    public override Color AugmentColor => Color.AntiqueWhite;

    public override string ExtraDescription
    {
        get
        {
            Player player = Main.LocalPlayer;
            float magicBonus = player.GetTotalDamage(DamageClass.Magic).Additive;
            float summonBonus = player.GetTotalDamage(DamageClass.Summon).Additive;

            string desc = "[c/F5F5DC:Ossified Resonance]\n" +
                          "Impacts release a burst of [c/F5F5DC:Bone Shards]\n" +
                          "Damage scales with both [c/6495ED:Magic] and [c/9370DB:Summoner] bonuses\n";

            if (magicBonus >= 1.25f)
                desc += "\n[c/6495ED:Magic Resonance:] Impacts call down falling stars";

            if (summonBonus >= 1.25f)
                desc += "\n[c/9370DB:Summoner Resonance:] Impacts restore the blade's flight time";

            if (Main.hardMode)
            {
                desc += "\n[c/ADFF2F:Hardmode Surge:] Resonances deal massive extra damage\n" +
                        "[c/ADFF2F:Shards are Empowered (Velocity & Pierce)]";
            }

            desc += "\n\n[c/F5F5DC:'Spooky, scary, and... Newton didn't account for necromancy.']";
            return desc;
        }
    }
    public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
        spriteBatch.Draw(texture, position, null, Color.AntiqueWhite * 0.8f, 0f, origin, scale, SpriteEffects.None, 0f);
    }
    
    public override void OnBeybladeHit(Projectile beyblade, Vector2 hitNormal, float impactStrength, Projectile otherBeyblade, NPC targetNpc, bool wasCrit)
{
    if (targetNpc == null) return;

    Player player = Main.player[beyblade.owner];
    float magicBonus = player.GetTotalDamage(DamageClass.Magic).Additive;
    float summonBonus = player.GetTotalDamage(DamageClass.Summon).Additive;

    // --- ESCALONAMENTO DE HARDMODE ---
    // Aumentamos o multiplicador base do resonance e o dano das estrelas
    float shardDamageMult = Main.hardMode ? 0.60f : 0.35f;
    float starScaling = Main.hardMode ? 0.80f : 0.50f; // Estrelas batem 80% do dano híbrido no HM

    // --- LÓGICA SUMMONER: Extensão de Vida ---
    if (summonBonus >= 1.25f)
    {
        // No Hardmode, recupera um pouco mais de tempo (25 vs 20)
        beyblade.timeLeft += Main.hardMode ? 25 : 20;
        if (beyblade.timeLeft > 3600) beyblade.timeLeft = 3600;

        if (Main.rand.NextBool(5))
            Dust.NewDust(beyblade.position, beyblade.width, beyblade.height, DustID.PurpleCrystalShard, 0, 0, 150, default, 0.7f);
    }

    // --- LÓGICA MÁGICA: Estrela Cadente ---
    if (magicBonus >= 1.25f && Main.rand.NextBool(2))
    {
        // Dano escalonado pelo starScaling (0.5 ou 0.8)
        int starDamage = (int)((beyblade.damage * starScaling) + (player.GetTotalDamage(DamageClass.Magic).ApplyTo(beyblade.damage) * starScaling));

        Vector2 spawnPos = targetNpc.Center + new Vector2(Main.rand.Next(-100, 101), -600);
        Vector2 starSpeed = (targetNpc.Center - spawnPos).SafeNormalize(Vector2.UnitY) * 18f; // Mais rápidas

        int sIndex = Projectile.NewProjectile(beyblade.GetSource_FromThis(), spawnPos, starSpeed, ProjectileID.Starfury, starDamage, 2f, beyblade.owner);
        if (sIndex != Main.maxProjectiles)
        {
            Main.projectile[sIndex].DamageType = DamageClass.Magic;
            Main.projectile[sIndex].tileCollide = false;
        }
    }

    // --- EFEITO BASE + HARDMODE UPGRADE: Fragmentos de Osso ---
    if (Main.rand.NextBool(3))
    {
        float hybridMultiplier = (magicBonus + summonBonus) / 2f;
        int damage = (int)(beyblade.damage * shardDamageMult * hybridMultiplier);

        Vector2 speed = hitNormal.RotatedByRandom(0.8f);
        speed.Y = -Math.Abs(speed.Y) - 2f;
        speed *= Main.hardMode ? 6f : 2f;

        int pIndex = Projectile.NewProjectile(beyblade.GetSource_FromThis(), beyblade.Center, speed, ProjectileID.BoneGloveProj, damage, 1.5f, beyblade.owner);
        
        if (pIndex != Main.maxProjectiles)
        {
            Projectile p = Main.projectile[pIndex];
            p.DamageType = DamageClass.Generic;
            p.tileCollide = true;

            if (Main.hardMode)
            {
                p.penetrate = 4; // Aumentado para 4 no HM
                p.scale = 1.3f;
                // Luz ciana intensa para indicar o "Surge"
                Lighting.AddLight(p.Center, 0f, 0.7f, 0.8f);
            }

            Lighting.AddLight(p.Center, 0.3f, 0.3f, 0.2f);
            p.netUpdate = true;
        }
    }
}
    public override void AddRecipes()
    {
        Recipe recipe = CreateRecipe();
        recipe.AddIngredient(ItemID.Bone, 40);
        recipe.AddIngredient(ModContent.ItemType<BasicBladeItem>(), 5);
        recipe.AddTile(TileID.Anvils);
        recipe.Register();
    }
}