/// <summary>
/// StringsResource 
/// Строки ресурсов, на которые ссылается код
/// </summary>
internal sealed class Sr
{
    internal static string GetString(string name, params object[] args)
        => string.Format(name, args);

    #region GL

    internal const string ExtensionFunctionIsNotSupported = "Функция расширения {0} не поддерживается";
    internal const string CantCreateAOpenGLRenderingContext = "Невозможно создать контекст рендеринга OpenGL";
    internal const string CantActivateTheOpenGLRenderingContext = "Невозможно активировать контекст рендеринга OpenGL {0}";

    #endregion

    #region Shader

    internal const string FailedToCompileShaderWithID = "Не удалось скомпилировать шейдер с ID {0}\r\n{1}";
    internal const string FailedToAssociateShaderProgramWithID = "Не удалось связать шейдерную программу с ID {0}";

    #endregion

    #region Glm

    internal const string OutOfRange = "Вне диапазона";

    #endregion

    #region Window

    internal const string ErrorWhileStartingWindow = "Ошибка при запуске окна";
    internal const string FailedToRegisterTheWindowClass = "Не удалось зарегистрировать класс окна";
    internal const string WindowCreationError = "Ошибка создания окна";
    internal const string CantCreateAOpenGLDeviceContext = "Невозможно создать контекст устройства OpenGL";
    internal const string CantFindASuitablePixelFormat = "Не удалось найти подходящий формат пикселя";
    internal const string CantSetThePixelFormat = "Невозможно установить PixelFormat";

    #endregion
}

