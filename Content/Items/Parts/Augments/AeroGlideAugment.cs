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
    public override string ExtraDescription => "Grants near-weightless movement, allowing the beyblade to float and ignore gravity.";

    public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
        spriteBatch.Draw(texture, position, null, Color.SkyBlue * 0.8f, 0f, origin, scale, SpriteEffects.None, 0f);
    }

    public override void UpdateAugment(BaseBeybladeProjectile beybladeProj)
    {
        // Anula a gravidade de 0.35f definida no seu BaseBeybladeProjectile
        if (beybladeProj.Projectile.velocity.Y > 0)
            beybladeProj.Projectile.velocity.Y -= 0.35f;

        if (Main.rand.NextBool(4))
        {
            Dust d = Dust.NewDustPerfect(beybladeProj.Projectile.Center, DustID.Cloud, -beybladeProj.Projectile.velocity * 0.2f, 150, Color.White, 0.7f);
            d.noGravity = true;
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