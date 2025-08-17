using System;
using Gearstorm.Content.DamageClasses;
using Gearstorm.Content.Projectiles.Beyblades;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

// Bazinga //
namespace Gearstorm.Content.Items.Beyblades
{
    public class StarterBey : ModItem
    {
        public override string Texture => "Gearstorm/Assets/Items/StarterBey";

        public override void SetDefaults()
        {
            var s = StarterBeyProjectile.DefaultStats;
            float mass = s.Mass;
            float damageBase = s.DamageBase;
            float balance = s.Balance;
            float knockbackPower = s.KnockbackPower;

            float minimumDamage = damageBase * 1.0f;
            float calculatedDamage = damageBase * (1f + mass * 0.1f) * balance;

            Item.damage = (int)Math.Max(minimumDamage, calculatedDamage);
            
            Item.DamageType = ModContent.GetInstance<Spinner>();
            Item.width = 16;
            Item.height = 16;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = knockbackPower;
            Item.value = Item.buyPrice(silver: 5);
            Item.rare = ItemRarityID.White;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<StarterBeyProjectile>();
            Item.shootSpeed = 10f;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddRecipeGroup(RecipeGroupID.IronBar, 5);
            recipe.AddIngredient(ItemID.LeadBar, 3);
            recipe.AddIngredient(ItemID.Wood, 10);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
    }
}