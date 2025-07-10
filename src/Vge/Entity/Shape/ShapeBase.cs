using System.Runtime.CompilerServices;
using Vge.Entity.Model;
using Vge.Json;
using Vge.Renderer;
using Vge.Util;

namespace Vge.Entity.Shape
{
    public abstract class ShapeBase
    {
        /// <summary>
        /// Индекс в глобальном массиве
        /// </summary>
        public readonly ushort Index;
        /// <summary>
        /// Текстуры для моба
        /// </summary>
        public BufferedImage[] Textures { get; private set; }
        /// <summary>
        /// Индекс глубины текстуры для моба
        /// </summary>
        public readonly int[] DepthTextures;
        /// <summary>
        /// Минимальная текстура
        /// </summary>
        public bool TextureSmall { get; private set; } = true;

        /// <summary>
        /// Объект отвечает за определяение модели сущности
        /// </summary>
        protected ModelEntityDefinition _definition;

        public ShapeBase(ushort index, string alias, JsonCompound jsonModel)
        {
            Index = index;
            _CreateDefinition(alias);
            _definition.RunModelFromJson(jsonModel);
            Textures = _definition.GenTextures();
            DepthTextures = new int[Textures.Length];
        }

        /// <summary>
        /// Создаём объект отвечает за определяение модели сущности
        /// </summary>
        protected virtual void _CreateDefinition(string alias)
            => _definition = new ModelEntityDefinition(alias);

        /// <summary>
        /// Очистить объект отвечает за определяение модели сущности
        /// </summary>
        public void ClearDefinition() => _definition = null;

        /// <summary>
        /// Внести в текстуры в память OpenGL в массив текстур.
        /// </summary>
        public void SetImageTexture2dArray(TextureMap textureMap, uint idTextureSmall, uint idTextureBig)
        {
            for (int t = 0; t < Textures.Length; t++)
            {
                textureMap.SetImageTexture2dArray(Textures[t],
                    DepthTextures[t],
                    TextureSmall ? idTextureSmall : idTextureBig,
                    (uint)(TextureSmall ? Gi.ActiveTextureSamplerSmall : Gi.ActiveTextureSamplerBig));
            }
            Textures = null;
        }

        /// <summary>
        /// Пометить модель в максимальную группу текстур
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TextureGroupBig() => TextureSmall = false;

        /// <summary>
        /// Корректировка размера ширины текстуры, в буффере UV
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void SizeAdjustmentTextureWidth(float coef) { }

        /// <summary>
        /// Корректировка размера высоты текстуры, в буффере UV
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void SizeAdjustmentTextureHeight(float coef) { }

        public override string ToString() => Index + " " + (TextureSmall ? "Small" : "Big");
    }
}
