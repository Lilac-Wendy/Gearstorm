using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace Gearstorm.Content.Buffs
{
    public class ShimmerLevitationBuff : ModBuff
    {
        public override string Texture => $"Terraria/Images/Buff_{BuffID.Shimmer}";

        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;      
            Main.buffNoSave[Type] = true;  
            Main.buffNoTimeDisplay[Type] = false; 

            BuffID.Sets.LongerExpertDebuff[Type] = true; 
        }
        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.velocity.Y = -10f; 
            npc.velocity.X *= 0.9f;
            npc.netUpdate = true;
            if (Main.rand.NextBool(2))
            {
                int d = Dust.NewDust(npc.position, npc.width, npc.height, DustID.ShimmerSpark, 0f, -2f);
                Main.dust[d].noGravity = true;
                Main.dust[d].velocity *= 1.5f;
            }
        }
    }
}