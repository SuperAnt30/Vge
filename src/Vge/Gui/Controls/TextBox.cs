using System;
using System.Numerics;
using Vge.Renderer;
using Vge.Renderer.Font;
using WinGL.Actions;

namespace Vge.Gui.Controls
{
    /// <summary>
    /// Поля для редактирования текста
    /// </summary>
    public class TextBox : WidgetBase
    {
        /// <summary>
        /// Смещение от начала рамки до текста
        /// </summary>
        private const int marginLeft = 12;

        /// <summary>
        /// Сетка фона
        /// </summary>
        private readonly MeshGuiColor meshBg;
        /// <summary>
        /// Сетка текста
        /// </summary>
        private readonly MeshGuiColor meshTxt;
        /// <summary>
        /// Сетка курсора
        /// </summary>
        private readonly MeshGuiColor meshCursor;
        /// <summary>
        /// Коэфициент смещения вертикали для текстуры
        /// </summary>
        private readonly float vk;
        /// <summary>
        /// Ограничения набор символов 
        /// </summary>
        private readonly EnumRestrictions restrictions;
        /// <summary>
        /// Объект шрифта
        /// </summary>
        private readonly FontBase font;
        /// <summary>
        /// Максимальная длинна 
        /// </summary>
        private readonly int limit;

        /// <summary>
        /// Счётчик для анимации
        /// </summary>
        private int cursorCounter;
        /// <summary>
        /// Где стоит курсор номер символа
        /// </summary>
        private int stepCursor = 0;
        /// <summary>
        /// Видимость курсора
        /// </summary>
        private bool isVisibleCursor;

        public TextBox(WindowMain window, FontBase font, int width, int height, string text,
            EnumRestrictions restrictions, int limit = 24) : base(window)
        {
            this.limit = limit;
            this.font = font;
            this.restrictions = restrictions;
            meshBg = new MeshGuiColor(gl);
            meshTxt = new MeshGuiColor(gl);
            meshCursor = new MeshGuiColor(gl);
            vk = .078125f; // 40 / 512f;
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
                    if (!Focus)
                    {
                        isVisibleCursor = true;
                        Focus = true;
                    }
                    // Определяем куда установить курсор
                    int x0 = x / si - PosX;
                    int w1 = marginLeft;
                    int count = Text.Length;
                    stepCursor = count;
                    for (int i = 1; i <= count; i++)
                    {
                        w1 = font.WidthString(Text.Substring(0, i));
                        if (w1 > x0)
                        {
                            stepCursor = i - 1;
                            break;
                        }
                    }
                    IsRender = true;
                }
                else if (!enter && Focus)
                {
                    // Потерять фокус
                    isVisibleCursor = false;
                    Focus = false;
                    IsRender = true;
                }
            }
        }

        #endregion

        #region OnKey

        public override void OnKeyPress(char key)
        {
            int id = Convert.ToInt32(key);
            if (id == 8)
            {
                // back
                if (stepCursor > 0)
                {
                    string text = Text.Substring(0, stepCursor - 1);
                    if (Text.Length > stepCursor)
                    {
                        text += Text.Substring(stepCursor);
                    }
                    stepCursor--;
                    SetText(text);
                }
            }
            else if (Text.Length < limit && Check(key, id))
            {
                string text = Text.Substring(0, stepCursor) + key;
                if (Text.Length > stepCursor)
                {
                    text += Text.Substring(stepCursor);
                }
                stepCursor++;
                SetText(text);
            }
        }

        public override void OnKeyDown(Keys keys)
        {
            if (keys == Keys.Delete)
            {
                if (stepCursor < Text.Length)
                {
                    string text = "";
                    if (stepCursor > 0)
                    {
                        text = Text.Substring(0, stepCursor);
                    }
                    if (Text.Length > stepCursor)
                    {
                        text += Text.Substring(stepCursor + 1);
                    }
                    SetText(text);
                }
            }
            else if (keys == Keys.Left)
            {
                if (stepCursor > 0)
                {
                    stepCursor--;
                    IsRender = true;
                }
            }
            else if (keys == Keys.Right)
            {
                if (stepCursor < Text.Length)
                {
                    stepCursor++;
                    IsRender = true;
                }
            }
            else if (keys == Keys.Up)
            {
                KeyUpDown(true);
            }
            else if (keys == Keys.Down)
            {
                KeyUpDown(false);
            }
        }

        /// <summary>
        /// Действие клавиши Up или Down
        /// </summary>
        protected virtual void KeyUpDown(bool up) { }

        private bool Check(char key, int id)
        {
            switch (restrictions)
            {
                case EnumRestrictions.IpPort:
                    return (id >= 48 && id <= 57) // цифры
                    || id == 46 || id == 58; // точка и двое точие
                case EnumRestrictions.Number:
                    return id >= 48 && id <= 57; // цифры
                case EnumRestrictions.Name:
                    return (id >= 48 && id <= 57) // цифры
                    || (id >= 65 && id <= 90) // Большие англ
                    || (id >= 97 && id <= 122); // Маленькие англ
                case EnumRestrictions.All:
                    return font.IsPresent(key);
            }
            return false;
        }

        #endregion

        #region Draw

        public override void Render()
        {
            RenderInside(window.Render, PosX * si, PosY * si);
            IsRender = false;
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
            int biasY = (Height * si - font.GetVert()) / 2;

            // Чистим буфер
            font.Clear();
            // Указываем опции
            font.SetColor(color).SetFontFX(EnumFontFX.Outline);

            // Обрезка текста согласно ширины
            string text = font.TransferString(Text, Width);
            // Готовим рендер текста
            font.RenderString(x + marginLeft * si, y + biasY, text);

            // Имеется Outline значит рендерим FX
            font.RenderFX();
            // Вносим сетку
            font.Reload(meshTxt);

            // Сетка фона
            float v1 = Enabled ? enter ? vk * 4 : vk * 3 : 0f;
            meshBg.Reload(RectangleTwo(x, y, 0, v1, vk, 1, 1, 1));

            if (isVisibleCursor)
            {
                // Если нужен курсор, то рендерим сетку
                int w = (PosX + font.WidthString(Text.Substring(0, stepCursor)) + marginLeft) * si;
                // Чистим буфер
                font.Clear();
                // Указываем опции
                font.SetColor(color).SetFontFX(EnumFontFX.Outline);
                // Готовим рендер текста
                font.RenderString(w, y + biasY, stepCursor == Text.Length ? "_" : "|");
                // Имеется Outline значит рендерим FX
                font.RenderFX();
                // Вносим сетку
                font.Reload(meshCursor);
            }
        }

        /// <summary>
        /// Метод для прорисовки кадра
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        public override void Draw(float timeIndex)
        {
            // Рисуем фон
            window.Render.BindTextureWidgets();
            meshBg.Draw();
            // Рисуем текст
            font.BindTexture();
            meshTxt.Draw();

            // Рисуем контур если имеется
            if (isVisibleCursor)
            {
                meshCursor.Draw();
            }
        }

        #endregion

        public override void OnTick(float deltaTime)
        {
            if (Focus)
            {
                cursorCounter++;

                if ((cursorCounter >> 4) % 2 == 0)
                {
                    if (!isVisibleCursor)
                    {
                        isVisibleCursor = true;
                        IsRender = true;
                    }
                }
                else
                {
                    if (isVisibleCursor)
                    {
                        isVisibleCursor = false;
                        IsRender = true;
                    }
                }
            }
        }

        public override void Dispose()
        {
            if (meshTxt != null) meshTxt.Dispose();
            if (meshBg != null) meshBg.Dispose();
            if (meshCursor != null) meshCursor.Dispose();
        }

        /// <summary>
        /// Ограничения набор символов 
        /// </summary>
        public enum EnumRestrictions
        {
            /// <summary>
            /// Для ip. Цифры, точка и двоеточие
            /// </summary>
            IpPort,
            /// <summary>
            /// Только цифры
            /// </summary>
            Number,
            /// <summary>
            /// Цифры и английские буквы
            /// </summary>
            Name,
            /// <summary>
            /// Все доступные по графике
            /// </summary>
            All
        }
    }
}
