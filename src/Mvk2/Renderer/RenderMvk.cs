using Vge.Renderer;
using Vge.Renderer.Font;
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

        public RenderMvk(WindowMvk window) : base(window) => windowMvk = window;

        #region Texture

        /// <summary>
        /// Создать текстуру Мелкий шрифт
        /// </summary>
        public void CreateTextureFontSmall(BufferedImage buffered) => FontSmall = new FontBase(buffered, 1);
        /// <summary>
        /// Создать текстуру Крупный шрифт
        /// </summary>
        public void CreateTextureFontLarge(BufferedImage buffered) => FontLarge = new FontBase(buffered, 2);

        /// <summary>
        /// Запустить текстуру, указав индекс текстуры массива
        /// </summary>
        public void BindTexture(AssetsTexture index, uint texture = 0) 
            => textureMap.BindTexture((int)index, texture);

        #endregion

        /// <summary>
        /// На финише загрущика в основном потоке
        /// </summary>
        /// <param name="buffereds">буфер всех текстур для биндинга</param>
        public override void AtFinishLoading(BufferedImage[] buffereds)
        {
            base.AtFinishLoading(buffereds);
            FontSmall.CreateMesh(gl);
            FontLarge.CreateMesh(gl);
        }
    }
}
