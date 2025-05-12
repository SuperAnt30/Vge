using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Vge.Renderer.Font;
using Vge.Renderer.Shaders;
using Vge.Renderer.World;
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
        /// Текстурная карта освещения
        /// </summary>
        public TextureLightMap LightMap { get; protected set; }

        /// <summary>
        /// Объект текстур
        /// </summary>
        protected readonly TextureMap _texture;
        /// <summary>
        /// Переменные индексов текстур GUI
        /// </summary>
        protected TextureIndex _textureIndex;

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
        /// Шейдоры для 3d линий с альфа цветом
        /// </summary>
        public readonly ShaderLine ShLine;
        /// <summary>
        /// Шейдоры для сущностей
        /// </summary>
        public readonly ShaderEntity ShEntity;
        /// <summary>
        /// Шейдоры для примитивных сущностей без скелетной анимации
        /// </summary>
        public readonly ShaderEntityPrimitive ShEntityPrimitive;

        /// <summary>
        /// Время выполнения кадра
        /// </summary>
        private float _speedFrameAll;
        /// <summary>
        /// Время выполнения такта
        /// </summary>
        private float _speedTickAll;
        /// <summary>
        /// Максимальное время выполнения такта
        /// </summary>
        private float _speedTickMax;
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
            _texture = new TextureMap(gl);
            ShGuiColor = new ShaderGuiColor(gl);
            ShGuiLine = new ShaderGuiLine(gl);
            ShVoxel = new ShaderVoxel(gl);
            ShEntity = new ShaderEntity(gl);
            ShEntityPrimitive = new ShaderEntityPrimitive(gl);
            ShLine = new ShaderLine(gl);
            _Initialize();
        }

        /// <summary>
        /// Стартовая инициализация до загрузчика
        /// </summary>
        public void InitializeFirst()
        {
            _SetTextureSplash(Options.PathTextures + "Splash.png");
        }

        /// <summary>
        /// Инициализация различных библиотек
        /// </summary>
        protected virtual void _Initialize()
        {
            _textureIndex = new TextureIndex();
            LightMap = new TextureLightMap(gl);
        }

        public override void Dispose()
        {
            ShGuiColor.Delete(gl);
            ShGuiLine.Delete(gl);
            ShVoxel.Delete(gl);
            ShEntity.Delete(gl);
            ShEntityPrimitive.Delete(gl);
            ShLine.Delete(gl);
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
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        /// <param name="torchInHand">0-15 яркость в руке</param>
        public void ShaderBindVoxels(float[] view, float timeIndex, int overview, 
            float colorFogR, float colorFogG, float colorFogB, byte torchInHand)
        {
            ShVoxel.Bind(gl);
            ShVoxel.SetUniformMatrix4(gl, "view", view);
            ShVoxel.SetUniform1(gl, "takt", (int)window.Game.TickCounter);
            ShVoxel.SetUniform1(gl, "overview", (float)overview);
            ShVoxel.SetUniform3(gl, "colorfog", colorFogR, colorFogG, colorFogB);
            ShVoxel.SetUniform1(gl, "torch", (float)torchInHand);
            ShVoxel.SetUniform1(gl, "animOffset", Ce.ShaderAnimOffset);
            // Ветер, значение от -1 до 1
            int wind = (int)window.Time() / 48 & 0x7F;
            ShVoxel.SetUniform1(gl, "wind", Glm.Cos((wind + timeIndex) * .049f) * .16f);

            //// Активация текстуры атласа с размытостью (с Mipmap)
            //int atlasBlurry = ShVoxel.GetUniformLocation(gl, "atlas_blurry");
            //BindTextureAtlasBlurry();
            //gl.Uniform1(atlasBlurry, 0);

            //// Активация текстуры атласа с резкостью (без Mipmap)
            //BindTextureAtlasSharpness();
            //int atlasSharpness = ShVoxel.GetUniformLocation(gl, "atlas_sharpness");
            //gl.Uniform1(atlasSharpness, 1);

            //// Активация текстуры карты света
            //LightMap.BindTexture();
            //int lightMap = ShVoxel.GetUniformLocation(gl, "light_map");
            //gl.Uniform1(lightMap, 2);
        }

        /// <summary>
        /// Биндим атлас текстур
        /// </summary>
        public void BindTextureAtlasBlocks()
        {
            // Активация текстуры атласа с размытостью (с Mipmap)
            int atlasBlurry = ShVoxel.GetUniformLocation(gl, "atlas_blurry");
            BindTextureAtlasBlurry();
            gl.Uniform1(atlasBlurry, 0);

            // Активация текстуры атласа с резкостью (без Mipmap)
            BindTextureAtlasSharpness();
            int atlasSharpness = ShVoxel.GetUniformLocation(gl, "atlas_sharpness");
            gl.Uniform1(atlasSharpness, 1);

            // Активация текстуры карты света
            LightMap.BindTexture();
            int lightMap = ShVoxel.GetUniformLocation(gl, "light_map");
            gl.Uniform1(lightMap, 2);
        }

        /// <summary>
        /// Связать шейдер Entity
        /// </summary>
        public void ShaderBindEntity(float[] view, float posX, float posY, float posZ, float[] m)
        {
            ShEntity.Bind(gl);
            ShEntity.SetUniformMatrix4(gl, "view", view);
            ShEntity.SetUniform3(gl, "pos", posX, posY, posZ);
            ShEntity.SetUniformMatrix4x3(gl, "elementTransforms", m, Ce.MaxAnimatedBones);
        }

        /// <summary>
        /// Связать шейдер примитивных сущностей без скелетной анимации
        /// </summary>
        public void ShaderBindEntityPrimitive(float[] view, 
            float posX, float posY, float posZ)
            //float[] modelMatrix)
        {
            ShEntityPrimitive.Bind(gl);
            ShEntityPrimitive.SetUniformMatrix4(gl, "view", view);
            ShEntityPrimitive.SetUniform3(gl, "pos", posX, posY, posZ);
            //ShEntityPrimitive.SetUniformMatrix4(gl, "modelMatrix", modelMatrix);
        }

        /// <summary>
        /// Связать шейдер Line
        /// </summary>
        public void ShaderBindLine(float[] view, float posX, float posY, float posZ)
        {
            ShLine.Bind(gl);
            ShLine.SetUniformMatrix4(gl, "view", view);
            ShLine.SetUniform3(gl, "pos", posX, posY, posZ);
        }

        #endregion

        #region TextureSplash

        /// <summary>
        /// Запустить текстуру заставки
        /// </summary>
        public void BindTextureSplash()
        {
            if (_textureIndex.Splash != 0)
            {
                _texture.BindTexture(_textureIndex.Splash);
            }
        }
        /// <summary>
        /// Удалить текстуру заставки
        /// </summary>
        public void DeleteTextureSplash()
        {
            _texture.DeleteTexture(_textureIndex.Splash);
            _textureIndex.Splash = 0;
        }

        /// <summary>
        /// Задать текстуру заставки
        /// </summary>
        private void _SetTextureSplash(string fileName)
            => _textureIndex.Splash = _texture.SetTexture(BufferedFileImage.FileToBufferedImage(fileName));

        #endregion

        #region Texture

        /// <summary>
        /// Включить текстуру
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TextureEnable() => gl.Enable(GL.GL_TEXTURE_2D);
        /// <summary>
        /// Выключить текстуру
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TextureDisable() => gl.Disable(GL.GL_TEXTURE_2D);
        /// <summary>
        /// Запустить текстуру основного виджета
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void BindTextureWidgets() => _texture.BindTexture(_textureIndex.Widgets);
        
        /// <summary>
        /// Запустить текстуру атласа размытых блоков
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void BindTextureAtlasBlurry() => _texture.BindTexture(_textureIndex.AtlasBlurry);
        /// <summary>
        /// Запустить текстуру атласа блоков с чёткой резкостью
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void BindTextureAtlasSharpness() => _texture.BindTexture(_textureIndex.AtlasSharpness, 1);

        /// <summary>
        /// Удалить текстуру атласов
        /// </summary>
        public void DeleteTextureAtlases()
        {
            _texture.DeleteTexture(_textureIndex.AtlasBlurry);
            _textureIndex.AtlasBlurry = 0;
            _texture.DeleteTexture(_textureIndex.AtlasSharpness);
            _textureIndex.AtlasSharpness = 0;
        }

        /// <summary>
        /// Задать текстуру атласа размытых блоков
        /// </summary>
        public void AddTextureAtlasBlurry(BufferedImage bufferedImage)
            => _textureIndex.AtlasBlurry = _texture.SetTexture(bufferedImage);

        /// <summary>
        /// Задать текстуру атласа блоков с чёткой резкостью
        /// </summary>
        public void AddTextureAtlasSharpness(BufferedImage bufferedImage)
            => _textureIndex.AtlasSharpness = _texture.SetTexture(bufferedImage);

        /// <summary>
        /// Задать текстуру
        /// </summary>
        public uint SetTexture(BufferedImage bufferedImage) => _texture.SetTexture(bufferedImage);

        /// <summary>
        /// Запустить текстуру, указав индекс текстуры массива
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void BindTexture(uint index, uint texture = 0) => _texture.BindTexture(index, texture);

        /// <summary>
        /// Создать текстуру основного шрифта
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CreateTextureFontMain(BufferedImage buffered) => FontMain = new FontBase(buffered, 1, this);

        #endregion

        /// <summary>
        /// На финише загрузка в основном потоке
        /// </summary>
        /// <param name="buffereds">буфер всех текстур для биндинга</param>
        public virtual void AtFinishLoading(Dictionary<string, BufferedImage> buffereds)
        {
            if (buffereds.ContainsKey(EnumTexture.FontMain.ToString()))
            {
                FontMain.CreateMesh(gl, _texture.SetTexture(buffereds[EnumTexture.FontMain.ToString()]));
            }
            if (buffereds.ContainsKey(EnumTexture.Widgets.ToString()))
            {
                _textureIndex.Widgets = _texture.SetTexture(buffereds[EnumTexture.Widgets.ToString()]);
            }
        }

        #region Draw

        /// <summary>
        /// Прорисовка в начале мира
        /// </summary>
        public virtual void DrawWorldBegin()
        {
            gl.Enable(GL.GL_CULL_FACE);
            gl.PolygonMode(GL.GL_FRONT_AND_BACK, GL.GL_FILL);
            //gl.ClearColor(.5f, .7f, .99f, 1f);

            // Код с фиксированной функцией может использовать альфа-тестирование
            // Чтоб корректно прорисовывался кактус
            gl.Enable(GL.GL_ALPHA_TEST);
            gl.AlphaFunc(GL.GL_GREATER, 0.1f);
        }

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
                    int countChunk = Debug.CountUpdateChunck;
                    Debug.CountUpdateChunck = 0;
                    int countChunkAlpha = Debug.CountUpdateChunckAlpha;
                    Debug.CountUpdateChunckAlpha = 0;
                    float speedTick = 0;
                    if (_tps > 0) speedTick = _speedTickAll / _tps;
                    window.debug.SetTpsFps(_fps, _speedFrameAll / _fps, _tps, speedTick, _speedTickMax,
                        countChunk, countChunkAlpha);

                    _speedTickMax = 0;
                    _timeSecond += 1000;
                    _speedFrameAll = 0;
                    _speedTickAll = 0;
                    _fps = 0;
                    _tps = 0;
                }
                float timeFrame = (float)(window.TimeTicks() - _timeBegin) / Ticker.TimerFrequency;
                _speedFrameAll += timeFrame;
                Debug.FrizFps += timeFrame < 4 ? "." : timeFrame < 8 ? "," : timeFrame < 17 ? ":" : "|";
                if (Debug.FrizFps.Length > 240) Debug.FrizFps = Debug.FrizFps.Substring(1);
                //System.Console.Write(" MX:");
                if (timeFrame > 8)
                {
                    System.Console.WriteLine(timeFrame);
                }

                //_speedFrameAll += (float)(window.TimeTicks() - _timeBegin) / Ticker.TimerFrequency;
            }
        }

        /// <summary>
        /// Время выполнения такта в мс
        /// </summary>
        public void SetExecutionTime(float time)
        {
            _speedTickAll += time;
            if (time > _speedTickMax) _speedTickMax = time;
            _tps++;
        }

        #endregion
    }
}
