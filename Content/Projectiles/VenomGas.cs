using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Gearstorm.Content.Projectiles
{
    public class VenomGas : ModProjectile
    {
        public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.GasTrap}";

        private const float ColorCycleTime = 60f;
        private float fadeTimer;
        private const float MaxLife = 180f;
        
        private const float DpsInterval = 1f;
        private float[] npcDamageTimers = new float[Main.maxNPCs];
        
        private float storedOriginalDamage;
        private float storedCritMultiplier = 1.0f; // 🔥 NOVO: Armazena o CritMultiplier
        
        private VenomGasGlobalNpc GetVenomNpc(NPC npc) => npc.GetGlobalNPC<VenomGasGlobalNpc>();

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = Main.projFrames[ProjectileID.GasTrap];
            ProjectileID.Sets.DrawScreenCheckFluff[Type] = 10000;
            ProjectileID.Sets.DontAttachHideToAlpha[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.SporeCloud);

            Projectile.width = 100;
            Projectile.height = 100;
            Projectile.scale = 1.0f;
            Projectile.alpha = 200;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = (int)MaxLife;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.damage = 0;
        }

        public override void OnSpawn(IEntitySource source)
        {
            if (Projectile.ai[0] == 0)
            {
                Vector2 spawnPosition = Projectile.Center;
        
                for (int i = 0; i < 6; i++)
                {
                    Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                    Projectile.NewProjectile(
                        source,
                        spawnPosition,
                        vel,
                        Type,
                        Projectile.damage,
                        0f,
                        Projectile.owner,
                        1f, // ai[0] = 1 para projéteis filhos
                        Projectile.damage, // ai[1] = dano
                        Projectile.ai[2] // ai[2] = passa o CritMultiplier adiante 🔥
                    );
                }
                Projectile.Kill();
                return;
            }
    
            // 🔥 Armazena os valores passados
            storedOriginalDamage = Projectile.ai[1];
            storedCritMultiplier = Projectile.ai[2]; // 🔥 CritMultiplier
    
            Projectile.alpha = 150;
            fadeTimer = 0f;
    
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                npcDamageTimers[i] = 0f;
            }
        }

        public override void AI()
        {
            fadeTimer += 1f;
            
            float fadeProgress = fadeTimer / MaxLife;
            float fadeValue = fadeProgress * fadeProgress;
            Projectile.alpha = 150 + (int)(105f * fadeValue);
            
            Projectile.velocity *= 0.92f;
            Projectile.rotation += Projectile.velocity.X * 0.01f;
            Projectile.velocity.Y -= 0.02f;
            
            ApplyDamageOverTime();
            
            if (Projectile.timeLeft <= 0 || Projectile.alpha >= 255)
                Projectile.Kill();
        }
        
        private void ApplyDamageOverTime()
        {
            if (storedOriginalDamage <= 0) return;
            
            float baseDamagePerStack = storedOriginalDamage * 0.15f;
            float minDamage = Math.Max(1f, storedOriginalDamage * 0.05f);
            
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.dontTakeDamage && !npc.friendly && 
                    npc.life > 0)
                {
                    Rectangle projHitbox = new Rectangle(
                        (int)(Projectile.position.X + Projectile.width / 2f - 50),
                        (int)(Projectile.position.Y + Projectile.height / 2f - 50),
                        100,
                        100
                    );
                    
                    if (projHitbox.Intersects(npc.Hitbox))
                    {
                        var venomNpc = GetVenomNpc(npc);
                        npcDamageTimers[i] += 1f;
                        
                        if (npcDamageTimers[i] >= 20f)
                        {
                            // 🔥 AGORA USA O CRITMULTIPLIER ARMAZENADO
                            float critMultiplier = storedCritMultiplier;
                            
                            // 🔥 CÁLCULO DO LIMITE DE STACKS
                            int baseStacks = 10;
                            float critBonus = Math.Max(0, critMultiplier - 1.0f);
                            int extraStacks = (int)(critBonus * 30);
                            int maxStacks = Math.Min(baseStacks + extraStacks, 500);
                            
                            if (venomNpc.VenomStacks < maxStacks)
                            {
                                venomNpc.VenomStacks++;
                                venomNpc.VenomDecayTimer = 60;
                            }
                            
                            npcDamageTimers[i] = 0f;
                        }
                        
                        if (npcDamageTimers[i] >= DpsInterval * 60f)
                        {
                            // 🔥 DANO COM BÔNUS DE OVERCRIT
                            int damageFromStacks = (int)(baseDamagePerStack * venomNpc.VenomStacks);
                            
                            // Bônus adicional para stacks acima do limite base
                            if (venomNpc.VenomStacks > 10)
                            {
                                float overcritBonus = 1.0f + ((venomNpc.VenomStacks - 10) * 0.03f);
                                damageFromStacks = (int)(damageFromStacks * overcritBonus);
                            }
                            
                            int damage = Math.Max((int)minDamage, damageFromStacks);
                            
                            NPC.HitInfo hitInfo = new NPC.HitInfo
                            {
                                Damage = damage,
                                Knockback = 0f,
                                HitDirection = 0
                            };
                            
                            npc.StrikeNPC(hitInfo);
                            
                            // 🔥 Cor dinâmica baseada nos stacks
                            Color damageColor;
                            if (venomNpc.VenomStacks > 10)
                            {
                                float overcritRatio = (venomNpc.VenomStacks - 10) / 40f;
                                damageColor = Color.Lerp(Color.Lime, Color.Gold, overcritRatio);
                            }
                            else
                            {
                                damageColor = new Color(100, 255, 100);
                            }
                            
                            string damageText = $"{damage}";

                            CombatText.NewText(
                                npc.Hitbox,
                                damageColor,
                                damageText,
                                true
                            );
                            
                            npcDamageTimers[i] = 0f;
                        }
                    }
                    else
                    {
                        npcDamageTimers[i] = 0f;
                    }
                }
            }
        }
        
        private void ShowCustomCombatText(NPC target, int damage, int stacks)
        {
            // 🔥 CORREÇÃO: Usa MathHelper.Clamp corretamente
            int greenValue = (int)MathHelper.Clamp(150 + (stacks * 10), 150, 255);
            int redValue = (int)MathHelper.Clamp(100 - (stacks * 5), 50, 100);
            int blueValue = (int)MathHelper.Clamp(100 - (stacks * 3), 50, 100);
    
            Color textColor = new Color(redValue, greenValue, blueValue, 255);
            
            var damageText = $"{damage}";
            
            Rectangle npcRect = target.getRect();
            if (npcRect.Intersects(new Rectangle((int)Main.screenPosition.X, (int)Main.screenPosition.Y, Main.screenWidth, Main.screenHeight)))
            {
                CombatText.NewText(
                    npcRect,
                    textColor,
                    damageText,
                    true   // Não é crítico
                );
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[ProjectileID.GasTrap].Value;
            int frameCount = Main.projFrames[ProjectileID.GasTrap];
            
            int frame = (int)(fadeTimer / 10f) % frameCount;
            Rectangle frameRect = texture.Frame(1, frameCount, 0, frame);
            
            Vector2 origin = frameRect.Size() * 0.5f;
            
            float t = (Main.GameUpdateCount % ColorCycleTime) / ColorCycleTime;
            
            Color green = new(80, 255, 120, 180);
            Color yellow = new(255, 230, 80, 180);
            Color purple = new(190, 90, 255, 180);

            Color color;
            if (t < 0.33f)
                color = Color.Lerp(green, yellow, t / 0.33f);
            else if (t < 0.66f)
                color = Color.Lerp(yellow, purple, (t - 0.33f) / 0.33f);
            else
                color = Color.Lerp(purple, green, (t - 0.66f) / 0.34f);

            float alphaMultiplier = 1f - (Projectile.alpha / 255f);
            color *= alphaMultiplier;
            
            float pulse = 1f + (float)Math.Sin(fadeTimer * 0.1f) * 0.05f;
            float scale = 1.5f * pulse * (1f - fadeTimer / MaxLife * 0.3f);
            
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            
            Main.EntitySpriteDraw(
                texture,
                drawPosition,
                frameRect,
                color,
                Projectile.rotation,
                origin,
                scale,
                SpriteEffects.None
            );

            return false;
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            hit.Damage = 0;
            hit.Knockback = 0f;
            
            // 🔥 ADICIONA STACK INICIAL AO ACERTAR
            var venomNpc = GetVenomNpc(target);
            if (venomNpc.VenomStacks < 10)
            {
                venomNpc.VenomStacks++;
                venomNpc.VenomDecayTimer = 60;
            }
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.FinalDamage *= 0f;
        }
        
        // 🔥 NOVO: Draw depois dos NPCs para mostrar stacks acima deles
        public override void PostDraw(Color lightColor)
        {
            // Desenha ícone de stacks nos NPCs afetados (opcional)
            foreach (NPC npc in Main.npc)
            {
                if (npc.active && !npc.friendly && npc.life > 0)
                {
                    var venomNpc = GetVenomNpc(npc);
                    if (venomNpc.VenomStacks > 0)
                    {
                        // Posição acima do NPC
                        Vector2 stackPos = npc.Top - new Vector2(0, 20) - Main.screenPosition;
                        
                        // Desenha texto com stacks
                        Utils.DrawBorderStringFourWay(
                            Main.spriteBatch,
                            FontAssets.MouseText.Value,
                            $"x{venomNpc.VenomStacks} total stacks",
                            stackPos.X,
                            stackPos.Y,
                            Color.Lime * 0.8f,
                            Color.Black * 0.5f,
                            Vector2.Zero,
                            0.75f
                        );
                    }
                }
            }
        }
    }
}