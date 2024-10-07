using System.Collections.Generic;
using Vge.Util;
using Vge.World;
using Vge.World.Chunk;
using WinGL.Util;

namespace Vge.Management
{
    /// <summary>
    /// Объект управляет всеми чанками которые надо загрузить или выгрузить
    /// </summary>
    public class FragmentManager
    {
        /// <summary>
        /// Минимальный радиус обзора для сервера, нужен для спавна и тиков блоков
        /// </summary>
        public static int minRadius = 8;

        /// <summary>
        /// Сетевой мир
        /// </summary>
        public readonly WorldServer World;

        /// <summary>
        /// Спикок чанков где есть игроки
        /// </summary>
        public ListFast<ulong> ListChunkPlayer { get; private set; } = new ListFast<ulong>(9);
        /// <summary>
        /// Спикок чанков активных блоков
        /// </summary>
        public ListFast<ulong> ListChunkAction { get; private set; } = new ListFast<ulong>(400);

        /// <summary>
        /// Список всех якорей
        /// </summary>
        private readonly List<IAnchor> _anchors = new List<IAnchor>();

        /// <summary>
        /// Чанки игроков она же выступает в роли маски какие нельзя выгружать и какие может видеть игрок
        /// </summary>
        private MapChunk _chunkForAnchors = new MapChunk();
        
        /// <summary>
        /// Смещение чанка у якоря
        /// </summary>
        private bool _flagAnchorChunkOffset = true;

        /// <summary>
        /// Массив активных колец обзора
        /// </summary>
        private readonly Vector2i[] _overviewCirclesActive;


        public FragmentManager(WorldServer world)
        {
            World = world;
            _overviewCirclesActive = Ce.OverviewCircles[minRadius];
        }

        /// <summary>
        /// Добавить якорь
        /// </summary>
        public void AddAnchor(IAnchor anchor)
        {
            _anchors.Add(anchor);
            // Добавить все чанки вокруг якоря
            _OverviewChunkAddAnchor(anchor);
        }

        /// <summary>
        /// Удалить якорь
        /// </summary>
        public void RemoveAnchor(IAnchor anchor)
        {
            // Убрать все чанки принадлежащие якорю
            _OverviewChunkPutAwayAnchor(anchor);
            // Убрать из списка якорей
            _anchors.Remove(anchor);
        }

        public void Update()
        {
            // Обновить список активных чанков
            if (_flagAnchorChunkOffset)
            {
                _flagAnchorChunkOffset = false;
                // Обновить список активных чанков
                _UpListChunkAction();
                if (World.IdWorld == 0)
                {
                 //   World.Server.OnTagDebug("ListChunkAction", ListChunkAction.ToArray());
                    World.Server.OnTagDebug("ChunkForAnchors", _chunkForAnchors.GetList().ToArray());
                    //if (_anchors.Count > 0)
                    //{
                    //    World.Server.OnTagDebug("ChunkAnchors", _anchors[0].LoadingChunks.ToArray());
                    //}
                    //World.Server.OnTagDebug("1", null);
                }
            }
        }

        #region ChunkForAnchor

        /// <summary>
        /// Добавить якорь в конкретный чанк, если нет чанка, создаём его
        /// </summary>
        private void _ChunkForAnchorAdd(int x, int y, IAnchor anchor)
        {
            IChunkPosition chunkPosition = _chunkForAnchors.Get(x, y);
            ChunkForAnchor chunkForAnchor;
            if (chunkPosition == null)
            {
                // Создаём чанк для якоря
                chunkForAnchor = new ChunkForAnchor(World, x, y);
                _chunkForAnchors.Add(chunkForAnchor);
            }
            else
            {
                chunkForAnchor = chunkPosition as ChunkForAnchor;
            }
            chunkForAnchor.AddAnchor(anchor);
        }

        /// <summary>
        /// Удалить якорь с конкретного чанка 
        /// </summary>
        private void _ChunkForAnchorRemove(int x, int y, IAnchor anchor)
        {
            IChunkPosition chunkPosition = _chunkForAnchors.Get(x, y);
            if (chunkPosition != null && chunkPosition is ChunkForAnchor chunkForAnchor)
            {
                if (chunkForAnchor.RemoveAnchor(anchor))
                {
                    // Удалить чанк
                    _chunkForAnchors.Remove(x, y);
                }
            }
        }

        #endregion

        /// <summary>
        /// Обновлять фрагменты чанков вокруг якоря, перемещаемого логикой сервера
        /// </summary>
        public void UpdateMountedMovingAnchor(IAnchor anchor)
        {
            bool isFilter = false;
            int radius = anchor.OverviewChunk;
            
            // Проверяем изменение обзора чанка
            if (radius != anchor.OverviewChunkPrev)
            {
                int radiusPrev = anchor.OverviewChunkPrev;
                int chx = anchor.ChunkPosManagedX;
                int chz = anchor.ChunkPosManagedY;

                _UpdateMountedMovingAnchorOverviewChunk(anchor, chx, chz, 
                    Mth.Max(radius, radiusPrev), radiusPrev, radius);

                anchor.UpOverviewChunkPrev();
                isFilter = true;
            }

            // Проверяем смещение чанка на выбранный параметр, если есть начинаем обработку
            int chunkX = anchor.ChunkPositionX;
            int chunkY = anchor.ChunkPositionY;
            // Активация смещения при смещении количество чанков
            int bias = radius > 12 ? 2 : 1;
            if (Mth.Abs(chunkX - anchor.ChunkPosManagedX) >= bias || Mth.Abs(chunkY - anchor.ChunkPosManagedX) >= bias)
            {
                // Смещение чанка
                _flagAnchorChunkOffset = true;
                int radiusPlay = anchor.OverviewChunk;
                // TODO::2024-10-07 OverviewChunk продумать как жить с игроками у кого обзор маленький, а тики должны быть больше
                radius = radiusPlay;
                if (radius < minRadius) radius = minRadius;
                
                int chmx = anchor.ChunkPosManagedX;
                int chmz = anchor.ChunkPosManagedY;

                // Проверка перемещения обзора чанков у клиента
                _UpdateMountedMovingAnchorRadius(anchor, chunkX, chunkY, chmx, chmz, radiusPlay);

                anchor.UpChunkPosManaged();
                isFilter = true;
            }

            if (isFilter)
            {
              //  _FilterChunkLoadQueue(anchor);
            }
        }

        /// <summary>
        /// Удаляет все фрагменты из очереди загрузки фрагментов данного проигрывателя, которые не находятся в зоне видимости проигрывателя. 
        /// Обновляет список координат чанков и сортирует их с центра в даль для игрока entityPlayer
        /// </summary>
        private void _FilterChunkLoadQueue(IAnchor anchor)
        {
            //GetPlayerInstance(anchor.GetChunkPos(), true);
            
            // c кругом чанки иногда бывают пустые чанки, не догружены клиенту!!!
            // хотя с квадратом на увеличении и уменьшении обзора тоже получил пустые чанки
            // 24.03.2023

            //Hashtable mapLoaded = anchor.LoadedChunks.CloneMap();
            //Hashtable mapLoading = anchor.LoadingChunks.CloneMap();
            //anchor.LoadedChunks.Clear();
            anchor.LoadingChunks.Clear();

            //if (anchor..DistSqrt != null)
            {
                // +4 а не addServer, так как при радиусе, в краях радиуса, 4 мало, 6 норм.
                // Но тут надо срезать, чтоб круга не было, типа квадрат со срезаными углами, 
                // чтоб сократить память.
                int overviewChunk = anchor.OverviewChunk;
                if (overviewChunk < minRadius) overviewChunk = minRadius;
                int radius = overviewChunk;// + 4;
                Vector2i[] overviewCircles = Ce.OverviewCircles[radius]; // overviewChunk + addServer
                int count = overviewCircles.Length;
                //anchor.ChunkPosManagedX
                //vec2i chunkPosManaged = anchor.ChunkPosManaged;
                Vector2i vec;
                int chx = anchor.ChunkPosManagedX;
                int chy = anchor.ChunkPosManagedY;
                //vec2i pos;
                for (int d = 0; d < count; d++)
                {
                    vec = overviewCircles[d];
                    if (vec.X >= -radius && vec.X <= radius && vec.Y >= -radius && vec.Y <= radius)
                    {
                        //pos = vec + chunkPosManaged;
                        //if (mapLoaded.ContainsKey(pos))
                        //{
                        //    anchor.LoadedChunks.Add(pos);
                        //}
                        anchor.LoadingChunks.Add(Conv.ChunkXyToIndex(vec.X + chx, vec.Y + chy));
                    }
                }
            }
        }

        /// <summary>
        /// Обновить список активных чанков
        /// </summary>
        private void _UpListChunkAction()
        {
            ListChunkPlayer.Clear();
            ListChunkAction.Clear();
            if (_anchors.Count > 0)
            {
                int chX, chY, ch2X;
                Vector2i pos;
                ulong index;
                int count = _overviewCirclesActive.Length;
                for (int i = 0; i < _anchors.Count; i++)
                {
                    chX = _anchors[i].ChunkPositionX;
                    chY = _anchors[i].ChunkPositionY;

                    // Собираем чанки где есть игроки, тикущий и по соседнему
                    if (_anchors[i].IsPlayer)
                    {
                        for (int x = -1; x <= 1; x++)
                        {
                            ch2X = chX + x;
                            for (int y = -1; y <= 1; y++)
                            {
                                index = Conv.ChunkXyToIndex(ch2X, chY + y);
                                if (!ListChunkPlayer.Contains(index))
                                {
                                    ListChunkPlayer.Add(index);
                                }
                            }
                        }
                    }
                    // Собираем все активные чанки для будущих тиков
                    for (int j = 0; j < count; j++)
                    {
                        pos = _overviewCirclesActive[j];
                        index = Conv.ChunkXyToIndex(pos.X + chX, pos.Y + chY);
                        if (!ListChunkAction.Contains(index))
                        {
                            ListChunkAction.Add(index);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Обнолвение фрагмента при изменении обзора, для клиента или для кэш сервера
        /// </summary>
        private void _UpdateMountedMovingAnchorOverviewChunk(IAnchor anchor, int chx, int chz,
            int radius, int radiusPrev, int radiusMin)
        {
            int xmin = chx - radius;
            int xmax = chx + radius;
            int zmin = chz - radius;
            int zmax = chz + radius;

            if (anchor.OverviewChunk > anchor.OverviewChunkPrev)
            {
                // Увеличиваем обзор
                int xmin2 = chx - radiusPrev;
                int xmax2 = chx + radiusPrev + 1;
                int zmin2 = chz - radiusPrev;
                int zmax2 = chz + radiusPrev + 1;
                _OverviewChunkAddSquare(anchor, xmin, xmax, zmin, zmax, 
                    xmin2, xmax2, zmin2, zmax2);
            }
            else
            {
                // Уменьшить обзор
                int xmin2 = chx - radiusMin;
                int xmax2 = chx + radiusMin + 1;
                int zmin2 = chz - radiusMin;
                int zmax2 = chz + radiusMin + 1;
                _OverviewChunkPutAwaySquare(anchor, xmin, xmax, zmin, zmax, 
                    xmin2, xmax2, zmin2, zmax2);
            }
        }

        /// <summary>
        /// Обнолвение фрагмента при перемещении, для клиента или для кэш сервера
        /// </summary>
        private void _UpdateMountedMovingAnchorRadius(IAnchor anchor,
            int chx, int chz, int chmx, int chmz, int radius)
        {
            int xmin = chx - radius;
            int xmax = chx + radius;
            int zmin = chz - radius;
            int zmax = chz + radius;
            int xmin2 = chmx - radius;
            int xmax2 = chmx + radius;
            int zmin2 = chmz - radius;
            int zmax2 = chmz + radius;

            // Определяем добавление
            _OverviewChunkAddSquare(anchor, xmin, xmax, zmin, zmax, 
                xmin2, xmax2 + 1, zmin2, zmax2 + 1);
            // Определяем какие убираем
            _OverviewChunkPutAwaySquare(anchor, xmin2, xmax2, zmin2, zmax2, 
                xmin, xmax + 1, zmin, zmax + 1);
        }

        #region _Overview

        /// <summary>
        /// Добавить чанки обзора при необходимости
        /// </summary>
        /// <param name="entityPlayer">Объект игрока</param>
        /// <param name="xMinFor">минимальная кордината массива по X</param>
        /// <param name="xMaxFor">максимальная кордината массива по X</param>
        /// <param name="zMinFor">минимальная кордината массива по Z</param>
        /// <param name="zMaxFor">максимальная кордината массива по Z</param>
        /// <param name="xMinCheck">минимальная кордината проверки по X</param>
        /// <param name="xMaxCheck">максимальная кордината проверки по X</param>
        /// <param name="zMinCheck">минимальная кордината проверки по Z</param>
        /// <param name="zMaxCheck">максимальная кордината проверки по Z</param>
        private void _OverviewChunkAddSquare(IAnchor anchor,
            int xMinFor, int xMaxFor, int zMinFor, int zMaxFor,
            int xMinCheck, int xMaxCheck, int zMinCheck, int zMaxCheck)
        {
            int x, z;

            for (x = xMinFor; x < xMinCheck; x++)
            {
                for (z = zMinFor; z <= zMaxFor; z++)
                {
                    _ChunkForAnchorAdd(x, z, anchor);
                }
            }
            for (x = xMaxCheck; x <= xMaxFor; x++)
            {
                for (z = zMinFor; z <= zMaxFor; z++)
                {
                    _ChunkForAnchorAdd(x, z, anchor);
                }
            }
            for (z = zMinFor; z < zMinCheck; z++)
            {
                for (x = xMinFor; x <= xMaxFor; x++)
                {
                    _ChunkForAnchorAdd(x, z, anchor);
                }
            }
            for (z = zMaxCheck; z <= zMaxFor; z++)
            {
                for (x = xMinFor; x <= xMaxFor; x++)
                {
                    _ChunkForAnchorAdd(x, z, anchor);
                }
            }
        }

        /// <summary>
        /// Убрать чанки обзора при необходимости
        /// </summary>
        /// <param name="entityPlayer">Объект игрока</param>
        /// <param name="xMinFor">минимальная кордината массива по X</param>
        /// <param name="xMaxFor">максимальная кордината массива по X</param>
        /// <param name="zMinFor">минимальная кордината массива по Z</param>
        /// <param name="zMaxFor">максимальная кордината массива по Z</param>
        /// <param name="xMinCheck">минимальная кордината проверки по X</param>
        /// <param name="xMaxCheck">максимальная кордината проверки по X</param>
        /// <param name="zMinCheck">минимальная кордината проверки по Z</param>
        /// <param name="zMaxCheck">максимальная кордината проверки по Z</param>
        private void _OverviewChunkPutAwaySquare(IAnchor anchor,
            int xMinFor, int xMaxFor, int zMinFor, int zMaxFor,
            int xMinCheck, int xMaxCheck, int zMinCheck, int zMaxCheck)
        {
            int x, z;
            for (x = xMinFor; x < xMinCheck; x++)
            {
                for (z = zMinFor; z <= zMaxFor; z++)
                {
                    _ChunkForAnchorRemove(x, z, anchor);
                }
            }
            for (x = xMaxCheck; x <= xMaxFor; x++)
            {
                for (z = zMinFor; z <= zMaxFor; z++)
                {
                    _ChunkForAnchorRemove(x, z, anchor);
                }
            }
            for (z = zMinFor; z < zMinCheck; z++)
            {
                for (x = xMinFor; x <= xMaxFor; x++)
                {
                    _ChunkForAnchorRemove(x, z, anchor);
                }
            }
            for (z = zMaxCheck; z <= zMaxFor; z++)
            {
                for (x = xMinFor; x <= xMaxFor; x++)
                {
                    _ChunkForAnchorRemove(x, z, anchor);
                }
            }
        }

        /// <summary>
        /// Добавить все чанки вокруг якоря
        /// </summary>
        private void _OverviewChunkAddAnchor(IAnchor anchor)
        {
            int radius = anchor.OverviewChunk;
            int chx = anchor.ChunkPositionX;
            int chy = anchor.ChunkPositionY;
            int x, z;
            int xmin = chx - radius;
            int xmax = chx + radius;
            int zmin = chy - radius;
            int zmax = chy + radius;
            for (x = xmin; x <= xmax; x++)
            {
                for (z = zmin; z <= zmax; z++)
                {
                    _ChunkForAnchorAdd(x, z, anchor);
                }
            }
        }

        /// <summary>
        /// Убрать все чанки принадлежащие якорю
        /// </summary>
        private void _OverviewChunkPutAwayAnchor(IAnchor anchor)
        {
            int radius = anchor.OverviewChunk;
            int chx = anchor.ChunkPositionX;
            int chy = anchor.ChunkPositionY;
            int x, z;
            int xmin = chx - radius;
            int xmax = chx + radius;
            int zmin = chy - radius;
            int zmax = chy + radius;
            for (x = xmin; x <= xmax; x++)
            {
                for (z = zmin; z <= zmax; z++)
                {
                    _ChunkForAnchorRemove(x, z, anchor);
                }
            }
        }

        #endregion
    }
}
