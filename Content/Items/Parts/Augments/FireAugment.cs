using Gearstorm.Content.Projectiles.Beyblades;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gearstorm.Content.Items.Parts.Augments;

public class FireAugment : BeybladeAugment
{
    public override Color AugmentColor => Color.OrangeRed;
    public override string Texture => "Gearstorm/Assets/Items/Parts/Augment";

    public override string ExtraDescription
    {
        get
        {
            string currentFire = Main.hardMode ? "[c/FF0000:Hellfire]" : "[c/FF8C00:On Fire!]";
            return "[c/FF4500:Searing Friction]\n"
                   + $"Incinerates targets with {currentFire}\n"
                   + "Each hit triggers a [c/FF4500:Combustion Strike] dealing [c/FF8C00:15% additional damage]\n"
                   + "'The faster it spins, the hotter it burns.'";
        }
    }
    public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
        Color glowColor = Color.OrangeRed * 0.8f;
        spriteBatch.Draw(texture, position, null, glowColor, 0f, origin, scale, SpriteEffects.None, 0f);
    }
    public override void ApplyAugmentEffect(BaseBeybladeProjectile beybladeProj, NPC target)
    {
        int debuffType = Main.hardMode ? BuffID.OnFire3 : BuffID.OnFire;
        target.AddBuff(debuffType, 240);
        int bonusDamage = (int)(beybladeProj.Projectile.damage * 0.15f);
        var hit = target.CalculateHitInfo(bonusDamage, 0, false, 0);
        target.StrikeNPC(hit);
        for (int i = 0; i < 5; i++) {
            Dust.NewDust(target.position, target.width, target.height, DustID.Flare);
        }
    }
    public override void AddRecipes()
    {
        Recipe recipe = CreateRecipe();
        recipe.AddIngredient(ItemID.FlamingArrow, 50);
        recipe.AddIngredient(ModContent.ItemType<BasicBladeItem>(), 5);
        recipe.AddTile(TileID.Anvils);
        recipe.Register();
    }
}