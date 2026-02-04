using System;
using Gearstorm.Content.Items.Parts;
using Gearstorm.Content.Projectiles.Beyblades;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Gearstorm.Content.Items.Augments;

public class PoisonAugment : BeybladeAugment
{
    public override string Texture => "Gearstorm/Assets/Items/Parts/Augment";

    public override string ExtraDescription => 
        "[c/32CD32:Septic Tip]\n" +
        "Injects [c/00FF00:Neurotoxins] that deal damage over time\n" +
        "Striking poisoned foes [c/32CD32:permanently worsens] their condition\n" +
        "Each hit reduces defense by [c/32CD32:10% + 1], stacking with every rotation\n" +
        "'Nature's cruelest defense, refined for battle.'";

    public override Color AugmentColor => Color.MediumVioletRed;

    public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
        Color glowColor = Color.MediumVioletRed * 0.8f;
        spriteBatch.Draw(texture, position, null, glowColor, 0f, origin, scale, SpriteEffects.None, 0f);
    }
    
    public override void ApplyAugmentEffect(BaseBeybladeProjectile beybladeProj, NPC target)
    {
        // Apply basic Poisoned buff
        target.AddBuff(BuffID.Poisoned, 300);

        // If already poisoned, start "corroding" the defense
        if (target.HasBuff(BuffID.Poisoned))
        {
            /* LOGIC: Mixed Reduction
               1. We take 10% of the CURRENT defense.
               2. We add a flat -1 reduction to ensure it's ALWAYS linear even at low values.
               3. We cap it at 0 to prevent engine underflow.
            */
            int flatReduction = 1;
            float percentReduction = 0.90f;
            
            int newDefense = (int)(target.defense * percentReduction) - flatReduction;
            
            target.defense = Math.Max(0, newDefense);
            Dust.NewDust(target.position, target.width, target.height, DustID.GreenBlood, 0, 0, 150, default, 0.8f);
        }
    }

    public override void AddRecipes()
    {
        Recipe recipe = CreateRecipe();
        recipe.AddIngredient(ItemID.PoisonDart, 50);
        recipe.AddIngredient(ModContent.ItemType<BasicBladeItem>(), 5);
        recipe.AddTile(TileID.Anvils);
        recipe.Register();
    }
}