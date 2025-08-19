using Gearstorm.Content.Items.Parts;
using Gearstorm.Content.Projectiles.Beyblades;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Gearstorm.Content.Items.Augments;

public class ExplosionAugment : BeybladeAugment
{
    public override string Texture => "Gearstorm/Assets/Items/Parts/Augment";
    
    // Define a cor para o rastro do projétil
    public override Color AugmentColor => Color.Orange;

    public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
        Color glowColor = Color.Orange * 0.8f;
        spriteBatch.Draw(texture, position, null, glowColor, 0f, origin, scale, SpriteEffects.None, 0f);
    }
    
    public override void ApplyAugmentEffect(BaseBeybladeProjectile beybladeProj, NPC target)
    {
        // Causa a explosão, que é a principal função deste aprimoramento.
        // O dano da explosão será 1.5 vezes o dano atual da Beyblade.
        int explosionDamage = (int)(beybladeProj.Projectile.damage * 1.5f);
    
        // Spawna a bala explosiva no local do impacto.
        Projectile.NewProjectile(beybladeProj.Projectile.GetSource_FromThis(), target.Center, Vector2.Zero, ProjectileID.ExplosiveBullet, explosionDamage, 0, Main.player[beybladeProj.Projectile.owner].whoAmI);
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
        recipe.AddIngredient(ItemID.Bomb, 10);
        recipe.AddIngredient(ItemID.ExplodingBullet, 100);
        recipe.AddIngredient(ModContent.ItemType<BasicBladeItem>(), 5);
        recipe.AddTile(TileID.Anvils);
        recipe.Register();
    }
}