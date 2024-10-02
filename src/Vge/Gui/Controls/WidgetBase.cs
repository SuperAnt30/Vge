namespace Vge.Gui.Controls
{
    /// <summary>
    /// Виджет для GUI
    /// </summary>
    public abstract class WidgetBase : Warp
    {
        /// <summary>
        /// Нужен ли рендер
        /// </summary>
        public bool IsRender { get; protected set; } = true;
        /// <summary>
        /// Ширина
        /// </summary>
        public int Width { get; private set; }
        /// <summary>
        /// Высотаа
        /// </summary>
        public int Height { get; private set; }
        /// <summary>
        /// Активный
        /// </summary>
        public bool Enabled { get; private set; } = true;
        /// <summary>
        /// Видимый
        /// </summary>
        public bool Visible { get; private set; } = true;
        /// <summary>
        /// Фокус
        /// </summary>
        public bool Focus { get; protected set; } = false;
        /// <summary>
        /// Позиция X
        /// </summary>
        public int PosX { get; private set; }
        /// <summary>
        /// Позиция Y
        /// </summary>
        public int PosY { get; private set; }
        /// <summary>
        /// Текст кнопки
        /// </summary>
        public string Text { get; private set; }
        /// <summary>
        /// Дополнительный объект
        /// </summary>
        public object Tag { get; set; }

        /// <summary>
        /// Когда мышь находится на элементе
        /// </summary>
        protected bool enter = false;
        /// <summary>
        /// Размер интерфеса
        /// </summary>
        protected int si = 1;

        protected WidgetBase(WindowMain window) : base(window)
        {
            si = Gi.Si;
        }

        /// <summary>
        /// Изменён размер окна
        /// </summary>
        public virtual void OnResized()
        {
            if (si != Gi.Si)
            {
                si = Gi.Si;
                IsRender = true;
            }
        }

        #region OnMouse

        /// <summary>
        /// В облости ли мышь курсора
        /// </summary>
        private bool IsRectangleMouse(int x, int y) => Enabled && x >= PosX * si && y >= PosY * si
                && x < (PosX + Width) * si && y < (PosY + Height) * si;

        /// <summary>
        /// Перемещение мышки
        /// </summary>
        public override void OnMouseMove(int x, int y)
        {
            bool b = IsRectangleMouse(x, y);
            if (enter != b)
            {
                enter = b;
                IsRender = true;
            }
        }

        #endregion

        #region OnKey

        #endregion

        #region Set...

        /// <summary>
        /// Задать позицию контрола
        /// </summary>
        public WidgetBase SetPosition(int x, int y)
        {
            PosX = x;
            PosY = y;
            IsRender = true;
            return this;
        }

        /// <summary>
        /// Задать размер виджета
        /// </summary>
        public WidgetBase SetSize(int width, int height)
        {
            Width = width;
            Height = height;
            IsRender = true;
            return this;
        }

        /// <summary>
        /// Задать текст контрола
        /// </summary>
        public WidgetBase SetText(string text)
        {
            Text = text;
            IsRender = true;
            return this;
        }

        /// <summary>
        /// Изменить активность
        /// </summary>
        public WidgetBase SetEnable(bool enable)
        {
            Enabled = enable;
            IsRender = true;
            return this;
        }

        /// <summary>
        /// Изменить видимость
        /// </summary>
        public WidgetBase SetVisible(bool visible)
        {
            Visible = visible;
            IsRender = true;
            return this;
        }

        #endregion

        #region Draw

        /// <summary>
        /// рендер контрола
        /// </summary>
        public virtual void Render() => IsRender = false;

        /// <summary>
        /// Прямоугольник
        /// </summary>
        protected float[] Rectangle(int x1, float y1, int x2, int y2, float u1, float v1, float u2, float v2, float r, float g, float b)
        {
            return new float[]
            {
                x1, y1, u1, v1, r, g, b,
                x1, y2, u1, v2, r, g, b,
                x2, y1, u2, v1, r, g, b,
                x2, y2, u2, v2, r, g, b,
            };
        }

        /// <summary>
        /// Двойной прямоугольник, в ширину по полам
        /// </summary>
        protected float[] RectangleTwo(int width, int height, int x1, float y1, float u1, float v1, float vk, float r, float g, float b)
        {
            int w = width * si / 2;
            float wf = width / 1024f;
            float h = height * si;

            float y2 = y1 + h;
            float v2 = v1 + vk;
            float x2 = x1 + w;
            float x3 = x2 + w;
            float u2 = wf;
            float u4 = 1;
            float u3 = u4 - wf;

            return new float[]
            {
                x1, y1, u1, v1, r, g, b,
                x1, y2, u1, v2, r, g, b,
                x2, y1, u2, v1, r, g, b,
                x2, y2, u2, v2, r, g, b,

                x2, y1, u3, v1, r, g, b,
                x2, y2, u3, v2, r, g, b,
                x3, y1, u4, v1, r, g, b,
                x3, y2, u4, v2, r, g, b,
            };
        }

        /// <summary>
        /// Двойной прямоугольник, в ширину по полам
        /// </summary>
        protected float[] RectangleTwo(int x1, float y1, float u1, float v1, float vk, float r, float g, float b)
            => RectangleTwo(Width, Height, x1, y1, u1, v1, vk, r, g, b);

        #endregion
    }
}
