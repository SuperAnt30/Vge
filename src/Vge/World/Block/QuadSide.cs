using System.Collections.Generic;
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
        /// Нормаль
        /// </summary>
        public uint Normal;
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
        /// 0 - нет анимации
        /// </summary>
        public byte AnimationFrame;
        /// <summary>
        /// Для анимации блока, указывается пауза между кадрами в игровом времени (50 мс),
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
        public byte TypeColor = 0;
        /// <summary>
        /// Не крайняя сторона
        /// </summary>
        public bool NotExtremeSide = false;

        /// <summary>
        /// Прямоугольная сторона блока или элемента блока
        /// </summary>
        /// <param name = "typeColor" >Цвет биома, где 0 - нет цвета, 1 - трава, 2 - листа, 3 - вода, 4 - свой цвет</param>
        public QuadSide(byte typeColor = 0)
        {
            TypeColor = typeColor;
        }
        /// <summary>
        /// Не крайняя сторона, чтоб не отброкавать при прорисовке
        /// </summary>
        public void SetNotExtremeSide() => NotExtremeSide = true;

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
        /// <param name="shade">shade Отсутствие оттенка / Нет бокового затемнения, пример: трава, цветы</param>
        public QuadSide SetSide(Pole pole, bool shade = false,
            float x1i = 0, float y1i = 0, float z1i = 0, float x2i = 16, float y2i = 16, float z2i = 16)
        {
            float x1 = x1i / 16f;
            float y1 = y1i / 16f;
            float z1 = z1i / 16f;
            float x2 = x2i / 16f;
            float y2 = y2i / 16f;
            float z2 = z2i / 16f;
            Side = (int)pole;
            LightPole = shade ? 0f : 1f - Gi.LightPoles[Side];
            switch (pole)
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
            return this;
        }

        /// <summary>
        /// Задать текстуру, её можно повернуть кратно 90 гр. 0, 90, 180, 270 
        /// </summary>
        public QuadSide SetTexture(int numberTexture, int rotateYawUV = 0)
        {
            float u1 = (numberTexture % Ce.TextureAtlasBlockCount) * Ce.ShaderAnimOffset;
            float u2 = u1 + Ce.ShaderAnimOffset;
            float v1 = numberTexture / Ce.TextureAtlasBlockCount * Ce.ShaderAnimOffset;
            float v2 = v1 + Ce.ShaderAnimOffset;
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
        public QuadSide SetTexture(int numberTexture, float biasU1, float biasV1,
            float biasU2, float biasV2, int rotateYawUV = 0)
        {
            float u = (numberTexture % Ce.TextureAtlasBlockCount) * Ce.ShaderAnimOffset;
            float v = numberTexture / Ce.TextureAtlasBlockCount * Ce.ShaderAnimOffset;
            float k = Ce.TextureAtlasBlockCount * 16f;
            switch (rotateYawUV)
            {
                case 0:
                    Vertex[0].U = Vertex[1].U = u + biasU2 / k;
                    Vertex[2].U = Vertex[3].U = u + biasU1 / k;
                    Vertex[0].V = Vertex[3].V = v + biasV2 / k;
                    Vertex[1].V = Vertex[2].V = v + biasV1 / k;
                    break;
                case 270:
                    Vertex[1].U = Vertex[2].U = u + biasU2 / k;
                    Vertex[0].U = Vertex[3].U = u + biasU1 / k;
                    Vertex[0].V = Vertex[1].V = v + biasV2 / k;
                    Vertex[2].V = Vertex[3].V = v + biasV1 / k;
                    break;
                case 180:
                    Vertex[0].U = Vertex[1].U = u + biasU1 / k;
                    Vertex[2].U = Vertex[3].U = u + biasU2 / k;
                    Vertex[0].V = Vertex[3].V = v + biasV1 / k;
                    Vertex[1].V = Vertex[2].V = v + biasV2 / k;
                    break;
                case 90:
                    Vertex[1].U = Vertex[2].U = u + biasU1 / k;
                    Vertex[0].U = Vertex[3].U = u + biasU2 / k;
                    Vertex[0].V = Vertex[1].V = v + biasV1 / k;
                    Vertex[2].V = Vertex[3].V = v + biasV2 / k;
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
        public QuadSide SetRotate(float xR, float yR, float zR, float xO, float yO, float zO)
        {
            Vector3 vec;
            for (int i = 0; i < 4; i++)
            {
                vec = Vertex[i].ToPosition();
                vec.X -= xO;
                vec.Y -= yO;
                vec.Z -= zO;
                // Так правильно по Blockbench
                if (xR != 0) vec = Glm.Rotate(vec, Glm.Radians(xR), new Vector3(1, 0, 0));
                if (yR != 0) vec = Glm.Rotate(vec, Glm.Radians(yR), new Vector3(0, 1, 0));
                if (zR != 0) vec = Glm.Rotate(vec, Glm.Radians(zR), new Vector3(0, 0, 1));

                Vertex[i].X = Mth.Round(vec.X + xO, 3);
                Vertex[i].Y = Mth.Round(vec.Y + yO, 3);
                Vertex[i].Z = Mth.Round(vec.Z + zO, 3);
            }
            return this;
        }

        /// <summary>
        /// Вращение блока 90, 180, 270 
        /// </summary>
        /// <param name="shade">Отсутствие оттенка</param>
        public QuadSide SetRotateY(int rotate, bool shade)//, bool uvLock)
        {
            if (rotate != 0)
            {
                SetRotate(0, rotate, 0, .5f, 0, .5f);
                Side = PoleConvert.RotateY(Side, rotate);
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
        /// Имеется ли у блока смена цвета
        /// </summary>
        public bool IsTypeColor() => TypeColor > 0;
        /// <summary>
        /// Имеется ли у блока смена цвета травы от биома
        /// </summary>
        public bool IsColorGrass() => TypeColor == 1;
        /// <summary>
        /// Имеется ли у блока свой цвет, цвет как для GUI
        /// </summary>
        public bool IsYourColor() => TypeColor == 4;

        /// <summary>
        /// Генерация макси в заданный массив, и возвращает true если принудительное рисование стороны
        /// </summary>
        public bool GenMask(ulong[] ar)
        {
            if (Side == 0) return _GenUp(ar);
            if (Side == 1) return _GenDown(ar);
            if (Side == 2) return _GenEast(ar);
            if (Side == 3) return _GenWest(ar);
            if (Side == 4) return _GenNorth(ar);
            if (Side == 5) return _GenSouth(ar);
            return false;
        }

        #region Mask Side

        private bool _GenUp(ulong[] ar)
        {
            if (Vertex[0].Y == 1 && Vertex[1].Y == 1 && Vertex[2].Y == 1 && Vertex[3].Y == 1)
            {
                _GenMask(_MinX(), _MinZ(), _MaxX(), _MaxZ(), ar);
                return false;
            }
            return true;
        }

        private bool _GenDown(ulong[] ar)
        {
            if (Vertex[0].Y == 0 && Vertex[1].Y == 0 && Vertex[2].Y == 0 && Vertex[3].Y == 0)
            {
                _GenMask(_MinX(), _MinZ(), _MaxX(), _MaxZ(), ar);
                return false;
            }
            return true;
        }

        private bool _GenEast(ulong[] ar)
        {
            if (Vertex[0].X == 1 && Vertex[1].X == 1 && Vertex[2].X == 1 && Vertex[3].X == 1)
            {
                _GenMask(_MinZ(), _MinY(), _MaxZ(), _MaxY(), ar);
                return false;
            }
            return true;
        }

        private bool _GenWest(ulong[] ar)
        {
            if (Vertex[0].X == 0 && Vertex[1].X == 0 && Vertex[2].X == 0 && Vertex[3].X == 0)
            {
                _GenMask(_MinZ(), _MinY(), _MaxZ(), _MaxY(), ar);
                return false;
            }
            return true;
        }

        private bool _GenNorth(ulong[] ar)
        {
            if (Vertex[0].Z == 0 && Vertex[1].Z == 0 && Vertex[2].Z == 0 && Vertex[3].Z == 0)
            {
                _GenMask(_MinX(), _MinY(), _MaxX(), _MaxY(), ar);
                return false;
            }
            return true;
        }

        private bool _GenSouth(ulong[] ar)
        {
            if (Vertex[0].Z == 1 && Vertex[1].Z == 1 && Vertex[2].Z == 1 && Vertex[3].Z == 1)
            {
                _GenMask(_MinX(), _MinY(), _MaxX(), _MaxY(), ar);
                return false;
            }
            return true;
        }

        #endregion

        #region Min Max

        private float _MinX()
        {
            float f = float.MaxValue;
            for (int i = 0; i < 4; i++)
            {
                if (Vertex[i].X < f) f = Vertex[i].X;
            }
            return f;
        }
        private float _MaxX()
        {
            float f = float.MinValue;
            for (int i = 0; i < 4; i++)
            {
                if (Vertex[i].X > f) f = Vertex[i].X;
            }
            return f;
        }

        private float _MinY()
        {
            float f = float.MaxValue;
            for (int i = 0; i < 4; i++)
            {
                if (Vertex[i].Y < f) f = Vertex[i].Y;
            }
            return f;
        }
        private float _MaxY()
        {
            float f = float.MinValue;
            for (int i = 0; i < 4; i++)
            {
                if (Vertex[i].Y > f) f = Vertex[i].Y;
            }
            return f;
        }

        private float _MinZ()
        {
            float f = float.MaxValue;
            for (int i = 0; i < 4; i++)
            {
                if (Vertex[i].Z < f) f = Vertex[i].Z;
            }
            return f;
        }
        private float _MaxZ()
        {
            float f = float.MinValue;
            for (int i = 0; i < 4; i++)
            {
                if (Vertex[i].Z > f) f = Vertex[i].Z;
            }
            return f;
        }

        #endregion

        /// <summary>
        /// Генерация маски в 4 ulong-а (256 bit), x1, y1 от 0, x2, y2 до 1
        /// </summary>
        private void _GenMask(float x1, float y1, float x2, float y2, ulong[] ar)
        {
            //string s = "";
            int index = 0;
            int i = 0;
            bool b;
            float xf, yf;
            for (int y = 0; y < 16; y++)
            {
                yf = y / 16f;
                for (int x = 0; x < 16; x++)
                {
                    if ((ar[i] & (ulong)(1L << index)) == 0)
                    {
                        xf = x / 16f;
                        b = xf >= x1 && xf < x2 && yf >= y1 && yf < y2;
                        //s += b ? "1" : "0";
                        if (b)
                        {
                            ar[i] += (ulong)(1L << index);
                        }
                    }
                    if (++index > 63)
                    {
                        index = 0;
                        i++;
                    }
                }

                //s += "\r\n";
            }
            //string s2 = "";
            //index = 0;
            //for (i = 0; i < 4; i++)
            //{
            //    for (int j = 0; j < 64; j++)
            //    {
            //        s2 += ((ar[i] >> j) & 1).ToString();
            //        if (++index > 15)
            //        {
            //            index = 0;
            //            s2 += "\r\n";
            //        }
            //    }
            //}
        }

        /// <summary>
        /// Сгенерировать нормаль
        /// </summary>
        private Vector3 _GenNormal()
        {
            Vector3 vec0 = Vertex[0].ToPosition();
            Vector3 vec1 = Vertex[2].ToPosition() - vec0;
            Vector3 vec2 = Vertex[1].ToPosition() - vec0;
            return Glm.Cross(vec2.Normalize(), vec1.Normalize()).Normalize();
        }
             
        /// <summary>
        /// Нахождение нормали
        /// </summary>
        public void GenNormal()
        {
            Vector3 normal = _GenNormal();
            int x = (int)(normal.X * 127) + 127;
            int y = (int)(normal.Y * 127) + 127;
            int z = (int)(normal.Z * 127) + 127;
            Normal = (uint)(x | (y << 8) | (z << 16));
        }

        /// <summary>
        /// Сгенерировать буффер для сущности предмета
        /// </summary>
        public void GenBuffer(List<float> bufferFloat, List<int> bufferInt)
        {
            Vector3 normal = _GenNormal();
            for (int i = 0; i < 4; i++)
            {
                bufferFloat.AddRange(new float[] {
                    Vertex[i].X, Vertex[i].Y, Vertex[i].Z,
                    normal.X, normal.Y, normal.Z,
                    Vertex[i].U, Vertex[i].V
                });
                bufferInt.AddRange(new int[] { 0, -2 });
            }
        }

        public override string ToString()
            => "Side:" + Side + (NotExtremeSide ? " NotExtremeSide" : "");
    }
}
