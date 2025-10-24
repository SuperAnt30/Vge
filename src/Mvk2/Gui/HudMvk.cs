using Mvk2.Entity.Inventory;
using Mvk2.Renderer;
using System;
using System.Collections.Generic;
using Vge.Entity.Inventory;
using Vge.Games;
using Vge.Gui.Huds;
using Vge.Item;
using Vge.Realms;
using Vge.Renderer;
using Vge.Renderer.Font;
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
        /// Сетка текста
        /// </summary>
        private readonly MeshGuiColor _meshTxt;
        /// <summary>
        /// Объект шрифта
        /// </summary>
        public readonly FontBase _font;

        /// <summary>
        /// Объект OpenGL для элемента управления
        /// </summary>
        private readonly GL gl;

        /// <summary>
        /// Имеется ли подписи
        /// </summary>
        private bool _isText;

        public HudMvk(GameBase game, RenderMvk renderMvk) : base(game)
        {
            _renderMvk = renderMvk;
            gl = game.GetOpenGL();
            _meshInventoryBg = new MeshGuiColor(gl);
            _meshInventory = new MeshGuiColor(gl);
            _meshTxt = new MeshGuiColor(gl);

            _font = _renderMvk.FontMain;

            if (_game.Player.Inventory is InventoryPlayerMvk inventoryPlayer)
            {
                // Смена ячейки правой руки
                inventoryPlayer.LimitPocketChanged += InventoryPlayer_LimitPocketChanged;
                inventoryPlayer.SlotSetted += InventoryPlayer_SlotSetted;
                inventoryPlayer.OutsideChanged += InventoryPlayer_OutsideChanged;
            }
            _RenderInventoryBg();
        }

        private void InventoryPlayer_OutsideChanged(object sender, EventArgs e)
        {
            _RenderInventory();
        }

        private void InventoryPlayer_SlotSetted(object sender, SlotEventArgs e)
        { 
            _RenderInventory();
        }

        private void InventoryPlayer_LimitPocketChanged(object sender, EventArgs e)
        {
            // Тут смена БГ где будет смена фона
            _RenderInventoryBg();
        }

        /// <summary>
        /// Изменён размер окна
        /// </summary>
        public override void OnResized(int width, int height)
        {
            base.OnResized(width, height);
            _RenderInventoryBg();
            _RenderInventory();
        }

        /// <summary>
        /// Рендер фона инвенторя
        /// </summary>
        protected virtual void _RenderInventoryBg()
        {
            int wc = Gi.Width / 2;
            int w = 222 * Gi.Si;
            int h = 50 * Gi.Si;
            int x = wc - w;
            int y = Gi.Height - h - 8 * Gi.Si;
            _meshInventoryBg.Reload(RenderFigure.Rectangle(x, y, wc + w, y + h, 
                0, 0, .8671875f, .09765625f));

            x += Gi.Si;
            _meshInventory.Reload(RenderFigure.Rectangle(x, y, x + 46 * Gi.Si, y + 47 * Gi.Si,
                .91015625f, 0, 1, .091796875f));

            return;

            //List<float> buffer = new List<float>();
            
            //int size = 50 * Gi.Si;
            //int w = size * -4;
            //int h = Gi.Height - size - 8 * Gi.Si;

            //for (int i = 0; i < 12; i++)
            //{
            //    int w0 = wc + w + i * size;

            //    // Фон
            //    buffer.AddRange(RenderFigure.Rectangle(w0, h, w0 + size, h + size,
            //        .29296875f, .90234375f, 0.390625f, 1));

            //    /*
            //    if (ClientMain.Player.InventPlayer.CurrentItem == i)
            //    {
            //        // Выбранный
            //        //GLRender.Rectangle(w0, h, w0 + size, h + size, 0.1953125f, .46875f, 0.390625f, .6640625f);
            //        GLRender.Rectangle(w0 - 2, h - 2, w0 + 52, h + 52, 0.1953125f, .46875f, 0.40625f, .6796875f);
            //    }
            //    /*
            //    // Прорисовка предмета в стаке если есть
            //    ItemStack itemStack = ClientMain.Player.InventPlayer.GetStackInSlot(i);
            //    if (itemStack != null)
            //    {
            //        int w1 = w0 + size / 2;
            //        int h1 = h + size / 2;
            //        if (itemStack.Item.EItem == EnumItem.Block && itemStack.Item is ItemBlock itemBlock)
            //        {
            //            // Прорисовка блока
            //            ClientMain.World.WorldRender.GetBlockGui(itemBlock.Block.EBlock).Render(w1, h1, scale);
            //        }
            //        else
            //        {
            //            // Прорисовка предмета
            //            ClientMain.World.WorldRender.GetItemGui(itemStack.Item.EItem).Render(w1, h1);
            //        }
            //        if (itemStack.Amount > 1)
            //        {
            //            GLWindow.Texture.BindTexture(Assets.ConvertFontToTexture(fontSize));
            //            string str = itemStack.Amount.ToString();
            //            int ws = FontRenderer.WidthString(str, fontSize);
            //            FontRenderer.RenderString(w1 + 20 - ws, h1 + 9, str, fontSize, new vec3(1), 1, true, 0, 1);
            //        }
            //        if (itemStack.ItemDamage > 0)
            //        {
            //            int line = itemStack.ItemDamage * 36 / itemStack.Item.MaxDamage;
            //            GLRender.Texture2DDisable();
            //            GLRender.Rectangle(w0 + 6, h + 40, w0 + 44, h + 44, new vec4(.16f, .16f, .16f, 1));
            //            GLRender.Rectangle(w0 + 7, h + 41, w0 + 43 - line, h + 43, new vec4((new vec3(line / 36f, (36 - line) / 36f, 0)).normalize(), 1));
            //            GLRender.Texture2DEnable();
            //        }
            //    }
            //    */
            //}
            //_meshInventoryBg.Reload(buffer.ToArray());
        }

        /// <summary>
        /// Рендер выбранного инвенторя 
        /// </summary>
        protected virtual void _RenderInventory()
        {
            int wc = Gi.Width / 2;
            int size = 36 * Gi.Si;
            int x = wc - 222 * Gi.Si;
            int y = Gi.Height - 58 * Gi.Si;
            // Чистим буфер
            _font.Clear(true, true);
            _font.SetColorShadow(Gi.ColorTextBlack);
            // Указываем опции
            _font.SetFontFX(EnumFontFX.Outline);

            x += 9 * Gi.Si;
            y += 28 * Gi.Si;
            _isText = false;
            for (int i = 0; i < 12; i++)
            {
                ItemStack itemStack = _game.Player.Inventory.GetStackInSlot(i);
                if (itemStack != null)
                {
                    if (itemStack != null && itemStack.Amount != 1)
                    {
                        // Готовим рендер текста
                        _font.RenderString(x + i * size, y, ChatStyle.Bolb + itemStack.Amount.ToString());
                        _isText = true;
                    }
                }
            }

            if (_isText)
            {
                // Имеется Outline значит рендерим FX
                _font.RenderFX();
                // Вносим сетку
                _font.Reload(_meshTxt);
            }
        }

        /// <summary>
        /// Метод для прорисовки кадра
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        public override void Draw(float timeIndex)
        {
            base.Draw(timeIndex);

            _game.Render.DepthOff();
            // Фон
            _renderMvk.ShaderBindGuiColor();
            _renderMvk.BindTextureHud();
            _meshInventoryBg.Draw();

            // Заносим в шейдор
            _game.Render.ShsEntity.BindUniformBeginGui();
            // Всё 2d закончено, рисуем 3д элементы в Gui
            _game.Render.DepthOn();

            // Предметы
            int size = 36 * Gi.Si;
            int x = Gi.Width / 2 - 222 * Gi.Si + 24 * Gi.Si;
            int y = Gi.Height - 58 * Gi.Si + 23 * Gi.Si;
            for (int i = 0; i < 12; i++)
            {
                // Прорисовка предмета в стаке если есть
                ItemStack itemStack = _game.Player.Inventory.GetStackInSlot(i);
                if (itemStack != null)
                {
                    _game.WorldRender.Entities.GetItemGuiRender(itemStack.Item.IndexItem).MeshDraw(x + i * size, y);
                }
            }
            _game.Render.DepthOff();

            // Текст
            if (_isText)
            {
                _renderMvk.ShaderBindGuiColor(0, 0);
                _font.BindTexture();
                _meshTxt.Draw();
            }

            // Выбранный
            int index = _game.Player.Inventory.GetCurrentIndex();
            _renderMvk.ShaderBindGuiColor(index * size, 0);
            _renderMvk.BindTextureHud();
            _meshInventory.Draw();
            _renderMvk.ShaderBindGuiColor(0, 0);
            _game.Render.DepthOn();
        }

        public override void Dispose()
        {
            base.Dispose();
            _meshInventoryBg?.Dispose();
            _meshInventory?.Dispose();
            _meshTxt?.Dispose();
        }
    }
}
