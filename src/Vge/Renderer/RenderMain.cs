using System;
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
        public readonly TextureMap textureMap;
        /// <summary>
        /// Шейдоры для GUI цветных текстур без смещения
        /// </summary>
        public readonly ShaderGuiColor shaderGuiColor;
        /// <summary>
        /// Шейдоры для GUI линий с альфа цветом
        /// </summary>
        public readonly ShaderGuiLine shaderGuiLine;

        /// <summary>
        /// Время выполнения кадра
        /// </summary>
        private float speedFrameAll;
        /// <summary>
        /// Время выполнения такта
        /// </summary>
        private float speedTickAll;
        /// <summary>
        /// Счётчик времени кратно секундам в мс
        /// </summary>
        private long timeSecond;
        /// <summary>
        /// Количество фпс
        /// </summary>
        private int fps;
        /// <summary>
        /// Количество тпс
        /// </summary>
        private int tps;
        /// <summary>
        /// Время перед начало прорисовки кадра
        /// </summary>
        private long timeBegin;

        public RenderMain(WindowMain window) : base(window)
        {
            textureMap = new TextureMap(gl);
            shaderGuiColor = new ShaderGuiColor(gl);
            shaderGuiLine = new ShaderGuiLine(gl);
        }

        /// <summary>
        /// Стартовая инициализация до загрузчика
        /// </summary>
        public void InitializeFirst()
        {
            SetTextureSplash(Options.PathTextures + "Splash.png");
        }

        public override void Dispose()
        {
            shaderGuiColor.Delete(gl);
            shaderGuiLine.Delete(gl);
        }

        #region ShaderBind

        /// <summary>
        /// Связать шейдер GuiLine
        /// </summary>
        public void ShaderBindGuiLine()
        {
            shaderGuiLine.Bind(gl);
            shaderGuiLine.SetUniformMatrix4(gl, "projview", window.Ortho2D);
        }

        /// <summary>
        /// Связать шейдер GuiColor
        /// </summary>
        public void ShaderBindGuiColor()
        {
            shaderGuiColor.Bind(gl);
            shaderGuiColor.SetUniformMatrix4(gl, "projview", window.Ortho2D);
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
        public void BindTextureSplash() => textureMap.BindSplash();
        /// <summary>
        /// Удалить текстуру заставки
        /// </summary>
        public void DeleteTextureSplash() => textureMap.DeleteSplash();
        /// <summary>
        /// Запустить текстуру основного виджета
        /// </summary>
        public void BindTextureWidgets() => textureMap.BindTexture(1);

        /// <summary>
        /// Запустить текстуру, указав индекс текстуры массива
        /// </summary>
        public void BindTexture(int index, uint texture = 0) => textureMap.BindTexture(index, texture);

        /// <summary>
        /// Создать текстуру основного шрифта
        /// </summary>
        public void CreateTextureFontMain(BufferedImage buffered) => FontMain = new FontBase(buffered, 1, this, 0);

        /// <summary>
        /// Задать текстуру заставки
        /// </summary>
        private void SetTextureSplash(string fileName)
            => textureMap.SetSplash(BufferedFileImage.FileToBufferedImage(fileName));

        #endregion

        /// <summary>
        /// На финише загрузка в основном потоке
        /// </summary>
        /// <param name="buffereds">буфер всех текстур для биндинга</param>
        public virtual void AtFinishLoading(BufferedImage[] buffereds)
        {
            textureMap.SetCount(buffereds.Length);
            for (int i = 0; i < buffereds.Length; i++)
            {
                textureMap.SetTexture(i, buffereds[i]);
            }
            FontMain.CreateMesh(gl);
        }

        #region Draw

        /// <summary>
        /// Метод для прорисовки кадра
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        public override void Draw(float timeIndex) 
        {
            DrawBegin();
            if (window.Game == null)
            {
                // Нет игры
                if (window.Screen == null)
                {
                    // Отсутствует прорисовка
                    throw new Exception(Sr.ThereIsNoDrawing);
                }
                else
                {
                    window.Screen.Draw(timeIndex);
                }
            }
            else
            {
                // Есть игра
                window.Game.Draw(timeIndex);
                if (window.Screen != null)
                {
                    window.Screen.Draw(timeIndex);
                }
            }
            DrawEnd();
        }

        public virtual void DrawBegin()
        {
            fps++;
            timeBegin = window.TimeTicks();
        }

        /// <summary>
        /// После прорисовки каждого кадра OpenGL
        /// </summary>
        public virtual void DrawEnd()
        {
            // Перерасчёт кадров раз в секунду, и среднее время прорисовки кадра
            if (window.Time() >= timeSecond)
            {
                float speedTick = 0;
                if (tps > 0) speedTick = speedTickAll / tps;
                window.debug.SetTpsFps(fps, speedFrameAll / fps, tps, speedTick);

                timeSecond += 1000;
                speedFrameAll = 0;
                speedTickAll = 0;
                fps = 0;
                tps = 0;
            }

            speedFrameAll += (float)(window.TimeTicks() - timeBegin) / Ticker.TimerFrequency;
        }

        /// <summary>
        /// Время выполнения такта в мс
        /// </summary>
        public void SetExecutionTime(float time)
        {
            speedTickAll += time;
            tps++;
        }

        #endregion
    }
}
