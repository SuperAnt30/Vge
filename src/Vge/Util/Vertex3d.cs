using WinGL.Util;

namespace Vge.Util
{
    /// <summary>
    /// Вершина имеет xyz и текстуру uv
    /// </summary>
    public struct Vertex3d
    {
        public float X;
        public float Y;
        public float Z;

        public float U;
        public float V;

        public Vertex3d(float x, float y, float z, float u, float v)
        {
            X = x;
            Y = y;
            Z = z;
            U = u;
            V = v;
        }

        public Vector3 ToPosition() => new Vector3(X, Y, Z);

        public Vertex3d Copy() => new Vertex3d(X, Y, Z, U, V);

        public void SetPosition(Vertex3d vertex)
        {
            X = vertex.X;
            Y = vertex.Y;
            Z = vertex.Z;
        }

        public override string ToString() 
            => string.Format("{0:0.00}; {1:0.00}; {2:0.00} u{3:0.00} v{4:0.00}]", X, Y, Z, U, V);
    }
}
