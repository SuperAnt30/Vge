using Vge.Util;

/// <summary>
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
        Si = (Options.SizeInterface > 1 && Height < 880) ? 1 : Options.SizeInterface;
    }
}
