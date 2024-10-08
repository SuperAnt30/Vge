using System.Collections.Generic;
using Vge.Event;
using Vge.Renderer;
using Vge.Util;
using Vge.World.Chunk;
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

        /// <summary>
        /// Прилёт объектов с локального сервера для отладки
        /// </summary>
        public void SetTag(StringEventArgs e)
        {
            if (e.Text == "ChunksActive")
            {
                _flagBlockDraw = true;
                ulong[] ar = (ulong[])e.Tag;
                _chunksActive = new Vector2i[ar.Length];
                for (int i = 0; i < ar.Length; i++)
                {
                    _chunksActive[i] = Conv.IndexToChunkVector2i(ar[i]);
                }
                _flagBlockDraw = false;
                _renderChunks = 2;
            }
            else if (e.Text == "ChunkForAnchors")
            {
                _flagBlockDraw = true;
                IChunkPosition[] ar = (IChunkPosition[])e.Tag;
                _chunksForAnchors = new Vector2i[ar.Length];
                for (int i = 0; i < ar.Length; i++)
                {
                    _chunksForAnchors[i] = new Vector2i(ar[i].CurrentChunkX, ar[i].CurrentChunkY);
                }
                _flagBlockDraw = false;
                _renderChunks = 2;
            }
            else if (e.Text == "ChunkReady")
            {
                _flagBlockDraw = true;
                IChunkPosition[] ar = (IChunkPosition[])e.Tag;
                _chunksReady = new Vector2i[ar.Length];
                for (int i = 0; i < ar.Length; i++)
                {
                    _chunksReady[i] = new Vector2i(ar[i].CurrentChunkX, ar[i].CurrentChunkY);
                }
                _flagBlockDraw = false;
                _renderChunks = 2;
            }
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
        /// Чанки на сервере которые активные, т.е. тикают
        /// Синий
        /// </summary>
        private static Vector2i[] _chunksActive = new Vector2i[0];
        /// <summary>
        /// Готовые чанкина на сервере
        /// Зелёный
        /// </summary>
        private static Vector2i[] _chunksReady = new Vector2i[0];
        /// <summary>
        /// Чанки на сервере которые пренадлежат якорям, которые могут отправлять якорям изменения
        /// Белый
        /// </summary>
        private static Vector2i[] _chunksForAnchors = new Vector2i[0];
        
        

        private static bool _flagBlockDraw = false;

        private static byte _renderChunks = 0;

        private static MeshGuiColor meshChunks;

        public static void DrawChunks(WindowMain window)
        {
            if (meshChunks == null)
            {
                meshChunks = new MeshGuiColor(window.GetOpenGL());
            }
            if (_renderChunks > 0)
            {
                if (!_flagBlockDraw)
                {
                    _renderChunks--;
                    List<float> vs = new List<float>();

                    int xc = 800;
                    int yc = 500;
                    int x, y;

                    // Синий
                    for (int i = 0; i < _chunksActive.Length; i++)
                    {
                        if (_flagBlockDraw) return;
                        x = xc + _chunksActive[i].X * 8;
                        y = yc + _chunksActive[i].Y * 8;
                        vs.AddRange(MeshGuiColor.Rectangle(x, y, x + 8, y + 8, 0, 0, .7f));
                    }
                    // Зелёный
                    for (int i = 0; i < _chunksReady.Length; i++)
                    {
                        if (_flagBlockDraw) return;
                        x = xc + _chunksReady[i].X * 8;
                        y = yc + _chunksReady[i].Y * 8;
                        vs.AddRange(MeshGuiColor.Rectangle(x + 1, y + 1, x + 8, y + 8, 0, 1, 0));
                    }
                    // Красный
                    for (int i = 0; i < players.Length; i++)
                    {
                        x = xc + players[i].X * 8;
                        y = yc + players[i].Y * 8;
                        vs.AddRange(MeshGuiColor.Rectangle(x + 2, y + 1, x + 7, y + 8, 1, 0, 0));
                    }
                    // Белый
                    for (int i = 0; i < _chunksForAnchors.Length; i++)
                    {
                        if (_flagBlockDraw) return;
                        x = xc + _chunksForAnchors[i].X * 8;
                        y = yc + _chunksForAnchors[i].Y * 8;
                        vs.AddRange(MeshGuiColor.Rectangle(x + 2, y + 2, x + 7, y + 7, 1, 1, 1));
                    }

                    
                    // Игрок
                    x = xc + player.X * 8;
                    y = yc + player.Y * 8;
                    vs.AddRange(MeshGuiColor.Rectangle(x + 3, y + 3, x + 6, y + 6, .5f, 0, .5f));
                    meshChunks.Reload(vs.ToArray());
                }
            }
            window.Render.TextureDisable();
            meshChunks.Draw();
            window.Render.TextureEnable();
        }

        #endregion
    }
}
