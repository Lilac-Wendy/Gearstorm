using Gearstorm.Content.Projectiles.Beyblades;
using Terraria;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;

namespace Gearstorm.Content.Items.Parts.Augments;

public class ChlorophyteAugment : BeybladeAugment
{
    public override Color AugmentColor => Color.LimeGreen;
    public override string Texture => "Gearstorm/Assets/Items/Parts/Augment";
    public override string ExtraDescription => 
        "[c/32CD32:Chlorophyte Infusion]\n" +
        "Blade impacts have a [c/90EE90:25% chance] to release a toxic [c/32CD32:Spore Cloud]\n" +
        "Releasing spores [c/90EE90:siphons life energy] from the target\n" +
        "Instantly restores [c/32CD32:50% of damage dealt] as health\n" +
        "[c/FF4500:Hint: Life-steal efficiency scales infinitely with the Beyblade Damage]";
    public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
        spriteBatch.Draw(texture, position, null, Color.LimeGreen * 0.8f, 0f, origin, scale, SpriteEffects.None, 0f);
    }

public override void OnBeybladeHit(
    Projectile beyblade,
    Vector2 hitNormal,
    float impactStrength,
    Projectile otherBeyblade,
    NPC targetNpc,
    bool wasCrit
)
{
    // Chamada correta da base (assinatura NOVA)
    base.OnBeybladeHit(
        beyblade,
        hitNormal,
        impactStrength,
        otherBeyblade,
        targetNpc,
        wasCrit
    );

    // Segurança
    if (targetNpc == null)
        return;

    if (beyblade.ModProjectile is not BaseBeybladeProjectile bb)
        return;

    Player player = Main.player[beyblade.owner];

    // ==============================
    // CONDIÇÃO DE ATIVAÇÃO
    // ==============================
    // Este Augment:
    // - Só ativa em críticos
    // - E ainda tem 25% de chance
    if (!wasCrit || !Main.rand.NextBool(4))
        return;

    // ==============================
    // EFEITO OFENSIVO (SPORE CLOUD)
    // ==============================
    int sporeDamage = (int)(beyblade.damage * 0.5f);

    Projectile.NewProjectile(
        beyblade.GetSource_FromThis(),
        targetNpc.Center,
        Vector2.Zero,
        ProjectileID.SporeCloud,
        sporeDamage,
        1f,
        beyblade.owner
    );

    // ==============================
    // ROUBO DE VIDA (LIFESTEAL)
    // ==============================
    int healAmount = (int)(beyblade.damage * 0.50f);

    if (healAmount <= 0)
        return;

    if (player.statLife < player.statLifeMax2)
    {
        player.statLife += healAmount;
        if (player.statLife > player.statLifeMax2)
            player.statLife = player.statLifeMax2;

        player.HealEffect(healAmount);

        // ==============================
        // FEEDBACK VISUAL
        // ==============================
        for (int i = 0; i < 6; i++)
        {
            Dust d = Dust.NewDustDirect(
                player.position,
                player.width,
                player.height,
                DustID.ChlorophyteWeapon,
                Main.rand.NextFloat(-1f, 1f),
                Main.rand.NextFloat(-2.5f, -0.5f),
                150,
                default,
                1.1f
            );
            d.noGravity = true;
        }
    }
}


    public override void AddRecipes()
    {
        Recipe recipe = CreateRecipe();
        recipe.AddIngredient(ItemID.ChlorophyteBar, 8);
        recipe.AddIngredient(ModContent.ItemType<BasicBladeItem>(), 5);
        recipe.AddTile(TileID.MythrilAnvil);
        recipe.Register();
    }
}