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
    public override string ExtraDescription => 
        "[c/FF69B4:Crystalline Fracture]\n" +
        "High-velocity impacts release [c/FFB6C1:4 Crystal Shards] toward enemies,\n" +
        " including if it hits another Beyblade. \n" +
        "Each shard deals [c/FF69B4:50% of the blade's base damage]\n" +
        "Brittle structure causes [c/FFB6C1:constant splintering] upon heavy collisions\n" +
        "'Beauty and lethality, shattered into a thousand pieces'";
    public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
        spriteBatch.Draw(texture, position, null, Color.Pink * 0.8f, 0f, origin, scale, SpriteEffects.None, 0f);
    }

    public override void OnBeybladeHit(Projectile beyblade, Vector2 hitNormal, float impactStrength, Projectile otherBeyblade, NPC targetNpc, bool wasCrit)
    {
        if (targetNpc != null || impactStrength > 1.5f)
        {
            for (int i = 0; i < 4; i++)
            {
                // Velocidade um pouco maior para o estilhaço não "morrer" no pé da beyblade
                Vector2 speed = hitNormal.RotatedByRandom(0.5f) * 8f;
            
                int proj = Projectile.NewProjectile(
                    beyblade.GetSource_FromThis(), 
                    beyblade.Center, 
                    speed, 
                    ProjectileID.CrystalShard, 
                    (int)(beyblade.damage * 0.5f), 
                    2f, 
                    beyblade.owner
                    
                );

                if (proj != Main.maxProjectiles)
                {
                    Projectile p = Main.projectile[proj];
                    
                    p.friendly = true;
                    p.hostile = false;
                    p.timeLeft = 480; // Dá tempo de eles fazerem a curva e atingirem o alvo
                    p.netUpdate = true;
                    p.scale = 2.5f;
                    Lighting.AddLight(p.Center, 0.8f, 0.2f, 0.7f);
                }
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