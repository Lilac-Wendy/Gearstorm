using Gearstorm.Content.Projectiles.Beyblades;
using Terraria;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;

namespace Gearstorm.Content.Items.Parts.Augments;

public class ChlorophyteAugment : BeybladeAugment
{
    public override Color AugmentColor => Color.LimeGreen;
    public override string Texture => "Gearstorm/Assets/Items/Parts/Augment";
    public override string ExtraDescription => "Releases life-stealing spores on impact. Healing scale is based on damage dealt.";

    public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
        spriteBatch.Draw(texture, position, null, Color.LimeGreen * 0.8f, 0f, origin, scale, SpriteEffects.None, 0f);
    }

    public override void OnBeybladeHit(Projectile beyblade, Vector2 hitNormal, float impactStrength, Projectile otherBeyblade, NPC targetNPC)
    {
        base.OnBeybladeHit(beyblade, hitNormal, impactStrength, otherBeyblade, targetNPC);

        if (targetNPC != null && Main.rand.NextBool(4)) // 25% de chance de ativar o roubo de vida
        {
            // SporeCloud da 1.4.4
            Projectile.NewProjectile(beyblade.GetSource_FromThis(), targetNPC.Center, Vector2.Zero, ProjectileID.SporeCloud, (int)(beyblade.damage * 0.5f), 1f, beyblade.owner);
            
            Player player = Main.player[beyblade.owner];
            

            int healAmount = (int)(beyblade.damage * 0.50f);
            healAmount = (int)MathHelper.Clamp(healAmount, 1, 15);

            if (player.statLife < player.statLifeMax2)
            {
                // Aplica a cura e o efeito visual (números verdes)
                player.statLife += healAmount;
                if (player.statLife > player.statLifeMax2) player.statLife = player.statLifeMax2;
                player.HealEffect(healAmount);
                
                // Feedback visual de Chlorophyte (poeira verde subindo ao player)
                for (int i = 0; i < 5; i++)
                {
                    Dust d = Dust.NewDustDirect(player.position, player.width, player.height, DustID.ChlorophyteWeapon, 0, -2f);
                    d.noGravity = true;
                }
            }
        }
    }

    public override void AddRecipes()
    {
        Recipe recipe = CreateRecipe();
        recipe.AddIngredient(ItemID.ChlorophyteBar, 8);
        recipe.AddIngredient(ModContent.ItemType<BasicBladeItem>(), 5);
        recipe.AddTile(TileID.MythrilAnvil);
        recipe.Register();
    }
}