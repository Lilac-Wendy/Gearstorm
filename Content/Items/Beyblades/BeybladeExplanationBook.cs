using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Gearstorm.Content.Items
{
    public class BeybladeExplanationBook : ModItem
    {
        public override string Texture => $"Terraria/Images/Item_{ItemID.Book}";
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 32;
            Item.rare = ItemRarityID.Green;
            Item.value = Item.buyPrice(0, 1, 0, 0);
            Item.maxStack = 1;
            Item.useStyle = ItemUseStyleID.HoldUp;
            
        }

         public override void ModifyTooltips(List<TooltipLine> tooltips) {
            var info = new TooltipLine(Mod, "Manual",
                "[c/FFD700:Lesson 1: THE PHYSICS OF SPIN ]\n" +
                "• [c/66FFFF:Base Spin Speed:] Your primary resource. It determines your APS (Attacks Per Second) and Crit Chance.\n" +
                "• [c/FF5555:Spin Decay:] Spin Decay: How quickly you lose energy. Strongly mitigated by [c/FFD700:Balance]. Lower Decay is better.\n" +
                "• [c/BDBDBD:Moment of Inertia:] Calculated as [c/FFFFFF:Mass × Radius]. Higher inertia makes your spin resistant to decay from friction and impacts.\n" +
                "• [c/FFA500:Density:] Determines 'weight' in physics. Low density Beys bounce more; high density Beys stick to the floor.\n" +
                "• [c/808080:Tip Friction:] How much the ground slows your horizontal movement. Low friction allows for high-speed drifting.\n" +
                "\n" +
                "[c/FFD700:Lesson 2: COMBAT & CRITICALS ]\n" +
                "• [c/FF3333:Damage Calculation:] Final Damage = [b:BaseDamage × (1 + Mass × 0.1) × Balance].\n" +
                "• [c/FFFF66:Crit Chance:] Automatically becomes [b:SpinSpeed × 20%]. At 5.0 Spin, you have 100% Crit.\n" +
                "• [c/FF00FF:Over-Spinning:] Above 5.0 Spin, every extra unit adds [b:+40% Crit Damage] and massive [b:Armor Penetration] (Spin × 10).\n" +
                "• [c/FFCCAA:Knockback:] Scaled by [b:Mass] and [b:Knockback Power]. High mass Beys are harder to stop.\n" +
                "\n" +
                "[c/FFD700:Lesson 3: GAMEPLAY STYLES]\n" +
                "• [c/FF4444:STRIKER:] Maximize [b:Mass] and [b:Radius]. High impact damage and knockback. Weakness: High Spin Decay.\n" +
                "• [c/44FF44:SURVIVOR:] Maximize [b:Balance] and [b:Inertia]. Designed for long fights and high crit consistency.\n" +
                "• [c/4444FF:DRIFTER:] Maximize [b:MoveSpeed] and [b:Low Friction]. High mobility to hit multiple targets.\n" +
                "• [c/FFFF00:OVERCLOCKER:] Push [b:Base Spin Speed] beyond 5.0. Unlocks infinite crit damage scaling and total armor shredding.\n" +
                "• [c/808080:TANK:] Focus on [b:Density] and [b:Knockback Resistance]. Ignores recoil from impacts; it stays exactly where you launched it.\n" +
                "• [c/A020F0:RAIL-RIDER:] High [b:MoveSpeed] used on Minecart Tracks. Converts the Bey into a guided missile with near-zero friction loss.\n" +
                "• [c/8B4513:JUGGERNAUT:] Prioritize [b:Moment of Inertia] (Mass × Radius). Hard to accelerate, but almost impossible for enemies to slow down.\n" +
                "• [c/00FFFF:METEOR:] Low [b:Density] with High [b:Mass]. Causes chaotic bounces, allowing the Bey to 'fly' and hit aerial enemies.\n" +
                "\n" +
                "[c/FFD700:Lesson 4: MANEUVERS ]\n" +
                "• [b:Rail Grinding:] Landing on Minecart Tracks reduces friction and grants gravity control.\n" +
                "• [b:Augment Mixing:] Up to 4 Augments (Ammo Slots) can be active. Their colors mix, and their effects trigger on every hit.\n" +
                "• [b:Stability:] Impacts drain Spin. Avoid hitting heavy targets if your [c/BDBDBD:Inertia] is low.") 
            {
                OverrideColor = Color.White
            };

            tooltips.Add(info);
        }

        // Caso queira que ele faça algo ao usar, como um som de virar página
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