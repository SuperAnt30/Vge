using System.Collections.Generic;
using Vge.Event;
using Vge.Realms;
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
        public static string DebugString = "";
        public static string BlockChange = "";

        /// <summary>
        /// FrustumCulling
        /// </summary>
        public static int CountMeshFC = 0;
        /// <summary>
        /// Сколько обновилось чанков
        /// </summary>
        public static int CountUpdateChunck = 0;
        /// <summary>
        /// Сколько обновилось чанков
        /// </summary>
        public static int CountUpdateChunckAlpha = 0;
        /// <summary>
        /// Время рендера чанка в мс, среднее из последних 8
        /// </summary>
        public static float RenderChunckTime8 = 0;
        public static float RenderChunckTimeAlpha8 = 0;

        public static int dct = 0;
        public static float[] RenderChunckTime = new float[32];
        /// <summary>
        /// Прорисовка вокселей контуром линий
        /// </summary>
        public static bool IsDrawVoxelLine = false;
        /// <summary>
        /// Фокус блок
        /// </summary>
        public static string BlockFocus = "";

        public static string FrizFps = "";

        public void SetTpsFps(int fps, float speedFrame, int tps, float speedTick, float speedTickMax,
            int countUpdateChunk, int countUpdateChunkAlpha)
        {
            StrTpsFps = string.Format("Speed: {0} fps {1:0.00} ms {2} tps {3:0.00}|{4:0.00} ms ({5}/a{6})",
                fps, speedFrame, tps, speedTick, speedTickMax, countUpdateChunk, countUpdateChunkAlpha);
        }

        public string ToText()
        {
            return StrTpsFps
                + "\r\n" + FrizFps
                + "\r\nAudio: " + Audio
                + "\r\nMesh Id: " + MeshId + " C: " + MeshCount + " FC: " + CountMeshFC 
                    + " RCh: " + RenderChunckTime8.ToString("0.000")
                    + " a:" + RenderChunckTimeAlpha8.ToString("0.000")
                    + " rch: " + Mth.Average(RenderChunckTime)
                + "\r\n" + Server
                + ChatStyle.Bolb + "[Client]: " + ChatStyle.Reset + Client
                + "\r\nDS: " + DebugString + " " + BlockChange
                + "\r\n" + ChatStyle.Gold + BlockFocus + ChatStyle.Reset
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
                _chunksReady = new Vector3i[ar.Length];
                for (int i = 0; i < ar.Length; i++)
                {
                    _chunksReady[i] = new Vector3i(ar[i].CurrentChunkX, ar[i].CurrentChunkY, (int)ar[i].Tag);
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
            else if (e.Text == Key.FrustumCulling.ToString())
            {
                _flagBlockDraw = true;
                Vector2i[] ar = (Vector2i[])e.Tag;
                _frustumCulling = new Vector2i[ar.Length];
                for (int i = 0; i < ar.Length; i++)
                {
                    _frustumCulling[i] = new Vector2i(ar[i]);
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
            ChunkClient,
            /// <summary>
            /// FrustumCulling локально для клиента
            /// Голубой
            /// </summary>
            FrustumCulling
        }

        /// <summary>
        /// Сущности на сервере
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
        private static Vector3i[] _chunksReady = new Vector3i[0];
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
        /// <summary>
        /// FrustumCulling локально для клиента
        /// Голубой
        /// </summary>
        private static Vector2i[] _frustumCulling = new Vector2i[0];

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
                vs.AddRange(RenderFigure.Rectangle(x, y, x + 8, y + 8, 0, 0, .7f));
            }
            // Зелёный
            for (int i = 0; i < _chunksReady.Length; i++)
            {
                if (_flagBlockDraw) return false;
                x = xc + _chunksReady[i].X * 8;
                y = yc + _chunksReady[i].Y * 8;
                vs.AddRange(RenderFigure.Rectangle(x + 1, y + 1, x + 7, y + 7,
                    _chunksReady[i].Z == 0 ? 0 : .9f,
                    _chunksReady[i].Z == 0 ? .9f : 0, 0));
            }
            // Красный
            for (int i = 0; i < Players.Length; i++)
            {
                if (_flagBlockDraw) return false;
                x = xc + Players[i].X * 8;
                y = yc + Players[i].Y * 8;
                vs.AddRange(RenderFigure.Rectangle(x + 1, y + 1, x + 7, y + 7, .9f, 0, 0));
            }
            // Белый
            for (int i = 0; i < _chunksForAnchors.Length; i++)
            {
                if (_flagBlockDraw) return false;
                x = xc + _chunksForAnchors[i].X * 8;
                y = yc + _chunksForAnchors[i].Y * 8;
                vs.AddRange(RenderFigure.Rectangle(x + 2, y + 2, x + 6, y + 6, .9f, .9f, .9f));
            }
            // Голубой
            for (int i = 0; i < _frustumCulling.Length; i++)
            {
                if (_flagBlockDraw) return false;
                x = xc + _frustumCulling[i].X * 8;
                y = yc + _frustumCulling[i].Y * 8;
                vs.AddRange(RenderFigure.Rectangle(x + 2, y + 2, x + 6, y + 6, 0, .4f, 1));
            }

            // Черный
            for (int i = 0; i < _chunksСlient.Length; i++)
            {
                if (_flagBlockDraw) return false;
                x = xc + _chunksСlient[i].X * 8;
                y = yc + _chunksСlient[i].Y * 8;
                vs.AddRange(RenderFigure.Rectangle(x + 3, y + 3, x + 5, y + 5, 0, 0, .6f));
            }
            
            // Игрок
            x = xc + Player.X * 8;
            y = yc + Player.Y * 8;
            vs.AddRange(RenderFigure.Rectangle(x + 3, y + 3, x + 6, y + 6, .5f, 0, .5f));
            return true;
        }

        #endregion

        /// <summary>
        /// Нагрузка, без слипа, чтоб чувствоать CPU, задать в мс для debug на моём компе. Примерно
        /// </summary>
        public static void Burden(float ms)
        {
            //return;
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
