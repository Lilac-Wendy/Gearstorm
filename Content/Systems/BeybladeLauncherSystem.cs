using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Gearstorm.Content.Data;
using Gearstorm.Content.Items.Beyblades;
using Gearstorm.Content.Items.Parts;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.GameContent;

namespace Gearstorm.Content.Systems
{
    public class BeybladeLauncherSystem : ModSystem
    {
        private const int SLOT_SIZE = 52;
        private const int SLOT_SPACING = 58;
        
        private bool mouseWasPressed;

        // Centraliza a posição para evitar que o clique e o desenho fiquem desalinhados
        private Vector2 GetUIBasePosition()
        {
            return new Vector2(
                Main.screenWidth - 260, 
                Main.screenHeight - 450 
            );
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int inventoryIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));
            if (inventoryIndex != -1)
            {
                layers.Insert(inventoryIndex + 1, new LegacyGameInterfaceLayer(
                    "Gearstorm: Beyblade Launcher Slots",
                    delegate
                    {
                        if (Main.playerInventory) // Só desenha se o inventário estiver aberto
                        {
                            DrawBeybladeLauncherSlots();
                        }
                        return true;
                    },
                    InterfaceScaleType.UI)
                );
            }
        }

        private void DrawBeybladeLauncherSlots()
        {
            Player player = Main.LocalPlayer;
            if (player.HeldItem?.ModItem is not BeybladeLauncherItem launcher)
                return;

            DrawSlotsUI(launcher);
        }

        private void DrawSlotsUI(BeybladeLauncherItem launcher)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Vector2 basePosition = GetUIBasePosition();
            
            Texture2D slotTexture = TextureAssets.InventoryBack.Value;
            Texture2D slotTextureHighlight = TextureAssets.InventoryBack2.Value;

            for (int i = 0; i < 3; i++)
            {
                Vector2 slotPos = basePosition + new Vector2(0, i * SLOT_SPACING);
                Rectangle slotRect = new Rectangle((int)slotPos.X, (int)slotPos.Y, SLOT_SIZE, SLOT_SIZE);

                bool isHovered = slotRect.Contains(Main.MouseScreen.ToPoint());

                // Desenha o fundo do slot
                spriteBatch.Draw(isHovered ? slotTextureHighlight : slotTexture, slotRect, Color.White);

                Item item = launcher.BeybladeParts[i];
                if (!item.IsAir)
                {
                    Texture2D itemTexture = TextureAssets.Item[item.type].Value;
                    
                    // Lógica de Frames para evitar mostrar o sprite sheet inteiro
                    int frameCount = Main.itemAnimations[item.type] != null 
                                     ? Main.itemAnimations[item.type].FrameCount 
                                     : 1;
                    
                    int frameHeight = itemTexture.Height / frameCount;
                    Rectangle sourceRect = new Rectangle(0, 0, itemTexture.Width, frameHeight);
                    
                    float scale = Math.Min(SLOT_SIZE / (float)sourceRect.Width, SLOT_SIZE / (float)sourceRect.Height) * 0.85f;

                    spriteBatch.Draw(
                        itemTexture,
                        slotPos + new Vector2(SLOT_SIZE / 2, SLOT_SIZE / 2),
                        sourceRect, 
                        Color.White, 
                        0f, 
                        sourceRect.Size() / 2, 
                        scale, 
                        SpriteEffects.None, 
                        0f
                    );
                }

                if (isHovered)
                {
                    DrawHighlightBorder(spriteBatch, slotRect);
                    
                    // Reatribui o item ao mouse hover para mostrar Tooltip original do Terraria
                    Main.hoverItemName = item.Name;
                    Main.HoverItem = item.Clone();
                }
            }
        }
        
        private void DrawHighlightBorder(SpriteBatch spriteBatch, Rectangle rect)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            Color borderColor = Color.Gold;
            
            spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, rect.Width, 2), borderColor);
            spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y + rect.Height - 2, rect.Width, 2), borderColor);
            spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, 2, rect.Height), borderColor);
            spriteBatch.Draw(pixel, new Rectangle(rect.X + rect.Width - 2, rect.Y, 2, rect.Height), borderColor);
        }

        public override void UpdateUI(GameTime gameTime)
        {
            // Só processa cliques se o inventário estiver aberto
            if (!Main.playerInventory) return;

            Player player = Main.LocalPlayer;
            if (player.HeldItem?.ModItem is not BeybladeLauncherItem launcher)
                return;
                
            ProcessSlotClicks(launcher);
        }
        
        private void ProcessSlotClicks(BeybladeLauncherItem launcher)
        {
            bool mouseIsPressed = Main.mouseLeft;
            
            if (mouseWasPressed && !mouseIsPressed)
            {
                ProcessSlotClickRelease(launcher);
            }
            
            mouseWasPressed = mouseIsPressed;
        }
        
        private void ProcessSlotClickRelease(BeybladeLauncherItem launcher)
        {
            Vector2 mousePos = Main.MouseScreen;
            Vector2 basePosition = GetUIBasePosition(); // Agora bate com a posição do desenho
            
            for (int i = 0; i < 3; i++)
            {
                Vector2 slotPos = basePosition + new Vector2(0, i * SLOT_SPACING);
                Rectangle slotRect = new Rectangle((int)slotPos.X, (int)slotPos.Y, SLOT_SIZE, SLOT_SIZE);
                
                if (slotRect.Contains(mousePos.ToPoint()))
                {
                    ProcessSlotInteraction(launcher, i);
                    Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.MenuTick);
                    break;
                }
            }
        }
        
        private void ProcessSlotInteraction(BeybladeLauncherItem launcher, int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= launcher.BeybladeParts.Length)
                return;
            
            Item slotItem = launcher.BeybladeParts[slotIndex];
            Item mouseItem = Main.mouseItem;
            
            // Se o mouse estiver vazio e o slot tiver algo: Tira o item
            if (mouseItem.IsAir && !slotItem.IsAir)
            {
                Main.mouseItem = slotItem.Clone();
                launcher.BeybladeParts[slotIndex] = new Item();
                launcher.BeybladeParts[slotIndex].TurnToAir();
            }
            // Se o mouse tiver algo: Tenta colocar ou trocar
            else if (!mouseItem.IsAir)
            {
                if (IsValidPartForSlot(mouseItem, slotIndex))
                {
                    Item temp = slotItem.Clone();
                    launcher.BeybladeParts[slotIndex] = mouseItem.Clone();
                    Main.mouseItem = temp; // Se for ar, o mouse fica vazio; se tiver algo, troca.
                }
                else
                {
                    Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.MenuClose);
                }
            }
        }
        
        private bool IsValidPartForSlot(Item item, int slotIndex)
        {
            if (item.ModItem is not IHasBeybladeStats partStats) 
                return false;
    
            return slotIndex switch
            {
                0 => partStats.PartType == BeybladePartType.Top,
                1 => partStats.PartType == BeybladePartType.Blade,
                2 => partStats.PartType == BeybladePartType.Base,
                _ => false
            };
        }
    }
}