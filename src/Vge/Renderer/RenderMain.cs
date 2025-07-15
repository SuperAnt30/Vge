using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Vge.Renderer.Font;
using Vge.Renderer.Shaders;
using Vge.Renderer.World;
using Vge.Util;
using WinGL.Util;

namespace Vge.Renderer
{
    /// <summary>
    /// Основной класс рендера, он же клиентский основной объект
    /// Есть возможность мода
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
        public readonly TextureMap Texture;
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
        /// Шейдоры для 3d линий с альфа цветом
        /// </summary>
        public readonly ShaderLine ShLine;
        /// <summary>
        /// Шейдоры для вокселей
        /// </summary>
        public readonly ShaderVoxel ShVoxel;
        /// <summary>
        /// Шейдоры для сущностей
        /// </summary>
        public readonly ShaderEntity ShEntity;
        /// <summary>
        /// Шейдоры для вокселей карты теней 
        /// </summary>
        public readonly ShaderVoxel ShVoxelShadowMap;
        /// <summary>
        /// Шейдоры для сущностей карты теней
        /// </summary>
        public readonly ShaderEntity ShEntityShadowMap;
        /// <summary>
        /// Шейдоры для отладки карты теней
        /// </summary>
        public readonly ShaderShadowMap ShShadowMap;

        /// <summary>
        /// Активный шейдор для сущности, обычный или для теней
        /// </summary>
        private ShaderEntity _shEntityAction;
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
            Texture = new TextureMap(gl);
            ShGuiColor = new ShaderGuiColor(gl, "GuiColor");
            ShGuiLine = new ShaderGuiLine(gl);
            ShLine = new ShaderLine(gl);
            ShVoxel = new ShaderVoxel(gl, "Voxel");
            ShEntity = new ShaderEntity(gl, false, "Entity");
            ShVoxelShadowMap = new ShaderVoxel(gl, "VoxelShadow");
            ShEntityShadowMap = new ShaderEntity(gl, true, "EntityShadow");
            ShShadowMap = new ShaderShadowMap(gl);

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
            ShGuiColor.Delete();
            ShGuiLine.Delete();
            ShVoxel.Delete();
            ShEntity.Delete();
            ShLine.Delete();
            ShShadowMap.Delete();
            ShVoxelShadowMap.Delete();
            ShEntityShadowMap.Delete();
        }

        #region ShaderBind

        /// <summary>
        /// Активный шейдор для сущности, обычный или для теней
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ShaderEntity GetShaderEntityAction() => _shEntityAction;

        /// <summary>
        /// Активировать обычный шейдор
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ShaderEntityAction() => _shEntityAction = ShEntity;

        /// <summary>
        /// Активировать шейдор для теней
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ShaderEntityActionShadowMap() => _shEntityAction = ShEntityShadowMap;

        /// <summary>
        /// Получить массив матрицы для проецирования двумерных координат на экран
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float[] GetOrtho2D() => window.Ortho2D;

        /// <summary>
        /// Связать шейдер GuiLine
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ShaderBindGuiLine()
        {
            ShGuiLine.Bind();
            ShGuiLine.SetUniformMatrix4("projview", window.Ortho2D);
        }

        /// <summary>
        /// Связать шейдер GuiColor
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ShaderBindGuiColor()
        {
            ShGuiColor.Bind();
            ShGuiColor.SetUniformMatrix4("projview", window.Ortho2D);
        }

        /// <summary>
        /// Связать шейдер Voxels
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        /// <param name="torchInHand">0-15 яркость в руке</param>
        public void ShaderBindVoxels(float[] view, float timeIndex, int overview, 
            float colorFogR, float colorFogG, float colorFogB, byte torchInHand)
        {
            ShVoxel.Bind();
            ShVoxel.SetUniformMatrix4("view", view);
            ShVoxel.SetUniform1("takt", (int)window.Game.TickCounter);
            ShVoxel.SetUniform1("overview", (float)overview);
            ShVoxel.SetUniform3("colorfog", colorFogR, colorFogG, colorFogB);
            ShVoxel.SetUniform1("torch", (float)torchInHand);
            ShVoxel.SetUniform1("animOffset", Ce.ShaderAnimOffset);
            // Ветер, значение от -1 до 1
            int wind = (int)window.Time() / 48 & 0x7F;
            ShVoxel.SetUniform1("wind", Glm.Cos((wind + timeIndex) * .049f) * .16f);
            ShVoxel.SetUniform3("player", window.Game.Player.PosFrameX,
                window.Game.Player.PosFrameY,
                window.Game.Player.PosFrameZ);
        }

        /// <summary>
        /// Связать шейдер Voxels для карты теней
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        public void ShaderBindVoxelsShadowMap(float[] view, float timeIndex)
        {
            ShVoxelShadowMap.Bind();
            ShVoxelShadowMap.SetUniformMatrix4("view", view);
            ShVoxelShadowMap.SetUniform1("takt", (int)window.Game.TickCounter);
            ShVoxelShadowMap.SetUniform1("animOffset", Ce.ShaderAnimOffset);
            // Ветер, значение от -1 до 1
            int wind = (int)window.Time() / 48 & 0x7F;
            ShVoxelShadowMap.SetUniform1("wind", Glm.Cos((wind + timeIndex) * .049f) * .16f);
            ShVoxelShadowMap.SetUniform3("player", window.Game.Player.PosFrameX,
                window.Game.Player.PosFrameY,
                window.Game.Player.PosFrameZ);
        }

        /// <summary>
        /// Связать шейдер Line
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ShaderBindLine(float[] view, float posX, float posY, float posZ)
        {
            ShLine.Bind();
            ShLine.SetUniformMatrix4("view", view);
            ShLine.SetUniform3("pos", posX, posY, posZ);
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
                Texture.BindTexture(_textureIndex.Splash);
            }
        }
        /// <summary>
        /// Удалить текстуру заставки
        /// </summary>
        public void DeleteTextureSplash()
        {
            Texture.DeleteTexture(_textureIndex.Splash);
            _textureIndex.Splash = 0;
        }

        /// <summary>
        /// Задать текстуру заставки
        /// </summary>
        private void _SetTextureSplash(string fileName)
            => _textureIndex.Splash = Texture.SetTexture(BufferedFileImage.FileToBufferedImage(fileName));

        #endregion

        #region Texture

        /// <summary>
        /// Запустить текстуру основного виджета
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void BindTextureWidgets() => Texture.BindTexture(_textureIndex.Widgets);
        
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
                FontMain.CreateMesh(gl, Texture.SetTexture(buffereds[EnumTexture.FontMain.ToString()]));
            }
            if (buffereds.ContainsKey(EnumTexture.Widgets.ToString()))
            {
                _textureIndex.Widgets = Texture.SetTexture(buffereds[EnumTexture.Widgets.ToString()]);
            }
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
                //if (timeFrame > 8)
                //{
                //    System.Console.WriteLine(timeFrame);
                //}

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
