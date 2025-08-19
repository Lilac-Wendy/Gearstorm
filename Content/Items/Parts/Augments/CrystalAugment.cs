using Gearstorm.Content.Items.Parts;
using Gearstorm.Content.Projectiles.Beyblades;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Gearstorm.Content.Items.Augments;

public class CrystalAugment : BeybladeAugment
{
    public override string Texture => "Gearstorm/Assets/Items/Parts/Augment";

    // Define a cor para o rastro do projétil
    public override Color AugmentColor => Color.Cyan;

    public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
        Color glowColor = Color.Cyan * 0.8f;
        spriteBatch.Draw(texture, position, null, glowColor, 0f, origin, scale, SpriteEffects.None, 0f);
    }
    
    public override void ApplyAugmentEffect(BaseBeybladeProjectile beybladeProj, NPC target)
    {
        int shardDamage = (int)(beybladeProj.Projectile.damage * 0.5f);
        
        for (int i = 0; i < 3; i++)
        {
            Vector2 shardVelocity = new Vector2(Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-4f, 4f));
            Projectile.NewProjectile(beybladeProj.Projectile.GetSource_FromThis(), beybladeProj.Projectile.Center, shardVelocity, ProjectileID.CrystalShard, shardDamage, 2f, Main.player[beybladeProj.Projectile.owner].whoAmI);
        }
        if (!beybladeProj.bonusesApplied)
        {
            beybladeProj.Projectile.timeLeft = 60;
            beybladeProj.Projectile.penetrate = 1;
            beybladeProj.bonusesApplied = true;
        }
    }

    public override void AddRecipes()
    {
        Recipe recipe = CreateRecipe();
        recipe.AddIngredient(ItemID.CrystalShard, 50);
        recipe.AddIngredient(ItemID.SoulofLight, 5);
        recipe.AddIngredient(ModContent.ItemType<BasicBladeItem>(), 5);
        recipe.AddTile(TileID.MythrilAnvil);
        recipe.Register();
    }
}