using Gearstorm.Content.Players;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace Gearstorm.Content.UI
{
    public class AirTrekUI : ModSystem
    {
        public override void PostDrawInterface(SpriteBatch spriteBatch)
        {
            if (Main.LocalPlayer == null || !Main.LocalPlayer.active || Main.LocalPlayer.dead) return;

            AirTrekPlayer modPlayer = Main.LocalPlayer.GetModPlayer<AirTrekPlayer>();
            if (!modPlayer.AirTrekActive) return;

            Vector2 pos = new Vector2(40, 120);

            // COMBO
            Color comboColor = Color.Lerp(Color.White, Color.Gold, MathHelper.Clamp(modPlayer.ComboPoints / 5f, 0, 1));
            Utils.DrawBorderString(spriteBatch, $"Combo: Lvl {modPlayer.ComboPoints} ({modPlayer.ComboCounter}%)", pos, comboColor);

            // MOMENTUM
            float heat = modPlayer.Momentum / 100f;
            Color momColor = Color.Lerp(Color.Cyan, Color.OrangeRed, heat);
            Utils.DrawBorderString(spriteBatch, $"Momentum: {modPlayer.Momentum:0.0}", pos + new Vector2(0, 25), momColor);

            // STATE
            string state = "NORMAL";
            Color stateColor = Color.White;

            if (modPlayer.isStomping) { state = "STOMPING!!"; stateColor = Color.Red; }
            else if (modPlayer.isOnRail) { state = "GRINDING"; stateColor = Color.LimeGreen; }
            else if (modPlayer.isOnChain) { state = "SPINNING"; stateColor = Color.Yellow; }

            Utils.DrawBorderString(spriteBatch, $"State: {state}", pos + new Vector2(0, 50), stateColor);
            
            // DICA
            Utils.DrawBorderString(spriteBatch, "[Use Hook] Dash from Rope", pos + new Vector2(0, 80), Color.Gray * 0.7f, 0.8f);
        }
    }
}