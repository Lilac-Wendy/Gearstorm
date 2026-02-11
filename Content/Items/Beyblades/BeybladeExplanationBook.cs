using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Gearstorm.Content.Items.Beyblades
{
    public class BeybladeExplanationBook : ModItem
    {
        public override string Texture => $"Terraria/Images/Item_{ItemID.Book}";
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 32;
            Item.rare = ItemRarityID.Red;
            Item.value = Item.buyPrice(0, 2);
            Item.maxStack = 1;
            Item.useStyle = ItemUseStyleID.HoldUp;
            
        }

public override void ModifyTooltips(List<TooltipLine> tooltips)
{
    var info = new TooltipLine(Mod, "Manual",
        "[c/FFD700:Lesson 1: THE PHYSICS OF SPIN]\n" +
        "• [c/66FFFF:Base Spin Speed:]   The primary energy resource of a Beyblade. Base Spin Speed defines how fast the Bey starts spinning at launch and acts as the foundation for all critical calculations. \n  Spin Speed is NOT final by itself. It is directly multiplied by [c/00FFAA:Balance].\n" +
        "• [c/00FFAA:Balance:] Multiplies Base Spin Speed and reduces Spin Decay. Final Spin = [b:Base Spin × Balance].\n" +
        "• [c/FF5555:Spin Decay:] How fast Spin is lost over time and impacts. Lower is better; strongly mitigated by Balance.\n" +
        "• [c/BDBDBD:Moment of Inertia:] [c/FFFFFF:Mass × Radius]. Higher inertia resists Spin loss from friction and collisions. A large carousel is hard to stop.\n" +
        "• [c/FFA500:Density:] Controls physical behavior. Low density bounces; high density stays grounded.\n" +
        "• [c/808080:Tip Friction:] Ground drag. Low friction enables drifting and wide movement arcs.\n" +
        "\n" +
        "[c/FFD700:Lesson 2: COMBAT & CRITICALS]\n" +
        "• [c/FF3333:Damage:] [b:Base Damage × (1 + Mass × 0.1) × Balance].\n" +
        "• [c/FFFF66:Critical Chance:] Critical Chance represents a Beyblade's maximum potential for critical hits. Every [b:1.0 Spin] grants [b:20%Crit Chance] (capped at 100%).\n" +
        "• [c/FF00FF:Over-Spinning:] Spin above [b:5.0] converts into Critical Damage. Each extra Spin grants [b:+40% Crit Damage], stacking infinitely.\n" +
        "• [c/FFCCAA:Knockback:] Scales with Mass and Knockback Power. Heavy Beys are harder to stop.\n" +
        "\n" +
        "[c/FFD700:Lesson 3: PLAYSTYLES]\n" +
        "• [c/FF4444:STRIKER:] Mass + Radius. High impact damage, high Spin loss.\n" +
        "• [c/44FF44:SURVIVOR:] Balance + Inertia. Sustains Spin and guarantees critical hits.\n" +
        "• [c/4444FF:DRIFTER:] Move Speed + low Friction. Wide-area control through mobility.\n" +
        "• [c/FFFF00:OVERCLOCKER:] Push Spin beyond 5.0. Unlocks infinite Critical Damage scaling.\n" +
        "• [c/808080:TANK:] Density + Knockback Resistance. Ignores recoil and disruption.\n" +
        "• [c/A020F0:RAIL-RIDER:] Minecart Tracks convert Spin into guided, near-frictionless motion.\n" +
        "• [c/8B4513:JUGGERNAUT:] Extreme Inertia. Hard to accelerate, nearly impossible to slow.\n" +
        "• [c/00FFFF:METEOR:] Low Density, High Mass. Chaotic bounces and aerial hits.\n" +
        "\n" +
        "[c/FFD700:Lesson 4: ADVANCED MANEUVERS]\n" +
        "• [b:Rail Grinding:] Tracks reduce friction and allow directional control.\n" +
        "• [b:Augments:] Up to 4 can trigger simultaneously on hit.\n" +
        "• [b:Stability:] Heavy impacts drain Spin; low-Inertia builds must avoid dense targets."
    )
    {
        OverrideColor = Color.White
    };

    tooltips.Add(info);
}


        public override bool? UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                Terraria.Audio.SoundEngine.PlaySound(SoundID.MenuOpen);
            }
            return true;
        }
    }
}