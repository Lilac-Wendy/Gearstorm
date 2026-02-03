using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Gearstorm.Content.Data;
using Terraria.DataStructures;

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

        public BeybladePartType PartType => BeybladePartType.Base;
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