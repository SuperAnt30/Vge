using System.Runtime.CompilerServices;
using Vge.World.Block;
using WinGL.Util;

namespace Vge.Util
{
    /// <summary>
    /// Выровненные по осям ограничивающие рамки
    /// Axis-aligned bounding boxes 
    /// </summary>
    public struct AxisAlignedBB
    {
        /// <summary>
        /// Погрешность
        /// </summary>
        private const float _fault = 0.001f;

        public Vector3 Min { get; private set; }
        public Vector3 Max { get; private set; }

        public AxisAlignedBB(Vector3 from, Vector3 to)
        {
            Min = new Vector3(Mth.Min(from.X, to.X), Mth.Min(from.Y, to.Y), Mth.Min(from.Z, to.Z));
            Max = new Vector3(Mth.Max(from.X, to.X), Mth.Max(from.Y, to.Y), Mth.Max(from.Z, to.Z));
        }

        public AxisAlignedBB(float fromX, float fromY, float fromZ, float toX, float toY, float toZ)
        {
            Min = new Vector3(Mth.Min(fromX, toX), Mth.Min(fromY, toY), Mth.Min(fromZ, toZ));
            Max = new Vector3(Mth.Max(fromX, toX), Mth.Max(fromY, toY), Mth.Max(fromZ, toZ));
        }
        
        public Vector3i MinInt() => new Vector3i(Min);
        public Vector3i MaxInt() => new Vector3i(Mth.Ceiling(Max.X), Mth.Ceiling(Max.Y), Mth.Ceiling(Max.Z));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AxisAlignedBB Clone() => new AxisAlignedBB(Min, Max);
        
        /// <summary>
        /// Смещает текущую ограничивающую рамку на указанные координаты
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AxisAlignedBB Offset(float x, float y, float z) 
            => new AxisAlignedBB(Min.X + x, Min.Y + y, Min.Z + z, Max.X + x, Max.Y + y, Max.Z + z);

        /// <summary>
        /// Смещает текущую ограничивающую рамку на указанные координаты
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AxisAlignedBB Offset(Vector3 bias) => new AxisAlignedBB(Min + bias, Max + bias);

        /// <summary>
        /// Добавить координату в область как смещение от 0
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AxisAlignedBB AddCoordBias(float x, float y, float z)
        {
            Vector3 min = new Vector3(Min);
            Vector3 max = new Vector3(Max);
            if (x < 0f) min.X += x; else if (x > 0f) max.X += x;
            if (y < 0f) min.Y += y; else if (y > 0f) max.Y += y;
            if (z < 0f) min.Z += z; else if (z > 0f) max.Z += z;
            return new AxisAlignedBB(min, max);
        }
        /// <summary>
        /// Добавить вектор смещения
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AxisAlignedBB AddCoordBias(Vector3 pos)
        {
            Vector3 min = new Vector3(Min);
            Vector3 max = new Vector3(Max);
            if (pos.X < 0f) min.X += pos.X; else if (pos.X > 0f) max.X += pos.X;
            if (pos.Y < 0f) min.Y += pos.Y; else if (pos.Y > 0f) max.Y += pos.Y;
            if (pos.Z < 0f) min.Z += pos.Z; else if (pos.Z > 0f) max.Z += pos.Z;
            return new AxisAlignedBB(min, max);
        }

        /// <summary>
        /// Добавить координату в область
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AxisAlignedBB AddCoord(Vector3 pos)
        {
            Vector3 min = new Vector3(Min);
            Vector3 max = new Vector3(Max);
            if (pos.X < min.X) min.X = pos.X; else if (pos.X > max.X) max.X = pos.X;
            if (pos.Y < min.Y) min.Y = pos.Y; else if (pos.Y > max.Y) max.Y = pos.Y;
            if (pos.Z < min.Z) min.Z = pos.Z; else if (pos.Z > max.Z) max.Z = pos.Z;
            return new AxisAlignedBB(min, max);
        }

        /// <summary>
        /// Использовать низ, Y должен быть только отрицательный
        /// +-----+
        /// |     | Это рамка была
        /// +-----+
        /// |     | Эта рамка будет
        /// +-----+
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AxisAlignedBB Down(float y)
            => new AxisAlignedBB(Min.X, Min.Y + y, Min.Z, Max.X, Min.Y, Max.Z);

        /// <summary>
        /// Использовать вверх, Y должен быть только положительный
        /// +-----+
        /// |     | Эта рамка будет
        /// +-----+
        /// |     | Это рамка была
        /// +-----+
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AxisAlignedBB Up(float y)
            => new AxisAlignedBB(Min.X, Max.Y, Min.Z, Max.X, Max.Y + y, Max.Z);

        /// <summary>
        /// Возвращает ограничивающую рамку, расширенную указанным вектором
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AxisAlignedBB Expand(Vector3 vec) => new AxisAlignedBB(Min - vec, Max + vec);
        /// <summary>
        /// Возвращает ограничивающую рамку, расширенную указанным вектором
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AxisAlignedBB Expand(float vec) => new AxisAlignedBB(Min - vec, Max + vec);
        /// <summary>
        /// Возвращает ограничивающую рамку, расширенную указанным вектором
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AxisAlignedBB Expand(float x, float y, float z) 
            => new AxisAlignedBB(Min.X - x, Min.Y - y, Min.Z - z, Max.X + x, Max.Y + y, Max.Z + z);

        /// <summary>
        /// Возвращает ограничивающую рамку, уменьшенную указанным вектором
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AxisAlignedBB Contract(Vector3 vec) => new AxisAlignedBB(Min + vec, Max - vec);

        /// <summary>
        /// Возвращает ограничивающую рамку, пересечения
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AxisAlignedBB Intersection(AxisAlignedBB aabb)
            => new AxisAlignedBB(Mth.Max(Min.X, aabb.Min.X), Mth.Max(Min.Y, aabb.Min.Y), Mth.Max(Min.Z, aabb.Min.Z),
                Mth.Min(Max.X, aabb.Max.X), Mth.Min(Max.Y, aabb.Max.Y), Mth.Min(Max.Z, aabb.Max.Z));

        /// <summary>
        /// Вернуть размер коробки
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3 GetSize() => Max - Min;

        /// <summary>
        /// Если экземпляр и ограничивающие рамки аргумента перекрываются в измерениях Y и Z, 
        /// вычислите смещение между ними в измерении X. вернуть offset, если ограничивающие 
        /// рамки не перекрываются или если offset ближе к 0, чем вычисленное смещение. 
        /// В противном случае вернуть рассчитанное смещение.
        /// </summary>
        /// <param name="other"></param>
        /// <param name="offset">смещение</param>
        /// <returns></returns>
        public float CalculateXOffset(AxisAlignedBB other, float offset)
        {
            if (other.Max.Y > Min.Y && other.Min.Y < Max.Y && other.Max.Z > Min.Z && other.Min.Z < Max.Z)
            {
                if (offset > 0f && other.Max.X <= Min.X)
                {
                    float bias = Min.X - other.Max.X;
                    if (bias < offset) offset = bias - _fault;
                }
                else if (offset < 0f && other.Min.X >= Max.X)
                {
                    float bias = Max.X - other.Min.X;
                    if (bias > offset) offset = bias + _fault;
                }
            }
            return offset;
        }

        /// <summary>
        /// Если экземпляр и ограничивающие рамки аргумента перекрываются в измерениях X и Z, 
        /// вычислите смещение между ними в измерении Y. вернуть offset, если ограничивающие 
        /// рамки не перекрываются или если offset ближе к 0, чем вычисленное смещение. 
        /// В противном случае вернуть рассчитанное смещение.
        /// </summary>
        /// <param name="other"></param>
        /// <param name="offset">смещение</param>
        /// <returns></returns>
        public float CalculateYOffset(AxisAlignedBB other, float offset)
        {
            if (other.Max.X > Min.X && other.Min.X < Max.X && other.Max.Z > Min.Z && other.Min.Z < Max.Z)
            {
                if (offset > 0f && other.Max.Y <= Min.Y)
                {
                    float bias = Min.Y - other.Max.Y;
                    if (bias < offset) offset = bias - _fault;
                }
                else if (offset < 0f && other.Min.Y >= Max.Y)
                {
                    float bias = Max.Y - other.Min.Y;
                    if (bias > offset) offset = bias + _fault;
                }
            }
            return offset;
        }

        /// <summary>
        /// Если экземпляр и ограничивающие рамки аргумента перекрываются в измерениях Y и X, 
        /// вычислите смещение между ними в измерении Z. вернуть offset, если ограничивающие 
        /// рамки не перекрываются или если offset ближе к 0, чем вычисленное смещение. 
        /// В противном случае вернуть рассчитанное смещение.
        /// </summary>
        /// <param name="other"></param>
        /// <param name="offset">смещение</param>
        /// <returns></returns>
        public float CalculateZOffset(AxisAlignedBB other, float offset)
        {
            if (other.Max.X > Min.X && other.Min.X < Max.X && other.Max.Y > Min.Y && other.Min.Y < Max.Y)
            {
                if (offset > 0f && other.Max.Z <= Min.Z)
                {
                    float bias = Min.Z - other.Max.Z;
                    if (bias < offset) offset = bias - _fault;
                }
                else if (offset < 0f && other.Min.Z >= Max.Z)
                {
                    float bias = Max.Z - other.Min.Z;
                    if (bias > offset) offset = bias + _fault;
                }
            }
            return offset;
        }

        /// <summary>
        /// Рассчитать точку пересечения в AABB и отрезка, в виде вектора от pos1 до pos2
        /// </summary>
        public PointIntersection CalculateIntercept(Vector3 pos1, Vector3 pos2)
        {
            Vector3 posX1 = _GetIntermediateWithXValue(pos1, pos2, Min.X);
            Vector3 posX2 = _GetIntermediateWithXValue(pos1, pos2, Max.X);
            Vector3 posY1 = _GetIntermediateWithYValue(pos1, pos2, Min.Y);
            Vector3 posY2 = _GetIntermediateWithYValue(pos1, pos2, Max.Y);
            Vector3 posZ1 = _GetIntermediateWithZValue(pos1, pos2, Min.Z);
            Vector3 posZ2 = _GetIntermediateWithZValue(pos1, pos2, Max.Z);

            if (!_IsVecInYZ(posX1)) posX1 = new Vector3(0);
            if (!_IsVecInYZ(posX2)) posX2 = new Vector3(0);
            if (!_IsVecInXZ(posY1)) posY1 = new Vector3(0);
            if (!_IsVecInXZ(posY2)) posY2 = new Vector3(0);
            if (!_IsVecInXY(posZ1)) posZ1 = new Vector3(0);
            if (!_IsVecInXY(posZ2)) posZ2 = new Vector3(0);

            Vector3 vecResult = posX1;

            if (!posX2.IsZero() && (vecResult.IsZero() 
                || Glm.SquareDistanceTo(pos1, posX2) < Glm.SquareDistanceTo(pos1, vecResult)))
            {
                vecResult = posX2;
            }

            if (!posY1.IsZero() && (vecResult.IsZero() 
                || Glm.SquareDistanceTo(pos1, posY1) < Glm.SquareDistanceTo(pos1, vecResult)))
            {
                vecResult = posY1;
            }

            if (!posY2.IsZero() && (vecResult.IsZero() 
                || Glm.SquareDistanceTo(pos1, posY2) < Glm.SquareDistanceTo(pos1, vecResult)))
            {
                vecResult = posY2;
            }

            if (!posZ1.IsZero() && (vecResult.IsZero() 
                || Glm.SquareDistanceTo(pos1, posZ1) < Glm.SquareDistanceTo(pos1, vecResult)))
            {
                vecResult = posZ1;
            }

            if (!posZ2.IsZero() && (vecResult.IsZero() 
                || Glm.SquareDistanceTo(pos1, posZ2) < Glm.SquareDistanceTo(pos1, vecResult)))
            {
                vecResult = posZ2;
            }

            if (vecResult.IsZero()) return new PointIntersection();

            Pole side;
            if (vecResult == posX1) side = Pole.West;
            else if (vecResult == posX2) side = Pole.East;
            else if (vecResult == posY1) side = Pole.Down;
            else if (vecResult == posY2) side = Pole.Up;
            else if (vecResult == posZ1) side = Pole.North;
            else side = Pole.South;

            return new PointIntersection(vecResult, side);
        }

        /// <summary>
        /// Проверяет, находится ли указанный вектор в пределах размеров YZ ограничивающей рамки
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool _IsVecInYZ(Vector3 vec)
            => !vec.IsZero() && vec.Y >= Min.Y && vec.Y <= Max.Y && vec.Z >= Min.Z && vec.Z <= Max.Z;

        /// <summary>
        /// Проверяет, находится ли указанный вектор в пределах размеров XZ ограничивающей рамки
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool _IsVecInXZ(Vector3 vec)
            => !vec.IsZero() && vec.X >= Min.X && vec.X <= Max.X && vec.Z >= Min.Z && vec.Z <= Max.Z;

        /// <summary>
        /// Проверяет, находится ли указанный вектор в пределах размеров XY ограничивающей рамки
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool _IsVecInXY(Vector3 vec)
            => !vec.IsZero() && vec.X >= Min.X && vec.X <= Max.X && vec.Y >= Min.Y && vec.Y <= Max.Y;

        /// <summary>
        /// Возвращает, пересекается ли данная ограничивающая рамка с этой Рамкой
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IntersectsWith(AxisAlignedBB other)
            => other.Max.X > Min.X && other.Min.X < Max.X
                && other.Max.Y > Min.Y && other.Min.Y < Max.Y
                && other.Max.Z > Min.Z && other.Min.Z < Max.Z;

        /// <summary>
        /// Возвращает, пересекается ли данная ограничивающая рамка с блоком
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IntersectsWith(BlockPos blockPos) 
            => blockPos.X + 1 > Min.X && blockPos.X < Max.X
                && blockPos.Y + 1 > Min.Y && blockPos.Y < Max.Y 
                && blockPos.Z + 1 > Min.Z && blockPos.Z < Max.Z;

        /// <summary>
        /// Возвращает, пересекается ли данная ограничивающая рамка с блоком
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IntersectsWith(int x, int y, int z) 
            => x + 1 > Min.X && x < Max.X && y + 1 > Min.Y && y < Max.Y && z + 1 > Min.Z && z < Max.Z;

        /// <summary>
        /// Возвращает, если предоставленный точка полностью находится внутри ограничивающей рамки.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsPointInside(float x, float y, float z) 
            => x > Min.X && x < Max.X && y > Min.Y && y < Max.Y && z > Min.Z && z < Max.Z;

        /// <summary>
        /// Возвращает, если предоставленный Vector3 полностью находится внутри ограничивающей рамки.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsVecInside(Vector3 vec) 
            => vec.X > Min.X && vec.X < Max.X && vec.Y > Min.Y && vec.Y < Max.Y && vec.Z > Min.Z && vec.Z < Max.Z;

        /// <summary>
        /// Возвращает среднюю длину краев ограничивающей рамки
        /// </summary>
        public float GetAverageEdgeLength()
        {
            float x = Max.X - Min.X;
            float y = Max.Y - Min.Y;
            float z = Max.Z - Min.Z;
            return (x + y + z) / 3f;
        }

        #region Для коллизии Gjk или Sat

        /// <summary>
        /// Получить массив XZ
        ///   1 +-----+ 2
        ///    /     /
        /// 0 +-----+ 3
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2[] ToVector2Array()
            => new Vector2[]
            {
                new Vector2(Min.X, Min.Z),
                new Vector2(Min.X, Max.Z),
                new Vector2(Max.X, Max.Z),
                new Vector2(Max.X, Min.Z)
            };

        /// <summary>
        /// Получить массив XZ
        ///   5 +-----+ 6
        ///    /     /|
        /// 4 +-----+7+ 2
        ///   |     |/
        /// 0 +-----+ 3
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3[] ToVector3Array()
            => new Vector3[]
            {
                new Vector3(Min),
                new Vector3(Min.X, Min.Y, Max.Z),
                new Vector3(Max.X, Min.Y, Max.Z),
                new Vector3(Max.X, Min.Y, Min.Z),
                new Vector3(Min.X, Max.Y, Min.Z),
                new Vector3(Min.X, Max.Y, Max.Z),
                new Vector3(Max),
                new Vector3(Max.X, Max.Y, Min.Z),
            };

        #endregion

        #region IntermediateWith

        /// <summary>
        /// Возвращает новый вектор X со значением value, равным второму параметру, 
        /// вдоль линии между этим вектором и переданным вектором или vec(0), если это невозможно.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Vector3 _GetIntermediateWithXValue(Vector3 vec1, Vector3 vec2, float valueX)
        {
            float vx = vec2.X - vec1.X;
            if ((vx * vx) < 1E-7f) return new Vector3(0);
            float vy = vec2.Y - vec1.Y;
            float vz = vec2.Z - vec1.Z;
            float k = (valueX - vec1.X) / vx;
            return k >= 0f && k <= 1f ? new Vector3(vec1.X + vx * k, vec1.Y + vy * k, vec1.Z + vz * k) : new Vector3(0);
        }

        /// <summary>
        /// Возвращает новый вектор Y со значением value, равным второму параметру, 
        /// вдоль линии между этим вектором и переданным вектором или vec(0), если это невозможно.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Vector3 _GetIntermediateWithYValue(Vector3 vec1, Vector3 vec2, float valueY)
        {
            float vy = vec2.Y - vec1.Y;
            if ((vy * vy) < 1E-7f) return new Vector3(0);
            float vx = vec2.X - vec1.X;
            float vz = vec2.Z - vec1.Z;
            float k = (valueY - vec1.Y) / vy;
            return k >= 0f && k <= 1f ? new Vector3(vec1.X + vx * k, vec1.Y + vy * k, vec1.Z + vz * k) : new Vector3(0);
        }

        /// <summary>
        /// Возвращает новый вектор Z со значением value, равным второму параметру, 
        /// вдоль линии между этим вектором и переданным вектором или vec(0), если это невозможно.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Vector3 _GetIntermediateWithZValue(Vector3 vec1, Vector3 vec2, float valueZ)
        {
            float vz = vec2.Z - vec1.Z;
            if ((vz * vz) < 1E-7f) return new Vector3(0);
            float vx = vec2.X - vec1.X;
            float vy = vec2.Y - vec1.Y;
            float k = (valueZ - vec1.Z) / vz;
            return k >= 0f && k <= 1f ? new Vector3(vec1.X + vx * k, vec1.Y + vy * k, vec1.Z + vz * k) : new Vector3(0);
        }

        #endregion

        public override string ToString() 
            => "box[" + Min.ToString() + " -> " + Max.ToString() + "]";
    }
}
