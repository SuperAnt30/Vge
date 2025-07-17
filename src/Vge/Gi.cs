using Vge.Renderer.World;
using Vge.Util;
using Vge.World.Block;
using WinGL.Util;

/// <summary>
/// GraphicalInterface
/// Класс статический данных для GUI
/// </summary>
public sealed class Gi
{
    /// <summary>
    /// Ширина окна
    /// </summary>
    public static int Width = 1280;
    /// <summary>
    /// Высота окна
    /// </summary>
    public static int Height = 720;

    /// <summary>
    /// Размер интерфейса
    /// </summary>
    public static int Si = 1;

    /// <summary>
    /// Ширина текстового окна чата
    /// </summary>
    public static int WindowsChatWidthMessage = 496;
    /// <summary>
    /// Высота текстового окна чата
    /// </summary>
    public static int WindowsChatHeightMessage = 270;

    #region ActiveTexture

    /// <summary>
    /// ID Активации текстуры карты света
    /// GL_TEXTURE0 + ActiveTextureLightMap
    /// </summary>
    public static int ActiveTextureLightMap = 5;
    /// <summary>
    /// ID Активации текстуры атласа с резкостью (без Mipmap)
    /// GL_TEXTURE0 + ActiveTextureAatlasSharpness
    /// </summary>
    public static int ActiveTextureAatlasSharpness = 6;
    /// <summary>
    /// ID Активации текстуры атласа с размытостью (с Mipmap)
    /// GL_TEXTURE0 + ActiveTextureAatlasBlurry
    /// </summary>
    public static int ActiveTextureAatlasBlurry = 7;
    /// <summary>
    /// ID Активации текстуры маленьких сэмплов сущностей
    /// GL_TEXTURE0 + ActiveTextureSamplerSmall
    /// </summary>
    public static int ActiveTextureSamplerSmall = 8;
    /// <summary>
    /// ID Активации текстуры больших сэмплов сущностей
    /// GL_TEXTURE0 + ActiveTextureSamplerBig
    /// </summary>
    public static int ActiveTextureSamplerBig = 9;
    /// <summary>
    /// ID Активации текстуры карты теней
    /// GL_TEXTURE0 + ActiveTextureShadowMap
    /// </summary>
    public static int ActiveTextureShadowMap = 10;

    #endregion

    /// <summary>
    /// Обновить размер инерфейса
    /// </summary>
    public static void UpdateSizeInterface()
    {
        Si = (Options.SizeInterface > 1 && Height < 960) ? 1 : Options.SizeInterface;
    }

    /// <summary>
    /// Цвет текста
    /// </summary>
    public readonly static Vector3 ColorText = new Vector3(.8f);
    /// <summary>
    /// Цвет текста где наведена мышка
    /// </summary>
    public readonly static Vector3 ColorTextEnter = new Vector3(.9f, .9f, .5f);
    /// <summary>
    /// Цвет неактивного текста
    /// </summary>
    public readonly static Vector3 ColorTextInactive = new Vector3(.5f);

    /// <summary>
    /// Перечень 16-ти цветов Красного как в майне
    /// </summary>
    public readonly static float[] ColorReg = new float[] { 0, 0, 0, 0, .67f, .67f, 1, .67f, .33f, .33f, .33f, .33f, 1, 1, 1, 1 };
    /// <summary>
    /// Перечень 16-ти цветов Зелёного как в майне
    /// </summary>
    public readonly static float[] ColorGreen = new float[] { 0, 0, .67f, .67f, 0, 0, .67f, .67f, .33f, .33f, 1, 1, .33f, .33f, 1, 1 };
    /// <summary>
    /// Перечень 16-ти цветов Синего как в майне
    /// </summary>
    public readonly static float[] ColorBlue = new float[] { 0, .67f, 0, .67f, 0, .67f, 0, .67f, .33f, 1, .33f, 1, .33f, 1, .33f, 1 };

    /// <summary>
    /// Затемнение стороны от стороны блока
    /// </summary>
    public readonly static float[] LightPoles = new float[] { 1, .5f, .6f, .6f, .8f, .8f };

    /// <summary>
    /// Буфер для склейки рендера сплошных блоков всего ряда
    /// 128000 / 4 = 32000 квадов
    /// </summary>
    public readonly static VertexBlockBuffer VertexDense = new VertexBlockBuffer(128000);
    /// <summary>
    /// Буфер для склейки рендера альфа блоков всего ряда
    /// 128000 / 4 = 32000 квадов
    /// </summary>
    public readonly static VertexBlockBuffer VertexAlpha = new VertexBlockBuffer(128000);

    /// <summary>
    /// Выбранный объект блока для рендера
    /// </summary>
    public static BlockBase Block;
    /// <summary>
    /// Объект рендера блока
    /// </summary>
    public readonly static BlockRenderFull BlockRendFull = new BlockRenderFull(VertexDense);
    /// <summary>
    /// Объект рендера блока с альфа прозрачностью
    /// </summary>
    public readonly static BlockRenderFull BlockAlphaRendFull = new BlockRenderFull(VertexAlpha);
    /// <summary>
    /// Объект рендера жидкого блока с альфа прозрачностью
    /// </summary>
    public readonly static BlockRenderLiquid BlockLiquidAlphaRendFull = new BlockRenderLiquid(VertexAlpha);

    /// <summary>
    /// Какой радиус для рендера псевдо чанков альфа блоков, при смещении больше 16 блоков
    /// </summary>
    public const int UpdateAlphaChunk = 4;
    /// <summary>
    /// Дистанция в чанках, для прорисовки сетки без MipMap, как правило это уникальные блоки
    /// </summary>
    public const int MaxDistanceNotMipMap = 12;

    /// <summary>
    /// Матрица просмотра Projection * LookAt
    /// </summary>
    public static readonly float[] MatrixView = new float[16];
    /// <summary>
    /// Матрица просмотра Projection * LookAt для карты теней, от солнца или луны
    /// </summary>
    public static readonly float[] MatrixViewDepthMap = new float[16];
}
