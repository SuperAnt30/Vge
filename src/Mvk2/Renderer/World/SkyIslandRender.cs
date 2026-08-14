using Mvk2.Renderer.Mesh;
using Mvk2.Renderer.Shaders;
using Mvk2.World.Biome;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Vge.Entity.Player;
using Vge.Renderer.Mesh;
using Vge.Renderer.World;
using Vge.Util;
using Vge.World;
using Vge.World.Block;
using Vge.World.Gen;
using Vge.World.Сalendar;
using WinGL.OpenGL;
using WinGL.Util;

namespace Mvk2.Renderer.World
{
    /// <summary>
    /// Объект отвечает за прорисовку неба для мира с островом
    /// </summary>
    public class SkyIslandRender : SkyRender
    {
        /// <summary>
        /// Объект календаря
        /// </summary>
        private Сalendar32 _calendar;

        /// <summary>
        /// Цвет заката и рассвета
        /// </summary>
        private float[] _colors = new float[0];

        /// <summary>
        /// Сетка заката
        /// </summary>
        private readonly MeshSky _meshSunset;
        /// <summary>
        /// Сетка солнца
        /// </summary>
        private readonly MeshSkyElement _meshSun;
        /// <summary>
        /// Сетка солнца
        /// </summary>
        private readonly MeshSkyElement _meshMoon;
        /// <summary>
        /// Сетка дождя
        /// </summary>
        private readonly MeshSkyElement _meshRain;
        /// <summary>
        /// Сетка звёзд
        /// </summary>
        private readonly MeshSkyStar _meshStar;
        /// <summary>
        /// Сетка облаков
        /// </summary>
        private readonly MeshSkyClouds _meshClouds;

        /// <summary>
        /// Шейдор неба
        /// </summary>
        protected readonly ShaderSkyElement _shSkyElement;
        /// <summary>
        /// Шейдор звёзд
        /// </summary>
        protected readonly ShaderSkyStar _shSkyStar;
        /// <summary>
        /// Шейдор облаков
        /// </summary>
        protected readonly ShaderSkyClouds _shSkyClouds;

        /// <summary>
        /// Объект отвечающий за прорисовку Малювек
        /// </summary>
        private readonly RenderMvk _renderMvk;
        /// <summary>
        /// Матрица расположения солнца
        /// </summary>
        private Mat4 _matSun;
        /// <summary>
        /// Матрица расположения луны
        /// </summary>
        private Mat4 _matMoon;
        /// <summary>
        /// Матрица звёзд
        /// </summary>
        private Mat4 _matStar;
        /// <summary>
        /// Фаза луны прошлого такта
        /// </summary>
        private int _moonPhaseIndexPrev = -1;
        /// <summary>
        /// Моргающие звёзды, цвет
        /// </summary>
        private float _starRand1;
        /// <summary>
        /// Моргающие звёзды, цвет
        /// </summary>
        private float _starRand2;
        /// <summary>
        /// Моргающие звёзды, цвет
        /// </summary>
        private float _starRand3;

        #region Атрибуты для облак

        /// <summary>
        /// Размер половинки облака. Чем крупнее тем облако больше.
        /// </summary>
        private const int _cloudSizeSide = 1024;
        /// <summary>
        /// Размер растяжки текстуры облака. 
        /// </summary>
        private const float _cloudSizeTexure = _cloudSizeSide * 2;
        /// <summary>
        /// Размер пикселя в коэффициенте облака
        /// </summary>
        private const float _cloudSizePixelTexure = 1f / _cloudSizeTexure;
        /// <summary>
        /// Высота облак
        /// </summary>
        private const float _heightClouds = 132f;

        /// <summary>
        /// Счётчик движения облаков по X
        /// </summary>
        private uint _cloudTickCounterX = 0;
        /// <summary>
        /// Счётчик движения облаков по Z
        /// </summary>
        private uint _cloudTickCounterZ = 0;
        
        /// <summary>
        /// Индекс текстуры облак
        /// </summary>
        private uint _textureCloud = 0;
        /// <summary>
        /// Индекс текстуры дождя
        /// </summary>
        private uint _textureRain = 0;
        /// <summary>
        /// Индекс текстуры ливня
        /// </summary>
        private uint _textureShowers = 0;
        
        /// <summary>
        /// Начальная альфа
        /// </summary>
        private float _alpha = -.4f;

        #endregion

        /// <summary>
        /// Катет по X для дождя
        /// </summary>
        private readonly float[] _xSqrt = new float[1024];
        /// <summary>
        /// Катет по Z для дождя
        /// </summary>
        private readonly float[] _zSqrt = new float[1024];
        /// <summary>
        /// Счётчик дождя
        /// </summary>
        private int _rendererUpdateCount;
        /// <summary>
        /// Быстрый массив, для построения буффера дождя
        /// </summary>
        private ListFlout _bufferRain = new ListFlout();

        public SkyIslandRender(PlayerClientOwner player, WorldRenderer worldRenderer, RenderMvk renderMvk) 
            : base(player, worldRenderer)
        {
            _meshSunset = new MeshSky(gl, GL.GL_DYNAMIC_DRAW);
            float size = 32;

            _meshSun = new MeshSkyElement(gl, GL.GL_STATIC_DRAW);
            _meshSun.Reload(new float[] {
                -size, 0, -size, 0, 0,
                size, 0, -size, 1, 0,
                size, 0, size, 1, 1,
                -size, 0, size, 0, 1
            });

            _meshMoon = new MeshSkyElement(gl, GL.GL_DYNAMIC_DRAW);

            _meshClouds = new MeshSkyClouds(gl);
            _meshClouds.Reload(_GenBufferClouds());
            _CreateTextureClouds();

            _meshRain = new MeshSkyElement(gl, GL.GL_STREAM_DRAW);
            _CreateTextureRain();

            _meshStar = new MeshSkyStar(gl);
            _meshStar.Reload(_GenBufferStar());

            _shSkyElement = new ShaderSkyElement(gl);
            _shSkyStar = new ShaderSkyStar(gl);
            _shSkyClouds = new ShaderSkyClouds(gl);
            _renderMvk = renderMvk;

            for (int x = 0; x < 32; x++)
            {
                int x0 = x - 16;
                for (int z = 0; z < 32; z++)
                {
                    int z0 = z - 16;
                    float q = Mth.Sqrt(z0 * z0 + x0 * x0);
                    _xSqrt[x << 5 | z] = x0 / q;
                    _zSqrt[x << 5 | z] = -z0 / q;
                }
            }
        }

        /// <summary>
        /// Инициализация настроек мира
        /// </summary>
        public override void InitSetting()
        {
            _calendar = (Сalendar32)_player.GetWorld().Settings.Calendar;
        }

        /// <summary>
        /// Генерация буфера сетки заката
        /// </summary>
        private float[] _GenBufferSunset()
        {
            List<float> list = new List<float>();
            // Высота заката
            float height = 68f;  // 40
            float fob = 128f;
            float angleStep = Glm.Pi360 / 16;
            float x, y, z, x0, y0, z0, currentAngle;
            x0 = y0 = z0 = 0;
            float x1 = _colors[4] < 0 ? fob : -fob;
            
            // Генерация вершин по окружности
            for (int i = 0; i <= 16; i++)
            {
                currentAngle = angleStep * i;
                x = Glm.Cos(currentAngle);
                z = fob * Glm.Sin(currentAngle);

                if (_colors[4] < 0)
                {
                    y = x * height * _colors[3];
                }
                else
                {
                    y = -x * height * _colors[3];
                }
                x = x * fob - 16f;

                if (i > 0)
                {
                    list.AddRange(new float[] {
                        x0, y0, z0, 2,
                        x1, 0, 0, 0,
                        x, y, z, 2
                    });
                }

                x0 = x;
                y0 = y;
                z0 = z;
            }

            return list.ToArray();
        }

        /// <summary>
        /// Генерация буфера сетки звёзд
        /// </summary>
        private float[] _GenBufferStar()
        {
            List<float> list = new List<float>();
            Rand random = new Rand(10842);

            int i, j;
            float x, y, z, size, distance;
            float x2, y2, z2, angle, sa1, ca1, sa2, ca2, sa3, ca3, color;
            float f1, f2, f3, f4, f5, x3, y3, z3;

            // Предполагаемое количество звёзд 1500
            for (i = 0; i < 1500; i++)
            {
                x = random.NextFloat() * 2f - 1f;
                y = random.NextFloat() * 2f - 1f;
                z = random.NextFloat() * 2f - 1f;
                size = .15f + random.NextFloat() * .1f;
                distance = x * x + y * y + z * z;

                // будет 1234 согласно рандома seed 10842
              //  if (distance < 1.5f)
                {
                    distance = 1f / Mth.Sqrt(distance);
                    x *= distance;
                    y *= distance;
                    z *= distance;
                    x2 = x * 128f;
                    y2 = y * 128f;
                    z2 = z * 128f;
                    angle = Glm.Atan2(x, z);
                    sa1 = Glm.Sin(angle);
                    ca1 = Glm.Cos(angle);
                    angle = Glm.Atan2(Mth.Sqrt(x * x + z * z), y);
                    sa2 = Glm.Sin(angle);
                    ca2 = Glm.Cos(angle);
                    angle = random.NextFloat() * Glm.Pi360;
                    sa3 = Glm.Sin(angle);
                    ca3 = Glm.Cos(angle);

                    color = random.NextFloat() * .5f;
                    if (random.Next(10) != 0)
                    {
                        color += .5f;
                    }

                    for (j = 0; j < 4; j++)
                    {
                        f1 = ((j & 2) - 1) * size;
                        f2 = ((j + 1 & 2) - 1) * size;
                        f3 = f1 * ca3 - f2 * sa3;
                        f4 = f2 * ca3 + f1 * sa3;
                        f5 = f3 * ca2;

                        x3 = -f5 * sa1 - f4 * ca1;
                        y3 = f3 * sa2;
                        z3 = f4 * sa1 - f5 * ca1;
                        list.AddRange(new float[] {
                            x2 + x3, y2 + y3, z2 + z3, color
                        });
                    }
                }
            }

            return list.ToArray();
        }

        /// <summary>
        /// Игровой такт
        /// </summary>
        public override void Update()
        {
            if (_alpha < 1) _alpha += .1f;
            base.Update();

            // Счётяик дождя
            if (_calendar.IsRain) _rendererUpdateCount++;

            // Счётчик облаков
            _cloudTickCounterX++;
            if (_cloudTickCounterX > (_cloudSizeTexure / Mth.Abs(_calendar.SpeedCloudX)))
            {
                _cloudTickCounterX = 0;
            }
            _cloudTickCounterZ++;
            if (_cloudTickCounterZ > (_cloudSizeTexure / Mth.Abs(_calendar.SpeedCloudZ)))
            {
                _cloudTickCounterZ = 0;
            }

            if (_player.PosY < BiomeIsland.HeightWater)
            {
                _colorDown = new Vector3(0);
            }

            float celestialAngle = _calendar.GetCelestialAngle();
            _colors = _CalcSunriseSunsetColors(celestialAngle);

            // Параметра для размера солнца, растояние от глаз 64 - 128
            float sunLightAdd = 0;
            if (celestialAngle > .6f)
            {
                // восход
                sunLightAdd = (celestialAngle - .6f) * 2.5f;
            }
            else if (celestialAngle < .4f)
            {
                // заход
                sunLightAdd = (.4f - celestialAngle) * 2.5f;
            }

            // Матрица расположения солнца
            if (_calendar.GetSunLight() > 0)
            {
                _matSun = Mat4.Identity();
                _matSun.RotateX(Сalendar32.AngleSunTimeYear[_calendar.TimeYearIndex]);
                _matSun.RotateZ(celestialAngle * Glm.Pi360);
                _matSun.Translate(0, 64f + (_calendar.GetSunLight() + sunLightAdd) * 30f, 0);
            }

            if (_calendar.StarLight > 0)
            {
                _matStar = Mat4.Identity();
                _matStar.RotateX(Glm.Pi45);
                _matStar.RotateZ(celestialAngle * Glm.Pi360);

                _matMoon = new Mat4(_matStar);
                _matMoon.RotateY(celestialAngle * Glm.Pi90 + 2.4f); // Чтоб луна была в горизонте читабельная фазе
                _matMoon.Translate(0, -112f, 0);

                if (_moonPhaseIndexPrev != _calendar.MoonPhaseIndex)
                {
                    _moonPhaseIndexPrev = _calendar.MoonPhaseIndex;
                    int phaseV = _moonPhaseIndexPrev % 4;
                    int phaseH = _moonPhaseIndexPrev / 4 % 2;
                    float u1 = phaseV / 4f;
                    float v1 = phaseH / 2f;
                    float u2 = (phaseV + 1) / 4f;
                    float v2 = (phaseH + 1) / 2f;
                    float size = 20;

                    _meshMoon.Reload(new float[] {
                    -size, 0, -size, u2, v1,
                    -size, 0, size, u2, v2,
                    size, 0, size, u1, v2,
                    size, 0, -size, u1, v1,
                });
                }

                // Маргающие звёзда
                if (_player.GetWorld().Rnd.Next(10) == 0)
                    _starRand1 = _player.GetWorld().Rnd.NextFloat();
                if (_player.GetWorld().Rnd.Next(10) == 0)
                    _starRand2 = _player.GetWorld().Rnd.NextFloat();
                if (_player.GetWorld().Rnd.Next(10) == 0)
                    _starRand3 = _player.GetWorld().Rnd.NextFloat();
            }
        }

        /// <summary>
        /// Возвращает массив цветов восхода/заката
        /// </summary>
        public float[] _CalcSunriseSunsetColors(float angle)
        {
            float x = .4f;
            float y = Glm.Cos(angle * Glm.Pi360);
            float z = 0;

            if (y >= z - x && y <= z + x)
            {
                float f1 = (y - z) / x * .5f + .5f;
                float f2 = 1f - (1f - Glm.Sin(f1 * Glm.Pi)) * .99f;
                f2 *= f2;
                return new float[] {
                    f1 * .3f + .7f,
                    f1 * f1 * .7f + .2f,
                    f1 * f1 * 0 + .2f,
                    f2,
                    Glm.Sin(angle * Glm.Pi360)
                };
            }
            return new float[0];
        }

        /// <summary>
        /// Дополнительные элементы неба
        /// </summary>
        protected override void _DrawAddElementSky(float timeIndex)
        {
            // Восход и закат
            if (_colors.Length > 0)
            {
                _meshSunset.Reload(_GenBufferSunset());
                _shSky.SetUniform4("color", _colors[0], _colors[1], _colors[2], _colors[3]);
                _shSky.SetUniform4("colorfog", _colors[0], _colors[1], _colors[2], 0f);
                _meshSunset.Draw();
            }

            gl.BlendFuncSeparate(GL.GL_SRC_ALPHA, GL.GL_ONE, GL.GL_ONE, GL.GL_ZERO);

            // Солнце
            if (_calendar.GetSunLight() > 0)
            {
                _renderMvk.BindTextureSun();
                _shSkyElement.Bind();
                _shSkyElement.SetUniform1("transparency", _calendar.GetSunLight());
                _shSkyElement.SetUniformMatrix4("view", Gi.MatrixView);
                _shSkyElement.SetUniformMatrix4("model", _matSun.ToArray());
                _meshSun.Draw();
            }

            // Звёзды и луна
            if (_calendar.StarLight > 0)
            {
                _shSkyStar.Bind();
                _shSkyStar.SetUniform1("transparency", _calendar.StarLight);
                _shSkyStar.SetUniformMatrix4("view", Gi.MatrixView);
                _shSkyStar.SetUniformMatrix4("model", _matStar.ToArray());
                _shSkyStar.SetUniform3("color", _starRand1, _starRand2, _starRand3);
                _meshStar.Draw();

                _renderMvk.BindTextureMoon();
                _shSkyElement.Bind();
                _shSkyElement.SetUniform1("transparency", _calendar.StarLight + .15f);
                _shSkyElement.SetUniformMatrix4("view", Gi.MatrixView);
                _shSkyElement.SetUniformMatrix4("model", _matMoon.ToArray());
                _meshMoon.Draw();
            }

            gl.BlendFuncSeparate(GL.GL_SRC_ALPHA, GL.GL_ONE_MINUS_SRC_ALPHA, GL.GL_ONE, GL.GL_ZERO);
        }

        #region Clouds

        /// <summary>
        /// Генерация буфера сетки облаков
        /// </summary>
        private float[] _GenBufferClouds()
        {
            int sb = _cloudSizeSide;
            int ss = sb / 2;
            float tb = .5f;
            float ts = .25f;
            List<float> list = new List<float>();

            list.AddRange(_GenBufferCloudsSector(-ss, -ss, ss, ss, 
                -ts, -ts, ts, ts, 1, 1, 1, 1));
            // -X
            list.AddRange(_GenBufferCloudsSector(-sb, -ss, -ss, ss,
                -tb, -ts, -ts, ts, 0, 1, 1, 0));
            // +X
            list.AddRange(_GenBufferCloudsSector(ss, -ss, sb, ss,
                ts, -ts, tb, ts, 1, 0, 0, 1));
            // -Z
            list.AddRange(_GenBufferCloudsSector(-ss, -sb, ss, -ss,
                -ts, -tb, ts, -ts, 0, 0, 1, 1));
            // +Z
            list.AddRange(_GenBufferCloudsSector(-ss, ss, ss, sb,
                -ts, ts, ts, tb, 1, 1, 0, 0));

            // -X -Z
            list.AddRange(_GenBufferCloudsSector(-sb, -sb, -ss, -ss,
                -tb, -tb, -ts, -ts, 0, 0, 1, 0));
            // +X -Z
            list.AddRange(_GenBufferCloudsSector(ss, -sb, sb, -ss,
                ts, -tb, tb, -ts, 0, 0, 0, 1));
            // -X +Z
            list.AddRange(_GenBufferCloudsSector(-sb, ss, -ss, sb,
                -tb, ts, -ts, tb, 0, 1, 0, 0));
            // +X +Z
            list.AddRange(_GenBufferCloudsSector(ss, ss, sb, sb,
                ts, ts, tb, tb, 1, 0, 0, 0));

            return list.ToArray();
        }

        /// <summary>
        /// Сгенерировать сектора облаков
        /// </summary>
        private float[] _GenBufferCloudsSector(float x1, float z1, float x2, float z2, 
            float u1, float v1, float u2, float v2, float a1, float a2, float a3, float a4)
        {
            return new float[] {
                // Видим снизу
                x1, _heightClouds, z1, u1, v1, a1,
                x2, _heightClouds, z1, u2, v1, a2,
                x2, _heightClouds, z2, u2, v2, a3,
                x1, _heightClouds, z2, u1, v2, a4,
                // Видим сверху
                x1, _heightClouds, z2, u1, v2, a4,
                x2, _heightClouds, z2, u2, v2, a3,
                x2, _heightClouds, z1, u2, v1, a2,
                x1, _heightClouds, z1, u1, v1, a1
            };
        }

        /// <summary>
        /// Биндим шейдоры облаков и смещаем по ветру
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void _ShCloudsBind(float timeIndex)
        {
            float x0 = _player.PosFrameX - (_cloudTickCounterX + timeIndex) * _calendar.SpeedCloudX;
            x0 = (x0 - Mth.Floor(x0 / _cloudSizeTexure) * _cloudSizeTexure) * _cloudSizePixelTexure;

            float z0 = _player.PosFrameZ - (_cloudTickCounterZ + timeIndex) * _calendar.SpeedCloudZ;
            z0 = (z0 - Mth.Floor(z0 / _cloudSizeTexure) * _cloudSizeTexure) * _cloudSizePixelTexure;

            //Console.WriteLine(x0 + " - " + z0);
            _shSkyClouds.Bind();
            _shSkyClouds.SetUniform2("pos", x0, z0);
        }

        /// <summary>
        /// Прорисовка облака неба и дождя
        /// </summary>
        public override void DrawCloudsRain(float timeIndex)
        {
            _DrawClouds(timeIndex);
            _DrawRain(timeIndex);
        }

        /// <summary>
        /// Прорисовка облака неба для карты теней
        /// </summary>
        public override void DrawCloudsDepthMap(float timeIndex)
        {
            if (_calendar.FewClouds < .7f)
            {
                gl.ActiveTexture(GL.GL_TEXTURE0);
                gl.BindTexture(GL.GL_TEXTURE_2D, _textureCloud);
                _ShCloudsBind(timeIndex);
                _shSkyClouds.SetUniform1("transparency", 1f);
                _shSkyClouds.SetUniformMatrix4("view", Gi.MatrixViewDepthMap);
                _shSkyClouds.SetUniform1("posY", _player.PosFrameY + 4);
                _shSkyClouds.SetUniform1("few", _calendar.FewClouds + .14f);
                _meshClouds.Draw();
            }
        }

        /// <summary>
        /// Прорисовка облака
        /// </summary>
        private void _DrawClouds(float timeIndex)
        {
            // Это облака
            if (_alpha > 0)
            {
                if (Vge.Debug.IsDrawVoxelLine)
                {
                    gl.PolygonMode(GL.GL_FRONT_AND_BACK, GL.GL_LINE);
                    gl.Disable(GL.GL_CULL_FACE);
                }

                gl.ActiveTexture(GL.GL_TEXTURE0);
                gl.BindTexture(GL.GL_TEXTURE_2D, _textureCloud);

                _ShCloudsBind(timeIndex);
                _shSkyClouds.SetUniformMatrix4("view", Gi.MatrixView);

                // Второй уровень затемнённости
                if (_calendar.FewClouds < .7f)
                {
                    _shSkyClouds.SetUniform1("transparency", _alpha);
                    _shSkyClouds.SetUniform1("posY", _player.PosFrameY + 4);
                    _shSkyClouds.SetUniform1("few", _calendar.FewClouds + .14f);
                    _shSkyClouds.SetUniform3("color", _calendar.ColorClouds.X * .7f,
                    _calendar.ColorClouds.Y * .7f, _calendar.ColorClouds.Z * .7f);
                    _meshClouds.Draw();
                }

                // Светлые полупрозрачный слой
                _shSkyClouds.SetUniform1("transparency", .6f * _alpha);
                _shSkyClouds.SetUniform1("posY", _player.PosFrameY);
                _shSkyClouds.SetUniform1("few", _calendar.FewClouds);
                _shSkyClouds.SetUniform3("color", _calendar.ColorClouds.X,
                    _calendar.ColorClouds.Y, _calendar.ColorClouds.Z);
                _meshClouds.Draw();

                if (Vge.Debug.IsDrawVoxelLine)
                {
                    gl.Enable(GL.GL_CULL_FACE);
                    gl.PolygonMode(GL.GL_FRONT_AND_BACK, GL.GL_FILL);
                }
            }
        }

        /// <summary>
        /// Создать текстуру облаков
        /// </summary>
        private void _CreateTextureClouds()
        {
            // Используем шум 128 на 128 текстуру
            NoiseGeneratorPerlin noiseArea = new NoiseGeneratorPerlin(new Rand(512), 4);
            // Массив шума 128*128
            float[] areaNoise = new float[16384];
            noiseArea.GenerateNoise2d(areaNoise, 0, 0, 128, 128, 1.2f, 1.8f);
            // Байтовый шум
            byte[] byteNoise = new byte[16384];
            // Буфер текстуры
            byte[] buffer = new byte[65536];

            float f;
            int i, j;

            // Находим приделы шума
            float max = float.MinValue;
            float min = float.MaxValue;
            for (i = 0; i < 16384; i++)
            {
                f = areaNoise[i];
                if (f > max) max = f;
                if (f < min) min = f;
            }
            float delta = 255f / (max - min);

            // Заполняем байтами буфер
            for (i = 0; i < 16384; i++)
            {
                byteNoise[i] = (byte)((areaNoise[i] - min) * delta);
            }

            // Этап бесшовности
            // Массивы сторон для сглаживания, бесшовности
            byte[] ar0 = new byte[128];
            byte[] ar1 = new byte[128];
            byte[] ar2 = new byte[128];
            byte[] ar3 = new byte[128];

            for (i = 1; i < 127; i++)
            {
                ar0[i] = (byte)((
                    byteNoise[i] + byteNoise[i] +
                    byteNoise[i - 1] + byteNoise[i + 1] + byteNoise[i + 128] + byteNoise[i + 16256]
                    ) / 6);

                j = i + 16256;
                ar1[i] = (byte)((
                    byteNoise[j] + byteNoise[j] +
                    byteNoise[j - 1] + byteNoise[j + 1] + byteNoise[i] + byteNoise[j - 128]
                    ) / 6);

                j = i * 128;
                ar2[i] = (byte)((
                    byteNoise[j] + byteNoise[j] +
                    byteNoise[j - 1] + byteNoise[j + 1] + byteNoise[j + 128] + byteNoise[j - 128]
                    ) / 6);

                j += 127;
                ar3[i] = (byte)((
                    byteNoise[j] + byteNoise[j] +
                    byteNoise[j - 1] + byteNoise[j + 1] + byteNoise[j + 128] + byteNoise[j - 128]
                    ) / 6);
            }

            for (i = 1; i < 127; i++)
            {
                byteNoise[i] = ar0[i];
                byteNoise[16256 + i] = ar1[i];
                byteNoise[i * 128] = ar2[i];
                byteNoise[i * 128 + 127] = ar3[i];
            }

            // Заполняем байтами буфер
            for (i = 0; i < 16384; i++)
            {
                j = i * 4;
                buffer[j] = buffer[j + 1] = buffer[j + 2] = byteNoise[i];
                buffer[j + 3] = 255;
            }

            // Console.WriteLine(min + " .. " + max);
            bool isCreate = _textureCloud == 0;
            if (isCreate)
            {
                uint[] texture = new uint[1];
                gl.GenTextures(1, texture);
                _textureCloud = texture[0];
            }

            gl.ActiveTexture(GL.GL_TEXTURE0);
            gl.BindTexture(GL.GL_TEXTURE_2D, _textureCloud);

            if (isCreate)
            {
                gl.TexImage2D(GL.GL_TEXTURE_2D, 0, GL.GL_RGBA, 128, 128,
                0, GL.GL_BGRA, GL.GL_UNSIGNED_BYTE, buffer);

                gl.TexParameter(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_WRAP_S, GL.GL_REPEAT);
                gl.TexParameter(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_WRAP_T, GL.GL_REPEAT);
                gl.TexParameter(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_MIN_FILTER, GL.GL_NEAREST);
                gl.TexParameter(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_MAG_FILTER, GL.GL_NEAREST);
            }
            else
            {
                gl.TexSubImage2D(GL.GL_TEXTURE_2D, 0, 0, 0, 128, 128,
                        GL.GL_BGRA, GL.GL_UNSIGNED_BYTE, buffer);
            }
        }

        #endregion

        #region Rain

        private void _DrawRain(float timeIndex)
        {
            // Это дождь
            if (_calendar.IsRain)
            {
                gl.ActiveTexture(GL.GL_TEXTURE0);
                gl.BindTexture(GL.GL_TEXTURE_2D, 
                    _calendar.IsShowers ? _textureShowers : _textureRain);

                _shSkyElement.Bind();
                _shSkyElement.SetUniform1("transparency", 1f);
                _shSkyElement.SetUniformMatrix4("view", Gi.MatrixView);
                _shSkyElement.SetUniformMatrix4("model", Mat4.Identity().ToArray());

                float px = _player.PosFrameX;
                float py = _player.PosFrameY;
                float pz = _player.PosFrameZ;

                WorldBase world = _player.GetWorld();
                BlockPos blockPos = new BlockPos(px, py, pz);
                
                int bx = blockPos.X;
                int by = blockPos.Y;
                int bz = blockPos.Z;

                int step = 6; // Максималка 10 минималка 5
                int x, z, index;

                float x1, x2, y1, y2, z1, z2;
                float qx, qz, by0, v1, v2, y3;

                _bufferRain.Clear();
                for (x = bx - step; x <= bx + step; x++)
                {
                    for (z = bz - step; z <= bz + step; z++)
                    {
                        index = (x - bx + 16) * 32 + z - bz + 16;
                        qx = _zSqrt[index] * .5f;
                        x1 = x - qx - px + .5f;
                        x2 = x + qx - px + .5f;
                        qz = _xSqrt[index] * .5f;
                        z1 = z - qz - pz + .5f;
                        z2 = z + qz - pz + .5f;

                        by0 = world.GetHeightMap(x, z);
                        y1 = by - step;
                        y2 = by + 4 + step;

                        if (y1 < by0) y1 = by0;
                        if (y2 < by0) y2 = by0;
                        if (y2 > _heightClouds) y2 = _heightClouds;

                        if (y1 < y2)
                        {
                            y3 = ((_rendererUpdateCount & 511) + timeIndex) / 512f
                                + world.Rnd.NextFloat() + (_rendererUpdateCount + timeIndex) 
                                * world.Rnd.NextGaussian() * .001f;
                            //float y3 = ((_rendererUpdateCount & 511) + timeIndex) / 512f; // снег, медленно
                            v1 = y1 / 4f + y3;
                            v2 = y2 / 4f + y3;

                            y1 -= py;
                            y2 -= py;

                            _bufferRain.AddRange(new float[] {
                                x1, y1, z1, 0, v1, 
                                x2, y1, z2, 1, v1,
                                x2, y2, z2, 1, v2,
                                x1, y2, z1, 0, v2
                            });
                        }
                    }
                }

                _meshRain.Reload(_bufferRain);
                _meshRain.Draw();
            }
        }

        /// <summary>
        /// Создать текстуру дождя или ливня
        /// </summary>
        private uint _CreateTextureRain(byte[] byteNoise, uint idTexture, int noise)
        {
            // Буфер текстуры
            byte[] buffer = new byte[65536];

            int i, j;
            // Заполняем байтами буфер
            for (i = 0; i < 16384; i++)
            {
                j = i * 4;
                buffer[j + 2] = 65;
                buffer[j + 1] = 90;
                buffer[j + 0] = 172;

                if (byteNoise[i] < noise) // 40 ливень // 20 дождь
                {
                    buffer[j + 3] = 255;// (byte)(255 - byteNoise[i]);
                }
                else
                {
                    buffer[j + 3] = 0;
                }
            }

            bool isCreate = idTexture == 0;
            if (isCreate)
            {
                uint[] texture = new uint[1];
                gl.GenTextures(1, texture);
                idTexture = texture[0];
            }

            gl.ActiveTexture(GL.GL_TEXTURE0);
            gl.BindTexture(GL.GL_TEXTURE_2D, idTexture);

            if (isCreate)
            {
                gl.TexImage2D(GL.GL_TEXTURE_2D, 0, GL.GL_RGBA, 64, 256,
                0, GL.GL_BGRA, GL.GL_UNSIGNED_BYTE, buffer);

                gl.TexParameter(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_WRAP_S, GL.GL_REPEAT);
                gl.TexParameter(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_WRAP_T, GL.GL_REPEAT);
                gl.TexParameter(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_MIN_FILTER, GL.GL_NEAREST);
                gl.TexParameter(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_MAG_FILTER, GL.GL_NEAREST);
            }
            else
            {
                gl.TexSubImage2D(GL.GL_TEXTURE_2D, 0, 0, 0, 64, 256,
                        GL.GL_BGRA, GL.GL_UNSIGNED_BYTE, buffer);
            }

            return idTexture;
        }

        /// <summary>
        /// Создать текстуру дождя
        /// </summary>
        private void _CreateTextureRain()
        {
            // Используем шум 128 на 128 текстуру
            NoiseGeneratorPerlin noiseArea = new NoiseGeneratorPerlin(new Rand(200), 4);
            // Массив шума 64*256
            float[] areaNoise = new float[16384];
            noiseArea.GenerateNoise2d(areaNoise, 0, 0, 64, 256, 2.8f, .2f);
            // Байтовый шум
            byte[] byteNoise = new byte[16384];

            float f;
            int i;

            // Находим приделы шума
            float max = float.MinValue;
            float min = float.MaxValue;
            for (i = 0; i < 16384; i++)
            {
                f = areaNoise[i];
                if (f > max) max = f;
                if (f < min) min = f;
            }
            float delta = 255f / (max - min);

            // Заполняем байтами буфер
            for (i = 0; i < 16384; i++)
            {
                byteNoise[i] = (byte)((areaNoise[i] - min) * delta);
            }

            _textureRain = _CreateTextureRain(byteNoise, _textureRain, 20);
            _textureShowers = _CreateTextureRain(byteNoise, _textureShowers, 40);
        }

        #endregion

        public override void Dispose()
        {
            base.Dispose();
            _meshSunset.Dispose();
            _meshSun.Dispose();
            _meshMoon.Dispose();
            _meshStar.Dispose();
            _meshClouds.Dispose();
            _shSkyElement.Delete();
            _shSkyStar.Delete();
            _shSkyClouds.Delete();

            gl.DeleteTextures(3, new uint[] { _textureCloud, _textureRain, _textureShowers });
        }
    }
}
