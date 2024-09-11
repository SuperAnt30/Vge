using Mvk2.Realms;
using Vge.Gui.Screens;
using Vge.Renderer;
using Vge.Util;

namespace Mvk2.Gui.Screens
{
    /// <summary>
    /// Заставка
    /// </summary>
    public class ScreenSplashMvk : ScreenSplash
    {
        private readonly MeshGuiColor meshLogo;
        private readonly MeshGuiColor meshProcess;
        private readonly ListFast<float> list = new ListFast<float>();
        private int max;

        public ScreenSplashMvk(WindowMvk window) : base(window)
        {
            meshLogo = new MeshGuiColor(window.GetOpenGL());
            meshProcess = new MeshGuiColor(window.GetOpenGL());
        }

        /// <summary>
        /// Объвление объекта загрузки
        /// </summary>
        protected override void LoadingCreate()
        {
            loading = new LoadingMvk((WindowMvk)window);
            max = loading.GetMaxCountSteps();
        }

        /// <summary>
        /// Начальный рендер
        /// </summary>
        protected override void RenderBegin()
        {
            int w = window.Width / 2;
            int h = (window.Height - 608) / 2;
            meshLogo.Reload(MeshGuiColor.Rectangle(w - 512, h, w + 512, h + 512, 0, 0, 1, 1));
        }

        /// <summary>
        /// Рендер загрузчика шага
        /// </summary>
        protected override void RenderStep()
        {
            int w = window.Width / 2;
            int h = (window.Height - 608) / 2 + 512;
            list.Clear();
            list.AddRange(MeshGuiColor.Rectangle(w - 308, h - 40, w + 308, h, .13f, .44f, .91f), 4);
            list.AddRange(MeshGuiColor.Rectangle(w - 304, h - 36, w + 304, h - 4, 1, 1, 1), 4);
            int wcl = countDraw * 600 / max;
            list.AddRange(MeshGuiColor.Rectangle(w - 300, h - 32, w - 300 + wcl, h - 8, .13f, .44f, .91f), 4);

            meshProcess.Reload(list.GetBufferAll(), list.Count);
        }

        /// <summary>
        /// Метод для прорисовки кадра
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        public override void Draw(float timeIndex)
        {
            gl.ClearColor(1, 1, 1, 1);
            window.Render.ShaderBindGuiColor();
            window.Render.BindTexutreSplash();
            meshLogo.Draw();
            window.Render.Texture2DDisable();
            meshProcess.Draw();
            window.Render.Texture2DEnable();
        }
    }
}
