using Mvk2.Renderer.Mesh;
using Mvk2.Renderer.Shaders;
using System;
using System.Collections.Generic;
using Vge.Entity.Player;
using Vge.Renderer.Mesh;
using Vge.Renderer.World;
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
        /// Шейдор неба
        /// </summary>
        protected readonly ShaderSkyElement _shSkyElement;

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
        /// Фаза луны прошлого такта
        /// </summary>
        private int _moonPhaseIndexPrev = -1;

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
           
            _shSkyElement = new ShaderSkyElement(gl);
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
        /// Изменён обзор чанков
        /// </summary>
        public override void ModifyOverviewChunk(int overviewBlock)
        {
            base.ModifyOverviewChunk(overviewBlock);
            //_meshSun.Reload(_GenBufferSun());
        }

        /// <summary>
        /// Игровой такт
        /// </summary>
        public override void Update()
        {
            base.Update();
            if (_player.GetWorld().Settings.Calendar is Сalendar32 сalendar)
            {
                float celestialAngle = сalendar.GetCelestialAngle();
                _colors = _CalcSunriseSunsetColors(celestialAngle);

                // Параметра для размера солнца, растояние от глаз 64 - 128
                float sunLight = сalendar.GetSunLight();
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
                _matSun.Translate(0, 64f + (sunLight + sunLightAdd) * 32f, 0);

                _matMoon = Mat4.Identity();
                _matMoon.RotateX(Glm.Pi45);
                _matMoon.RotateZ(celestialAngle * Glm.Pi360);
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

            // Солнце
            gl.BlendFuncSeparate(GL.GL_SRC_ALPHA, GL.GL_ONE, GL.GL_ONE, GL.GL_ZERO);
            _renderMvk.BindTextureSun();
            _shSkyElement.Bind();
            _shSkyElement.SetUniformMatrix4("view", Gi.MatrixView);
            _shSkyElement.SetUniformMatrix4("model", _matSun.ToArray());
            _meshSun.Draw();

            // Звёзды и луна
            _renderMvk.BindTextureMoon();
            _shSkyElement.SetUniformMatrix4("model", _matMoon.ToArray());
            _meshMoon.Draw();

            gl.BlendFuncSeparate(GL.GL_SRC_ALPHA, GL.GL_ONE_MINUS_SRC_ALPHA, GL.GL_ONE, GL.GL_ZERO);
        }

        public override void Dispose()
        {
            base.Dispose();
            _meshSunset.Dispose();
            _meshSun.Dispose();
            _meshMoon.Dispose();
            _shSkyElement.Delete();
        }
    }
}
