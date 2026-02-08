using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Gearstorm.Content.Projectiles.Beyblades;
using Terraria.ModLoader;

namespace Gearstorm.Content.Items.Parts.Augments
{
    public class ShroomiteAugment : BeybladeAugment
    {
        public override string Texture => "Gearstorm/Assets/Items/Parts/Augment";
        public override Color AugmentColor => new Color(43, 100, 255);
        public static float LastDistanceBonus = 0f;


        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Color mainColor = new Color(43, 100, 255);

            tooltips.Add(new TooltipLine(Mod, "AugHeader", "[c/2B64FF:Fungal Stealth]"));

            float currentBonus = LastDistanceBonus; // pega o valor atualizado
            tooltips.Add(new TooltipLine(Mod, "AugmentDistanceBonus",
                $"Current distance bonus: [c/00FF00:+{currentBonus:F0}%]"
            ));

            tooltips.Add(new TooltipLine(Mod, "AugmentDescription",
                "Deals extra damage based on distance from the Beyblade.\n" +
                "Each 2 blocks increases damage by [c/00FF00:+5%].\n" +
                "Extra damage is half of the Beyblade's base damage.\n" +
                "Numbers will appear in [c/2b64ff:Dark Blue]."
            ));

            tooltips.Add(new TooltipLine(Mod, "AugmentFlavor",
                "[c/AAAAAA:Just a totally unsuspicious Beyblade minding its own business.]"));
        }


        public override void ApplyAugmentEffect(BaseBeybladeProjectile beybladeProj, NPC target, bool wasCrit)
        {
            if (target == null)
                return;

            Player player = Main.player[beybladeProj.Projectile.owner];

            // Distância entre jogador e Beyblade em pixels
            float dist = Vector2.Distance(player.Center, beybladeProj.Projectile.Center);

            // Proporção: +5% cada 2 blocos (32px)
            float damageMultiplier = 1f + (dist / 32f) * 0.05f;

            // Base do dano extra = metade do projétil * multiplicador de distância
            int baseExtraDamage = (int)(beybladeProj.Projectile.damage * 0.5f * damageMultiplier);
            if (baseExtraDamage < 1)
                return;

            // Aplica multiplicador de crítico do projétil
            if (wasCrit)
            {
                baseExtraDamage = (int)(baseExtraDamage * beybladeProj.CritMultiplier);
            }
            LastDistanceBonus = (damageMultiplier - 1f) * 100f; // percentual
            // Cria o HitInfo manualmente (compatível com seu BaseBeybladeProjectile)
            NPC.HitInfo hitInfo = new NPC.HitInfo
            {
                Damage = baseExtraDamage,
                SourceDamage = baseExtraDamage,
                Crit = wasCrit,
                Knockback = 0f,
                HitDirection = Math.Sign(target.Center.X - beybladeProj.Projectile.Center.X),
                DamageType = beybladeProj.Projectile.DamageType,
                InstantKill = false,
                HideCombatText = true
            };

            // Aplica dano
            target.StrikeNPC(hitInfo);

            // Mostra o dano azul escuro
            CombatText.NewText(
                target.Hitbox,
                new Color(43, 100, 255),
                hitInfo.Damage,
                dramatic: false,
                dot: false
            );

            // Feedback visual (opcional)
            if (damageMultiplier > 1.1f)
            {
                for (int i = 0; i < 3; i++)
                {
                    Dust d = Dust.NewDustDirect(
                        target.position,
                        target.width,
                        target.height,
                        DustID.GlowingMushroom,
                        0f, 0f, 100,
                        default,
                        1.2f
                    );
                    d.velocity *= 0.5f;
                    d.noGravity = true;
                }
            }
        }



        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.ShroomiteBar, 10);
            recipe.AddTile(TileID.Autohammer);
            recipe.Register();
        }
    }
}
