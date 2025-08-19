using Gearstorm.Content.Projectiles.Beyblades;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework; // Necessário para Color

namespace Gearstorm.Content.Items.Parts;

public abstract class BeybladeAugment : ModItem
{
    public virtual Color AugmentColor => Color.Transparent;
    
    public override void SetDefaults()
    {
        Item.maxStack = 1;
        Item.consumable = false; 
        Item.ammo = Item.type; 
        Item.rare = ItemRarityID.Blue;
        Item.value = Item.sellPrice(silver: 5);
    }
    
    public virtual void ApplyAugmentEffect(BaseBeybladeProjectile beybladeProj, NPC target)
    {
    }
}