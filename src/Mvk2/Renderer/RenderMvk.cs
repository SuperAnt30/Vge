using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Vge.Renderer;
using Vge.Renderer.Font;
using Vge.Renderer.World;
using Vge.Util;

namespace Mvk2.Renderer
{
    /// <summary>
    /// Основной класс рендера, он же клиентский основной объект
    /// Для Малювек 2
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
        private readonly WindowMvk _windowMvk;
        /// <summary>
        /// Переменные индексов текстур GUI для малювек
        /// </summary>
        private TextureIndexMvk _textureIndexMvk;

        public RenderMvk(WindowMvk window) : base(window) => _windowMvk = window;

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
        /// <summary>
        /// Запустить текстуру Heads-Up Display
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void BindTextureHud() => Texture.BindTexture(_textureIndexMvk.Hud);
        /// <summary>
        /// Запустить текстуру Inventory
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void BindTextureInventory() => Texture.BindTexture(_textureIndexMvk.Inventory);
        /// <summary>
        /// Запустить текстуру ConteinerStorage
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void BindTextureConteinerStorage() => Texture.BindTexture(_textureIndexMvk.ConteinerStorage);

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
            if (buffereds.ContainsKey(EnumTextureMvk.Hud.ToString()))
            {
                _textureIndexMvk.Hud = Texture.SetTexture(buffereds[EnumTextureMvk.Hud.ToString()]);
            }
            if (buffereds.ContainsKey(EnumTextureMvk.Inventory.ToString()))
            {
                _textureIndexMvk.Inventory = Texture.SetTexture(buffereds[EnumTextureMvk.Inventory.ToString()]);
            }
            if (buffereds.ContainsKey(EnumTextureMvk.ConteinerStorage.ToString()))
            {
                _textureIndexMvk.ConteinerStorage = Texture.SetTexture(buffereds[EnumTextureMvk.ConteinerStorage.ToString()]);
            }
        }
    }
}
