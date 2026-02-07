using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Gearstorm.Content.Data;
using Terraria.DataStructures;

namespace Gearstorm.Content.Items.Parts
{
    public class GoldBladeItem : ModItem, BeybladeStats.IHasBeybladeStats
    {
        public override string Texture => "Gearstorm/Assets/Items/Parts/Blade_Gold";

// No seu BasicBladeItem.cs
        public BeybladeStats Stats => new BeybladeStats
        {
            DamageBase = 15f,
            KnockbackPower = 0.4f,
            KnockbackResistance = 1f,
            Radius = 0.5f,
            BaseSpinSpeed = 20f, // Atribuição explícita pelo nome da variável
            SpinDecay = 0.010f
            // Height fica como 0f automaticamente se não declarado
        };

        public BeybladeStats.BeybladePartType PartType => BeybladeStats.BeybladePartType.Blade;


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