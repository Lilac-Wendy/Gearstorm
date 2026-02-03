using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Gearstorm.Content.Data;
using Terraria.DataStructures;

namespace Gearstorm.Content.Items.Parts
{
    public class GoldBladeItem : ModItem, IHasBeybladeStats
    {
        public override string Texture => "Gearstorm/Assets/Items/Parts/Blade_Gold";

        public BeybladeStats Stats => new BeybladeStats(
            damageBase: 15f,
            knockbackPower: 0.9f,  
            knockbackResistance: 2.8f, 
            radius: 0.6f,
            spinSpeed: 2.5f, 
            spinDecay: 0.012f
        );

        public BeybladePartType PartType => BeybladePartType.Blade;


        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 8;
            Item.maxStack = 1;
            Item.value = Item.sellPrice(silver: 2);
            Item.rare = ItemRarityID.White;
        }
        public override void SetStaticDefaults()
        {
            Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(5, 4));
        }
    }
}