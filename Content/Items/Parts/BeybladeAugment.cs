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

    // Key para tradução da descrição
    public virtual string AugmentDescriptionKey => "Mods.Gearstorm.Augments.DefaultDescription";

    // Se precisar exibir algum "valor" extra no tooltip (ex: -3 dano fixo, +600 lifetime)
    public virtual string ExtraDescription => "";

    public override void SetDefaults()
    {
        Item.maxStack = 1;
        Item.consumable = false;
        Item.ammo = Item.type;
        Item.rare = ItemRarityID.Blue;
        Item.value = Item.sellPrice(silver: 5);
    }

    public virtual void ApplyAugmentEffect(BaseBeybladeProjectile beybladeProj, NPC target) { }

    public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
    {
        // Acessando a nova chave "Header" dentro da seção "BeybladeAugments"
        tooltips.Add(new TooltipLine(Mod, "AugHeader", 
            Language.GetTextValue("Mods.Gearstorm.Items.BeybladeAugments.Header"))
        {
            OverrideColor = AugmentColor == Color.Transparent ? Color.Gold : AugmentColor
        });

        string desc = Language.GetTextValue(AugmentDescriptionKey);
        if (!string.IsNullOrEmpty(desc))
        {
            tooltips.Add(new TooltipLine(Mod, "AugmentDescription", desc)
            {
                OverrideColor = Color.White
            });
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