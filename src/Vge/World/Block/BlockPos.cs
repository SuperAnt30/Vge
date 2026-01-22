using System;
using System.Runtime.CompilerServices;
using Vge.Network;
using Vge.Util;
using Vge.World.Chunk;
using WinGL.Util;

namespace Vge.World.Block
{
    /// <summary>
    /// Позиция блока
    /// </summary>
    public struct BlockPos
    {
        /// <summary>
        /// Статический массив направлений векторов
        /// </summary>
        public static readonly Vector3i[] DirectionVectors = new Vector3i[]
        {
            new Vector3i(0, 1, 0), new Vector3i(0, -1, 0),
            new Vector3i(1, 0, 0), new Vector3i(-1, 0, 0),
            new Vector3i(0, 0, -1), new Vector3i(0, 0, 1)
        };

        /// <summary>
        /// Статический массив направлений векторов реверс
        /// </summary>
        public static readonly Vector3i[] DirectionReversalVectors = new Vector3i[]
        {
            new Vector3i(0, -1, 0), new Vector3i(0, 1, 0),
            new Vector3i(-1, 0, 0), new Vector3i(1, 0, 0),
            new Vector3i(0, 0, 1), new Vector3i(0, 0, -1)
        };

        public int X;
        public int Y;
        public int Z;

        public BlockPos(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        public BlockPos(float x, float y, float z)
        {
            X = Mth.Floor(x);
            Y = Mth.Floor(y);
            Z = Mth.Floor(z);
        }
        public BlockPos(Vector3 v)
        {
            X = Mth.Floor(v.X);
            Y = Mth.Floor(v.Y);
            Z = Mth.Floor(v.Z);
        }
        public BlockPos(Vector3i v)
        {
            X = v.X;
            Y = v.Y;
            Z = v.Z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private BlockPos _Plus(Vector3i v) => new BlockPos(X + v.X, Y + v.Y, Z + v.Z);

        /// <summary>
        /// Позиция соседнего блока
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BlockPos Offset(int i)
        {
            try
            {
                return _Plus(DirectionVectors[i]);
            }
            catch
            {
                throw new Exception(Sr.GetString(Sr.ThereIsNoSuchSide, i));
            }
        }
        /// <summary>
        /// Позиция соседнего блока обратной стороны
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BlockPos OffsetReversal(int i)
        {
            try
            {
                return _Plus(DirectionReversalVectors[i]);
            }
            catch
            {
                throw new Exception(Sr.GetString(Sr.ThereIsNoSuchSide, i));
            }
        }
        /// <summary>
        /// Позиция соседнего блока
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BlockPos Offset(Pole pole)
        {
            try
            {
                return _Plus(DirectionVectors[(int)pole]);
            }
            catch
            {
                throw new Exception(Sr.GetString(Sr.ThereIsNoSuchSide, pole));
            }
        }
        /// <summary>
        /// Позиция соседнего блока
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BlockPos Offset(Vector3i vec) => new BlockPos(X + vec.X, Y + vec.Y, Z + vec.Z);
        /// <summary>
        /// Позиция соседнего блока
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BlockPos Offset(int x, int y, int z) => new BlockPos(X + x, Y + y, Z + z);
        /// <summary>
        /// Позиция блока снизу
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BlockPos OffsetDown() => new BlockPos(X, Y - 1, Z);
        /// <summary>
        /// Позиция блока сверху
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BlockPos OffsetUp() => new BlockPos(X, Y + 1, Z);
        /// <summary>
        /// Позиция блока сверху
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BlockPos OffsetUp(int i) => new BlockPos(X, Y + i, Z);
        /// <summary>
        /// Позиция блока восток
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BlockPos OffsetEast() => new BlockPos(X + 1, Y, Z);
        /// <summary>
        /// Позиция блока запад
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BlockPos OffsetWest() => new BlockPos(X - 1, Y, Z);
        /// <summary>
        /// Позиция блока юг
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BlockPos OffsetSouth() => new BlockPos(X, Y, Z + 1);
        /// <summary>
        /// Позиция блока север
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BlockPos OffsetNorth() => new BlockPos(X, Y, Z - 1);

        /// <summary>
        /// Позиция блока юга-запад
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BlockPos OffsetSouthWest() => new BlockPos(X - 1, Y, Z + 1);
        /// <summary>
        /// Позиция блока юга-восток
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BlockPos OffsetSouthEast() => new BlockPos(X + 1, Y, Z + 1);
        /// <summary>
        /// Позиция блока севера-запад
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BlockPos OffsetNorthWest() => new BlockPos(X - 1, Y, Z - 1);
        /// <summary>
        /// Позиция блока севера-восток
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BlockPos OffsetNorthEast() => new BlockPos(X + 1, Y, Z - 1);

        /// <summary>
        /// Создать вектор с целыми числами
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3i ToVector3i() => new Vector3i(X, Y, Z);
        /// <summary>
        /// Создать вектор
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3 ToVector3() => new Vector3(X, Y, Z);
        /// <summary>
        /// Создать вектор по центру блока
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3 ToVector3Center() => new Vector3(X + .5f, Y + .5f, Z + .5f);

        /// <summary>
        /// Получить позицию блока в чанке, 0..15 0..255 0..15
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3i GetPositionInChunk() => new Vector3i(X & 15, Y, Z & 15);

        /// <summary>
        /// Получить позицию чанка XZ
        /// </summary
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2i GetPositionChunk() => new Vector2i(X >> 4, Z >> 4);

        /// <summary>
        /// Получить позицию чанка X
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetPositionChunkX() => X >> 4;
        /// <summary>
        /// Получить позицию чанка Y
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetPositionChunkY() => Y >> 4;
        /// <summary>
        /// Получить позицию чанка Z
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetPositionChunkZ() => Z >> 4;

        /// <summary>
        /// Получить растояние между двумя точками но не возводя в квадратный корень, на скорости
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float DistanceNotSqrt(Vector3 pos)
        {
            float x = X - pos.X;
            float y = Y - pos.Y;
            float z = Z - pos.Z;
            return x * x + y * y + z * z;
        }

        /// <summary>
        /// Проверить локально позицию блока, 0..15
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool EqualsPositionInChunk(int x, int y, int z) 
            => (X & 15) == x && Y == y && (Z & 15) == z;

        /// <summary>
        /// Проверьте, имеет ли данный BlockPos действительные координаты
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsValid(ChunkSettings settings) 
            => X >= -30000000 && Z >= -30000000 && X < 30000000 && Z < 30000000 && Y >= 0 
            && Y <= settings.NumberMaxBlock;

        /// <summary>
        /// Получить массив всех позиция попадающих в облость
        /// </summary>
        public static BlockPos[] GetAllInBox(Vector3i from, Vector3i to)
        {
            Vector3i f = new Vector3i(Mth.Min(from.X, to.X), Mth.Min(from.Y, to.Y), Mth.Min(from.Z, to.Z));
            Vector3i t = new Vector3i(Mth.Max(from.X, to.X), Mth.Max(from.Y, to.Y), Mth.Max(from.Z, to.Z));

            BlockPos[] list = new BlockPos[(t.X - f.X + 1) * (t.Y - f.Y + 1) * (t.Z - f.Z + 1)];

            int i = 0;
            for (int x = f.X; x <= t.X; x++)
            {
                for (int y = f.Y; y <= t.Y; y++)
                {
                    for (int z = f.Z; z <= t.Z; z++)
                    {
                        list[i++] = new BlockPos(x, y, z);
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// Записать блок в буффер пакета
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteStream(WritePacket stream)
        {
            stream.Int(X);
            stream.Int(Y);
            stream.Int(Z);
        }

        /// <summary>
        /// Прочесть блок из буффер пакета и занести в чанк
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReadStream(ReadPacket stream)
        {
            X = stream.Int();
            Y = stream.Int();
            Z = stream.Int();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsZero() => X == 0 && Y == 0 && Z == 0;

        public override string ToString() => X + "; " + Y + "; " + Z;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(BlockPos))
            {
                var vec = (BlockPos)obj;
                if (X == vec.X && Y == vec.Y && Z == vec.Z) return true;
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() 
            => X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode();
    }
}
