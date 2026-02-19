using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Gearstorm.Content.Data;
using Terraria.DataStructures;

namespace Gearstorm.Content.Items.Parts
{
    public class BasicTopItem : ModItem, BeybladeStats.IHasBeybladeStats
    {
        public override string Texture => "Gearstorm/Assets/Items/Parts/Top_Iron";

        public BeybladeStats Stats => new BeybladeStats 
        {  
            Mass = 1.0f,
            Balance = 2.00f,
            Height = 0.5f
        };
        public BeybladeStats.BeybladePartType PartType => BeybladeStats.BeybladePartType.Top;

        public override void SetStaticDefaults()
        {
            // O primeiro parâmetro é o tempo entre frames, o segundo é a quantidade de frames
            Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(5, 8));
        }

        public override void SetDefaults()
        {
            Item.width = 46;
            Item.height = 10; // AQUI: Apenas a altura de UM frame
            Item.maxStack = 1;
            Item.value = Item.sellPrice(silver: 2);
            Item.rare = ItemRarityID.White;
        }
    }
}