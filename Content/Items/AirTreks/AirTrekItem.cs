using Gearstorm.Content.DamageClasses;
using Gearstorm.Content.Player;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Gearstorm.Content.Items.AirTreks
{
    public class AirTrek : ModItem
    {
        public override string Texture => "Gearstorm/Assets/Items/AirTreks/AirTrekItem";

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.accessory = true;
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(silver: 50);
        }

        public override void UpdateAccessory(Terraria.Player player, bool hideVisual)
        {
            if (player.TryGetModPlayer(out AirTrekPlayer atp))
            {
                atp.active = true;
                player.GetDamage<AirTrekDamage>() += atp.comboPoints * 0.05f;
            }
        }

        public override void OnHitNPC(Terraria.Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (player.TryGetModPlayer(out AirTrekPlayer atp))
            {
                atp.AddComboPoints(10);
            }
        }
    }
}