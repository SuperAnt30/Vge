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
        public int ChunkPositionX => (int)X >> 4;
        /// <summary>
        /// Координата Z в каком чанке находится
        /// </summary>
        public int ChunkPositionZ => (int)Z >> 4;
        /// <summary>
        /// Получить координаты чанка
        /// </summary>
        public Vector2i GetChunkPosition() => new Vector2i(ChunkPositionX, ChunkPositionZ);

        /// <summary>
        /// Разные ли 
        /// </summary>
        public bool IsChange(EntityPos entityPos)
        {
            return X != entityPos.X || Y != entityPos.Y || Z != entityPos.Z || Yaw != entityPos.Yaw
                || Pitch != entityPos.Pitch;
        }

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

        public override string ToString()
            => string.Format("{0:0.00}; {1:0.00}; {2:0.00} Y:{3:0.00} P:{4:0.00}",
                X, Y, Z, Glm.Degrees(Yaw), Glm.Degrees(Pitch));
    }
}
