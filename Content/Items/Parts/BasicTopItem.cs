using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Gearstorm.Content.Data;
using Terraria.DataStructures;

namespace Gearstorm.Content.Items.Parts
{
    public class BasicTopItem : ModItem, IHasBeybladeStats
    {
        public override string Texture => "Gearstorm/Assets/Items/Parts/Top_Default";

        public BeybladeStats Stats => new BeybladeStats(
            mass: 1.0f,
            balance: 0.85f,
            height: 0.5f
        );

        public BeybladePartType PartType => BeybladePartType.Top;

        public override void SetStaticDefaults()
        {
            Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(5, 2));
        }


        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 18;
            Item.maxStack = 1;
            Item.value = Item.sellPrice(silver: 2);
            Item.rare = ItemRarityID.White;
        }
    }
}