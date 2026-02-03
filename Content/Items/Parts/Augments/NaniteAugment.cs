using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.GameContent;
using Gearstorm.Content.Projectiles.Beyblades;
using Gearstorm.Content.Items.Parts;

namespace Gearstorm.Content.Items.Parts.Augments;

public class NaniteAugment : BeybladeAugment
{
    public override Color AugmentColor => Color.Cyan;
    public override string Texture => "Gearstorm/Assets/Items/Parts/Augment";
    public override string ExtraDescription => "Nanite Overdrive: Arcs between blades and triggers a contagious chain reaction between enemies.";

    private const int AI_CHARGE_TIMER = 2; 

    public override void UpdateAugment(BaseBeybladeProjectile beybladeProj)
    {
        Projectile self = beybladeProj.Projectile;
        
        // 1. ARCO ENTRE BEYBLADES (PERSISTENTE)
        if (self.ai[AI_CHARGE_TIMER] > 0)
        {
            self.ai[AI_CHARGE_TIMER]--;
            Projectile partner = FindPartner(self);
            if (partner != null)
            {
                CreateElectricityEffect(self.Center, partner.Center, 0.6f);
                HandleCollisionDamage(self, partner);
            }
        }

        // 2. REAÇÃO EM CADEIA E CONTÁGIO (ENTRE NPCs)
        // Roda a cada 15 frames para manter performance e legibilidade visual
        if (Main.GameUpdateCount % 15 == 0)
        {
            HandleNPCContagion(self);
        }
    }

    private void HandleNPCContagion(Projectile self)
    {
        float contagionRadius = 200f;

        // Itera pelos NPCs para encontrar quem está eletrificado
        for (int i = 0; i < Main.maxNPCs; i++)
        {
            NPC sourceNpc = Main.npc[i];
            
            if (sourceNpc.active && !sourceNpc.friendly && sourceNpc.HasBuff(BuffID.Electrified))
            {
                // Busca um alvo próximo para espalhar a carga
                for (int j = 0; j < Main.maxNPCs; j++)
                {
                    if (i == j) continue; // Não pula nele mesmo

                    NPC targetNpc = Main.npc[j];
                    if (targetNpc.active && !targetNpc.friendly && !targetNpc.dontTakeDamage && Vector2.Distance(sourceNpc.Center, targetNpc.Center) < contagionRadius)
                    {
                        // Efeito visual do arco de contágio
                        CreateElectricityEffect(sourceNpc.Center, targetNpc.Center, 0.4f);

                        // Aplica dano e ESPALHA o debuff (Contágio)
                        if (Main.myPlayer == self.owner)
                        {
                            var hit = targetNpc.CalculateHitInfo(self.damage / 3, 0, false, 0f);
                            targetNpc.StrikeNPC(hit);
                            targetNpc.AddBuff(BuffID.Electrified, 120); 
                            targetNpc.AddBuff(BuffID.Confused, 120); 
                        }

                        // Som de estática suave para o contágio
                        if (Main.rand.NextBool(3))
                        {
                            SoundEngine.PlaySound(SoundID.Item93 with { Pitch = 0.5f, Volume = 0.3f }, targetNpc.Center);
                        }
                    }
                }
            }
        }
    }

    private void CreateElectricityEffect(Vector2 start, Vector2 end, float intensity)
    {
        Vector2 diff = end - start;
        float length = diff.Length();
        if (length <= 1f) return;
        Vector2 direction = Vector2.Normalize(diff);
        
        int segments = (int)(length / 22f);
        if (segments < 2) segments = 2;

        Vector2 lastPos = start;

        for (int i = 1; i <= segments; i++)
        {
            Vector2 targetPos = start + direction * (length * (i / (float)segments));
            
            // Jitter para zigue-zague
            if (i < segments)
            {
                Vector2 ortho = new Vector2(-direction.Y, direction.X);
                targetPos += ortho * Main.rand.NextFloat(-14f, 14f);
            }

            float distBetween = Vector2.Distance(lastPos, targetPos);
            int dustCount = (int)(distBetween / 5f);

            for (int j = 0; j < dustCount; j++)
            {
                Vector2 dustPos = Vector2.Lerp(lastPos, targetPos, j / (float)dustCount);
                
                // Usando AncientLight rotacionado para o efeito de "fita de energia"
                Dust d = Dust.NewDustPerfect(dustPos, DustID.AncientLight, Vector2.Zero, 0, Color.Cyan, 0.45f * intensity);
                d.noGravity = true;
                d.rotation = (targetPos - lastPos).ToRotation();
                
                if (Main.rand.NextBool(10)) // Faíscas ocasionais
                {
                    Dust.NewDustPerfect(dustPos, DustID.Electric, Main.rand.NextVector2Circular(2f, 2f), 100, Color.White, 0.3f).noGravity = true;
                }
            }
            lastPos = targetPos;
        }
    }

    private Projectile FindPartner(Projectile self)
    {
        float closestDist = 750f;
        Projectile partner = null;
        for (int i = 0; i < Main.maxProjectiles; i++)
        {
            Projectile p = Main.projectile[i];
            if (p.active && p.whoAmI != self.whoAmI && p.owner == self.owner && p.ModProjectile is BaseBeybladeProjectile)
            {
                float d = Vector2.Distance(self.Center, p.Center);
                if (d < closestDist) { closestDist = d; partner = p; }
            }
        }
        return partner;
    }

    private void HandleCollisionDamage(Projectile self, Projectile partner)
    {
        float point = 0f;
        for (int i = 0; i < Main.maxNPCs; i++)
        {
            NPC npc = Main.npc[i];
            if (npc.active && !npc.friendly && Collision.CheckAABBvLineCollision(npc.position, npc.Size, self.Center, partner.Center, 32f, ref point))
            {
                if (Main.GameUpdateCount % 12 == 0)
                {
                    npc.StrikeNPC(npc.CalculateHitInfo(self.damage / 2, (npc.Center.X < self.Center.X) ? 1 : -1));
                    npc.AddBuff(BuffID.Electrified, 120);
                    npc.AddBuff(BuffID.Confused, 120); 
                }
            }
        }
    }

    public override void OnBeybladeHit(Projectile beyblade, Vector2 hitNormal, float impactStrength, Projectile otherBeyblade, NPC targetNPC)
    {
        beyblade.ai[AI_CHARGE_TIMER] = 360f;
        if (targetNPC != null)
        {
            HashSet<int> hitNpcs = new HashSet<int>();
            ApplyChainToNPCs(Main.player[beyblade.owner], targetNPC, beyblade.damage, hitNpcs, 0, beyblade.Center);
            SoundEngine.PlaySound(SoundID.Item94 with { Pitch = -0.2f, Volume = 0.5f }, targetNPC.Center);
        }
    }

    private void ApplyChainToNPCs(Player player, NPC currentTarget, int damage, HashSet<int> hitNpcs, int chainCount, Vector2 lastPos)
    {
        if (chainCount > 5 || !hitNpcs.Add(currentTarget.whoAmI)) return;

        int chainDamage = (int)(damage * (1.0f - (chainCount * 0.12f)));
        currentTarget.StrikeNPC(currentTarget.CalculateHitInfo(Math.Max(1, chainDamage), player.direction));
        currentTarget.AddBuff(BuffID.Electrified, 240);
        currentTarget.AddBuff(BuffID.Confused, 120); 
        CreateElectricityEffect(lastPos, currentTarget.Center, 1.0f);

        float scanRadius = 280f;
        foreach (NPC next in Main.npc)
        {
            if (next.active && !next.friendly && !hitNpcs.Contains(next.whoAmI) && Vector2.Distance(next.Center, currentTarget.Center) < scanRadius)
            {
                ApplyChainToNPCs(player, next, damage, hitNpcs, chainCount + 1, currentTarget.Center);
                break;
            }
        }
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.Nanites, 30)
            .AddIngredient(ItemID.RainCloud, 10)
            .AddIngredient(ModContent.ItemType<BasicBladeItem>(), 5)
            .AddTile(TileID.Anvils)
            .Register();
    }
}