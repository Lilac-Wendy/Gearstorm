using Gearstorm.Content.DamageClasses;
using Gearstorm.Content.Data;
using Gearstorm.Content.Items.Parts;
using Gearstorm.Content.Projectiles.Beyblades;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Gearstorm.Content.Items.Beyblades
{
    public class StarterBeyItem : ModItem
    {
        public override string Texture => "Gearstorm/Assets/Items/StarterBey";

        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 44;
            Item.maxStack = 1;
            Item.value = Item.sellPrice(silver: 10);
            Item.rare = ItemRarityID.White;
            Item.shoot = ModContent.ProjectileType<StarterBeyProjectile>();
            Item.shootSpeed = 10f;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useAnimation = 30;
            Item.useTime = 30;
            Item.DamageType = ModContent.GetInstance<Spinner>();
        }
        
        public override bool Shoot(Terraria.Player player, Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            int proj = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);

            if (Main.projectile[proj].ModProjectile is StarterBeyProjectile beyProj)
            {
                var basePart = new BasicBaseItem();
                var bladePart = new BasicBladeItem();
                var topPart = new BasicTopItem();

                var combined = BeybladeCombiner.CombineStats(basePart, bladePart, topPart);
                beyProj.InitializeStats(combined); // garante que defaults recalculam com stats certos
            }

            return false; // não deixa o spawn duplicado
        }
        public BeybladeStats Stats => BeybladeCombiner.CombineStats(
            new BasicBaseItem(),
            new BasicBladeItem(),
            new BasicTopItem()
        );
        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ModContent.ItemType<BasicBaseItem>(), 1);
            recipe.AddIngredient(ModContent.ItemType<BasicBladeItem>(), 1);
            recipe.AddIngredient(ModContent.ItemType<BasicTopItem>(), 1);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }


    }
}