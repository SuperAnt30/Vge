using System;
using System.Numerics;
using Vge.Renderer;
using WinGL.Actions;

namespace Vge.Gui.Controls
{
    /// <summary>
    /// Текстовая метка, на которую можно нажать
    /// </summary>
    public class Label : WidgetBase
    {
        private readonly MeshGuiColor meshTxt;
        private readonly MeshGuiLine meshLine;

        /// <summary>
        /// Текстовая метка, на которую можно нажать
        /// </summary>
        /// <param name="isLine">Нужен ли контур, для отладки</param>
        public Label(WindowMain window, int width, int height, string text, bool isLine = false) : base(window)
        {
            meshTxt = new MeshGuiColor(gl);
            if (isLine)
            {
                meshLine = new MeshGuiLine(gl);
            }
            SetText(text);
            SetSize(width, height);
        }

        #region OnMouse

        /// <summary>
        /// Нажатие клавиши мышки
        /// </summary>
        public override void OnMouseDown(MouseButton button, int x, int y)
        {
            if (button == MouseButton.Left)
            {
                OnMouseMove(x, y);
                if (enter)
                {
                    // Звук клика
                    window.SoundClick(.3f);
                    OnClick();
                }
            }
        }

        #endregion

        #region Draw

        public override void Render()
        {
            IsRender = false;
            RenderInside(window.Render, PosX * si, PosY * si);
        }

        /// <summary>
        /// Прорисовка внутреняя
        /// </summary>
        /// <param name="x">Позиция X с учётом интерфейса</param>
        /// <param name="y">Позиция Y с учётом интерфейса</param>
        protected virtual void RenderInside(RenderMain render, int x, int y)
        {
            // Определяем цвет текста
            Vector3 color = Enabled ? enter ? Gi.ColorTextEnter : Gi.ColorText : Gi.ColorTextInactive;
            // Чистим буфер
            render.FontMain.Clear();
            // Обрезка текста согласно ширины
            string text = window.Render.FontMain.TransferString(Text, Width);
            // Определяем смещение
            int biasX = (Width - render.FontMain.WidthString(text)) / 2 * si;
            // Указываем опции
            render.FontMain.SetColor(color).SetFontFX(Renderer.Font.EnumFontFX.Outline);
            // Готовим рендер текста
            render.FontMain.RenderString(x + biasX, y + (Height - 16) / 2 * si, text);
            // Имеется Outline значит рендерим FX
            render.FontMain.RenderFX();
            // Вносим сетку
            render.FontMain.Reload(meshTxt);

            // Если нужен контур, то рендерим сетку
            if (meshLine != null)
            {
                meshLine.Reload(MeshGuiLine.RectangleLine(PosX * si, PosY * si, (PosX + Width) * si, (PosY + Height) * si,
                    0, 0, 0, .5f));
            }
        }

        /// <summary>
        /// Метод для прорисовки кадра
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        public override void Draw(float timeIndex)
        {
            // Рисуем контур если имеется
            if (meshLine != null)
            {
                // Для контура надо перекулючится без текстуры
                window.Render.Texture2DDisable();
                // И заменить шейдёр
                window.Render.ShaderBindGuiLine();
                meshLine.Draw();
                // После прорисовки возращаем шейдер и текстуру
                window.Render.Texture2DEnable();
                window.Render.ShaderBindGuiColor();
            }
            // Рисуем текст кнопки
            window.Render.BindTextureFontMain();
            meshTxt.Draw();
        }

        #endregion

        #region Event

        /// <summary>
        /// Событие клика по кнопке
        /// </summary>
        public event EventHandler Click;
        /// <summary>
        /// Событие клика по кнопке
        /// </summary>
        protected virtual void OnClick() => Click?.Invoke(this, new EventArgs());

        #endregion
    }
}
