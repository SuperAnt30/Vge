using Vge.Util;
using WinGL.Util;

namespace Vge.World.Block
{
    /// <summary>
    /// Прямоугольная сторона блока или элемента блока
    /// </summary>
    public class QuadSide
    {
        /// <summary>
        /// Четыре вершины
        /// </summary>
        public readonly Vertex3d[] Vertex = new Vertex3d[4];
        /// <summary>
        /// С какой стороны
        /// </summary>
        public int Side;
        /// <summary>
        /// Боковое затемнение
        /// </summary>
        public float LightPole;
        /// <summary>
        /// Для анимации блока, указывается количество кадров в игровом времени (50 мс),
        /// можно кратно 2 в степени (2, 4, 8, 16, 32, 64...)
        /// 0 - нет анимации
        /// </summary>
        public byte AnimationFrame;
        /// <summary>
        /// Для анимации блока, указывается пауза между кадрами в игровом времени (50 мс),
        /// можно кратно 2 в степени (2, 4, 8, 16, 32, 64...)
        /// 0 или 1 - нет задержки, каждый такт игры смена кадра
        /// </summary>
        public byte AnimationPause;
        /// <summary>
        /// Флаг имеется ли ветер, для уникальных блоков: листвы, травы и подобного
        /// 0 - нет, 1 - вверхние точки, 2 - нижние
        /// </summary>
        public byte Wind;
        /// <summary>
        /// Резкость = 4; mipmap = 0;
        /// </summary>
        public byte Sharpness;

        /// <summary>
        /// Цвет биома, где 0 - нет цвета, 1 - трава, 2 - листа, 3 - вода, 4 - свой цвет
        /// </summary>
        public byte BiomeColor = 0;

        /// <summary>
        /// Прямоугольная сторона блока или элемента блока
        /// </summary>
        /// <param name = "biomeColor" >Цвет биома, где 0 - нет цвета, 1 - трава, 2 - листа, 3 - вода, 4 - свой цвет</param>
        public QuadSide(byte biomeColor = 0)
        {
            BiomeColor = biomeColor;
        }

        /// <summary>
        /// Задать анимацию
        /// </summary>
        /// <param name="frame">Количество кадров в игровом времени</param>
        /// <param name="pause">Пауза между кадрами в игровом времени</param>
        public QuadSide SetAnimal(byte frame, byte pause)
        {
            AnimationFrame = frame;
            AnimationPause = pause;
            return this;
        }

        /// <summary>
        /// Задаём сторону и размеры на стороне
        /// </summary>
        /// <param name="pole">индекс стороны</param>
        /// <param name="noSideDimming">shade Отсутствие оттенка / Нет бокового затемнения, пример: трава, цветы</param>
        public QuadSide SetSide(Pole pole, bool noSideDimming = false, int x1i = 0, int y1i = 0, int z1i = 0, int x2i = 16, int y2i = 16, int z2i = 16)
        {
            float x1 = x1i / 16f;
            float y1 = y1i / 16f;
            float z1 = z1i / 16f;
            float x2 = x2i / 16f;
            float y2 = y2i / 16f;
            float z2 = z2i / 16f;
            Side = (int)pole;
            LightPole = noSideDimming ? 0f : 1f - Gi.LightPoles[Side];
            switch (pole)
            {
                case Pole.Up:
                    Vertex[0].X = Vertex[1].X = x1;
                    Vertex[2].X = Vertex[3].X = x2;
                    Vertex[0].Y = Vertex[1].Y = Vertex[2].Y = Vertex[3].Y = y2;
                    Vertex[0].Z = Vertex[3].Z = z1;
                    Vertex[1].Z = Vertex[2].Z = z2;
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
            return this;
        }

        /// <summary>
        /// Задать текстуру, её можно повернуть кратно 90 гр. 0, 90, 180, 270 
        /// </summary>
        public QuadSide SetTexture(int numberTexture, int rotateYawUV = 0)
        {
            float u1 = (numberTexture % 64) * .015625f;
            float u2 = u1 + .015625f;
            float v1 = numberTexture / 64 * .015625f;
            float v2 = v1 + .015625f;
            switch (rotateYawUV)
            {
                case 0:
                    Vertex[0].U = Vertex[1].U = u2;
                    Vertex[2].U = Vertex[3].U = u1;
                    Vertex[0].V = Vertex[3].V = v2;
                    Vertex[1].V = Vertex[2].V = v1;
                    break;
                case 90:
                    Vertex[1].U = Vertex[2].U = u2;
                    Vertex[0].U = Vertex[3].U = u1;
                    Vertex[0].V = Vertex[1].V = v2;
                    Vertex[2].V = Vertex[3].V = v1;
                    break;
                case 180:
                    Vertex[0].U = Vertex[1].U = u1;
                    Vertex[2].U = Vertex[3].U = u2;
                    Vertex[0].V = Vertex[3].V = v1;
                    Vertex[1].V = Vertex[2].V = v2;
                    break;
                case 270:
                    Vertex[1].U = Vertex[2].U = u1;
                    Vertex[0].U = Vertex[3].U = u2;
                    Vertex[0].V = Vertex[1].V = v1;
                    Vertex[2].V = Vertex[3].V = v2;
                    break;
            }
            return this;
        }

        /// <summary>
        /// Задать текстуру, и задать ей размеры не полного сэмпла и можно повернуть кратно 90 гр. 0, 90, 180, 270 
        /// </summary>
        public QuadSide SetTexture(int numberTexture, int biasU1, int biasV1, int biasU2, int biasV2, int rotateYawUV = 0)
        {
            float u = (numberTexture % 64) * .015625f;
            float v = numberTexture / 64 * .015625f;

            switch (rotateYawUV)
            {
                case 0:
                    Vertex[0].U = Vertex[1].U = u + biasU2 / 1024f;
                    Vertex[2].U = Vertex[3].U = u + biasU1 / 1024f;
                    Vertex[0].V = Vertex[3].V = v + biasV2 / 1024f;
                    Vertex[1].V = Vertex[2].V = v + biasV1 / 1024f;
                    break;
                case 270:
                    Vertex[1].U = Vertex[2].U = u + biasU2 / 1024f;
                    Vertex[0].U = Vertex[3].U = u + biasU1 / 1024f;
                    Vertex[0].V = Vertex[1].V = v + biasV2 / 1024f;
                    Vertex[2].V = Vertex[3].V = v + biasV1 / 1024f;
                    break;
                case 180:
                    Vertex[0].U = Vertex[1].U = u + biasU1 / 1024f;
                    Vertex[2].U = Vertex[3].U = u + biasU2 / 1024f;
                    Vertex[0].V = Vertex[3].V = v + biasV1 / 1024f;
                    Vertex[1].V = Vertex[2].V = v + biasV2 / 1024f;
                    break;
                case 90:
                    Vertex[1].U = Vertex[2].U = u + biasU1 / 1024f;
                    Vertex[0].U = Vertex[3].U = u + biasU2 / 1024f;
                    Vertex[0].V = Vertex[1].V = v + biasV1 / 1024f;
                    Vertex[2].V = Vertex[3].V = v + biasV2 / 1024f;
                    break;
            }
            return this;
        }

        /// <summary>
        /// Задать смещение
        /// </summary>
        public QuadSide SetTranslate(float x, float y, float z)
        {
            for (int i = 0; i < 4; i++)
            {
                Vertex[i].X += x;
                Vertex[i].Y += y;
                Vertex[i].Z += z;
            }
            return this;
        }

        /// <summary>
        /// Задать смещение
        /// </summary>
        public QuadSide SetTranslate(Vector3 bias)
        {
            for (int i = 0; i < 4; i++)
            {
                Vertex[i].X += bias.X;
                Vertex[i].Y += bias.Y;
                Vertex[i].Z += bias.Z;
            }
            return this;
        }

        /// <summary>
        /// Задать вращение в градусах по центру блока
        /// </summary>
        public QuadSide SetRotate(float yaw, float pitch, float roll)
        {
            Vector3 vec;
            for (int i = 0; i < 4; i++)
            {
                vec = Vertex[i].ToPosition() - .5f;
                if (roll != 0) vec = Glm.Rotate(vec, Glm.Radians(roll), new Vector3(0, 0, 1));
                if (pitch != 0) vec = Glm.Rotate(vec, Glm.Radians(pitch), new Vector3(1, 0, 0));
                if (yaw != 0) vec = Glm.Rotate(vec, Glm.Radians(yaw), new Vector3(0, 1, 0));
                Vertex[i].X = vec.X + .5f;
                Vertex[i].Y = vec.Y + .5f;
                Vertex[i].Z = vec.Z + .5f;
            }
            return this;
        }

        /// <summary>
        /// Вращение блока 90, 180, 270 
        /// </summary>
        /// <param name="shade">Отсутствие оттенка</param>
        public QuadSide SetRotateY(int rotate, bool shade)//, bool uvLock)
        {
            if (rotate == 90 || rotate == 180 || rotate == 270)
            {
                SetRotate(rotate, 0, 0);
                if (Side == 2) // East Восток
                {
                    Side = rotate == 90 ? 4 : rotate == 180 ? 3 : 5;
                }
                else if (Side == 3) // West Запад
                {
                    Side = rotate == 90 ? 5 : rotate == 180 ? 2 : 4;
                }
                else if (Side == 4) // North Север
                {
                    Side = rotate == 90 ? 3 : rotate == 180 ? 5 : 2;
                }
                else if (Side == 5) // South Юг
                {
                    Side = rotate == 90 ? 2 : rotate == 180 ? 4 : 3;
                }
                LightPole = shade ? 0f : 1f - Gi.LightPoles[Side];
            }
            return this;
        }

        /// <summary>
        /// Задать стороне ветер, 0 - нет движения 1 - (по умолчанию) вверх двигается низ нет, 2 - низ двигается вверх нет, 3 - двигается всё
        /// </summary>
        public QuadSide SetWind(byte wind = 1)
        {
            Wind = wind;
            return this;
        }

        /// <summary>
        /// Резкость = 4; mipmap = 0;
        /// </summary>
        public QuadSide SetSharpness()
        {
            Sharpness = 4;
            return this;
        }

        /// <summary>
        /// Имеется ли у блока смена цвета от биома
        /// </summary>
        public bool IsBiomeColor() => BiomeColor > 0;
        /// <summary>
        /// Имеется ли у блока смена цвета травы от биома
        /// </summary>
        public bool IsBiomeColorGrass() => BiomeColor == 1;
        /// <summary>
        /// Имеется ли у блока свой цвет, цвет как для GUI
        /// </summary>
        public bool IsYourColor() => BiomeColor == 4;
    }
}
