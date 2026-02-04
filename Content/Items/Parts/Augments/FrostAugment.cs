using Gearstorm.Content.Items.Parts;
using Gearstorm.Content.Projectiles.Beyblades;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Gearstorm.Content.Items.Augments;

public class FrostAugment : BeybladeAugment
{
    public override string Texture => "Gearstorm/Assets/Items/Parts/Augment";
    public override Color AugmentColor => Color.LightBlue;

    public override string ExtraDescription
    {
        get
        {
            string currentFrost = Main.hardMode ? "[c/00BFFF:Frostbite]" : "[c/00FFFF:Frostburn]";
            return "[c/ADD8E6:Cryogenic Impact]\n"
                   + $"Strikes inflict {currentFrost}\n"
                   + "Hitting an already frozen foe [c/ADD8E6:shatters] the ice, dealing [c/00FFFF:20% bonus damage]\n"
                   + "'A cold precision that splinters even the toughest armor.'";
        }
    }
    public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
        Color glowColor = Color.LightBlue * 0.8f;
        spriteBatch.Draw(texture, position, null, glowColor, 0f, origin, scale, SpriteEffects.None, 0f);
    }
    
    public override void ApplyAugmentEffect(BaseBeybladeProjectile beybladeProj, NPC target)
    {
        // 1. Progressão de Debuff: Frostburn (Pre-HM) -> Frostbite (Hardmode)
        int debuffType = Main.hardMode ? BuffID.Frostburn2 : BuffID.Frostburn;
        target.AddBuff(debuffType, 240);

        // 2. Multiplicador de "Estilhaço": Se o alvo já tiver o debuff, ele toma 20% a mais de dano do impacto
        // Isso simula o gelo quebrando.
        if (target.HasBuff(BuffID.Frostburn) || target.HasBuff(BuffID.Frostburn2))
        {
            int shatterDamage = (int)(beybladeProj.Projectile.damage * 0.20f);
            var hit = target.CalculateHitInfo(shatterDamage, 0, false, 0);
            target.StrikeNPC(hit);
        
            // Efeito de gelo quebrando
            for (int i = 0; i < 10; i++) {
                Dust.NewDust(target.position, target.width, target.height, DustID.SnowflakeIce);
            }
        }
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