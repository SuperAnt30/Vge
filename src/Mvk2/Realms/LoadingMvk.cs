using Mvk2.Renderer;
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
        private readonly WindowMvk _window;
        /// <summary>
        /// Количество текстур
        /// </summary>
        private readonly int _countTexture = 4;
        /// <summary>
        /// Количество звуков
        /// </summary>
        private readonly int _countSample;

        public LoadingMvk(WindowMvk window) : base(window)
        {
            _window = window;
            _countSample = window.GetAudio().GetCountStep();
        }

        /// <summary>
        /// Максимальное количество шагов
        /// </summary>
        public override int GetMaxCountSteps() => base.GetMaxCountSteps() + _countTexture + _countSample;

        /// <summary>
        /// Этот метод как раз и реализует список загрузок
        /// </summary>
        protected override void _Steps()
        {
            // Загружаем  по умолчанию
            base._Steps();
            _StepsTextures();
            _StepsSample();
        }

        /// <summary>
        /// Шаги текстур
        /// </summary>
        private void _StepsTextures()
        {
            RenderMvk renderMvk = _window.Render as RenderMvk;
            // Шрифты
            renderMvk.CreateTextureFontSmall(
                _FileToBufferedImage(EnumTextureMvk.FontSmall.ToString(),
                Options.PathTextures + EnumTextureMvk.FontSmall.ToString() + ".png"));
            OnStep();

            renderMvk.CreateTextureFontLarge(
                _FileToBufferedImage(EnumTextureMvk.FontLarge.ToString(), 
                Options.PathTextures + EnumTextureMvk.FontLarge.ToString() + ".png"));
            OnStep();
            // Чат Gui
            _FileToBufferedImage(EnumTextureMvk.Chat.ToString(),
                Options.PathTextures + EnumTextureMvk.Chat.ToString() + ".png");
            OnStep();

            _FileToBufferedImage(EnumTextureMvk.Hud.ToString(),
                Options.PathTextures + EnumTextureMvk.Hud.ToString() + ".png");
            OnStep();
        }

        /// <summary>
        /// Шаги звуков
        /// </summary>
        private void _StepsSample()
        {
            _window.GetAudio().Initialize(_countSample);
            _window.GetAudio().Step += (sender, e) => OnStep();
            _window.GetAudio().InitializeSample();
        }
    }
}
