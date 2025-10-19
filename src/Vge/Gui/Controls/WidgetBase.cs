﻿namespace Vge.Gui.Controls
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
        /// Текст
        /// </summary>
        public string Text { get; private set; }
        /// <summary>
        /// Дополнительный объект
        /// </summary>
        public object Tag { get; set; }
        /// <summary>
        /// Когда мышь находится на элементе
        /// </summary>
        public bool Enter { get; private set; }

        /// <summary>
        /// Размер интерфеса
        /// </summary>
        protected int _si = 1;

        protected WidgetBase(WindowMain window) : base(window)
        {
            _si = Gi.Si;
        }

        /// <summary>
        /// Изменён размер окна
        /// </summary>
        public virtual void OnResized()
        {
            if (_si != Gi.Si)
            {
                _si = Gi.Si;
                IsRender = true;
            }
        }

        #region OnMouse

        /// <summary>
        /// В облости ли мышь курсора
        /// </summary>
        private bool _IsRectangleMouse(int x, int y) => Visible && Enabled && x >= PosX * _si && y >= PosY * _si
                && x < (PosX + Width) * _si && y < (PosY + Height) * _si;

        /// <summary>
        /// Перемещение мышки
        /// </summary>
        public override void OnMouseMove(int x, int y) => _CanEnter(x, y);

        /// <summary>
        /// Перемещение мышки, для контроля мышь над элементом
        /// </summary>
        protected void _CanEnter(int x, int y)
        {
            bool b = _IsRectangleMouse(x, y);
            if (Enter != b)
            {
                Enter = b;
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
        public virtual void Rendering() => IsRender = false;

        /// <summary>
        /// Двойной прямоугольник, в ширину по полам
        /// </summary>
        protected float[] _RectangleTwo(int width, int height, int x1, float y1, 
            float u1, float v1, float vk, 
            float r, float g, float b, float a = 1f)
        {
            int w = width * _si / 2;
            float wf = width / 1024f;
            float h = height * _si;

            float y2 = y1 + h;
            float v2 = v1 + vk;
            float x2 = x1 + w;
            float x3 = x2 + w;
            float u2 = wf;
            float u4 = 1;
            float u3 = u4 - wf;

            return new float[]
            {
                x1, y1, u1, v1, r, g, b, a,
                x1, y2, u1, v2, r, g, b, a,
                x2, y2, u2, v2, r, g, b, a,
                x2, y1, u2, v1, r, g, b, a,

                x2, y1, u3, v1, r, g, b, a,
                x2, y2, u3, v2, r, g, b, a,
                x3, y2, u4, v2, r, g, b, a,
                x3, y1, u4, v1, r, g, b, a
            };
        }

        /// <summary>
        /// Двойной прямоугольник, в ширину по полам
        /// </summary>
        protected float[] _RectangleTwo(int x1, float y1,
            float u1, float v1, float uk, float vk, float height)
        {
            int w = Width * _si / 2;
            float wf = w / (512f * _si);
            float h = height * _si;

            float y2 = y1 + h;
            float v2 = v1 + vk;
            float x2 = x1 + w;
            float x3 = x2 + w;
            float u2 = u1 + wf;
            float u4 = u1 + uk;
            float u3 = u4 - wf;

            return new float[]
            {
                x1, y1, u1, v1, 1, 1, 1, 1,
                x1, y2, u1, v2, 1, 1, 1, 1,
                x2, y2, u2, v2, 1, 1, 1, 1,
                x2, y1, u2, v1, 1, 1, 1, 1,

                x2, y1, u3, v1, 1, 1, 1, 1,
                x2, y2, u3, v2, 1, 1, 1, 1,
                x3, y2, u4, v2, 1, 1, 1, 1,
                x3, y1, u4, v1, 1, 1, 1, 1
            };
        }

        /// <summary>
        /// Двойной прямоугольник, в ширину по полам
        /// </summary>
        protected float[] _RectangleTwo(int x1, float y1, 
            float u1, float v1, float vk, 
            float r, float g, float b, float a = 1f)
            => _RectangleTwo(Width, Height, x1, y1, u1, v1, vk, r, g, b, a);

        #endregion

        /// <summary>
        /// Вернуть подсказку у контрола
        /// </summary>
        public virtual string GetToolTip() => "";
    }
}
