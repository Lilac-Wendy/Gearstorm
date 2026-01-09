using Gearstorm.Content.DamageClasses;
using Gearstorm.Content.Players;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Gearstorm.Content.Items.AirTreks
{
    public class AirTrek : ModItem
    {
        public override string Texture => "Gearstorm/Assets/Items/AirTreks/AirTrekItem";



        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.accessory = true;
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(silver: 50);
        }

        public override bool CanEquipAccessory(Terraria.Player player, int slot, bool modded)
        {
            return slot < 10; // Permite equipar em qualquer slot de acessório
        }

        public override void UpdateAccessory(Terraria.Player player, bool hideVisual)
        {
            if (!player.TryGetModPlayer(out AirTrekPlayer atp))
                return;

            // Ativa as funcionalidades básicas do item
            atp.AirTrekActive = true;
            
            // Aplica bônus de movimento básico
            if (atp.AirTrekActive)
            {
                player.moveSpeed += 0.15f;
                player.maxRunSpeed += 1.5f;
                player.accRunSpeed += 1.0f;
            }
        }

        // Método para alternar o modo de patinação
        public override void UpdateInventory(Terraria.Player player)
        {
            if (!player.TryGetModPlayer(out AirTrekPlayer atp))
                return;

            // Alterna ativação com botão direito quando o item está na mão
            if (player.altFunctionUse == 2 && player.HeldItem.type == Item.type)
            {
                atp.AirTrekActive = !atp.AirTrekActive;
                Main.NewText($"Air Trek {(atp.AirTrekActive ? "Ativado" : "Desativado")}", 
                    atp.AirTrekActive ? Color.Cyan : Color.Gray);
            }
        }

        public override bool AltFunctionUse(Terraria.Player player)
        {
            return true;
        }

        // Adiciona pontos de combo quando atinge NPCs
        public override void OnHitNPC(Terraria.Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!player.TryGetModPlayer(out AirTrekPlayer atp) || !atp.AirTrekActive)
                return;

            // Adiciona mais pontos se estiver em movimento rápido
            float speedBonus = System.Math.Min(atp.Momentum / 100f, 2f);
            int points = 10 + (int)(10 * speedBonus);
            
            atp.AddCombo(points);
            
            // Efeito visual
            if (Main.rand.NextBool(3))
            {
                Dust.NewDustPerfect(
                    target.Center,
                    DustID.GemSapphire,
                    new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-3f, -1f)),
                    100, default, 1.2f
                );
            }
        }

        // Adiciona dano baseado no combo atual
        public override void ModifyWeaponDamage(Terraria.Player player, ref StatModifier damage)
        {
            if (!player.TryGetModPlayer(out AirTrekPlayer atp) || !atp.AirTrekActive)
                return;

            // Danos adicionais baseados no combo
            // 5% por ponto de combo, máximo de 100% (20 pontos)
            float comboBonus = System.Math.Min(atp.ComboPoints * 0.05f, 1.0f);
            damage += comboBonus;
            
            // Bônus adicional baseado no momentum
            float momentumBonus = System.Math.Min(atp.Momentum / 600f, 0.5f);
            damage += momentumBonus;
        }

        // Tooltip dinâmico mostrando status atual
        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            if (Main.LocalPlayer.TryGetModPlayer(out AirTrekPlayer atp))
            {
                tooltips.Add(new TooltipLine(Mod, "Status", 
                    $"Ativo: {(atp.AirTrekActive ? "[c/00FFFF:✓]" : "[c/888888:✗]")}"));
                tooltips.Add(new TooltipLine(Mod, "Combo", 
                    $"Combo: [c/FFFF00:{atp.ComboPoints}] pontos"));
                tooltips.Add(new TooltipLine(Mod, "Momentum", 
                    $"Momentum: [c/00FF00:{atp.Momentum:F0}]"));
                tooltips.Add(new TooltipLine(Mod, "Instructions", 
                    "[c/AAAAAA:Botão direito para alternar modo]"));
            }
        }
    }
}