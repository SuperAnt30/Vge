using Vge.Network;
using WinGL.Util;

namespace Vge.Entity
{
    public class EntityPos
    {
        public float X;
        public float Y;
        public float Z;

        public float Yaw;
        public float Pitch;

        /// <summary>
        /// Координату X в каком чанке находится
        /// </summary>
        public int ChunkPositionX => Mth.Floor(X) >> 4;
        /// <summary>
        /// Координата Z в каком чанке находится
        /// </summary>
        public int ChunkPositionZ => Mth.Floor(Z) >> 4;
        /// <summary>
        /// Координата Y
        /// </summary>
        public int PositionY => (int)Y;
        /// <summary>
        /// Получить координаты чанка
        /// </summary>
        public Vector2i GetChunkPosition() => new Vector2i(ChunkPositionX, ChunkPositionZ);
        /// <summary>
        /// Получить вектор 3
        /// </summary>
        public Vector3 GetVector3() => new Vector3(X, Y, Z);

        /// <summary>
        /// Разные ли 
        /// </summary>
        public bool IsChange(EntityPos entityPos)
        {
            return X != entityPos.X || Y != entityPos.Y || Z != entityPos.Z || Yaw != entityPos.Yaw
                || Pitch != entityPos.Pitch;
        }

        /// <summary>
        /// Разные ли по вращению
        /// </summary>
        public bool IsChangeRotate(EntityPos entityPos)
            => Yaw != entityPos.Yaw || Pitch != entityPos.Pitch;

        /// <summary>
        /// Задать значения
        /// </summary>
        public void Set(EntityPos entityPos)
        {
            X = entityPos.X;
            Y = entityPos.Y;
            Z = entityPos.Z;
            Yaw = entityPos.Yaw;
            Pitch = entityPos.Pitch;
        }

        /// <summary>
        /// Обновить значения в кадре, между тикущем и предыдущем
        /// </summary>
        public void UpdateFrame(float timeIndex, EntityPos entityPos, EntityPos entityPosPrev)
        {
            if (timeIndex >= 1f)
            {
                if (entityPos.X != entityPosPrev.X) X = entityPos.X;
                if (entityPos.Y != entityPosPrev.Y) Y = entityPos.Y;
                if (entityPos.Z != entityPosPrev.Z) Z = entityPos.Z;
                if (entityPos.Yaw != entityPosPrev.Yaw) Yaw = entityPos.Yaw;
                if (entityPos.Pitch != entityPosPrev.Pitch) Pitch = entityPos.Pitch;
            }
            else
            {
                X = entityPosPrev.X + (entityPos.X - entityPosPrev.X) * timeIndex;
                Y = entityPosPrev.Y + (entityPos.Y - entityPosPrev.Y) * timeIndex;
                Z = entityPosPrev.Z + (entityPos.Z - entityPosPrev.Z) * timeIndex;
                if (entityPos.Yaw - entityPosPrev.Yaw > Glm.Pi)
                {
                    Yaw = entityPosPrev.Yaw + ((entityPos.Yaw - Glm.Pi360) - entityPosPrev.Yaw) * timeIndex;
                }
                else if (entityPos.Yaw - entityPosPrev.Yaw < -Glm.Pi)
                {
                    Yaw = entityPosPrev.Yaw + ((entityPos.Yaw + Glm.Pi360) - entityPosPrev.Yaw) * timeIndex;
                }
                else
                {
                    Yaw = entityPosPrev.Yaw + (entityPos.Yaw - entityPosPrev.Yaw) * timeIndex;
                }
                Pitch = entityPosPrev.Pitch + (entityPos.Pitch - entityPosPrev.Pitch) * timeIndex;
            }
        }

        public void ReadPacket(ReadPacket stream)
        {
            X = stream.Float();
            Y = stream.Float();
            Z = stream.Float();
            Yaw = stream.Float();
            Pitch = stream.Float();
        }

        public void WritePacket(WritePacket stream)
        {
            stream.Float(X);
            stream.Float(Y);
            stream.Float(Z);
            stream.Float(Yaw);
            stream.Float(Pitch);
        }

        public string ToStringPos()
            => string.Format("{0:0.00}; {1:0.00}; {2:0.00}", X, Y, Z);

        public override string ToString()
            => string.Format("{0:0.00}; {1:0.00}; {2:0.00} Y:{3:0.00} P:{4:0.00}",
                X, Y, Z, Glm.Degrees(Yaw), Glm.Degrees(Pitch));
    }
}
