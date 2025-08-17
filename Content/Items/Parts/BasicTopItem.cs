using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Gearstorm.Content.Data;
// Bazinga //
namespace Gearstorm.Content.Items.Parts
{
    public class BasicTopItem : ModItem, IHasBeybladeStats
    {
        public override string Texture => "Gearstorm/Assets/Items/Parts/Top_Default";

        public BeybladeStats Stats => new BeybladeStats(
            mass: 1.0f,
            balance: 0.85f,
            Height: 0.5f
        );
        

        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 16;
            Item.maxStack = 99;
            Item.value = Item.sellPrice(silver: 2);
            Item.rare = ItemRarityID.White;
        }
    }
}