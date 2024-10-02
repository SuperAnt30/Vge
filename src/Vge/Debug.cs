namespace Vge
{
    /// <summary>
    /// Отладочный класс
    /// </summary>
    public class Debug
    {
        public string strTpsFps = "";
        public string audio = "";
        public string client = "";
        public string server = "";

        public static uint meshId = 0;
        public static int meshCount = 0;

        public void SetTpsFps(int fps, float speedFrame, int tps, float speedTick)
        {
            strTpsFps = string.Format("Speed: {0} fps {1:0.00} ms {2} tps {3:0.00} ms",
                fps, speedFrame, tps, speedTick);
        }

        public string ToText()
        {
            return string.Format("{0}\r\nAudio: {1}\r\nMesh Id: {2} C: {3}\r\nClient: {4}\r\n{5}",
                strTpsFps,
                audio,
                meshId,
                meshCount,
                client,
                server
            );
        }
    }
}
