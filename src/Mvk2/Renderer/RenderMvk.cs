using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Vge.Renderer;
using Vge.Renderer.Font;
using Vge.Renderer.World;
using Vge.Util;

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
        /// <summary>
        /// Переменные индексов текстур GUI для малювек
        /// </summary>
        private TextureIndexMvk _textureIndexMvk;

        public RenderMvk(WindowMvk window) : base(window) => windowMvk = window;

        protected override void _Initialize()
        {
            _textureIndex = _textureIndexMvk = new TextureIndexMvk();
            LightMap = new TextureLightMap(gl);
        }


        #region Texture

        /// <summary>
        /// Создать текстуру Мелкий шрифт
        /// </summary>
        public void CreateTextureFontSmall(BufferedImage buffered)
            => FontSmall = new FontBase(buffered, 1, this);
        /// <summary>
        /// Создать текстуру Крупный шрифт
        /// </summary>
        public void CreateTextureFontLarge(BufferedImage buffered)
            => FontLarge = new FontBase(buffered, 2, this);
        /// <summary>
        /// Запустить текстуру чата
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void BindTextureChat() => Texture.BindTexture(_textureIndexMvk.Chat);

        #endregion

        /// <summary>
        /// На финише загрущика в основном потоке
        /// </summary>
        /// <param name="buffereds">буфер всех текстур для биндинга</param>
        public override void AtFinishLoading(Dictionary<string, BufferedImage> buffereds)
        {
            base.AtFinishLoading(buffereds);

            if (buffereds.ContainsKey(EnumTextureMvk.FontSmall.ToString()))
            {
                FontSmall.CreateMesh(gl, Texture.SetTexture(buffereds[EnumTextureMvk.FontSmall.ToString()]));
            }
            if (buffereds.ContainsKey(EnumTextureMvk.FontLarge.ToString()))
            {
                FontLarge.CreateMesh(gl, Texture.SetTexture(buffereds[EnumTextureMvk.FontLarge.ToString()]));
            }
            if (buffereds.ContainsKey(EnumTextureMvk.Chat.ToString()))
            {
                _textureIndexMvk.Chat = Texture.SetTexture(buffereds[EnumTextureMvk.Chat.ToString()]);
            }
        }
    }
}
