using Gearstorm.Content.Items;
using Gearstorm.Content.Tiles.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.ID;

namespace Gearstorm.Content.Tiles;

public class CogwheelTile : ModTile
{
    public override string Texture => "Gearstorm/Assets/Items/Cogwheel";

    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true;
        Main.tileSolid[Type] = true; // Make it solid
        Main.tileBlockLight[Type] = true; // Block light
        Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = false;
        Main.tileLighted[Type] = true;

        // Setup for a 4x4 solid tile
        TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
        TileObjectData.newTile.Width = 4;
        TileObjectData.newTile.Height = 4;
        TileObjectData.newTile.CoordinateHeights = new[] { 16, 16, 16, 16 };
        TileObjectData.newTile.CoordinatePadding = 2;
        TileObjectData.newTile.Origin = new Point16(1, 1); // Center of the tile
        TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(
            ModContent.GetInstance<CogwheelEntity>().Hook_AfterPlacement,
            -1, 0, false);
        TileObjectData.addTile(Type);
        AddMapEntry(new Color(200, 200, 200), Language.GetText("Mods.Gearstorm.Tiles.CogwheelTile"));
        DustType = DustID.Iron;
        HitSound = SoundID.Tink;
    }

    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
    {
        Tile tile = Main.tile[i, j];
        
        // Only draw from the top-left tile of the multi-tile
        if (tile.TileFrameX == 0 && tile.TileFrameY == 0)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;

            // Find the top-left corner of the multi-tile
            int topLeftX = i - tile.TileFrameX / 18;
            int topLeftY = j - tile.TileFrameY / 18;

            if (TileEntity.ByPosition.TryGetValue(new Point16(topLeftX, topLeftY), out TileEntity entity) && entity is CogwheelEntity cogwheelEntity)
            {
                float rotation = cogwheelEntity.GetRotation();
                Rectangle sourceRect = new Rectangle(0, 0, texture.Width, texture.Height);
                Vector2 origin = new Vector2(texture.Width / 2f, texture.Height / 2f);
                Vector2 drawPosition = new Vector2(topLeftX * 16 + 32, topLeftY * 16 + 32) - Main.screenPosition;

                spriteBatch.Draw(
                    texture,
                    drawPosition,
                    sourceRect,
                    Lighting.GetColor(i, j),
                    rotation,
                    origin,
                    1f,
                    SpriteEffects.None,
                    0f
                );
            }
        }
        return false;
    }

    public override void KillMultiTile(int i, int j, int frameX, int frameY)
    {
        if (Main.netMode != NetmodeID.MultiplayerClient)
        {
            Tile tile = Main.tile[i, j];
            int topLeftX = i - tile.TileFrameX / 18;
            int topLeftY = j - tile.TileFrameY / 18;
            
            ModContent.GetInstance<CogwheelEntity>().Kill(topLeftX, topLeftY);
            Item.NewItem(new EntitySource_TileBreak(i, j), 
                        new Rectangle(topLeftX * 16, topLeftY * 16, 64, 64), 
                        ModContent.ItemType<CogwheelItem>());
        }
    }
}