using System.Runtime.CompilerServices;
using Vge.Entity.Model;
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
        public BufferedImage[] Textures { get; protected set; }
        /// <summary>
        /// Индекс глубины текстуры для моба
        /// </summary>
        public int[] DepthTextures { get; protected set; }
        /// <summary>
        /// Минимальная текстура
        /// </summary>
        public bool TextureSmall { get; protected set; } = true;

        /// <summary>
        /// Объект отвечает за определяение модели сущности
        /// </summary>
        protected ModelEntityDefinition _definition;

        public ShapeBase(ushort index) => Index = index;

        /// <summary>
        /// Очистить объект отвечает за определяение модели сущности
        /// </summary>
        public void ClearDefinition() => _definition = null;

        /// <summary>
        /// Внести в текстуры в память OpenGL в массив текстур.
        /// </summary>
        public void SetImageTexture2dArray(TextureMap textureMap, uint idTextureSmall, uint idTextureBig)
        {
            for (int i = 0; i < Textures.Length; i++)
            {
                textureMap.SetImageTexture2dArray(Textures[i],
                    DepthTextures[i],
                    TextureSmall ? idTextureSmall : idTextureBig,
                    (uint)(TextureSmall ? Gi.ActiveTextureSamplerSmall : Gi.ActiveTextureSamplerBig));

                if (!TextureSmall)
                {
                    // Если текстура большая, делаем корректировку по данным для будушего шейдора
                    DepthTextures[i] += 65536;
                }
            }
            Textures = null;
        }

        /// <summary>
        /// Пометить модель в максимальную группу текстур
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void TextureGroupBig() => TextureSmall = false;

        /// <summary>
        /// Задать глубину в текстуру
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void SetDepthTextures(int index, int depth) 
            => DepthTextures[index] = depth;

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
