using Gearstorm.Content.Projectiles.Beyblades;
using Terraria;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;

namespace Gearstorm.Content.Items.Parts.Augments;

public class VenomAugment : BeybladeAugment
{
    public override Color AugmentColor => Color.Purple;
    public override string Texture => "Gearstorm/Assets/Items/Parts/Augment";

    public override string ExtraDescription => "[c/800080:Acidic Dissolution]\n" +
                                               "Coats the blade in Lethal Venom, stopping health regeneration\n" +
                                               "[c/BF00FF:Vile Reaction:] Striking a venomed foe [c/FFFF00:injects Ichor], massiveley reducing armor\n" +
                                               "'It doesn't just kill; it liquefies.'";
    public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
        spriteBatch.Draw(texture, position, null, Color.Purple * 0.8f, 0f, origin, scale, SpriteEffects.None, 0f);
    }

    public override void ApplyAugmentEffect(BaseBeybladeProjectile beybladeProj, NPC target, bool wasCrit)
    {
        target.AddBuff(BuffID.Venom, 420);
        if (target.HasBuff(BuffID.Venom))
        {
            target.AddBuff(BuffID.Ichor, 180); 
            
            int erosionDamage = (int)(beybladeProj.Projectile.damage * 0.25f);
            target.SimpleStrikeNPC(erosionDamage, 0);
            
            for (int i = 0; i < 5; i++) {
                Dust.NewDust(target.position, target.width, target.height, DustID.Ichor, 0, 5);
            }
        }
    }

    public override void AddRecipes()
    {
        Recipe recipe = CreateRecipe();
        recipe.AddIngredient(ItemID.VialofVenom, 5);
        recipe.AddIngredient(ModContent.ItemType<BasicBladeItem>(), 5);
        recipe.AddTile(TileID.Anvils);
        recipe.Register();
    }
}