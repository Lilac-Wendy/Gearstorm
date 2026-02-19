using System;
using Gearstorm.Content.Buffs;
using Gearstorm.Content.Projectiles.Beyblades;
using Gearstorm.Content.Systems;
using Gearstorm.Content.Items.Parts;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Gearstorm.Content.Items.Parts.Augments;

public class ShimmerAugment : BeybladeAugment
{
    public override string Texture => "Gearstorm/Assets/Items/Parts/Augment";
    public override Color AugmentColor => new Color(250, 150, 255);

    public override string ExtraDescription => 
        "Grants [c/F096FF:Phasing Strike]: Hits launch enemies upward.\n" +
        "After hitting an enemy, the Beyblade can pass through walls horizontally for 3 seconds.\n" +
        "'The ceiling is just another floor you haven't stood on yet.'";

    // O UpdateAugment corre a cada frame da AI da Beyblade se este Augment estiver equipado.
    public override void UpdateAugment(BaseBeybladeProjectile beybladeProj)
    {
        // Usamos localAI[2] como o timer de fase para não poluir a base com variáveis novas.
        int timer = (int)beybladeProj.Projectile.localAI[2];

        // Chama o sistema externo que criamos no Canvas
        ShimmerPhaseSystem.HandleHorizontalPhasing(beybladeProj.Projectile, timer);

        // Decrementa o timer se ele estiver ativo
        if (beybladeProj.Projectile.localAI[2] > 0)
        {
            beybladeProj.Projectile.localAI[2]--;
        }
    }

    public override void ApplyAugmentEffect(BaseBeybladeProjectile beybladeProj, NPC target, bool wasCrit)
    {
        // Ativa o timer de 3 segundos (60 frames * 3)
        beybladeProj.Projectile.localAI[2] = 180;

        // Lógica original de lançamento e buffs
        target.velocity.Y = -6f;
        
        target.AddBuff(BuffID.Slow, 240);
        target.AddBuff(BuffID.Confused, 240);
        target.AddBuff(BuffID.WindPushed, 240);
        
        // Verifica se o buff de levitação existe antes de aplicar
        int levitationBuff = ModContent.TryFind<ModBuff>("Gearstorm/ShimmerLevitationBuff", out var buff) ? buff.Type : -1;
        if (levitationBuff != -1)
        {
            target.AddBuff(levitationBuff, 240);
        }

        // Partículas de impacto
        for (int i = 0; i < 12; i++) 
        {
            Dust.NewDust(target.position, target.width, target.height, DustID.ShimmerSpark, 0, -2f);
        }
    }

    public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
        float pulse = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 4f) * 0.2f + 0.8f;
        Color glowColor = new Color(250, 150, 255) * pulse;
        spriteBatch.Draw(texture, position, null, glowColor, 0f, origin, scale, SpriteEffects.None, 0f);
    }

    public override void AddRecipes()
    {
        Recipe recipe = CreateRecipe();
        recipe.AddIngredient(ModContent.ItemType<BasicBladeItem>(), 5);
        recipe.AddIngredient(ItemID.BottomlessShimmerBucket); 
        recipe.AddTile(TileID.Anvils);
        recipe.Register();
    }
}