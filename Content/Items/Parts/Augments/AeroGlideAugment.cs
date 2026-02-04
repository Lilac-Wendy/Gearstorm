using System;
using Gearstorm.Content.Projectiles.Beyblades;
using Terraria;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;

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
                desc += "[c/FFFFFF:Hardmode Bonus:] Beyblade [c/ADD8E6:slices through] enemies with terminal velocity\n";
            }

            desc += "'Swift as a gale, untouchable as the horizon.'";
            return desc;
        }
    }
    public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
        spriteBatch.Draw(texture, position, null, Color.SkyBlue * 0.8f, 0f, origin, scale, SpriteEffects.None, 0f);
    }



public override void UpdateAugment(BaseBeybladeProjectile beybladeProj)
{
    Projectile p = beybladeProj.Projectile;
    Player player = Main.player[p.owner];

    // 1. Negar gravidade
    if (p.velocity.Y > 0) p.velocity.Y -= 0.35f;

    // 2. Escalonamento de Spin Speed baseado na velocidade do jogador
    // Compara a velocidade atual do player com a base (máximo de 1.0f extra de hits/seg a cada 20%)
    float playerSpeedBonus = (player.moveSpeed - 1f) / 0.20f; // Ex: +20% (1.2) resulta em 1.0
    if (playerSpeedBonus > 0)
    {
        // Aumenta o spinSpeed do projétil, o que afetará o ApplyContinuousDamage
        beybladeProj.currentSpinSpeed += playerSpeedBonus * 0.1f; 
    }

    // 3. Aceleração constante e Limites
    float accel = Main.hardMode ? 1.025f : 1.01f;
    p.velocity *= accel;

    float maxSpeed = Main.hardMode ? 26f : 18f;
    if (p.velocity.Length() > maxSpeed) p.velocity = Vector2.Normalize(p.velocity) * maxSpeed;

    // Visual
    if (Main.rand.NextBool(8))
    {
        Dust d = Dust.NewDustDirect(p.position, p.width, p.height, DustID.Cloud, 0f, 0f, 150, default, 0.9f);
        d.noGravity = true;
    }
}

public override void OnBeybladeHit(Projectile beyblade, Vector2 hitNormal, float impactStrength, Projectile otherBeyblade, NPC targetNPC)
{
    // 1. Bônus de Velocidade por Colisão
    float speedBoost = (targetNPC != null) ? 1.2f : 1.05f; // Maior contra NPCs
    beyblade.velocity *= speedBoost;


    var beyProj = beyblade.ModProjectile as BaseBeybladeProjectile;
    if (beyProj != null)
    {
        beyProj.stats.Mass = Math.Max(0.5f, beyProj.stats.Mass - 1.0f);
        
        // Empuxo para cima ao atingir alvo (Simulando aerofólio)
        beyblade.velocity.Y -= 4f; 
    }

    // 3. Mecânica Hardmode: Atravessar
    if (Main.hardMode && targetNPC != null)
    {
        // Anula o ricochete para atravessar o inimigo
        beyblade.velocity += hitNormal * (impactStrength * 0.85f); 
        
        if (Main.rand.NextBool(2))
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item120 with { Volume = 0.5f, Pitch = 0.9f }, beyblade.Center);
    }
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