using System;
using System.Collections.Generic;
using System.Threading;
using Vge.Util;
using WinGL.Util;

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
        private readonly WindowMain window;

        public List<BufferedImage> buffereds = new List<BufferedImage>();

        public Loading(WindowMain window) => this.window = window;

        /// <summary>
        /// Запустить загрузку в отдельном потоке
        /// </summary>
        public void Starting()
        {
            Thread myThread = new Thread(Loop) { Name = "Loading" };
            myThread.Start();
        }

        /// <summary>
        /// Метод должен работать в отдельном потоке
        /// </summary>
        private void Loop()
        {
            Steps();
            OnFinish();
        }

        /// <summary>
        /// Максимальное количество шагов
        /// </summary>
        public virtual int GetMaxCountSteps() => 3;

        /// <summary>
        /// Этот метод как раз и реализует список загрузок
        /// </summary>
        protected virtual void Steps()
        {
            string[] vs = GetFileNameTextures();
            // Основной шрифт
            window.Render.CreateTextureFontMain(FileToBufferedImage(vs[0]));
            OnStep();
            // Виджет Gui
            FileToBufferedImage(vs[1]);
            OnStep();
            // AtlasBlocks
            FileToBufferedImage(vs[2], true);
            OnStep();
        }

        /// <summary>
        /// Получить массив имён файл текстур,
        /// 0 - FontMain основной шрифт
        /// 1 - Widgets
        /// 2 - AtlasBlocks
        /// </summary>
        protected virtual string[] GetFileNameTextures() => new string[] {
            Options.PathTextures + "FontMain.png",
            Options.PathTextures + "Widgets.png",
            Options.PathTextures + "AtlasBlocks.png"
        };

        /// <summary>
        /// Конвертировать картинку в структуру BufferedImage
        /// и занести в массиф буферов
        /// </summary>
        protected BufferedImage FileToBufferedImage(string fileName, bool minmap = false)
        {
            BufferedImage buffered = BufferedFileImage.FileToBufferedImage(fileName, minmap);
            buffereds.Add(buffered);
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
