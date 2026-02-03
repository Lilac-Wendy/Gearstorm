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

    public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
        spriteBatch.Draw(texture, position, null, Color.Purple * 0.8f, 0f, origin, scale, SpriteEffects.None, 0f);
    }

    public override void ApplyAugmentEffect(BaseBeybladeProjectile beybladeProj, NPC target)
    {
        target.AddBuff(BuffID.Venom, 420);
        // O debuff Venom por padrão em Terraria já reduz regeneração a zero e causa dano por tempo alto.
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