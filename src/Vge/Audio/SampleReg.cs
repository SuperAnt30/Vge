namespace Vge.Audio
{
    /// <summary>
    /// Структура семпла для регистрации
    /// </summary>
    public struct SampleReg
    {
        public readonly string Key;
        public readonly string Path;

        public SampleReg(string key, string path = "")
        {
            Key = key;
            Path = path + key + ".ogg";
        }
    }
}
