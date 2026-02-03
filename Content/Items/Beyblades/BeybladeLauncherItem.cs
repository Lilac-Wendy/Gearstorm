using Gearstorm.Content.DamageClasses;
using Gearstorm.Content.Data;
using Gearstorm.Content.Items.Parts;
using Gearstorm.Content.Projectiles.Beyblades;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;
using System.IO;
using Terraria.DataStructures;
using Terraria.ModLoader.IO;

namespace Gearstorm.Content.Items.Beyblades
{
    public class BeybladeLauncherItem : ModItem
    {
        public override string Texture => "Gearstorm/Assets/Items/BeybladeLauncher";

        public Item[] BeybladeParts = new Item[3];
        public const int SLOT_SIZE = 52;

        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 44;
            Item.maxStack = 1;
            Item.value = Item.sellPrice(silver: 10);
            Item.rare = ItemRarityID.White;
            Item.shoot = ModContent.ProjectileType<GenericBeybladeProjectile>();
            Item.shootSpeed = 10f;
            Item.noMelee = true;
            Item.noUseGraphic = false;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useAnimation = 30;
            Item.useTime = 30;
            Item.damage = 0; 
            Item.DamageType = ModContent.GetInstance<Spinner>();
            Item.knockBack = 0f; 
            
            for (int i = 0; i < BeybladeParts.Length; i++)
            {
                BeybladeParts[i] = new Item();
                BeybladeParts[i].TurnToAir();
            }
        }

        public override bool CanRightClick()
        {
            return false;
        }

        public override bool ConsumeItem(Player player)
        {
            return false;
        }

        public override bool CanUseItem(Player player)
        {
            if (!HasAllParts())
            {
                Main.NewText("Monte seu beyblade primeiro! Coloque as partes nos slots.", Color.Red);
                return false;
            }
            return base.CanUseItem(player);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (!HasAllParts())
            {
                Main.NewText("Monte um beyblade completo primeiro!", Color.Red);
                return false;
            }

            // Combina os stats
            BeybladeStats combinedStats = CombineBeybladeParts();
            
            // Atualiza item antes de lançar (para consistência)
            UpdateItemStatsFromParts(combinedStats);
            
            Vector2 shootVelocity = velocity;
            if (shootVelocity.Length() < 0.1f)
            {
                Vector2 direction = Main.MouseWorld - player.Center;
                direction.Normalize();
                shootVelocity = direction * Item.shootSpeed;
            }
            
            int projType = ModContent.ProjectileType<GenericBeybladeProjectile>();
            
            // Lança o projétil passando o dano calculado nos stats
            int finalDamage = (int)combinedStats.DamageBase; // Poderia aplicar modificadores do player aqui
            
            int projIndex = Projectile.NewProjectile(source, position, shootVelocity, projType, 
                finalDamage, combinedStats.KnockbackPower, player.whoAmI);
            
            if (Main.projectile[projIndex].ModProjectile is GenericBeybladeProjectile beybladeProj)
            {
                string topPath = GetTexturePath(BeybladeParts[2]);
                string basePath = GetTexturePath(BeybladeParts[0]);
                string bladePath = GetTexturePath(BeybladeParts[1]);
                
                // Passa os stats para o projétil
                beybladeProj.InitializeWithParts(combinedStats, topPath, basePath, bladePath);
            }
            
            return false;
        }

        private string GetTexturePath(Item part)
        {
            if (part.ModItem != null)
            {
                // Prioriza pegar a textura diretamente do ModItem se possível
                var textureAttr = part.ModItem.Texture;
                if (!string.IsNullOrEmpty(textureAttr))
                    return textureAttr;
            }
    
            // Fallback baseados em interfaces ou tipos
            if (part.ModItem is IHasBeybladeStats) return part.ModItem.Texture;
    
            return "Terraria/Images/Item_0";
        }

        private BeybladeStats CombineBeybladeParts()
        {
            // 🔥 CORREÇÃO: Utiliza o BeybladeCombiner.CombineStats com a interface genérica
            var basePart = BeybladeParts[0].ModItem as IHasBeybladeStats;
            var bladePart = BeybladeParts[1].ModItem as IHasBeybladeStats;
            var topPart = BeybladeParts[2].ModItem as IHasBeybladeStats;

            if (basePart != null && bladePart != null && topPart != null)
            {
                return BeybladeCombiner.CombineStats(basePart, bladePart, topPart);
            }
            
            // Fallback se algo der errado (não deveria acontecer devido ao CanUseItem)
            return new BeybladeStats(10f, 5f, 1f, 0.5f, 1f, 0.02f);
        }

        private void UpdateItemStatsFromParts(BeybladeStats stats)
        {
            Item.damage = (int)stats.DamageBase;
            Item.knockBack = stats.KnockbackPower;
        }

        private bool HasAllParts()
        {
            return !BeybladeParts[0].IsAir && !BeybladeParts[1].IsAir && !BeybladeParts[2].IsAir;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Instructions", 
                "Segure este item para ver os slots de partes do beyblade.") 
                { OverrideColor = Color.Gray });
            
            tooltips.Add(new TooltipLine(Mod, "BaseSlot", 
                $"Base: {(BeybladeParts[0].IsAir ? "Vazio" : BeybladeParts[0].Name)}") 
                { OverrideColor = BeybladeParts[0].IsAir ? Color.Red : Color.LightBlue });
            
            tooltips.Add(new TooltipLine(Mod, "BladeSlot", 
                $"Lâmina: {(BeybladeParts[1].IsAir ? "Vazio" : BeybladeParts[1].Name)}") 
                { OverrideColor = BeybladeParts[1].IsAir ? Color.Red : Color.LightGreen });
            
            tooltips.Add(new TooltipLine(Mod, "TopSlot", 
                $"Topo: {(BeybladeParts[2].IsAir ? "Vazio" : BeybladeParts[2].Name)}") 
                { OverrideColor = BeybladeParts[2].IsAir ? Color.Red : Color.LightGoldenrodYellow });
                
            bool ready = HasAllParts();
            tooltips.Add(new TooltipLine(Mod, "ReadyStatus", 
                $"Status: {(ready ? "PRONTO PARA LANÇAR" : "INCOMPLETO")}") 
                { OverrideColor = ready ? Color.Green : Color.Red });
        }

        public override void SaveData(TagCompound tag)
        {
            for (int i = 0; i < BeybladeParts.Length; i++)
            {
                if (!BeybladeParts[i].IsAir)
                {
                    tag[$"beybladePart{i}Type"] = BeybladeParts[i].type;
                    tag[$"beybladePart{i}Stack"] = BeybladeParts[i].stack;
                    tag[$"beybladePart{i}Prefix"] = (byte)BeybladeParts[i].prefix;
                }
            }
        }

        public override void LoadData(TagCompound tag)
        {
            for (int i = 0; i < BeybladeParts.Length; i++)
            {
                if (tag.ContainsKey($"beybladePart{i}Type"))
                {
                    int type = tag.GetInt($"beybladePart{i}Type");
                    int stack = tag.GetInt($"beybladePart{i}Stack");
                    byte prefix = tag.ContainsKey($"beybladePart{i}Prefix") ? 
                        tag.GetByte($"beybladePart{i}Prefix") : (byte)0;
                    
                    BeybladeParts[i] = new Item();
                    BeybladeParts[i].SetDefaults(type);
                    BeybladeParts[i].stack = stack;
                    BeybladeParts[i].Prefix(prefix);
                }
                else
                {
                    BeybladeParts[i] = new Item();
                    BeybladeParts[i].TurnToAir();
                }
            }
        }

        public override void NetSend(BinaryWriter writer)
        {
            for (int i = 0; i < BeybladeParts.Length; i++)
            {
                writer.Write(!BeybladeParts[i].IsAir);
                if (!BeybladeParts[i].IsAir)
                {
                    writer.Write(BeybladeParts[i].type);
                    writer.Write(BeybladeParts[i].stack);
                    writer.Write((byte)BeybladeParts[i].prefix);
                }
            }
        }

        public override void NetReceive(BinaryReader reader)
        {
            for (int i = 0; i < BeybladeParts.Length; i++)
            {
                bool hasItem = reader.ReadBoolean();
                if (hasItem)
                {
                    int type = reader.ReadInt32();
                    int stack = reader.ReadInt32();
                    byte prefix = reader.ReadByte();
                    
                    BeybladeParts[i] = new Item();
                    BeybladeParts[i].SetDefaults(type);
                    BeybladeParts[i].stack = stack;
                    BeybladeParts[i].Prefix(prefix);
                }
                else
                {
                    BeybladeParts[i].TurnToAir();
                }
            }
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.IronBar, 10);
            recipe.AddIngredient(ItemID.Wood, 20);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
    }
}