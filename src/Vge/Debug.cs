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
        public string StrTpsFps = "";
        public string Audio = "";
        public string Client = "";
        public string Server = "";

        public static string Text = "";
        public static uint MeshId = 0;
        public static int MeshCount = 0;

        public void SetTpsFps(int fps, float speedFrame, int tps, float speedTick)
        {
            StrTpsFps = string.Format("Speed: {0} fps {1:0.00} ms {2} tps {3:0.00} ms",
                fps, speedFrame, tps, speedTick);
        }

        public string ToText()
        {
            return StrTpsFps
                + "\r\nAudio: " + Audio
                + "\r\nMesh Id: " + MeshId + " C: " + MeshCount
                + "\r\n" + Server
                + "[Client]: " + Client
                + "\r\n" + Text;
        }

        #region Chunk

        /// <summary>
        /// Прилёт объектов с локального сервера для отладки
        /// </summary>
        public void SetTag(StringEventArgs e)
        {
            if (e.Text == Key.ChunksActive.ToString())
            {
                _flagBlockDraw = true;
                ulong[] ar = (ulong[])e.Tag;
                _chunksActive = new Vector2i[ar.Length];
                for (int i = 0; i < ar.Length; i++)
                {
                    _chunksActive[i] = Conv.IndexToChunkVector2i(ar[i]);
                }
                _flagBlockDraw = false;
                _renderChunks = true;
            }
            else if (e.Text == Key.ChunkForAnchors.ToString())
            {
                _flagBlockDraw = true;
                IChunkPosition[] ar = (IChunkPosition[])e.Tag;
                _chunksForAnchors = new Vector2i[ar.Length];
                for (int i = 0; i < ar.Length; i++)
                {
                    _chunksForAnchors[i] = new Vector2i(ar[i].CurrentChunkX, ar[i].CurrentChunkY);
                }
                _flagBlockDraw = false;
                _renderChunks = true;
            }
            else if (e.Text == Key.ChunkReady.ToString())
            {
                _flagBlockDraw = true;
                IChunkPosition[] ar = (IChunkPosition[])e.Tag;
                _chunksReady = new Vector2i[ar.Length];
                for (int i = 0; i < ar.Length; i++)
                {
                    _chunksReady[i] = new Vector2i(ar[i].CurrentChunkX, ar[i].CurrentChunkY);
                }
                _flagBlockDraw = false;
                _renderChunks = true;
            }
            else if (e.Text == Key.ChunkClient.ToString())
            {
                _flagBlockDraw = true;
                IChunkPosition[] ar = (IChunkPosition[])e.Tag;
                _chunksСlient = new Vector2i[ar.Length];
                for (int i = 0; i < ar.Length; i++)
                {
                    _chunksСlient[i] = new Vector2i(ar[i].CurrentChunkX, ar[i].CurrentChunkY);
                }
                _flagBlockDraw = false;
                _renderChunks = true;
            }
        }

        public enum Key
        {
            /// <summary>
            /// Чанки на сервере которые активные, т.е. тикают
            /// Синий
            /// </summary>
            ChunksActive,
            /// <summary>
            /// Чанки на сервере которые пренадлежат якорям, которые могут отправлять якорям изменения
            /// Белый
            /// </summary>
            ChunkForAnchors,
            /// <summary>
            /// Готовые чанкина на сервере
            /// Зелёный
            /// </summary>
            ChunkReady,
            /// <summary>
            /// Готовые чанкина на клиенте
            /// Черные
            /// </summary>
            ChunkClient
        }

        /// <summary>
        /// Игроки на сервере
        /// </summary>
        public static Vector2i[] Players = new Vector2i[0];
        /// <summary>
        /// Игрок локальный
        /// </summary>
        public static Vector2i Player = new Vector2i();
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
        /// <summary>
        /// Готовые чанкина на клиенте
        /// Черные
        /// </summary>
        private static Vector2i[] _chunksСlient = new Vector2i[0];

        private static bool _flagBlockDraw = false;

        private static bool _renderChunks;

        private static MeshGuiColor _meshChunks;

        public static void DrawChunks(WindowMain window)
        {
            if (_meshChunks == null)
            {
                _meshChunks = new MeshGuiColor(window.GetOpenGL());
            }
            if (_renderChunks)
            {
                if (!_flagBlockDraw)
                {
                    List<float> vs = new List<float>();
                    if (_RenderChuks(vs))
                    {
                        _renderChunks = false;
                        _meshChunks.Reload(vs.ToArray());
                    }
                }
            }

            window.Render.FontMain.BindTexture();
            window.Render.ShaderBindGuiColor();
            _meshChunks.Draw();
        }

        private static bool _RenderChuks(List<float> vs)
        {
            int xc = 800;
            int yc = 500;
            int x, y;

            // Синий
            for (int i = 0; i < _chunksActive.Length; i++)
            {
                if (_flagBlockDraw) return false;
                x = xc + _chunksActive[i].X * 8;
                y = yc + _chunksActive[i].Y * 8;
                vs.AddRange(MeshGuiColor.Rectangle(x, y, x + 8, y + 8, 0, 0, .7f));
            }
            // Зелёный
            for (int i = 0; i < _chunksReady.Length; i++)
            {
                if (_flagBlockDraw) return false;
                x = xc + _chunksReady[i].X * 8;
                y = yc + _chunksReady[i].Y * 8;
                vs.AddRange(MeshGuiColor.Rectangle(x + 1, y + 1, x + 7, y + 7, 0, .9f, 0));
            }
            // Красный
            for (int i = 0; i < Players.Length; i++)
            {
                if (_flagBlockDraw) return false;
                x = xc + Players[i].X * 8;
                y = yc + Players[i].Y * 8;
                vs.AddRange(MeshGuiColor.Rectangle(x + 1, y + 1, x + 7, y + 7, .9f, 0, 0));
            }
            // Белый
            for (int i = 0; i < _chunksForAnchors.Length; i++)
            {
                if (_flagBlockDraw) return false;
                x = xc + _chunksForAnchors[i].X * 8;
                y = yc + _chunksForAnchors[i].Y * 8;
                vs.AddRange(MeshGuiColor.Rectangle(x + 2, y + 2, x + 6, y + 6, .9f, .9f, .9f));
            }
            // Черный
            for (int i = 0; i < _chunksСlient.Length; i++)
            {
                if (_flagBlockDraw) return false;
                x = xc + _chunksСlient[i].X * 8;
                y = yc + _chunksСlient[i].Y * 8;
                vs.AddRange(MeshGuiColor.Rectangle(x + 3, y + 3, x + 5, y + 5, 0, 0, .6f));
            }

            // Игрок
            x = xc + Player.X * 8;
            y = yc + Player.Y * 8;
            vs.AddRange(MeshGuiColor.Rectangle(x + 3, y + 3, x + 6, y + 6, .5f, 0, .5f));
            return true;
        }

        #endregion

        /// <summary>
        /// Нагрузка, без слипа, чтоб чувствоать CPU, задать в мс для debug на моём компе. Примерно
        /// </summary>
        public static void Burden(float ms)
        {
            // Пробуем загрузить с файла
            float f, d;
            f = d = .5f;
            int count = (int)(420000 * ms);
            // 1.2-2.1 мс = 500тыс
            // 0.55-0.61 мс = 250тыс
            for (int i = 0; i < count; i++)
            {
                f *= d + i;
            }
        }
    }
}
