using Gearstorm.Content.Player;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace Gearstorm.Content.UI;

public class AirTrekUI : ModSystem
{
    public override void PostDrawInterface(SpriteBatch spriteBatch)
    {
        AirTrekPlayer player = Main.LocalPlayer.GetModPlayer<AirTrekPlayer>();
        AirTrekPlayer p = Main.LocalPlayer.GetModPlayer<AirTrekPlayer>();
        if (!p.hasAirTrekEquipped) return; 
        if (!player.active) return;
        Vector2 pos = new Vector2(50, 50);
        string comboText = $"Combo Points: {player.comboPoints}";
        string momentumText = $"Momentum: {player.momentum:0.0}";
        Utils.DrawBorderString(spriteBatch, comboText, pos, Color.Yellow);
        Utils.DrawBorderString(spriteBatch, momentumText, pos + new Vector2(0, 20), Color.Cyan);
        string tricks = "Tricks:\n - Rail Jump\n - Wall Jump\n - Chain Climb\n - Stomp";
        Utils.DrawBorderString(spriteBatch, tricks, pos + new Vector2(0, 50), Color.White);
    }
}