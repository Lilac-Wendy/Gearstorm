using Gearstorm.Content.Tiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Gearstorm.Items;

// This class defines the item that places the Cogwheel tile.
public class CogwheelItem : ModItem
{
    // Points to the texture for the item itself. This should be a 64x64 image.
    public override string Texture => "Gearstorm/Assets/Items/Cogwheel";

    public override void SetDefaults() {
        Item.width = 64; // Width of the item sprite in pixels (changed to 64 to match new sprite size)
        Item.height = 64; // Height of the item sprite in pixels (changed to 64 to match new sprite size)
        Item.maxStack = 99; // Maximum stack size for the item
        Item.useStyle = ItemUseStyleID.Swing; // Defines the animation when the item is used
        Item.useTime = 10; // How long the item takes to use (in game ticks)
        Item.useAnimation = 15; // How long the use animation plays (in game ticks)
        Item.autoReuse = true; // Allows the item to be used continuously by holding the mouse button
        Item.consumable = true; // Item is consumed when placed
        Item.value = Item.buyPrice(silver: 10); // Sell/buy price (10 silver coins)
        // Links this item to the CogwheelTile, so it places that tile when used.
        Item.createTile = ModContent.TileType<CogwheelTile>();
    }
}