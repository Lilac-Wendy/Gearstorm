using System.Collections.Generic;
using Gearstorm.Content.Projectiles.Beyblades;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.Localization;

namespace Gearstorm.Content.Items.Parts;

public abstract class BeybladeAugment : ModItem
{
    public virtual Color AugmentColor => Color.Transparent;

    public virtual string AugmentDescriptionKey
        => "Mods.Gearstorm.Augments.DefaultDescription";

    public virtual string ExtraDescription => "";

    public override void SetDefaults()
    {
        Item.maxStack = 1;
        Item.consumable = false;
        Item.ammo = Item.type;
        Item.rare = ItemRarityID.Blue;
        Item.value = Item.sellPrice(silver: 5);

        Item.color = AugmentColor == Color.Transparent
            ? Color.White
            : AugmentColor;
    }

    // Chamado a cada frame no AI() da Beyblade
    public virtual void UpdateAugment(BaseBeybladeProjectile beybladeProj) { }

    public virtual void OnBeybladeHit(
        Projectile beyblade,
        Vector2 hitNormal,
        float impactStrength,
        Projectile otherBeyblade,
        NPC targetNPC
    )
    {
        if (targetNPC != null)
        {
            if (beyblade.ModProjectile is BaseBeybladeProjectile bb)
            {
                ApplyAugmentEffect(bb, targetNPC);
            }
        }
    }

    public virtual void ApplyAugmentEffect(
        BaseBeybladeProjectile beybladeProj,
        NPC target
    )
    {
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        tooltips.Add(new TooltipLine(Mod, "AugHeader", Language.GetTextValue("Mods.Gearstorm.Items.BeybladeAugments.Header"))
        {
            OverrideColor = AugmentColor == Color.Transparent ? Color.Gold : AugmentColor
        });

        string desc = Language.GetTextValue(AugmentDescriptionKey);
        
        if (!string.IsNullOrEmpty(desc) && desc != AugmentDescriptionKey)
        {
            tooltips.Add(new TooltipLine(Mod, "AugmentDescription", desc));
        }

        if (!string.IsNullOrEmpty(ExtraDescription))
        {
            tooltips.Add(new TooltipLine(Mod, "AugmentExtra", ExtraDescription)
            {
                OverrideColor = Color.LightGray
            });
        }
    }
}