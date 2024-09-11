using Mvk2.Util;
using System.Numerics;
using Vge.Renderer;
using Vge.Renderer.Font;
using Vge.Util;
using WinGL.OpenGL;
using WinGL.Util;

namespace Mvk2.Renderer
{
    /// <summary>
    /// Класс отвечающий за прорисовку для малювек
    /// </summary>
    public class RenderMvk : RenderMain
    {
        /// <summary>
        /// Мелкий шрифт
        /// </summary>
        public FontBase FontSmall { get; private set; }
        /// <summary>
        /// Крупный шрифт
        /// </summary>
        public FontBase FontLarge { get; private set; }

        /// <summary>
        /// Объект окна малювек
        /// </summary>
        private readonly WindowMvk windowMvk;

        public RenderMvk(WindowMvk window) : base(window)
        {
            windowMvk = window;
        }

        /// <summary>
        /// Стартовая инициализация до загрузчика
        /// </summary>
        public override void InitializeFirst()
        {
            base.InitializeFirst();

            FontSmall = new FontBase(gl, 
                SetTexture(OptionsMvk.PathTextures + "FontSmall.png", AssetsTexture.FontSmall), 1);
            FontLarge = new FontBase(gl, 
                SetTexture(OptionsMvk.PathTextures + "FontLarge.png", AssetsTexture.FontLarge), 2);
        }

        /// <summary>
        /// Получить массив имён файл текстур,
        /// 0 - FontMain основной шрифт
        /// 2 - Widgets
        /// </summary>
        protected override string[] GetFileNameTextures() => new string[] {
          //  Options.PathTextures + "Splash.png",
            Options.PathTextures + "FontMain.png",
            OptionsMvk.PathTextures + "WidgetsMvk.png"
        };

        #region Texture

        /// <summary>
        /// Задать количество текстур
        /// </summary>
        protected override void TextureSetCount() => textureMap.SetCount(5);

        /// <summary>
        /// Запустить текстуру, указав индекс текстуры массива
        /// </summary>
        public void BindTexture(AssetsTexture index, uint texture = 0) 
            => textureMap.BindTexture((int)index, texture);

        /// <summary>
        /// Задать текстуру
        /// </summary>
        protected BufferedImage SetTexture(string fileName, AssetsTexture index)
            => SetTexture(fileName, (int)index);

        #endregion

        


        
    }
}
