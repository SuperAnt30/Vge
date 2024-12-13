using System;
using System.Collections.Generic;
using System.Threading;
using Vge.Renderer;
using Vge.Util;

namespace Vge.Realms
{
    /// <summary>
    /// Объект который запускает текстуры, звук, и формирует всё для игры
    /// </summary>
    public class Loading
    {
        /// <summary>
        /// Объект окна малювек
        /// </summary>
        private readonly WindowMain _window;

        public Dictionary<string, BufferedImage> Buffereds = new Dictionary<string, BufferedImage>();

        public Loading(WindowMain window) => _window = window;

        /// <summary>
        /// Запустить загрузку в отдельном потоке
        /// </summary>
        public void Starting()
        {
            Thread myThread = new Thread(_Loop) { Name = "Loading" };
            myThread.Start();
        }

        /// <summary>
        /// Метод должен работать в отдельном потоке
        /// </summary>
        private void _Loop()
        {
            _Steps();
            OnFinish();
        }

        /// <summary>
        /// Максимальное количество шагов
        /// </summary>
        public virtual int GetMaxCountSteps() => 2;

        /// <summary>
        /// Этот метод как раз и реализует список загрузок
        /// </summary>
        protected virtual void _Steps()
        {
            // Основной шрифт
            _window.Render.CreateTextureFontMain(
                _FileToBufferedImage(EnumTexture.FontMain.ToString(), 
                Options.PathTextures + EnumTexture.FontMain.ToString() + ".png"));
            OnStep();
            // Виджет Gui
            _FileToBufferedImage(EnumTexture.Widgets.ToString(), 
                Options.PathTextures + EnumTexture.Widgets.ToString() + ".png");
            OnStep();
        }

        /// <summary>
        /// Конвертировать картинку в структуру BufferedImage
        /// и занести в массиф буферов
        /// </summary>
        protected BufferedImage _FileToBufferedImage(string key, string fileName)
        {
            BufferedImage buffered = BufferedFileImage.FileToBufferedImage(fileName);
            if (Buffereds.ContainsKey(key))
            {
                Buffereds[key] = buffered;
            }
            else
            {
                Buffereds.Add(key, buffered);
            }
            return buffered;
        }

        #region Event

        /// <summary>
        /// Событие шаг
        /// </summary>
        public event EventHandler Step;
        /// <summary>
        /// Событие шаг
        /// </summary>
        protected void OnStep() => Step?.Invoke(this, new EventArgs());

        /// <summary>
        /// Событие заканчивать
        /// </summary>
        public event EventHandler Finish;
        /// <summary>
        /// Событие заканчивать
        /// </summary>
        private void OnFinish() => Finish?.Invoke(this, new EventArgs());

        #endregion
    }
}
