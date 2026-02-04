using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Gearstorm.Content.Data;
using Terraria.DataStructures;

namespace Gearstorm.Content.Items.Parts
{
    public class BasicBladeItem : ModItem, IHasBeybladeStats
    {
        public override string Texture => "Gearstorm/Assets/Items/Parts/Blade_Default";

        public BeybladeStats Stats => new BeybladeStats(
            damageBase: 10f,
            knockbackPower: 0.4f,  
            knockbackResistance: 1f, 
            radius: 0.5f,
            baseSpinSpeed: 1.2f, 
            spinDecay: 0.010f
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