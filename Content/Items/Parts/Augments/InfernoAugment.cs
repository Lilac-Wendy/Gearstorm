using Terraria;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;
using Terraria.Audio;

namespace Gearstorm.Content.Items.Parts.Augments;

public class InfernoAugment : BeybladeAugment
{
    public override Color AugmentColor => Color.DarkOrange;
    public override string Texture => "Gearstorm/Assets/Items/Parts/Augment";
    public override string ExtraDescription => 
        "[c/FF4500:Thermal Cataclysm]\n" +
        "Heavy impacts trigger a [c/FF8C00:Fiery Explosion] dealing 50% area damage\n" +
        "Spreads [c/FF4500:Hellfire] to all enemies within a wide radius\n" +
        "Explosions occur upon [c/FF8C00:NPC contact] or high-velocity collisions\n" +
        "'Set the arena ablaze with every revolution'";
    public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
        spriteBatch.Draw(texture, position, null, Color.DarkOrange * 0.9f, 0f, origin, scale, SpriteEffects.None, 0f);
    }

    public override void OnBeybladeHit(Projectile beyblade, Vector2 hitNormal, float impactStrength, Projectile otherBeyblade, NPC targetNpc, bool wasCrit)
    {
        // Só explode se o impacto for considerável ou atingir um NPC
        if (targetNpc != null || impactStrength > 3f)
        {
            Explode(beyblade);
        }
    }

    private void Explode(Projectile beyblade)
    {
        SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.6f, Pitch = 0.2f }, beyblade.Center);

        // Efeitos visuais da explosão
        for (int i = 0; i < 20; i++)
        {
            Dust d = Dust.NewDustDirect(beyblade.position, beyblade.width, beyblade.height, DustID.SolarFlare, 0f, 0f, 100, default, 2f);
            d.noGravity = true;
            d.velocity *= 3f;
        }

        // Dano em área e debuff
        float explosionRadius = 120f;
        foreach (NPC npc in Main.npc)
        {
            if (npc.active && !npc.friendly && Vector2.Distance(npc.Center, beyblade.Center) < explosionRadius)
            {
                npc.AddBuff(BuffID.OnFire3, 300); // Hellfire
                npc.SimpleStrikeNPC((int)(beyblade.damage * 1.5f), 0);
            }
        }
    }

    public override void AddRecipes()
    {
        Recipe recipe = CreateRecipe();
        recipe.AddIngredient(ModContent.ItemType<InfernoAugment>()); // Precisa da versão base (ou remova se for upgrade direto)
        recipe.AddIngredient(ItemID.StickyBomb, 10);
        recipe.AddIngredient(ModContent.ItemType<FireAugment>());
        recipe.AddTile(TileID.Anvils);
        recipe.Register();
    }
}