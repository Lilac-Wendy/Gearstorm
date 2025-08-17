using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Gearstorm.Content.Data;
using Gearstorm.GlobalItems;

namespace Gearstorm.Content.Items.Parts
{
    public class BasicBladeItem : ModItem, IHasBeybladeStats
    {
        public override string Texture => "Gearstorm/Assets/Items/Parts/Blade_Default";

        public BeybladeStats Stats => new BeybladeStats(
            damageBase: 15f,
            knockbackPower: 3.0f,  
            knockbackResistance: 1.8f, 
            Radius: 0.5f,
            spinSpeed: 1.5f, 
            SpinDecay: 0.010f
        );
        

        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 6;
            Item.maxStack = 99;
            Item.value = Item.sellPrice(silver: 2);
            Item.rare = ItemRarityID.White;
        }
    }
}