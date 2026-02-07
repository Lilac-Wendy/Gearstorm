using System;
using Gearstorm.Content.Buffs;
using Gearstorm.Content.Data;
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
                          + "Alters aerodynamic response, enabling sustained lift through momentum.\n"
                          + "Each [c/FFFFFF:20% Move Speed] of the Player grants [c/87CEEB:+10% Crit Chance].\n"
                          + "Continuously accelerates up to a speed cap.\n"
                          + "Successive hits between Beyblades [c/ADD8E6:shred 0.4 Mass], increasing airtime and rebound.\n"
                          + "On hit: launches enemies upward and destabilizes movement.\n";


            if (Main.hardMode)
            {
                desc += "\n[c/FFFFFF:Hardmode: Face of the Wind]\n"
                        + "Reaches [c/ADD8E6:Supercritical Velocity] (Higher speed cap).\n"
                        + "Hits deal additional damage based on current velocity.\n"
                        + "Strikes extend Beyblade lifetime and boost post-impact speed.\n"
                        + "Enemies have a [c/ADD8E6:25% chance] to be directionally reversed on impact.\n";
            }
            
            desc += "\n[c/808080:'Momentum is law. The wind merely enforces it.']";
            return desc;
        }
    }

    public override void UpdateAugment(BaseBeybladeProjectile beybladeProj)
    {
        Projectile p = beybladeProj.Projectile;
        Player player = Main.player[p.owner];

        if (float.IsNaN(p.velocity.X) || float.IsNaN(p.velocity.Y)) p.velocity = Vector2.UnitX * 2f;
        
        float movementBonus = (player.moveSpeed - 1f) / 0.20f;
        if (movementBonus > 0)
        {
            p.CritChance = (int)(10 * movementBonus); 
        }
        
        float targetMaxSpeed = Main.hardMode ? 35f : 18f;
        float currentSpeed = p.velocity.Length();

        if (currentSpeed < targetMaxSpeed)
        {
            p.velocity *= 1.02f;
        }
        else if (currentSpeed > targetMaxSpeed + 2f)
        {
            p.velocity = Vector2.Normalize(p.velocity) * targetMaxSpeed;
        }
    }

    public override void ApplyAugmentEffect(BaseBeybladeProjectile beybladeProj, NPC target)
    {
        Projectile p = beybladeProj.Projectile;

        // 5. IMPACTO CINÉTICO (Substituindo o True Damage)
        // 30% do dano da Beyblade + Bônus por Velocidade atual.
        float kineticFactor = p.velocity.Length() * 0.02f; // Ex: 35 de speed = +70% de dano no estilhaço
        int shatterDamage = (int)(p.damage * (0.30f + kineticFactor));

        // Aplica um hit secundário oficial do Terraria (Pode ser crítico e respeita defesa)
        var hit = target.CalculateHitInfo(shatterDamage, 1, true, 0);
        target.StrikeNPC(hit);

        // Feedback visual
        for (int i = 0; i < 6; i++) {
            Dust d = Dust.NewDustDirect(target.position, target.width, target.height, DustID.Cloud);
            d.noGravity = true;
            d.velocity *= 1.5f;
        }
    }

public override void OnBeybladeHit(Projectile beyblade, Vector2 hitNormal, float impactStrength, Projectile otherBeyblade, NPC targetNPC)
{

    var beyProj = beyblade.ModProjectile as BaseBeybladeProjectile;
    if (beyProj == null)
        return;

    if (targetNPC != null)
    {
        // sempre aplica o deslocamento vertical
        targetNPC.velocity.Y -= 2f;

        // 25% de chance de inverter a direção
        if (Main.rand.NextFloat() < 0.25f)
        {
            targetNPC.direction *= -1;
        }
    }

// 1. REDUÇÃO DE MASSA (simples, previsível, configurável externamente)
    beyProj.stats.Mass = Math.Max(0.1f, beyProj.stats.Mass - 0.4f);

// 2. LIFT AERODINÂMICO DIRETO
    beyblade.velocity.Y -= 10f;
    beyblade.timeLeft += 10;

// 3. HARDMODE SLICE 
    if (Main.hardMode && targetNPC != null)
    {
        Vector2 ov = beyblade.oldVelocity;

        if (ov.LengthSquared() > 0.001f) { beyblade.velocity = ov * 1.02f; }
        else
        { beyblade.velocity += Vector2.UnitX * 0.5f; }
        Terraria.Audio.SoundEngine.PlaySound(SoundID.Item105 with { Pitch = 0.4f, Volume = 0.6f });
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
        recipe.AddIngredient(ItemID.Feather, 100);
        recipe.AddIngredient(ModContent.ItemType<BasicBladeItem>(), 5);
        recipe.AddTile(TileID.Anvils);
        recipe.Register();
    }
}