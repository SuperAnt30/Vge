using Mvk2.Util;
using System.Numerics;
using Vge.Renderer;
using Vge.Renderer.Font;
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
        protected readonly WindowMvk windowMvk;
        /// <summary>
        /// Объект сетки курсора, временно
        /// </summary>
        private Mesh cursorVBO;

        public RenderMvk(WindowMvk window) : base(window)
        {
            windowMvk = window;
            cursorVBO = new Mesh(gl, RenderFigure.Rectangle2d(0, 0, 24, 24, 0, 0, 1, 1), new int[] { 2, 2 });
        }

        /// <summary>
        /// Стартовая инициализация до загрузчика
        /// </summary>
        public override void InitializeFirst()
        {
            base.InitializeFirst();

            SetTexture(OptionsMvk.PathTextures + "Cursor.png", AssetsTexture.Cursor);

            FontSmall = new FontBase(gl, 
                SetTexture(OptionsMvk.PathTextures + "FontSmall.png", AssetsTexture.FontSmall), 1);
            FontLarge = new FontBase(gl, 
                SetTexture(OptionsMvk.PathTextures + "FontLarge.png", AssetsTexture.FontLarge), 2);
        }

        #region Texture

        /// <summary>
        /// Задать количество текстур
        /// </summary>
        protected override void TextureSetCount() => textureMap.SetCount(4);

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
