using Gearstorm.Content.Projectiles.Beyblades;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Gearstorm.Content.Items.Parts.Augments;

public class FrostAugment : BeybladeAugment
{
    public override string Texture => "Gearstorm/Assets/Items/Parts/Augment";
    public override Color AugmentColor => Color.LightBlue;

    public override string ExtraDescription =>
        "[c/ADD8E6:Cryogenic Impact]\n" +
        $"Strikes inflict {(Main.hardMode ? "[c/00BFFF:Frostbite]" : "[c/00FFFF:Frostburn]")}\n" +
        "Hitting an already frozen foe [c/ADD8E6:shatters] the ice for [c/00FFFF:30% bonus damage that can crit and slow them down]\n"; 


    public override void ApplyAugmentEffect(BaseBeybladeProjectile beybladeProj, NPC target, bool wasCrit)
    {
        int debuffType = Main.hardMode ? BuffID.Frostburn2 : BuffID.Frostburn;

        if (target.HasBuff(BuffID.Frostburn) || target.HasBuff(BuffID.Frostburn2))
        {
            int shatterDamage = (int)(beybladeProj.Projectile.damage * 1.30f);
            
            if (wasCrit)
            {
                shatterDamage = (int)(shatterDamage * beybladeProj.CritMultiplier);
            }
            if (shatterDamage < 1)
                for (int i = 0; i < 10; i++)
                    Dust.NewDust(target.position, target.width, target.height, DustID.SnowflakeIce);
                    target.AddBuff(BuffID.Chilled,240);
                    target.AddBuff(BuffID.Slow,120);
        }
        target.AddBuff(debuffType, 240);
    }

    public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
        spriteBatch.Draw(texture, position, null, Color.LightBlue * 0.8f, 0f, origin, scale, SpriteEffects.None, 0f);
    }

    public override void AddRecipes()
    {
        Recipe recipe = CreateRecipe();
        recipe.AddIngredient(ModContent.ItemType<BasicBladeItem>(), 5);
        recipe.AddIngredient(ItemID.FrostburnArrow, 50);
        recipe.AddTile(TileID.Anvils);
        recipe.Register();
    }
}