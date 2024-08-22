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

        public void SetTpsFps(int fps, float speedFrame, int tps, float speedTick)
        {
            strTpsFps = string.Format("Speed: {0} fps {1:0.00} ms {2} tps {3:0.00} ms",
                fps, speedFrame, tps, speedTick);
        }

        public string ToText()
        {
            return string.Format("{0}\r\nAudio: {1}\r\nClient: {2}\r\n{3}",
                strTpsFps,
                audio,
                client,
                server
            );
        }
    }
}
