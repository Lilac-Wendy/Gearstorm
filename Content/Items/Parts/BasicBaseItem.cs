using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Gearstorm.Content.Data;

namespace Gearstorm.Content.Items.Parts
{
    public class BasicBaseItem : ModItem, IHasBeybladeStats
    {
        public override string Texture => "Gearstorm/Assets/Items/Parts/Base_Default";
        public BeybladeStats Stats => new BeybladeStats(
            tipFriction: 0.020f,
            moveSpeed: 1.0f,
            mass: 1.0f, 
            density: 1.00f
        );
        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 18;
            Item.maxStack = 99;
            Item.value = Item.sellPrice(silver: 2);
            Item.rare = ItemRarityID.White;
        }
    }
}