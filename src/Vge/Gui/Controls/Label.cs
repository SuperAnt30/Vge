using System;
using System.Numerics;
using Vge.Renderer;
using Vge.Renderer.Font;
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
        /// Объект шрифта
        /// </summary>
        private readonly FontBase font;

        /// <summary>
        /// Текстовая метка, на которую можно нажать
        /// </summary>
        /// <param name="isLine">Нужен ли контур, для отладки</param>
        public Label(WindowMain window, FontBase font, int width, int height, string text, bool isLine = false) : base(window)
        {
            this.font = font;
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
            font.Clear();
            // Указываем опции
            font.SetColor(color).SetFontFX(EnumFontFX.Outline);

            int biasX = 0;
            int biasY = 0;

            if (multiline)
            {
                // Обрезка текста согласно ширины
                string text = font.TransferWidth(Text, Width);
                // Определяем смещение
                if (TextAlight == EnumAlight.Left)
                {
                    // Определяем смещение BiasY
                    if (TextAlightVert == EnumAlightVert.Middle)
                    {
                        biasY = (Height * si - (font.Transfer.GetStringsCount() * font.GetVertStep())) / 2;
                    }
                    else if (TextAlightVert == EnumAlightVert.Bottom)
                    {
                        biasY = Height * si - (font.Transfer.GetStringsCount() * font.GetVertStep());
                    }
                    // Готовим рендер текста
                    font.RenderText(x + biasX, y + si + biasY, text);
                }
                else
                {
                    // Готовим рендер текста
                    // Кажду строку отдельно перепроверять по смещению
                    string[] vs = font.Transfer.GetStrings();
                    if (TextAlightVert == EnumAlightVert.Middle)
                    {
                        biasY = (Height * si - (vs.Length * font.GetVertStep())) / 2;
                    }
                    else if (TextAlightVert == EnumAlightVert.Bottom)
                    {
                        biasY = Height * si - (vs.Length * font.GetVertStep());
                    }
                    int center = TextAlight == EnumAlight.Center ? 2 : 1;
                    foreach (string s in vs)
                    {
                        biasX = (Width - font.WidthString(s)) * si / center;
                        // Готовим рендер текста
                        font.RenderString(x + biasX, y + biasY, s);
                        y += font.GetVertStep();
                    }
                }
            }
            else
            {
                // Обрезка текста согласно ширины
                string text = font.TransferString(Text, Width);
                // Определяем смещение BiasX
                if (TextAlight == EnumAlight.Center)
                {
                    biasX = (Width - font.WidthString(text)) / 2 * si;
                }
                else if (TextAlight == EnumAlight.Right)
                {
                    biasX = (Width - font.WidthString(text)) * si;
                }
                // Определяем смещение BiasY
                if (TextAlightVert == EnumAlightVert.Middle)
                {
                    biasY = (Height * si - font.GetVert()) / 2;
                }
                else if (TextAlightVert == EnumAlightVert.Bottom)
                {
                    biasY = Height * si - font.GetVert();
                }
                // Готовим рендер текста
                font.RenderString(x + biasX, y + biasY, text);
            }

            // Имеется Outline значит рендерим FX
            font.RenderFX();
            // Вносим сетку
            font.Reload(meshTxt);

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
                window.Render.TextureDisable();
                // И заменить шейдёр
                window.Render.ShaderBindGuiLine();
                meshLine.Draw();
                // После прорисовки возращаем шейдер и текстуру
                window.Render.TextureEnable();
                window.Render.ShaderBindGuiColor();
            }
            // Рисуем текст
            font.BindTexture();
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
