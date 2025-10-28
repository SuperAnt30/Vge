using Mvk2.Renderer;
using System;
using Vge.Item;
using Vge.Realms;
using Vge.Renderer;
using Vge.Renderer.Font;
using Vge.Util;

namespace Mvk2.Gui
{
    /// <summary>
    /// Рендер слотов
    /// </summary>
    public class RenderSlots : IDisposable
    {
        private readonly RenderMvk _renderMvk;

        /// <summary>
        /// Сетка текста
        /// </summary>
        private readonly MeshGuiColor _meshTxt;
        /// <summary>
        /// Сетка уровня урона
        /// </summary>
        private readonly MeshGuiColor _meshDamage;
        /// <summary>
        /// Объект шрифта
        /// </summary>
        private readonly FontBase _font;
        /// <summary>
        /// Список буфера для построения сеток
        /// </summary>
        public static readonly ListFlout ListBuffer = new ListFlout();

        /// <summary>
        /// Имеется ли текст
        /// </summary>
        private bool _isText;
        /// <summary>
        /// Имеется ли урон в предметах
        /// </summary>
        private bool _isDamage;

        public RenderSlots(RenderMvk renderMvk)
        {
            _renderMvk = renderMvk;
            _font = _renderMvk.FontMain;
            _meshTxt = new MeshGuiColor(_renderMvk.GetOpenGL());
            _meshDamage = new MeshGuiColor(_renderMvk.GetOpenGL());
        }

        public void Clear()
        {
            _isDamage = false;
            _isText = false;
        }

        /// <summary>
        /// Перед рендером для текста
        /// </summary>
        public void BeforeRenderText()
        {
            // Чистим буфер
            _font.Clear(true, true);
            _font.SetColorShadow(Gi.ColorTextBlack);
            // Указываем опции
            _font.SetFontFX(EnumFontFX.Outline);
            _isText = false;
        }

        /// <summary>
        /// Перед рендером для урона
        /// </summary>
        public void BeforeRenderDamage()
        {
            _isDamage = false;
            ListBuffer.Clear();
        }

        /// <summary>
        /// Перед рендером
        /// </summary>
        public void BeforeRender()
        {
            BeforeRenderText();
            BeforeRenderDamage();
        }

        /// <summary>
        /// После рендера
        /// </summary>
        public void AfterRender()
        {
            if (_isText)
            {
                // Имеется Outline значит рендерим FX
                _font.RenderFX();
                // Вносим сетку
                _font.Reload(_meshTxt);
            }
            if (_isDamage)
            {
                _meshDamage.Reload(ListBuffer.GetBufferAll(), ListBuffer.Count);
            }
        }

        /// <summary>
        /// Рендер текста без смещения и без проверки. Для Slot
        /// </summary>
        public void TextRender(ItemStack itemStack)
        {
            // Готовим рендер текста
            _font.RenderString(3 * Gi.Si, 3 * Gi.Si, ChatStyle.Bolb + itemStack.Amount.ToString());
            _isText = true;
        }

        /// <summary>
        /// Рендер текста со смещением и с проверкой, что он есть. Для Hud
        /// </summary>
        public void TextRenderCheck(ItemStack itemStack, int x, int y)
        {
            if (itemStack.Amount != 1)
            {
                // Готовим рендер текста
                _font.RenderString((x + 3) * Gi.Si, (y + 3) * Gi.Si, ChatStyle.Bolb + itemStack.Amount.ToString());
                _isText = true;
            }
        }

        public void DamageRender(ItemStack itemStack, int x, int y)
        {
            if (itemStack.Item.MaxDamage != 0)
            //if (itemStack.Amount != 1)
            {
                // Ренедер урона
                int line = itemStack.ItemDamage * 30 / itemStack.Item.MaxDamage;
                //int line = itemStack.Amount * 30 / itemStack.Item.MaxStackSize;
                if (line > 30) line = 30;

                ListBuffer.AddRange(RenderFigure.Rectangle(
                    (x + 2) * Gi.Si, (y + 30) * Gi.Si, (x + 34) * Gi.Si, (y + 34) * Gi.Si, .16f, .16f, .16f));

                ListBuffer.AddRange(RenderFigure.Rectangle(
                    (x + 3) * Gi.Si, (y + 31) * Gi.Si, (x + 3 + line) * Gi.Si, (y + 33) * Gi.Si,
                    line > 15 ? (30 - line) / 15f : 1,
                    line < 15 ? line / 15f : 1,
                    .16f));

                _isDamage = true;
            }
        }

        /// <summary>
        /// Метод для прорисовки кадра
        /// </summary>
        public void Draw(int x, int y)
        {
            if (_isText || _isDamage)
            {
                // Рисуем текст
                _renderMvk.ShaderBindGuiColor(x, y);
                _font.BindTexture();
                if (_isDamage) _meshDamage.Draw();
                if (_isText) _meshTxt.Draw();
            }
        }

        public void Dispose()
        {
            _meshTxt?.Dispose();
            _meshDamage?.Dispose();
        }
    }
}
