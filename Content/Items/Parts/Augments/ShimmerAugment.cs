using System;
using Gearstorm.Content.Buffs;
using Gearstorm.Content.Projectiles.Beyblades;
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
        "'The ceiling is just another floor you haven't stood on yet.'";


    public override void ApplyAugmentEffect(BaseBeybladeProjectile beybladeProj, NPC target, bool wasCrit)
        {
            
            for (int i = 0; i < 60; i++) 
            {
                target.velocity.Y = -6f;
            }
            target.AddBuff(BuffID.Slow, 240);
            target.AddBuff(BuffID.Confused, 240);
            target.AddBuff(BuffID.WindPushed, 240);
            target.AddBuff(ModContent.BuffType<ShimmerLevitationBuff>(), 240);
            // Partículas de impacto
            for (int i = 0; i < 12; i++) {
                Dust.NewDust(target.position, target.width, target.height, DustID.ShimmerSpark, 0, -2f);
            }
        }
    public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
        // Brilho pulsante perolado
        float pulse = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 4f) * 0.2f + 0.8f;
        Color glowColor = new Color(250, 150, 255) * pulse;
        spriteBatch.Draw(texture, position, null, glowColor, 0f, origin, scale, SpriteEffects.None, 0f);
    }

    public override void AddRecipes()
    {
        Recipe recipe = CreateRecipe();
        recipe.AddIngredient(ModContent.ItemType<BasicBladeItem>(), 5);
        // Pode ser feito com o balde de Shimmer (pós-Skeletron ou ML)
        recipe.AddIngredient(ItemID.BottomlessShimmerBucket); 
        recipe.AddTile(TileID.Anvils);
        recipe.Register();
    }
}