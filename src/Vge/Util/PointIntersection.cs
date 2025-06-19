using WinGL.Util;

namespace Vge.Util
{
    /// <summary>
    /// Точка пересечения
    /// </summary>
    public readonly struct PointIntersection
    {
        /// <summary>
        /// Координата куда попал луч в глобальных координатах по блоку
        /// </summary>
        public readonly Vector3 RayHit;
        /// <summary>
        /// Сторона блока куда смотрит луч, нельзя по умолчанию All, надо строго из 6 сторон
        /// </summary>
        public readonly Pole Side;
        /// <summary>
        /// Имеется ли пересечение
        /// </summary>
        public readonly bool Intersection;

        /// <summary>
        /// Точка пересечения, с какой стороны попали
        /// </summary>
        /// <param name="vec">точка попадания</param>
        /// <param name="side">с какой стороны попали</param>
        public PointIntersection(Vector3 vec, Pole side)
        {
            RayHit = vec;
            Side = side;
            Intersection = true;
        }

        public override string ToString() 
            => Intersection ? Side.ToString() + " " + RayHit.ToString() : "Null";
    }
}
