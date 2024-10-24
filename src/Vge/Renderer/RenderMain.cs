﻿using Vge.Games;
using Vge.Renderer.Font;
using Vge.Renderer.Shaders;
using Vge.Util;
using WinGL.OpenGL;
using WinGL.Util;

namespace Vge.Renderer
{
    /// <summary>
    /// Основной класс рендера
    /// </summary>
    public class RenderMain : Warp
    {
        /// <summary>
        /// Основной шрифт
        /// </summary>
        public FontBase FontMain { get; private set; }

        /// <summary>
        /// Объект текстур
        /// </summary>
        public readonly TextureMap Texture;
        /// <summary>
        /// Шейдоры для GUI цветных текстур без смещения
        /// </summary>
        public readonly ShaderGuiColor ShGuiColor;
        /// <summary>
        /// Шейдоры для GUI линий с альфа цветом
        /// </summary>
        public readonly ShaderGuiLine ShGuiLine;
        /// <summary>
        /// Шейдоры для вокселей
        /// </summary>
        public readonly ShaderVoxel ShVoxel;

        /// <summary>
        /// Время выполнения кадра
        /// </summary>
        private float _speedFrameAll;
        /// <summary>
        /// Время выполнения такта
        /// </summary>
        private float _speedTickAll;
        /// <summary>
        /// Счётчик времени кратно секундам в мс
        /// </summary>
        private long _timeSecond;
        /// <summary>
        /// Количество фпс
        /// </summary>
        private int _fps;
        /// <summary>
        /// Количество тпс
        /// </summary>
        private int _tps;
        /// <summary>
        /// Время перед начало прорисовки кадра
        /// </summary>
        private long _timeBegin;

        public RenderMain(WindowMain window) : base(window)
        {
            Texture = new TextureMap(gl);
            ShGuiColor = new ShaderGuiColor(gl);
            ShGuiLine = new ShaderGuiLine(gl);
            ShVoxel = new ShaderVoxel(gl);
        }

        /// <summary>
        /// Стартовая инициализация до загрузчика
        /// </summary>
        public void InitializeFirst()
        {
            _SetTextureSplash(Options.PathTextures + "Splash.png");
        }

        public override void Dispose()
        {
            ShGuiColor.Delete(gl);
            ShGuiLine.Delete(gl);
            ShVoxel.Delete(gl);
        }

        #region ShaderBind

        /// <summary>
        /// Связать шейдер GuiLine
        /// </summary>
        public void ShaderBindGuiLine()
        {
            ShGuiLine.Bind(gl);
            ShGuiLine.SetUniformMatrix4(gl, "projview", window.Ortho2D);
        }

        /// <summary>
        /// Связать шейдер GuiColor
        /// </summary>
        public void ShaderBindGuiColor()
        {
            ShGuiColor.Bind(gl);
            ShGuiColor.SetUniformMatrix4(gl, "projview", window.Ortho2D);
        }

        /// <summary>
        /// Связать шейдер Voxels
        /// </summary>
        /// <param name="torchInHand">0-15 яркость в руке</param>
        public void ShaderBindVoxels(float[] view, short overview, 
            float colorFogR, float colorFogG, float colorFogB, byte torchInHand)
        {
            ShVoxel.Bind(gl);
            ShVoxel.SetUniformMatrix4(gl, "view", view);
            ShVoxel.SetUniform1(gl, "takt", window.Game.TickCounter);
            ShVoxel.SetUniform1(gl, "overview", overview);
            ShVoxel.SetUniform3(gl, "colorfog", colorFogR, colorFogG, colorFogB);
            ShVoxel.SetUniform1(gl, "torch", torchInHand);

            int atlas = ShVoxel.GetUniformLocation(gl, "atlas");
            int lightMap = ShVoxel.GetUniformLocation(gl, "light_map");
            BindTextureAtlasBlocks();
            gl.Uniform1(atlas, 0);
            TextureLightmapEnable();
            gl.Uniform1(lightMap, 1);
            TextureLightmapDisable();
        }

        #endregion

        #region Texture

        /// <summary>
        /// Включить текстуру
        /// </summary>
        public void TextureEnable() => gl.Enable(GL.GL_TEXTURE_2D);
        /// <summary>
        /// Выключить текстуру
        /// </summary>
        public void TextureDisable() => gl.Disable(GL.GL_TEXTURE_2D);

        /// <summary>
        /// Запустить текстуру заставки
        /// </summary>
        public void BindTextureSplash() => Texture.BindSplash();
        /// <summary>
        /// Удалить текстуру заставки
        /// </summary>
        public void DeleteTextureSplash() => Texture.DeleteSplash();
        /// <summary>
        /// Запустить текстуру основного виджета
        /// </summary>
        public void BindTextureWidgets() => Texture.BindTexture(1);
        /// <summary>
        /// Запустить текстуру атласа блоков
        /// </summary>
        public void BindTextureAtlasBlocks() => Texture.BindTexture(2);

        /// <summary>
        /// Запустить текстуру, указав индекс текстуры массива
        /// </summary>
        public void BindTexture(int index, uint texture = 0) => Texture.BindTexture(index, texture);

        /// <summary>
        /// Создать текстуру основного шрифта
        /// </summary>
        public void CreateTextureFontMain(BufferedImage buffered) => FontMain = new FontBase(buffered, 1, this, 0);

        /// <summary>
        /// Активировать мульти текстуру освещения
        /// </summary>
        public void TextureLightmapEnable()
        {
            gl.ActiveTexture(GL.GL_TEXTURE1);
            TextureEnable();
            gl.ActiveTexture(GL.GL_TEXTURE0);
        }

        /// <summary>
        /// Деактивировать мульти текстуру освещения
        /// </summary>
        public void TextureLightmapDisable()
        {
            gl.ActiveTexture(GL.GL_TEXTURE1);
            TextureDisable();
            gl.ActiveTexture(GL.GL_TEXTURE0);
        }

        /// <summary>
        /// Задать текстуру заставки
        /// </summary>
        private void _SetTextureSplash(string fileName)
            => Texture.SetSplash(BufferedFileImage.FileToBufferedImage(fileName));

        #endregion

        /// <summary>
        /// На финише загрузка в основном потоке
        /// </summary>
        /// <param name="buffereds">буфер всех текстур для биндинга</param>
        public virtual void AtFinishLoading(BufferedImage[] buffereds)
        {
            Texture.SetCount(buffereds.Length);
            for (int i = 0; i < buffereds.Length; i++)
            {
                Texture.SetTexture(i, buffereds[i]);
            }
            FontMain.CreateMesh(gl);
        }

        public void TestRun()
        {
        //    gl.Enable(GL.GL_CULL_FACE);
            gl.PolygonMode(GL.GL_FRONT_AND_BACK, GL.GL_FILL);
            //gl.ClearColor(.5f, .7f, .99f, 1f);

            // Код с фиксированной функцией может использовать альфа-тестирование
            // Чтоб корректно прорисовывался кактус
            gl.AlphaFunc(GL.GL_GREATER, 0.1f);
            gl.Enable(GL.GL_ALPHA_TEST);
        }

        #region Draw

        public virtual void DrawBegin()
        {
            _fps++;
            Debug.MeshCount = 0;
            _timeBegin = window.TimeTicks();
        }

        /// <summary>
        /// После прорисовки каждого кадра OpenGL
        /// </summary>
        public virtual void DrawEnd()
        {
            // Перерасчёт кадров раз в секунду, и среднее время прорисовки кадра
            if (Ce.IsDebugDraw)
            {
                if (window.Time() >= _timeSecond)
                {
                    float speedTick = 0;
                    if (_tps > 0) speedTick = _speedTickAll / _tps;
                    window.debug.SetTpsFps(_fps, _speedFrameAll / _fps, _tps, speedTick);

                    _timeSecond += 1000;
                    _speedFrameAll = 0;
                    _speedTickAll = 0;
                    _fps = 0;
                    _tps = 0;
                }
                _speedFrameAll += (float)(window.TimeTicks() - _timeBegin) / Ticker.TimerFrequency;
            }
        }

        /// <summary>
        /// Время выполнения такта в мс
        /// </summary>
        public void SetExecutionTime(float time)
        {
            _speedTickAll += time;
            _tps++;
        }

        #endregion
    }
}
