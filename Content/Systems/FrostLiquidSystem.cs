using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace Gearstorm.Content.Systems
{
    public static class FrostLiquidSystem
    {
        public static void FreezeLiquidsAround(Projectile projectile, int radius = 1)
        {
            if (projectile == null || !projectile.active)
                return;

            Point center = projectile.Center.ToTileCoordinates();

            for (int i = -radius; i <= radius; i++)
            {
                for (int j = -radius; j <= radius; j++)
                {
                    int tileX = center.X + i;
                    int tileY = center.Y + j;

                    if (!WorldGen.InWorld(tileX, tileY))
                        continue;

                    Tile tile = Framing.GetTileSafely(tileX, tileY);

                    if (tile.LiquidAmount <= 0)
                        continue;

                    FreezeTile(tileX, tileY, tile);
                }
            }
        }

        private static void FreezeTile(int x, int y, Tile tile)
        {
            int liquidType = tile.LiquidType;

            tile.LiquidAmount = 0;

            if (liquidType == LiquidID.Water)
            {
                tile.HasTile = true;
                tile.TileType = TileID.IceBlock;
            }
            else if (liquidType == LiquidID.Lava)
            {
                tile.HasTile = true;
                tile.TileType = TileID.Obsidian;
            }
            else if (liquidType == LiquidID.Honey)
            {
                tile.HasTile = true;
                tile.TileType = TileID.HoneyBlock;
            }
            else if (liquidType == LiquidID.Shimmer)
            {
                tile.HasTile = true;
                tile.TileType = TileID.ShimmerBlock;
            }

            WorldGen.SquareTileFrame(x, y, true);

            if (Main.netMode != NetmodeID.SinglePlayer)
                NetMessage.SendTileSquare(-1, x, y, 1);
        }
    }
}