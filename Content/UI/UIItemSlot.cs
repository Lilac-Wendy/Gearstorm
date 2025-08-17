using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.UI;
using Terraria.ModLoader;

namespace Gearstorm.Content.UI
{
    public class UIItemSlot : UIElement
    {
        public Item StoredItem = new Item();
        public System.Func<Item, bool> ValidItemFunc;

        public UIItemSlot()
        {
            StoredItem.SetDefaults(0);
            Width.Set(44f, 0f);
            Height.Set(44f, 0f);
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            base.DrawSelf(spriteBatch);

            CalculatedStyle dimensions = GetDimensions();
            Vector2 pos = new Vector2(dimensions.X, dimensions.Y);
            
            // Desenha o fundo do slot
            Texture2D slotTex = TextureAssets.InventoryBack.Value;
            spriteBatch.Draw(slotTex, pos, Color.White);

            if (!StoredItem.IsAir)
            {
                // Desenha o item
                Texture2D tex = TextureAssets.Item[StoredItem.type].Value;
                Rectangle dest = new Rectangle((int)pos.X + 4, (int)pos.Y + 4, 36, 36);
                spriteBatch.Draw(tex, dest, null, Color.White);
                
                // Se quiser mostrar a quantidade
                if (StoredItem.stack > 1)
                {
                    Utils.DrawBorderStringFourWay(spriteBatch, FontAssets.ItemStack.Value, 
                        StoredItem.stack.ToString(), pos.X + 26f, pos.Y + 26f, 
                        Color.White, Color.Black, new Vector2(0.4f));
                }
            }
        }

        public override void LeftClick(UIMouseEvent evt)
        {
            base.LeftClick(evt);
            
            Item mouseItem = Main.mouseItem;
            if (mouseItem.IsAir && !StoredItem.IsAir)
            {
                // pega do slot para o mouse
                Main.mouseItem = StoredItem.Clone();
                StoredItem.TurnToAir();
                SoundEngine.PlaySound(SoundID.MenuTick);
            }
            else if (!mouseItem.IsAir)
            {
                // verifica validade do item
                if (ValidItemFunc == null || ValidItemFunc(mouseItem))
                {
                    Item temp = StoredItem.Clone();
                    StoredItem = mouseItem.Clone();
                    Main.mouseItem = temp;
                    SoundEngine.PlaySound(SoundID.MenuTick);
                }
            }
        }

        public override void RightClick(UIMouseEvent evt)
        {
            base.RightClick(evt);
            
            // Implemente comportamento para clique direito se necessário
            // Por exemplo, pegar metade da pilha, etc.
        }
    }
}