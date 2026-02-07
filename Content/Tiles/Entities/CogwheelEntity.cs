using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Gearstorm.Content.Tiles.Entities;

public class CogwheelEntity : ModTileEntity
{
    private float rotation;
    private float angularVelocity;

    public override void Update()
    {
        Rectangle hitbox = new Rectangle(Position.X * 1, Position.Y * 1, 64, 64);

        foreach (Player player in Main.player)
        {
            if (player.active && player.Hitbox.Intersects(hitbox))
            {
                angularVelocity += 0.20f;
            }
        }

        angularVelocity *= 0.98f;
        rotation += angularVelocity;

        if (rotation > MathHelper.TwoPi) rotation -= MathHelper.TwoPi;
        if (rotation < 0f) rotation += MathHelper.TwoPi;
    }

    public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate)
    {
        int topLeftX = i - 1;
        int topLeftY = j - 1;

        if (Main.netMode == NetmodeID.MultiplayerClient)
        {
            NetMessage.SendTileSquare(Main.myPlayer, topLeftX, topLeftY, 4);
            NetMessage.SendData(MessageID.TileEntityPlacement, number: topLeftX, number2: topLeftY, number3: Type);
            return -1;
        }
        return Place(topLeftX, topLeftY);
    }

    public override bool IsTileValidForEntity(int x, int y)
    {
        Tile tile = Main.tile[x, y];
        return tile.HasTile && tile.TileType == ModContent.TileType<CogwheelTile>();
    }

    public float GetRotation() => rotation;
}