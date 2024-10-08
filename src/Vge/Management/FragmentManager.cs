using System;
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
        /// Активный радиус обзора для сервера, нужен для спавна и тиков блоков
        /// </summary>
        public readonly byte ActiveRadius;
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
        /// Список всех активных якорей
        /// </summary>
        private readonly List<IAnchor> _actionAnchors = new List<IAnchor>();
        /// <summary>
        /// Список всех мировх якорей 
        /// </summary>
        private readonly List<WorldAnchor> _worldAnchors = new List<WorldAnchor>();

        /// <summary>
        /// Чанки игроков она же выступает в роли маски какие нельзя выгружать и какие может видеть игрок
        /// </summary>
        private MapChunk _chunkForAnchors = new MapChunk();
        
        /// <summary>
        /// флаг для обновления активных чанков
        /// </summary>
        private bool _flagUpListChunkAction = true;

        /// <summary>
        /// Объект сыщик
        /// </summary>
        private readonly Profiler _filer;

        public FragmentManager(WorldServer world)
        {
            World = world;
            ActiveRadius = World.Settings.ActiveRadius;
            _filer = new Profiler(World.Server.Log, "[Server] ");
        }

        /// <summary>
        /// Создать мировой якорь на чанк
        /// </summary>
        public void AddWorldAnchorChunk(int x, int y)
        {
            // Сразу пробуем найти в том же чанке
            int count = _worldAnchors.Count;
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    if (_worldAnchors[i].ChunkPositionX == x
                        && _worldAnchors[i].ChunkPositionY == y)
                    {
                        // Найден мировой якор в том же чанке, значит продлемаем ему просто жизнь
                        _worldAnchors[i].ProlongLife();
                        return;
                    }
                }
            }
            AddAnchor(new WorldAnchor(World, x, y));
            _flagDebugAnchorChunkOffset = true;
        }

        /// <summary>
        /// Добавить якорь
        /// </summary>
        public void AddAnchor(IAnchor anchor)
        {
            if (anchor is WorldAnchor worldAnchor)
            {
                _worldAnchors.Add(worldAnchor);
            }
            if (anchor.IsActive)
            {
                _actionAnchors.Add(anchor);
                _flagUpListChunkAction = true;
            }
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
            if (anchor.IsActive)
            {
                _actionAnchors.Remove(anchor);
                _flagUpListChunkAction = true;
            }
            if (anchor is WorldAnchor worldAnchor)
            {
                _worldAnchors.Remove(worldAnchor);
            }
        }

        public void Update()
        {
            // Обновление жизни мировых чанков 1/5 в секунду
            if (World.Server.TickCounter % 6 == 0)
            {
                if (World.Server.TickCounter % 12 == 0)
                {
                    _filer.StartSection("AddChunkAction");
                    _AddChunkAction();
                    _filer.EndSection();
                }
                else
                {
                    _filer.StartSection("UpdateWorldAnchors");
                    _UpdateWorldAnchors();
                    _filer.EndSection();
                }
            }

            // Обновить список активных чанков
            if (_flagUpListChunkAction)
            {
                _flagUpListChunkAction = false;
                // Обновить список активных чанков
                _filer.StartSection("UpListChunkAction");
                _UpListChunkAction();
                _filer.EndSection();
                _flagDebugAnchorChunkOffset = true;
            }

            _filer.StartSection("LoadingChunks");
            if (_UpdateLoadingChunks()) flagDebugChunkProvider = true;
            _filer.EndStartSection("FragmentManagerDebug");
            _UpdateDebug();
            _filer.EndSection();
        }

        /// <summary>
        /// Загружаем чанки при необходимости
        /// </summary>
        private bool _UpdateLoadingChunks()
        {
            bool load = false;
            int a, number, x, y;
            ulong index;

            try
            {
                // TODO::2024-10-08 эту загрузку чанков продумать от времени, и по очереди для каждого якоря
                int countAnchor = _anchors.Count;
                for (a = 0; a < countAnchor; a++)
                {
                    number = 0;
                    while (_anchors[a].LoadingChunks.Count > 0 && number < 100)
                    {
                        index = _anchors[a].LoadingChunks[_anchors[a].LoadingChunks.Count - 1];
                        _anchors[a].LoadingChunks.RemoveLast();
                        x = Conv.IndexToChunkX(index);
                        y = Conv.IndexToChunkY(index);
                        if (!World.ChunkPrServ.NeededChunk(x, y))
                        {
                            // Чанк отсутствовал
                            if (!load) load = true;
                            number++;
                        }
                    }
                    //count = _anchors[a].LoadingChunks.Count;
                    //for (i = 0; i < count; i++)
                    //{
                    //    index = _anchors[a].LoadingChunks[i];
                    //    x = Conv.IndexToChunkX(index);
                    //    y = Conv.IndexToChunkY(index);
                    //    if (!World.ChunkPrServ.NeededChunk(x, y))
                    //    {
                    //        // Чанк отсутствовал
                    //        if (!load) load = true;
                    //        if (++number > 20)
                    //        {
                    //            // Больше 12 за такт нельзя, ибо просадка
                    //            break;
                    //        }
                    //    }
                    //}
                }
            }
            catch (Exception ex)
            {
                World.Server.Log.Error("LoadingChunks");
                Logger.Crash(ex, "LoadingChunks");
                throw;
            }
            return load;
        }

        /// <summary>
        /// Обновление жизни мировых чанков
        /// </summary>
        private void _UpdateWorldAnchors()
        {
            int count = _worldAnchors.Count - 1;
            int number = 0;
            bool removed = true;
            for (int i = count; i >= 0; i--)
            {
                if (_worldAnchors[i].UpdateAndCheck() && removed)
                {
                    RemoveAnchor(_worldAnchors[i]);
                    // За раз больше 100 нельзя выгружать
                    if (++number > 100)
                    {
                        removed = false;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Дабваить активные чанки если это нужно
        /// </summary>
        private void _AddChunkAction()
        {
            int i, x, y;
            ulong index;
            int count = ListChunkAction.Count;
            ChunkForAnchor chunkForAnchor;
            for (i = 0; i < count; i++)
            {
                index = ListChunkAction[i];
                x = Conv.IndexToChunkX(index);
                y = Conv.IndexToChunkY(index);
                chunkForAnchor = _chunkForAnchors.Get(x, y) as ChunkForAnchor;
                if (chunkForAnchor == null)
                {
                    // Тут надо добавить мировой якорь
                    AddWorldAnchorChunk(x, y);
                }
                else
                {
                    // Продлить жизнь если один мировой якорь 
                    chunkForAnchor.ProlongLife();
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
                    World.ChunkPrServ.DropChunk(x, y);
                    _chunkForAnchors.Remove(x, y);
                    _flagDebugAnchorChunkOffset = true;
                    flagDebugChunkProvider = true;
                }
            }
        }

        #endregion

        #region Обновление фрагментов

        /// <summary>
        /// Обновлять фрагменты чанков вокруг якоря, перемещаемого логикой сервера
        /// </summary>
        public void UpdateMountedMovingAnchor(IAnchor anchor)
        {
            bool change = false;
            // Проверяем изменение обзора чанка
            if (anchor.IsChangeOverview())
            {
                int radiusPrev = anchor.OverviewChunkPrev;
                int chx = anchor.ChunkPosManagedX;
                int chz = anchor.ChunkPosManagedY;

                _UpdateMountedMovingAnchorOverviewChunk(anchor, chx, chz, 
                    Mth.Max(anchor.OverviewChunk, radiusPrev), radiusPrev, anchor.OverviewChunk);
                change = true;
            }

            // Проверяем смещение чанка на выбранный параметр, если есть начинаем обработку
            int chunkX = anchor.ChunkPositionX;
            int chunkY = anchor.ChunkPositionY;
            // Активация смещения при смещении количество чанков
            if (anchor.IsAnOffsetNecessary())
            //int bias = radius > 12 ? 2 : 1;
            //if (Mth.Abs(chunkX - anchor.ChunkPosManagedX) >= bias || Mth.Abs(chunkY - anchor.ChunkPosManagedX) >= bias)
            {
                // Смещение чанка
                int chmx = anchor.ChunkPosManagedX;
                int chmz = anchor.ChunkPosManagedY;

                // Проверка перемещения обзора чанков у клиента
                _UpdateMountedMovingAnchorRadius(anchor, chunkX, chunkY, chmx, chmz, anchor.OverviewChunk);
                change = true;
            }

            if (change)
            {
                anchor.MountedMovedAnchor();
                _flagDebugAnchorChunkOffset = true;
                _flagUpListChunkAction = true;
            }
        }

        /// <summary>
        /// Обновить список активных чанков
        /// </summary>
        private void _UpListChunkAction()
        {
            ListChunkPlayer.Clear();
            ListChunkAction.Clear();
            int count = _anchors.Count;
            if (count > 0)
            {
                int chX, chY, ch2X, x, y, i;
                ulong index;
                for (i = 0; i < count; i++)
                {
                    // Собираем чанки где есть игроки, тикущий и по соседнему
                    if (_anchors[i].IsActive)
                    {
                        chX = _anchors[i].ChunkPositionX;
                        chY = _anchors[i].ChunkPositionY;

                        for (x = -1; x <= 1; x++)
                        {
                            ch2X = chX + x;
                            for (y = -1; y <= 1; y++)
                            {
                                index = Conv.ChunkXyToIndex(ch2X, chY + y);
                                if (!ListChunkPlayer.Contains(index))
                                {
                                    ListChunkPlayer.Add(index);
                                }
                            }
                        }

                        // Собираем все активные чанки для будущих тиков
                        for (x = -ActiveRadius; x <= ActiveRadius; x++)
                        {
                            ch2X = chX + x;
                            for (y = -ActiveRadius; y <= ActiveRadius; y++)
                            {
                                index = Conv.ChunkXyToIndex(ch2X, chY + y);
                                if (!ListChunkAction.Contains(index))
                                {
                                    ListChunkAction.Add(index);
                                }
                            }
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

        #endregion

        #region Overview

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

        #region Debug

        /// <summary>
        /// Флаг для дебага, когда смещались чанки или менялся обзор
        /// </summary>
        private bool _flagDebugAnchorChunkOffset;
        /// <summary>
        /// Флаг для дебага, изменены чанки в ChunkProvider
        /// </summary>
        public bool flagDebugChunkProvider;

        /// <summary>
        /// В тике обновляем
        /// </summary>
        private void _UpdateDebug()
        {
            if (Ce.FlagDebugDrawChunks && World.IdWorld == 0)
            {
                if (_flagDebugAnchorChunkOffset)
                {
                    _flagDebugAnchorChunkOffset = false;
                    World.Server.OnTagDebug("ChunksActive", ListChunkAction.ToArray());
                    World.Server.OnTagDebug("ChunkForAnchors", _chunkForAnchors.GetList().ToArray());
                }
                if (flagDebugChunkProvider)
                {
                    flagDebugChunkProvider = false;
                    World.Server.OnTagDebug("ChunkReady", World.ChunkPr.GetListDebug());
                }
            }
        }

        #endregion

        public override string ToString() => string.Format("A:{0} Wa:{1} ChA:{2}", 
                _anchors.Count, _worldAnchors.Count, _chunkForAnchors.Count);
    }
}
