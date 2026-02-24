using System.IO;
using Vge.Util;
using WinGL.Util;

namespace Vge.World.Block
{
    /// <summary>
    /// Процесс разрушения блока
    /// </summary>
    public class DestroyBlockProgress
    {
        /// <summary>
        /// Вершины всех шагов разрушения
        /// </summary>
        private Vertex3d[][] _vertices;

        /// <summary>
        /// Регистрация вставляется в разделе регистрации блоков, для вставки в тикстурный атлас
        /// </summary>
        public void Registration(int count, string pathName)
        {
            _vertices = new Vertex3d[count][];
            QuadSide quads;
            for (int i = 0; i < count; i++)
            {
                quads = new QuadSide();
                quads.SetTexture(BlocksReg.BlockItemAtlas.AddSprite(pathName + i).Index, 
                    0, 0, 32, 32);
                _vertices[i] = quads.Vertex;
            }
        }

        /// <summary>
        /// Смена позиций всего квада, для разрушенния нужен
        /// </summary>
        public Vertex3d[] GetVertices(int progress, Vertex3d[] vertices)
        {
            Vertex3d[] ds = _vertices[progress - 1];
            ds[0].SetPosition(vertices[0]);
            ds[1].SetPosition(vertices[1]);
            ds[2].SetPosition(vertices[2]);
            ds[3].SetPosition(vertices[3]);
            return ds;
        }
    }
}
