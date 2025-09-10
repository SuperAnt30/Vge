using Mvk2.Entity.Inventory;
using Mvk2.Renderer;
using System.Collections.Generic;
using Vge.Games;
using Vge.Gui.Huds;
using Vge.Item;
using Vge.Renderer;
using WinGL.OpenGL;

namespace Mvk2.Gui
{
    /// <summary>
    /// Индикация для Малювек 
    /// </summary> 
    public class HudMvk : HudDebug // HudBase HubDebug
    {
        private readonly RenderMvk _renderMvk;
        /// <summary>
        /// Сетка фона инвентаря
        /// </summary>
        private readonly MeshGuiColor _meshInventoryBg;
        /// <summary>
        /// Сетка выбранного инвентаря и предметов
        /// </summary>
        private readonly MeshGuiColor _meshInventory;

        /// <summary>
        /// Объект OpenGL для элемента управления
        /// </summary>
        private readonly GL gl;

        public HudMvk(GameBase game, RenderMvk renderMvk) : base(game)
        {
            _renderMvk = renderMvk;
            gl = game.GetOpenGL();
            _meshInventoryBg = new MeshGuiColor(game.GetOpenGL());
            _meshInventory = new MeshGuiColor(game.GetOpenGL());

            if (_game.Player.Inventory is InventoryPlayer inventoryPlayer)
            {
                inventoryPlayer.CurrentItemChanged += InventoryPlayer_CurrentItemChanged;
            }
            _RenderInventoryBg();
            _RenderInventorySelect();
        }

        private void InventoryPlayer_CurrentItemChanged(object sender, System.EventArgs e)
        {
            _RenderInventorySelect();
        }

        /// <summary>
        /// Изменён размер окна
        /// </summary>
        public override void OnResized(int width, int height)
        {
            base.OnResized(width, height);
            _RenderInventoryBg();
            _RenderInventorySelect();
        }

        /// <summary>
        /// Рендер фона инвенторя
        /// </summary>
        protected virtual void _RenderInventoryBg()
        {
            int wc = Gi.Width / 2;
            int hc = Gi.Height / 2;

            List<float> buffer = new List<float>();
            
            int size = 50 * Gi.Si;
            int w = size * -4;
            int h = Gi.Height - size - 8 * Gi.Si;

            for (int i = 0; i < 8; i++)
            {
                int w0 = wc + w + i * size;

                // Фон
                buffer.AddRange(RenderFigure.Rectangle(w0, h, w0 + size, h + size,
                    .29296875f, .90234375f, 0.390625f, 1));

                /*
                if (ClientMain.Player.InventPlayer.CurrentItem == i)
                {
                    // Выбранный
                    //GLRender.Rectangle(w0, h, w0 + size, h + size, 0.1953125f, .46875f, 0.390625f, .6640625f);
                    GLRender.Rectangle(w0 - 2, h - 2, w0 + 52, h + 52, 0.1953125f, .46875f, 0.40625f, .6796875f);
                }
                /*
                // Прорисовка предмета в стаке если есть
                ItemStack itemStack = ClientMain.Player.InventPlayer.GetStackInSlot(i);
                if (itemStack != null)
                {
                    int w1 = w0 + size / 2;
                    int h1 = h + size / 2;
                    if (itemStack.Item.EItem == EnumItem.Block && itemStack.Item is ItemBlock itemBlock)
                    {
                        // Прорисовка блока
                        ClientMain.World.WorldRender.GetBlockGui(itemBlock.Block.EBlock).Render(w1, h1, scale);
                    }
                    else
                    {
                        // Прорисовка предмета
                        ClientMain.World.WorldRender.GetItemGui(itemStack.Item.EItem).Render(w1, h1);
                    }
                    if (itemStack.Amount > 1)
                    {
                        GLWindow.Texture.BindTexture(Assets.ConvertFontToTexture(fontSize));
                        string str = itemStack.Amount.ToString();
                        int ws = FontRenderer.WidthString(str, fontSize);
                        FontRenderer.RenderString(w1 + 20 - ws, h1 + 9, str, fontSize, new vec3(1), 1, true, 0, 1);
                    }
                    if (itemStack.ItemDamage > 0)
                    {
                        int line = itemStack.ItemDamage * 36 / itemStack.Item.MaxDamage;
                        GLRender.Texture2DDisable();
                        GLRender.Rectangle(w0 + 6, h + 40, w0 + 44, h + 44, new vec4(.16f, .16f, .16f, 1));
                        GLRender.Rectangle(w0 + 7, h + 41, w0 + 43 - line, h + 43, new vec4((new vec3(line / 36f, (36 - line) / 36f, 0)).normalize(), 1));
                        GLRender.Texture2DEnable();
                    }
                }
                */
            }
            _meshInventoryBg.Reload(buffer.ToArray());
        }

        /// <summary>
        /// Рендер выбранного инвенторя 
        /// </summary>
        protected virtual void _RenderInventorySelect()
        {
            int wc = Gi.Width / 2;

            int two = 2 * Gi.Si;
            int size = 50 * Gi.Si;
            int w = size * -4;
            int h = Gi.Height - size - 8 * Gi.Si;

            int index = _game.Player.Inventory.GetCurrentIndex();
            int w0 = wc + w + index * size;

            _meshInventory.Reload(RenderFigure.Rectangle(w0 - two, h - two, w0 + size + two, h + size + two,
                0.390625f, .89453125f, .49609375f, 1));

            // Прорисовка предмета
            //ClientMain.World.WorldRender.GetItemGui(itemStack.Item.EItem).Render(w1, h1)
        }

        /// <summary>
        /// Метод для прорисовки кадра
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        public override void Draw(float timeIndex)
        {
            base.Draw(timeIndex);

            _game.Render.DepthOff();
            _renderMvk.BindTextureHud();
            _meshInventoryBg.Draw();
            _meshInventory.Draw();
           
            // Заносим в шейдор
            _game.Render.ShsEntity.BindUniformBiginGui();
            //_game.Render.ShsEntity.UniformDataGui(200, 200);
            //_game.WorldRender.Entities.GetItemRender(3).MeshDraw();
            ////_game.Render.ShsEntity.UniformDataGui(200, 200);
            ////_game.WorldRender.Entities.GetEntityRender(1).MeshDraw();
            //_game.Render.ShsEntity.UniformDataGui(300, 300);

            // Всё два 2 закончено, рисуем 3д элементы в Gui
            _game.Render.DepthOn();


            int wc = Gi.Width / 2;
            int hc = Gi.Height / 2;

            int size = 50 * Gi.Si;
            int w = size * -4;
            int h = Gi.Height - 33 * Gi.Si;

            for (int i = 0; i < 4; i++)
            {
                int w0 = wc + w + i * size;
                int w1 = w0 + size / 2;
               // int h1 = h + 16 * Gi.Si;// - size / 2;

                // Прорисовка предмета в стаке если есть
                _game.Render.ShsEntity.UniformDataGui(w1, h, 
                    _game.WorldRender.Entities.GetItemGuiRender(i).Volume);
                _game.WorldRender.Entities.GetItemGuiRender(i).MeshDraw();

                //if (itemStack.Amount > 1)
                //{
                //    GLWindow.Texture.BindTexture(Assets.ConvertFontToTexture(fontSize));
                //    string str = itemStack.Amount.ToString();
                //    int ws = FontRenderer.WidthString(str, fontSize);
                //    FontRenderer.RenderString(w1 + 20 - ws, h1 + 9, str, fontSize, new vec3(1), 1, true, 0, 1);
                //}
                //if (itemStack.ItemDamage > 0)
                //{
                //    int line = itemStack.ItemDamage * 36 / itemStack.Item.MaxDamage;
                //    GLRender.Texture2DDisable();
                //    GLRender.Rectangle(w0 + 6, h + 40, w0 + 44, h + 44, new vec4(.16f, .16f, .16f, 1));
                //    GLRender.Rectangle(w0 + 7, h + 41, w0 + 43 - line, h + 43, new vec4((new vec3(line / 36f, (36 - line) / 36f, 0)).normalize(), 1));
                //    GLRender.Texture2DEnable();
                //}
            }
            
            //_game.Render.ShaderBindGuiColor();
            //_renderMvk.BindTextureHud();
            //_meshInventoryBg.Draw();
        }

        public override void Dispose()
        {
            base.Dispose();
            _meshInventoryBg.Dispose();
        }
    }
}
