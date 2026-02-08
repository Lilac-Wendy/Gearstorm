using System.IO;
using Gearstorm.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.DataStructures;

using Gearstorm.Content.Data;
using Gearstorm.Content.Projectiles.Beyblades;

namespace Gearstorm.Content.Items.Beyblades
{
    public class BeybladeLauncherItem : ModItem
    {
        // ==================================================
        // CONFIG
        // ==================================================

        public override string Texture => "Gearstorm/Assets/Items/BeybladeLauncher";

        /// <summary>
        /// [0] = Top
        /// [1] = Blade
        /// [2] = Base
        /// </summary>
        public Item[] BeybladeParts = new Item[3];

        // ==================================================
        // DEFAULTS
        // ==================================================

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.maxStack = 1;
            Item.value = Item.buyPrice(gold: 1);
            Item.rare = ItemRarityID.Blue;
            Item.DamageType = ModContent.GetInstance<Spinner>();
            Item.noMelee = true;
            Item.useStyle = ItemUseStyleID.Shoot; // ⚠️ correto para launcher
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.autoReuse = true;
            Item.noUseGraphic = false;
            Item.shoot = ModContent.ProjectileType<GenericBeybladeProjectile>();
            Item.shootSpeed = 10f;

            InitializeParts();
        }

        // ==================================================
        // INIT
        // ==================================================

        private void InitializeParts()
        {
            for (int i = 0; i < 3; i++)
            {
                if (BeybladeParts[i] == null)
                    BeybladeParts[i] = new Item();

                BeybladeParts[i].TurnToAir();
            }
        }

        // ==================================================
        // CLONE (CRÍTICO PRA MP / INVENTORY)
        // ==================================================

        public override ModItem Clone(Item newEntity)
        {
            BeybladeLauncherItem clone = (BeybladeLauncherItem)base.Clone(newEntity);

            clone.BeybladeParts = new Item[3];
            for (int i = 0; i < 3; i++)
            {
                clone.BeybladeParts[i] = BeybladeParts[i]?.Clone() ?? new Item();
                if (clone.BeybladeParts[i].IsAir)
                    clone.BeybladeParts[i].TurnToAir();
            }

            return clone;
        }

        // ==================================================
        // USO
        // ==================================================

        public override bool CanUseItem(Player player)
            => HasAllParts();

        public override bool Shoot(
            Player player,
            EntitySource_ItemUse_WithAmmo source,
            Vector2 position,
            Vector2 velocity,
            int type,
            int damage,
            float knockback
        )
        {
            if (!HasAllParts())
                return false;

            BeybladeStats stats = GetCurrentStats();
            UpdateItemStats(stats);

            int projIndex = Projectile.NewProjectile(
                source,
                position,
                velocity,
                type,
                (int)stats.DamageBase,
                stats.KnockbackPower,
                player.whoAmI
            );

            if (projIndex >= 0 &&
                projIndex < Main.maxProjectiles &&
                Main.projectile[projIndex].ModProjectile is GenericBeybladeProjectile beybladeProj)
            {
                beybladeProj.InitializeWithParts(
                    stats,
                    GetTexturePath(BeybladeParts[2]), // Base
                    GetTexturePath(BeybladeParts[0]), // Top
                    GetTexturePath(BeybladeParts[1])  // Blade
                );
            }

            return false; // evita disparo vanilla duplicado
        }

        // ==================================================
        // STATUS
        // ==================================================

        public BeybladeStats GetCurrentStats()
        {
            if (!HasAllParts())
                return new BeybladeStats();

            if (BeybladeParts[0].ModItem is not BeybladeStats.IHasBeybladeStats top) return new BeybladeStats();
            if (BeybladeParts[1].ModItem is not BeybladeStats.IHasBeybladeStats blade) return new BeybladeStats();
            if (BeybladeParts[2].ModItem is not BeybladeStats.IHasBeybladeStats @base) return new BeybladeStats();

            return BeybladeStats.CombineStats(top, blade, @base);
        }

        private void UpdateItemStats(BeybladeStats stats)
        {
            Item.damage = (int)stats.DamageBase;
            Item.knockBack = stats.KnockbackPower;
        }

        // ==================================================
        // VALIDAÇÃO
        // ==================================================

        private bool HasAllParts()
        {
            return BeybladeParts != null && BeybladeParts.Length == 3
                                         && !BeybladeParts[0].IsAir && BeybladeParts[0].ModItem is BeybladeStats.IHasBeybladeStats
                                         && !BeybladeParts[1].IsAir && BeybladeParts[1].ModItem is BeybladeStats.IHasBeybladeStats
                                         && !BeybladeParts[2].IsAir && BeybladeParts[2].ModItem is BeybladeStats.IHasBeybladeStats;
        }

        private string GetTexturePath(Item part)
        {
            return part?.ModItem?.Texture ?? "Terraria/Images/Item_0";
        }

        // ==================================================
        // SAVE / LOAD
        // ==================================================

        public override void SaveData(TagCompound tag)
        {
            for (int i = 0; i < 3; i++)
            {
                if (!BeybladeParts[i].IsAir)
                    tag[$"part{i}"] = BeybladeParts[i];
            }
        }

        public override void LoadData(TagCompound tag)
        {
            InitializeParts();

            for (int i = 0; i < 3; i++)
            {
                if (tag.ContainsKey($"part{i}"))
                    BeybladeParts[i] = tag.Get<Item>($"part{i}");
            }
            var global = Item.GetGlobalItem<BeybladeLauncherGlobalItem>();
            global.RecalculateStats(this);
        }

        // ==================================================
        // MULTIPLAYER
        // ==================================================

        public override void NetSend(BinaryWriter writer)
        {
            for (int i = 0; i < 3; i++)
                ItemIO.Send(BeybladeParts[i], writer);
        }

        public override void NetReceive(BinaryReader reader)
        {
            InitializeParts();

            for (int i = 0; i < 3; i++)
                BeybladeParts[i] = ItemIO.Receive(reader);
            var global = Item.GetGlobalItem<BeybladeLauncherGlobalItem>();
            global.RecalculateStats(this);

        }

        // ==================================================
        // RECIPE
        // ==================================================

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.IronBar, 10)
                .AddIngredient(ItemID.Wood, 20)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}
