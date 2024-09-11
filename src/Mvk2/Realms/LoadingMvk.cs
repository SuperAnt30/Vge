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

        /// <summary>
        /// Количество текстур
        /// </summary>
        private readonly int countTexture;
        /// <summary>
        /// Количество звуков
        /// </summary>
        private readonly int countSample;

        public LoadingMvk(WindowMvk window) : base(window)
        {
            this.window = window;
            countTexture = 2;
            countSample = window.GetAudio().GetCountStep();
        }

        /// <summary>
        /// Максимальное количество шагов
        /// </summary>
        public override int GetMaxCountSteps() => base.GetMaxCountSteps() + countTexture + countSample;

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
            StepsTextures();
            StepsSample();
        }

        /// <summary>
        /// Шаги текстур
        /// </summary>
        private void StepsTextures()
        {
            RenderMvk renderMvk = window.Render as RenderMvk;
            // Шрифты
            renderMvk.CreateTextureFontSmall(
                FileToBufferedImage(OptionsMvk.PathTextures + "FontSmall.png"));
            //System.Threading.Thread.Sleep(100);
            OnStep();

            renderMvk.CreateTextureFontLarge(
                FileToBufferedImage(OptionsMvk.PathTextures + "FontLarge.png"));
           // System.Threading.Thread.Sleep(100);
            OnStep();

            // Текстуры
        }

        /// <summary>
        /// Шаги звуков
        /// </summary>
        private void StepsSample()
        {
            window.GetAudio().Initialize(countSample);
            window.GetAudio().Step += (sender, e) => OnStep();
            window.GetAudio().InitializeSample();
        }
    }
}
