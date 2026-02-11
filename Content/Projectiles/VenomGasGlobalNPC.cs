using Terraria;
using Terraria.ModLoader;

namespace Gearstorm.Content.Projectiles
{
    public class VenomGasGlobalNpc : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public int VenomStacks;
        public int VenomDecayTimer;

        public override void ResetEffects(NPC npc)
        {
            if (VenomDecayTimer > 0)
            {
                VenomDecayTimer--;
            }
            else if (VenomStacks > 0)
            {
                VenomStacks--;
                VenomDecayTimer = 30; // Delay entre perdas de stack
            }
        }

        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            if (VenomStacks <= 0)
                return;

            npc.lifeRegen -= VenomStacks * 12;
            damage = System.Math.Max(damage, VenomStacks);
        }
    }
}