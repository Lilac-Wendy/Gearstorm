using Gearstorm.Content.Projectiles.Beyblades;
using Terraria;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;

namespace Gearstorm.Content.Items.Parts.Augments;

public class CrystalAugment : BeybladeAugment
{
    public override Color AugmentColor => Color.Pink;
    public override string Texture => "Gearstorm/Assets/Items/Parts/Augment";
    public override string ExtraDescription => "Shatters on impact, releasing homing crystal shards toward the nearest foe.";

    public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
        spriteBatch.Draw(texture, position, null, Color.Pink * 0.8f, 0f, origin, scale, SpriteEffects.None, 0f);
    }

    public override void OnBeybladeHit(Projectile beyblade, Vector2 hitNormal, float impactStrength, Projectile otherBeyblade, NPC targetNPC)
    {
        if (impactStrength > 2f)
        {
            for (int i = 0; i < 3; i++)
            {
                Vector2 speed = hitNormal.RotatedByRandom(0.4f) * 6f;
                Projectile.NewProjectile(beyblade.GetSource_FromThis(), beyblade.Center, speed, ProjectileID.CrystalShard, (int)(beyblade.damage * 0.5f), 2f, beyblade.owner);
            }
        }
    }

    public override void AddRecipes()
    {
        Recipe recipe = CreateRecipe();
        recipe.AddIngredient(ItemID.CrystalShard, 15);
        recipe.AddIngredient(ModContent.ItemType<BasicBladeItem>(), 5);
        recipe.AddTile(TileID.Anvils);
        recipe.Register();
    }
}