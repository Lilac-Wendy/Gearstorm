using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Gearstorm.Content.Data;
using Terraria.DataStructures;

namespace Gearstorm.Content.Items.Parts
{
    public class BasicTopItem : ModItem, BeybladeStats.IHasBeybladeStats
    {
        public override string Texture => "Gearstorm/Assets/Items/Parts/Top_Default";

        public BeybladeStats Stats => new BeybladeStats 
        {  
            Mass = 1.0f,
            Balance = 2.00f,
            Height = 0.5f
        };
        public BeybladeStats.BeybladePartType PartType => BeybladeStats.BeybladePartType.Top;

        public override void SetStaticDefaults()
        {
            Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(5, 2));
        }


        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 9;
            Item.maxStack = 1;
            Item.value = Item.sellPrice(silver: 2);
            Item.rare = ItemRarityID.White;
        }
    }
}