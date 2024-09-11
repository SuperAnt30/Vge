using Mvk2.Renderer;
using Mvk2.Util;
using Vge.Realms;
using Vge.Util;

namespace Mvk2.Realms
{
    /// <summary>
    /// Объект который запускает текстуры, звук, и формирует всё для игры
    /// </summary>
    public class LoadingMvk : Loading
    {
        /// <summary>
        /// Объект окна малювек
        /// </summary>
        private readonly WindowMvk window;

        public LoadingMvk(WindowMvk window) : base(window) 
            => this.window = window;

        /// <summary>
        /// Максимальное количество шагов
        /// </summary>
        public override int GetMaxCountSteps() => base.GetMaxCountSteps() + 2;

        // <summary>
        /// Получить массив имён файл текстур,
        /// 0 - FontMain основной шрифт
        /// 1 - Widgets
        /// </summary>
        protected override string[] GetFileNameTextures() => new string[] {
            Options.PathTextures + "FontMain.png",
            OptionsMvk.PathTextures + "WidgetsMvk.png"
        };

        /// <summary>
        /// Этот метод как раз и реализует список загрузок
        /// </summary>
        protected override void Steps()
        {
            // Загружаем  по умолчанию
            base.Steps();

            RenderMvk renderMvk = window.Render as RenderMvk;

            renderMvk.CreateTextureFontSmall(
                FileToBufferedImage(OptionsMvk.PathTextures + "FontSmall.png"));
            System.Threading.Thread.Sleep(100);
            OnStep();

            renderMvk.CreateTextureFontLarge(
                FileToBufferedImage(OptionsMvk.PathTextures + "FontLarge.png"));
            System.Threading.Thread.Sleep(100);
            OnStep();

            //for (int i = 0; i < 100; i++)
            //{
            //    System.Threading.Thread.Sleep(5);
            //    OnStep();
            //}
        }
    }
}
