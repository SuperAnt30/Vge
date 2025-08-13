using System.Collections.Generic;
using Vge.Util;
using WinGL.Util;

namespace Vge.Entity.Model
{
    /// <summary>
    /// Сторона куба части моба
    /// </summary>
    public class ModelFace
    {
        public readonly float FromU;
        public readonly float FromV;

        public readonly float ToU;
        public readonly float ToV;

        /// <summary>
        /// Сторона
        /// </summary>
        public readonly Pole Side;
        /// <summary>
        /// Четыре вершины
        /// </summary>
        public readonly Vertex3d[] Vertex = new Vertex3d[4];

        /// <summary>
        /// Пустой ли
        /// </summary>
        private readonly bool _empty;

        public ModelFace(Pole side, int[] uv, bool empty)
        {
            Side = side;
            FromU = uv[0];
            FromV = uv[1];
            ToU = uv[2];
            ToV = uv[3];

            _empty = empty;
        }

        /// <summary>
        /// Сгенерировать буффер
        /// </summary>
        public void GenBuffer(List<float> bufferFloat, List<int> bufferInt, ModelCube modelCube)
        {
            if (!_empty)
            {
                // Устанавливаем вершины согласно стороны
                float x1 = modelCube.FromX / 16f;
                float y1 = modelCube.FromY / 16f;
                float z1 = modelCube.FromZ / 16f;
                float x2 = modelCube.ToX / 16f;
                float y2 = modelCube.ToY / 16f;
                float z2 = modelCube.ToZ / 16f;

                switch (Side)
                {
                    case Pole.Up:
                        Vertex[0].X = Vertex[1].X = x2;
                        Vertex[2].X = Vertex[3].X = x1;
                        Vertex[0].Y = Vertex[1].Y = Vertex[2].Y = Vertex[3].Y = y2;
                        Vertex[0].Z = Vertex[3].Z = z2;
                        Vertex[1].Z = Vertex[2].Z = z1;
                        break;
                    case Pole.Down:
                        Vertex[0].X = Vertex[1].X = x2;
                        Vertex[2].X = Vertex[3].X = x1;
                        Vertex[0].Y = Vertex[1].Y = Vertex[2].Y = Vertex[3].Y = y1;
                        Vertex[0].Z = Vertex[3].Z = z1;
                        Vertex[1].Z = Vertex[2].Z = z2;
                        break;
                    case Pole.East:
                        Vertex[0].X = Vertex[1].X = Vertex[2].X = Vertex[3].X = x2;
                        Vertex[0].Y = Vertex[3].Y = y1;
                        Vertex[1].Y = Vertex[2].Y = y2;
                        Vertex[0].Z = Vertex[1].Z = z1;
                        Vertex[2].Z = Vertex[3].Z = z2;
                        break;
                    case Pole.West:
                        Vertex[0].X = Vertex[1].X = Vertex[2].X = Vertex[3].X = x1;
                        Vertex[0].Y = Vertex[3].Y = y1;
                        Vertex[1].Y = Vertex[2].Y = y2;
                        Vertex[0].Z = Vertex[1].Z = z2;
                        Vertex[2].Z = Vertex[3].Z = z1;
                        break;
                    case Pole.North:
                        Vertex[0].X = Vertex[1].X = x1;
                        Vertex[2].X = Vertex[3].X = x2;
                        Vertex[0].Y = Vertex[3].Y = y1;
                        Vertex[1].Y = Vertex[2].Y = y2;
                        Vertex[0].Z = Vertex[1].Z = Vertex[2].Z = Vertex[3].Z = z1;
                        break;
                    case Pole.South:
                        Vertex[0].X = Vertex[1].X = x2;
                        Vertex[2].X = Vertex[3].X = x1;
                        Vertex[0].Y = Vertex[3].Y = y1;
                        Vertex[1].Y = Vertex[2].Y = y2;
                        Vertex[0].Z = Vertex[1].Z = Vertex[2].Z = Vertex[3].Z = z2;
                        break;
                }

                // Вращение если имеется
                if (modelCube.RotationX != 0 || modelCube.RotationY != 0 || modelCube.RotationZ != 0)
                {
                    _Rotate(modelCube.RotationY, modelCube.RotationX, modelCube.RotationZ,
                        modelCube.OriginX / 16f, modelCube.OriginY / 16f, modelCube.OriginZ / 16f);
                }

                // Устанавливаем текстурные координаты
                float u1 = FromU / modelCube.Width;
                float u2 = ToU / modelCube.Width;
                float v1 = FromV / modelCube.Height;
                float v2 = ToV / modelCube.Height;

                Vertex[0].U = Vertex[1].U = u2;
                Vertex[2].U = Vertex[3].U = u1;
                Vertex[0].V = Vertex[3].V = v2;
                Vertex[1].V = Vertex[2].V = v1;

                // Формируем буфер
                int param = modelCube.EyeMouth << 8 | modelCube.BoneIndex;

                // Нахождение нормали
                Vector3 normal = Vertex[0].ToPosition();
                Vector3 vec1 = Vertex[2].ToPosition() - normal;
                Vector3 vec2 = Vertex[1].ToPosition() - normal;
                normal = Glm.Cross(vec2.Normalize(), vec1.Normalize()).Normalize();

                for (int i = 0; i < 4; i++)
                {
                    bufferFloat.AddRange(new float[] {
                        Vertex[i].X,  Vertex[i].Y,  Vertex[i].Z,
                        normal.X, normal.Y, normal.Z,
                        Vertex[i].U, Vertex[i].V
                    });
                    bufferInt.AddRange(new int[] { param, -1 });
                }
            }
        }

        /// <summary>
        /// Применить вращение в градусах по центру блока
        /// </summary>
        private void _Rotate(float yaw, float pitch, float roll,
            float originX, float originY, float originZ)
        {
            Vector3 vec;
            for (int i = 0; i < 4; i++)
            {
                vec = Vertex[i].ToPosition();
                vec.X -= originX;
                vec.Y -= originY;
                vec.Z -= originZ;
                // Так правильно по Blockbench
                if (pitch != 0) vec = Glm.Rotate(vec, Glm.Radians(pitch), new Vector3(1, 0, 0));
                if (yaw != 0) vec = Glm.Rotate(vec, Glm.Radians(yaw), new Vector3(0, 1, 0));
                if (roll != 0) vec = Glm.Rotate(vec, Glm.Radians(roll), new Vector3(0, 0, 1));
                Vertex[i].X = Mth.Round(vec.X + originX, 3);
                Vertex[i].Y = Mth.Round(vec.Y + originY, 3);
                Vertex[i].Z = Mth.Round(vec.Z + originZ, 3);
            }
        }

        public override string ToString()
        {
            if (_empty) return Side.ToString() + " Empty";
            return Side.ToString() + " " + FromU + " " + FromV + " " + ToU + " " + ToV;
        }
    }
}
