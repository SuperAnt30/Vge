using System.Runtime.CompilerServices;
using WinGL.Util;

namespace Vge.Util
{
    /// <summary>
    /// Frustum Culling
    /// </summary>
    public class Frustum
    {
        private readonly float[] _frustum0 = new float[4];
        private readonly float[] _frustum1 = new float[4];
        private readonly float[] _frustum2 = new float[4];
        private readonly float[] _frustum3 = new float[4];
        private readonly float[] _frustum4 = new float[4];
        private readonly float[] _frustum5 = new float[4];

        /// <summary>
        /// Иницилизация данных
        /// </summary>
        /// <param name="view">матрица lookAt GL_MODELVIEW_MATRIX * projection GL_PROJECTION_MATRIX</param>
        public void Init(float[] view)
        {
            _Divide(_frustum0, view[3] - view[0], view[7] - view[4], view[11] - view[8], view[15] - view[12]);
            _Divide(_frustum1, view[3] + view[0], view[7] + view[4], view[11] + view[8], view[15] + view[12]);
            _Divide(_frustum2, view[3] + view[1], view[7] + view[5], view[11] + view[9], view[15] + view[13]);
            _Divide(_frustum3, view[3] - view[1], view[7] - view[5], view[11] - view[9], view[15] - view[13]);
            _Divide(_frustum4, view[3] - view[2], view[7] - view[6], view[11] - view[10], view[15] - view[14]);
            _Divide(_frustum5, view[3] + view[2], view[7] + view[6], view[11] + view[10], view[15] + view[14]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void _Divide(float[] frustum, float v0, float v1, float v2, float v3)
        {
            float f = Mth.Sqrt(v0 * v0 + v1 * v1 + v2 * v2);
            frustum[0] = v0 / f;
            frustum[1] = v1 / f;
            frustum[2] = v2 / f;
            frustum[3] = v3 / f;
        }


        public bool IsBoxInFrustum(float x1, float y1, float z1, float x2, float y2, float z2)
        {
            if (!_check(_frustum0, x1, y1, z1, x2, y2, z2)) return false;
            if (!_check(_frustum1, x1, y1, z1, x2, y2, z2)) return false;
            if (!_check(_frustum2, x1, y1, z1, x2, y2, z2)) return false;
            if (!_check(_frustum3, x1, y1, z1, x2, y2, z2)) return false;
            if (!_check(_frustum4, x1, y1, z1, x2, y2, z2)) return false;
            if (!_check(_frustum5, x1, y1, z1, x2, y2, z2)) return false;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool _check(float[] frustum, float x1, float y1, float z1, float x2, float y2, float z2)
        {
            if ((frustum[0] * x1 + frustum[1] * y1 + frustum[2] * z1 + frustum[3] <= 0)
                   && (frustum[0] * x2 + frustum[1] * y1 + frustum[2] * z1 + frustum[3] <= 0)
                   && (frustum[0] * x1 + frustum[1] * y2 + frustum[2] * z1 + frustum[3] <= 0)
                   && (frustum[0] * x2 + frustum[1] * y2 + frustum[2] * z1 + frustum[3] <= 0)
                   && (frustum[0] * x1 + frustum[1] * y1 + frustum[2] * z2 + frustum[3] <= 0)
                   && (frustum[0] * x2 + frustum[1] * y1 + frustum[2] * z2 + frustum[3] <= 0)
                   && (frustum[0] * x1 + frustum[1] * y2 + frustum[2] * z2 + frustum[3] <= 0)
                   && (frustum[0] * x2 + frustum[1] * y2 + frustum[2] * z2 + frustum[3] <= 0)
                   ) return false;
            return true;
        }

        /// <summary>
        /// Возвращает true, если прямоугольник находится внутри всех 6 плоскостей отсечения,
        /// в противном случае возвращает false.
        /// </summary>
        public bool IsBoxInFrustum(AxisAlignedBB aabb)
            => IsBoxInFrustum(aabb.Min.X, aabb.Min.Y, aabb.Min.Z, aabb.Max.X, aabb.Max.Y, aabb.Max.Z);
    }
}
