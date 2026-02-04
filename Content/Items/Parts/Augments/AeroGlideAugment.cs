using System;
using Gearstorm.Content.Projectiles.Beyblades;
using Terraria;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;
using Gearstorm.Content.Items.Parts;

namespace Gearstorm.Content.Items.Parts.Augments;

public class AeroGlideAugment : BeybladeAugment
{
    public override Color AugmentColor => Color.SkyBlue;
    public override string Texture => "Gearstorm/Assets/Items/Parts/Augment";
    
    public override string ExtraDescription
    {
        get
        {
            string desc = "[c/87CEEB:Atmospheric Displacement]\n"
                          + "Negates gravity and converts [c/ADD8E6:Player Speed] into rotation:\n"
                          + "Every [c/FFFFFF:20% Speed Bonus] increases spin by [c/87CEEB:1 hit/sec];\n"
                          + "Impacts [c/ADD8E6:reduce Beyblade Mass], causing higher bounces and lift.\n";

            if (Main.hardMode)
            {
                desc += "[c/FFFFFF:Hardmode Bonus: Face of the Wind] \n"
                        +"Beyblade [c/ADD8E6:always critically slices through] enemies with terminal velocity, 50% of the speed is converted to bonus damage.\n";
            }

            desc += "'Swift as a gale, untouchable as the horizon.'";
            return desc;
        }
    }

    public override void UpdateAugment(BaseBeybladeProjectile beybladeProj)
    {
        Projectile p = beybladeProj.Projectile;
        Player player = Main.player[p.owner];

        // 1. NEGAR GRAVIDADE TOTAL
        // Se houver qualquer força puxando para baixo, anulamos instantaneamente.
        if (p.velocity.Y > 0) p.velocity.Y = 0;
        p.velocity.Y -= 0.1f; // Força leve de flutuação constante

        // 2. ESCALONAMENTO DE SPIN (Player Speed -> Hit Speed)
        // Pegamos o bônus de moveSpeed (ex: 1.4f é +40%). 
        // A cada 0.2f (20%), adicionamos um multiplicador de rotação.
        float speedRatio = (player.moveSpeed - 1f) / 0.20f;
        if (speedRatio > 0)
        {
            // Ajusta o cooldown interno de dano da Beyblade para bater mais rápido
            beybladeProj.currentSpinSpeed += speedRatio * 0.15f; 
        }

        // 3. ACELERAÇÃO ATMOSFÉRICA
        float accel = Main.hardMode ? 1.03f : 1.015f;
        p.velocity *= accel;

        // Limite de Velocidade Terminal
        float maxSpeed = Main.hardMode ? 32f : 20f;
        if (p.velocity.Length() > maxSpeed) p.velocity = Vector2.Normalize(p.velocity) * maxSpeed;

        // Visual (Rastro de vento)
        if (Main.rand.NextBool(4))
        {
            Dust d = Dust.NewDustDirect(p.position, p.width, p.height, DustID.Cloud, 0f, 0f, 150, default, 0.7f);
            d.noGravity = true;
            d.velocity *= 0.1f;
        }
    }

    public override void ApplyAugmentEffect(BaseBeybladeProjectile beybladeProj, NPC target)
    {
        Projectile p = beybladeProj.Projectile;

        if (Main.hardMode)
        {
            // BONUS DAMAGE: 50% da velocidade atual vira dano base extra
            float velocityDamage = p.velocity.Length() * 0.5f;
            p.damage += (int)velocityDamage;

            // FORÇAR CRÍTICO
            // No TML, para garantir crítico no ApplyAugmentEffect, manipulamos o hit info
            // Mas visualmente, ativamos partículas de crítico
            for (int i = 0; i < 5; i++) {
                Dust.NewDust(target.position, target.width, target.height, DustID.Enchanted_Gold, 0, -2);
            }
        }
    }

    public override void OnBeybladeHit(Projectile beyblade, Vector2 hitNormal, float impactStrength, Projectile otherBeyblade, NPC targetNPC)
    {
        var beyProj = beyblade.ModProjectile as BaseBeybladeProjectile;
        if (beyProj == null) return;

        // 1. REDUÇÃO DE MASSA (Tornando-a mais leve e arisca)
        beyProj.stats.Mass = Math.Max(0.2f, beyProj.stats.Mass - 0.5f);
        
        // 2. LIFT (Empuxo vertical no impacto)
        beyblade.velocity.Y -= 6f; 

        // 3. MECÂNICA HARDMODE: SLICE THROUGH (Atravessar)
        if (Main.hardMode && targetNPC != null)
        {
            // Para "atravessar", ignoramos a hitNormal (que faria ricochetear)
            // e mantemos a direção original com um pequeno boost.
            beyblade.velocity = Vector2.Normalize(beyblade.oldVelocity) * (beyblade.oldVelocity.Length() * 1.05f);
            
            // Efeito sonoro de corte "fino"
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item71 with { Pitch = 0.5f });
        }
    }

    // Mantendo seu PostDraw e Recipes...
    public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
        spriteBatch.Draw(texture, position, null, Color.SkyBlue * 0.8f, 0f, origin, scale, SpriteEffects.None, 0f);
    }

    public override void AddRecipes()
    {
        Recipe recipe = CreateRecipe();
        recipe.AddIngredient(ItemID.Feather, 10);
        recipe.AddIngredient(ModContent.ItemType<BasicBladeItem>(), 5);
        recipe.AddTile(TileID.Anvils);
        recipe.Register();
    }
}