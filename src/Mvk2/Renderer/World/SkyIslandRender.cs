using Mvk2.Renderer.Mesh;
using Mvk2.Renderer.Shaders;
using Mvk2.World.Biome;
using System;
using System.Collections.Generic;
using Vge.Entity.Player;
using Vge.Renderer.Mesh;
using Vge.Renderer.World;
using Vge.Util;
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
        // Цвет заката и рассвета
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
        /// Сетка звёзд
        /// </summary>
        private readonly MeshSkyStar _meshStar;
        /// <summary>
        /// Шейдор неба
        /// </summary>
        protected readonly ShaderSkyElement _shSkyElement;
        /// <summary>
        /// Шейдор звёзд
        /// </summary>
        protected readonly ShaderSkyStar _shSkyStar;

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

            _meshStar = new MeshSkyStar(gl);
            _meshStar.Reload(_GenBufferStar());

            _shSkyElement = new ShaderSkyElement(gl);
            _shSkyStar = new ShaderSkyStar(gl);
            _renderMvk = renderMvk;
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
                        x0, y0, z0, 1,
                        x1, 0, 0, 0,
                        x, y, z, 1
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
            base.Update();

            if (_player.PosY < BiomeIsland.HeightWater)
            {
                _colorDown = new Vector3(0);
            }

            if (_player.GetWorld().Settings.Calendar is Сalendar32 сalendar)
            {
                float celestialAngle = сalendar.GetCelestialAngle();
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
                _matSun = Mat4.Identity();
                _matSun.RotateX(Сalendar32.AngleSunTimeYear[сalendar.TimeYearIndex]);
                _matSun.RotateZ(celestialAngle * Glm.Pi360);
                _matSun.Translate(0, 64f + (сalendar.GetSunLight() + sunLightAdd) * 30f, 0);

                _matStar = Mat4.Identity();
                _matStar.RotateX(Glm.Pi45);
                _matStar.RotateZ(celestialAngle * Glm.Pi360);

                _matMoon = new Mat4(_matStar);
                _matMoon.RotateY(celestialAngle * Glm.Pi90 + 2.4f); // Чтоб луна была в горизонте читабельная фазе
                _matMoon.Translate(0, -112f, 0);

                if (_moonPhaseIndexPrev != сalendar.MoonPhaseIndex)
                {
                    _moonPhaseIndexPrev = сalendar.MoonPhaseIndex;
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
            if (_player.GetWorld().Settings.Calendar is Сalendar32 сalendar)
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
                if (сalendar.GetSunLight() > 0)
                {
                    _renderMvk.BindTextureSun();
                    _shSkyElement.Bind();
                    _shSkyElement.SetUniform1("transparency", сalendar.GetSunLight());
                    _shSkyElement.SetUniformMatrix4("view", Gi.MatrixView);
                    _shSkyElement.SetUniformMatrix4("model", _matSun.ToArray());
                    _meshSun.Draw();
                }

                // Звёзды и луна
                if (сalendar.StarLight > 0)
                {
                    _shSkyStar.Bind();
                    _shSkyStar.SetUniform1("transparency", сalendar.StarLight);
                    _shSkyStar.SetUniformMatrix4("view", Gi.MatrixView);
                    _shSkyStar.SetUniformMatrix4("model", _matStar.ToArray());
                    _shSkyStar.SetUniform3("color", _starRand1, _starRand2, _starRand3);
                    _meshStar.Draw();

                    _renderMvk.BindTextureMoon();
                    _shSkyElement.Bind();
                    _shSkyElement.SetUniform1("transparency", сalendar.StarLight + .15f);
                    _shSkyElement.SetUniformMatrix4("view", Gi.MatrixView);
                    _shSkyElement.SetUniformMatrix4("model", _matMoon.ToArray());
                    _meshMoon.Draw();
                }

                gl.BlendFuncSeparate(GL.GL_SRC_ALPHA, GL.GL_ONE_MINUS_SRC_ALPHA, GL.GL_ONE, GL.GL_ZERO);
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            _meshSunset.Dispose();
            _meshSun.Dispose();
            _meshMoon.Dispose();
            _meshStar.Dispose();
            _shSkyElement.Delete();
            _shSkyStar.Delete();
        }
    }
}
