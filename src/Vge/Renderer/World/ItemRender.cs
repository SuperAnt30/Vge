using System;
using System.Runtime.CompilerServices;
using Vge.Entity.Render;
using Vge.Item;
using WinGL.OpenGL;

namespace Vge.Renderer.World
{
    /// <summary>
    /// Объект рисует предмет в Gui
    /// </summary>
    public class ItemRender : IDisposable
    {
        /// <summary>
        /// Сетка сущности
        /// </summary>
        private readonly MeshEntity _mesh;
        /// <summary>
        /// Объём
        /// </summary>
        public readonly bool Volume;

        public ItemRender(int id, GL gl, ItemRenderBuffer renderBuffer)
        {
            VertexEntityBuffer buffer = renderBuffer.GetBufferGui();
            if (buffer == null)
            {
                throw new Exception(Sr.GetString(Sr.BufferItemIsMissing, Ce.Items.ItemAlias[id]));
            }
            Volume = renderBuffer.Valume;
            _mesh = new MeshEntity(gl);
            _mesh.Reload(buffer);
        }

        /// <summary>
        /// Прорисовать сетку
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void MeshDraw() => _mesh.Draw();

        /// <summary>
        /// Выгрузить сетку с OpenGL
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() => _mesh.Dispose();
    }
}
