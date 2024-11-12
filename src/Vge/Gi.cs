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
    public readonly static float[] LightPoles = new float[] { 1, .6f, .7f, .7f, .85f, .85f };

    /// <summary>
    /// Буфер для склейки рендера сплошных блоков всего ряда
    /// 128000 / 4 = 32000 квадов
    /// </summary>
    public readonly static VertexBuffer VertexDense = new VertexBuffer(128000);
    /// <summary>
    /// Буфер для склейки рендера альфа блоков всего ряда
    /// 128000 / 4 = 32000 квадов
    /// </summary>
    public readonly static VertexBuffer VertexAlpha = new VertexBuffer(128000);
    /// <summary>
    /// Буфер одного альфа кэш блока
    /// 128 / 4 = 32 квадов
    /// </summary>
    public readonly static VertexBuffer VertexAlphaCache = new VertexBuffer(128);

    /// <summary>
    /// Объект рендера целого блока с эффектом размытия (Mipmap)
    /// </summary>
    public readonly static BlockRenderFull BlockRendFull = new BlockRenderFull(VertexDense);
    /// <summary>
    /// Объект рендера целого блока с эффектом размытия (Mipmap)
    /// </summary>
    public readonly static BlockRenderFull BlockAlphaRendFull = new BlockRenderFull(VertexAlpha);
    /// <summary>
    /// Объект рендера уникального блока с эффектом контраста (Not Mipmap)
    /// </summary>
    //public readonly static BlockRenderUnique BlockRendUnique = new BlockRenderUnique();
    /// <summary>
    /// Объект рендера жидкого блока с эффектом контраста (Not Mipmap)
    /// </summary>
    //public readonly static BlockRenderLiquid BlockRendLiquid = new BlockRenderLiquid();

    public static BlockBase Block;

}
