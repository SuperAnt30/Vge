namespace Vge.Audio
{
    /// <summary>
    /// Структура семпла для регистрации
    /// </summary>
    public struct SampleReg
    {
        public readonly string Key;
        public readonly string Path;

        public SampleReg(string key)
        {
            if (key.Contains("/"))
            {
                int index = key.LastIndexOf("/") + 1;
                Key = key.Substring(index, key.Length - index);
                Path = key + ".ogg";
            }
            else
            {
                Key = key;
                Path = key + ".ogg";
            }
        }
    }
}
