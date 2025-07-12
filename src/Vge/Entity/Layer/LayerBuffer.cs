using Vge.Entity.Render;

namespace Vge.Entity.Layer
{
    /// <summary>
    /// Объект конкретного слоя с буфером
    /// </summary>
    public class LayerBuffer
    {
        /// <summary>
        /// Название слоя
        /// </summary>
        public readonly string Name;
        /// <summary>
        /// Название папок
        /// </summary>
        public readonly string[] Folder;
        /// <summary>
        /// Название текстуры
        /// </summary>
        public readonly string Texture;

        /// <summary>
        /// порядковый номер текстур
        /// </summary>
        public ushort TextureId;
        /// <summary>
        /// Глубина текстуры для OpenGL
        /// </summary>
        //public int DepthTexture;

        /// <summary>
        /// Буфер сетки формы, для рендера
        /// </summary>
        public VertexEntityBuffer BufferMesh;

        public LayerBuffer(string name, string texture, string[] folder)
        {
            Name = name;
            Texture = texture;
            Folder = folder;
        }
    }
}
