/// <summary>
/// StringsResource 
/// Строки ресурсов, на которые ссылается код
/// </summary>
internal sealed class SrMvk
{
    internal static string GetString(string name, params object[] args)
        => string.Format(name, args);

    internal const string SoundSampleMissing = "Отсутствует звуковой семпл [{0}]!";
}
