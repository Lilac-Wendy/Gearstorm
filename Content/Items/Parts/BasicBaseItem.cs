using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Gearstorm.Content.Data;
using Terraria.DataStructures;

namespace Gearstorm.Content.Items.Parts
{
    public class BasicBaseItem : ModItem, BeybladeStats.IHasBeybladeStats
    {
        public override string Texture => "Gearstorm/Assets/Items/Parts/Base_Default";
        public BeybladeStats Stats => new BeybladeStats 
        {
            TipFriction = 0.020f,
            MoveSpeed = 1.0f,
            Mass = 1.0f, 
            Density = 1.00f,
        };

        public BeybladeStats.BeybladePartType PartType => BeybladeStats.BeybladePartType.Base;
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