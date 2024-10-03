using System.Collections.Generic;
using Vge.Renderer;
using WinGL.Util;

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

        #region Chunk

        /// <summary>
        /// Игроки на сервере
        /// </summary>
        public static Vector2i[] players = new Vector2i[0];
        /// <summary>
        /// Игрок локальный
        /// </summary>
        public static Vector2i player = new Vector2i();
        /// <summary>
        /// Чанки на сервере
        /// </summary>
        public static Vector2i[] chunks = new Vector2i[0];

        private static MeshGuiColor meshChunks;

        public static void DrawChunks(WindowMain window)
        {
            if (meshChunks == null)
            {
                meshChunks = new MeshGuiColor(window.GetOpenGL());
            }
            List<float> vs = new List<float>();

            int xc = 800;
            int yc = 500;
            int x, y;

            for (int i = 0; i < chunks.Length; i++)
            {
                x = xc + chunks[i].X * 8;
                y = yc + chunks[i].Y * 8;
                vs.AddRange(MeshGuiColor.Rectangle(x + 1, y + 1, x + 8, y + 8, 0, 1, 0));
            }
            for (int i = 0; i < players.Length; i++)
            {
                x = xc + players[i].X * 8;
                y = yc + players[i].Y * 8;
                vs.AddRange(MeshGuiColor.Rectangle(x + 2, y + 2, x + 7, y + 7, 1, 1, 0));
            }

            // Игрок
            x = xc + player.X * 8;
            y = yc + player.Y * 8;
            vs.AddRange(MeshGuiColor.Rectangle(x + 3, y + 3, x + 6, y + 6, .5f, 0, .5f));

            meshChunks.Reload(vs.ToArray());
            window.Render.TextureDisable();
            meshChunks.Draw();
            window.Render.TextureEnable();
        }

        #endregion
    }
}
