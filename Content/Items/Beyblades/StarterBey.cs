using Gearstorm.Content.DamageClasses;
using Gearstorm.Content.Data;
using Gearstorm.Content.Items.Parts;
using Gearstorm.Content.Projectiles.Beyblades;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Gearstorm.Content.Items.Beyblades
{
    public class StarterBeyItem : ModItem
    {
        public override string Texture => "Gearstorm/Assets/Items/StarterBey";

        // Variáveis para controlar a animação de uso
        private float rotationDuringUse = 0f;
        private float scaleDuringUse = 0f;

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
            Item.noUseGraphic = false; // IMPORTANTE: Permitir que o item seja desenhado
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useAnimation = 30;
            Item.useTime = 30;
            Item.DamageType = ModContent.GetInstance<Spinner>();
        }

        public override void UseStyle(Terraria.Player player, Rectangle heldItemFrame)
        {
            // Controla a animação durante o uso
            if (player.itemAnimation > 0)
            {
                float progress = 1f - (float)player.itemAnimation / player.itemAnimationMax;
                
                // Giro rápido no início
                rotationDuringUse = progress * MathHelper.TwoPi * 3f;
                
                // Escala reduzida pela metade com ease-out
                scaleDuringUse = MathHelper.Lerp(0f, 0.5f, progress * progress);
            }
            else
            {
                rotationDuringUse = 0f;
                scaleDuringUse = 0f;
            }
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            // Só desenha o item no mundo se não estiver sendo usado
            return Main.LocalPlayer.itemAnimation <= 0;
        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            // Desenho normal no inventário
            return true;
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
                beyProj.InitializeStats(combined);
            }

            return false;
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