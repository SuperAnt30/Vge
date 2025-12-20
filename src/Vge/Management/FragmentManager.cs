using System;
using System.Collections.Generic;
using Vge.Entity.Player;
using Vge.Games;
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
        /// Дополнение для чанков сервера обзора
        /// </summary>
        public const int AddOverviewChunkServer = 4;
        /// <summary>
        /// Сетевой мир
        /// </summary>
        public readonly WorldServer World;
        /// <summary>
        /// Основной сервер
        /// </summary>
        public readonly GameServer Server;

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
        /// Чанки игроков она же выступает в роли маски какие нельзя выгружать и какие может видеть игрок
        /// </summary>
        private MapChunk _chunkForAnchors = new MapChunk();
        /// <summary>
        /// Этот список используется, когда чанк должен быть обработан (каждые 8000 тиков)
        /// </summary>
        private ListMessy<ChunkForAnchor> _chunkAnchorsList = new ListMessy<ChunkForAnchor>();
        /// <summary>
        /// Список ChunkForAnchor которые надо обновить
        /// </summary>
        private ListMessy<ChunkForAnchor> _chunkAnchorsToUpdate = new ListMessy<ChunkForAnchor>();

        /// <summary>
        /// флаг для обновления активных чанков
        /// </summary>
        private bool _flagUpListChunkAction = true;

        /// <summary>
        /// Время, которое используется, чтобы проверить, следует ли рассчитывать _chunkForAnchors 
        /// </summary>
        private uint _previousTotalWorldTime;

        /// <summary>
        /// Объект сыщик
        /// </summary>
        private readonly Profiler _filer;
        

        public FragmentManager(WorldServer world)
        {
            World = world;
            Server = World.Server;
            _filer = new Profiler(Server.Log, "[Server] ");
        }

        /// <summary>
        /// Получить радиус для сервера учитывая активные чанки
        /// </summary>
        public static int GetActiveRadiusAddServer(int radius, IAnchor anchor)
        {
            if (radius < anchor.ActiveRadius)
            {
                radius = anchor.ActiveRadius;
            }
            radius += AddOverviewChunkServer;
            return radius;
        }

        #region Anchor

        /// <summary>
        /// Добавить якорь
        /// </summary>
        public void AddAnchor(IAnchor anchor)
        {
            if (anchor.IsActive)
            {
                _actionAnchors.Add(anchor);
                _flagUpListChunkAction = true;
            }
            _anchors.Add(anchor);
            // Добавить все чанки вокруг якоря
            _OverviewChunkAddAnchor(anchor, GetActiveRadiusAddServer(anchor.OverviewChunk, anchor), false);
            if (anchor.IsPlayer)
            {
                _OverviewChunkAddAnchor(anchor, anchor.OverviewChunk, true);
            }
        }

        /// <summary>
        /// Удалить якорь
        /// </summary>
        public void RemoveAnchor(IAnchor anchor)
        {
            // Убрать все чанки принадлежащие якорю
            if (anchor.IsPlayer)
            {
                _OverviewChunkPutAwayAnchor(anchor, anchor.OverviewChunk, true);
            }
            _OverviewChunkPutAwayAnchor(anchor, GetActiveRadiusAddServer(anchor.OverviewChunk, anchor), false);
            // Убрать из списка якорей
            _anchors.Remove(anchor);
            if (anchor.IsActive)
            {
                _actionAnchors.Remove(anchor);
                _flagUpListChunkAction = true;
            }
        }

        #endregion

        #region Update

        public void Update()
        {
            // Обновить список активных чанков
            if (_flagUpListChunkAction)
            {
                _flagUpListChunkAction = false;
                // Обновить список активных чанков
                _filer.StartSection("Fragment.UpListChunkAction");
                _UpListChunkAction();
                _filer.EndSection();
                flagDebugAnchorChunkOffset = true;
            }

            _filer.StartSection("Fragment.LoadingChunks");
            if (_UpdateLoadingChunks()) flagDebugChunkProviderServer = true;
            _filer.EndStartSection("Fragment.ChunksUpdate");
            _ChunksUpdate();
            _filer.EndSection();
        }

        /// <summary>
        /// Загружаем чанки при необходимости
        /// </summary>
        private bool _UpdateLoadingChunks()
        {
            int countAnchor = _anchors.Count;
            bool load = false;
            if (countAnchor > 0)
            {
                int a, x, y;
                ulong index;
                bool present = true;
                // Размер партии закачки чанков
                int loadingBatchSize = World.ChunkPrServ.LoadingBatchSize;
                
                try
                {
                    while (present)
                    {
                        present = false;
                        for (a = 0; a < countAnchor; a++)
                        {
                            if (_anchors[a].CheckLoadingChunks())
                            {
                                present = true;
                                index = _anchors[a].ReturnChunkForLoading();
                                x = Conv.IndexToChunkX(index);
                                y = Conv.IndexToChunkY(index);
                                if (!World.ChunkPrServ.NeededChunk(x, y))
                                {
                                    // Чанк отсутствовал
                                    if (!load) load = true;
                                }
                            }
                        }
                        if (present && --loadingBatchSize <= 0)
                        {
                            present = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Server.Log.Error("LoadingChunks");
                    Logger.Crash(ex, "LoadingChunks");
                    throw;
                }
            }
            return load;
        }

        /// <summary>
        /// Обновление в каждом такте чанков которые были помечены для обнолвения
        /// </summary>
        private void _ChunksUpdate()
        {
            // Чистка чанков по времени
            uint time = World.TickCounter;
            int i, count;
            if (time - _previousTotalWorldTime > 8000)
            {
                _previousTotalWorldTime = time;
                count = _chunkAnchorsList.Count;
                for (i = 0; i < count; i++)
                {
                    _chunkAnchorsList[i].Update();
                    _chunkAnchorsList[i].ProcessChunk();
                }
                _chunkAnchorsToUpdate.Clear();
            }
            else
            {
                count = _chunkAnchorsToUpdate.Count - 1;
                for (i = count; i >= 0; i--)
                {
                    _chunkAnchorsToUpdate[i].Update();
                    _chunkAnchorsToUpdate.RemoveLast();
                }
            }
        }

        #endregion

        #region ChunkForAnchor

        /// <summary>
        /// Добавить якорь в конкретный чанк, если нет чанка, создаём его
        /// </summary>
        private void _ChunkForAnchorAdd(int x, int y, IAnchor anchor, bool isClient)
        {
            if (isClient)
            {
                if (anchor is PlayerServer playerServer)
                {
                    playerServer.AddChunkClient(x, y);
                }
            }
            else
            {
                ChunkForAnchor chunkForAnchor = _chunkForAnchors.Get(x, y) as ChunkForAnchor;
                if (chunkForAnchor == null)
                {
                    // Создаём чанк для якоря
                    chunkForAnchor = new ChunkForAnchor(World, x, y);
                    _chunkForAnchors.Add(chunkForAnchor);
                    _chunkAnchorsList.Add(chunkForAnchor);
                }
                chunkForAnchor.AddAnchor(anchor);
            }
        }

        /// <summary>
        /// Удалить якорь с конкретного чанка 
        /// </summary>
        private void _ChunkForAnchorRemove(int x, int y, IAnchor anchor, bool isClient)
        {
            ChunkForAnchor chunkForAnchor = _chunkForAnchors.Get(x, y) as ChunkForAnchor;
            if (chunkForAnchor != null)
            {
                if (isClient)
                {
                    if (anchor is PlayerServer playerServer)
                    {
                        playerServer.RemoveChunkClient(x, y);
                    }
                }
                else
                {
                    if (chunkForAnchor.RemoveAnchor(anchor))
                    {
                        // Удалить чанк
                        World.ChunkPrServ.DropChunk(x, y);
                        _chunkForAnchors.Remove(x, y);
                        _chunkAnchorsList.Remove(chunkForAnchor);
                        if (chunkForAnchor.NumBlocksToUpdate > 0)
                        {
                            _chunkAnchorsToUpdate.Remove(chunkForAnchor);
                        }
                        flagDebugAnchorChunkOffset = true;
                        flagDebugChunkProviderServer = true;
                    }
                }
            }
        }

        #endregion

        #region Обновление фрагментов

        /// <summary>
        /// Флаг блока который был изменён
        /// </summary>
        public void FlagBlockForUpdate(int x, int y, int z)
        {
            if (_chunkForAnchors.Get(x >> 4, z >> 4) is ChunkForAnchor chunkForAnchor)
            {
                if (chunkForAnchor.NumBlocksToUpdate == 0)
                {
                    _chunkAnchorsToUpdate.Add(chunkForAnchor);
                }
                chunkForAnchor.FlagBlockForUpdate(x & 15, y, z & 15);
            }
        }

        /// <summary>
        /// Флаг сектора чанка который был изменён
        /// </summary>
        public void FlagChunkForUpdate(int x, int y, int z)
        {
            if (_chunkForAnchors.Get(x, z) is ChunkForAnchor chunkForAnchor)
            {
                if (chunkForAnchor.NumBlocksToUpdate == 0)
                {
                    _chunkAnchorsToUpdate.Add(chunkForAnchor);
                }
                chunkForAnchor.FlagChunkForUpdate(y);
            }
        }

        /// <summary>
        /// Обновлять фрагменты чанков вокруг якоря, перемещаемого логикой сервера
        /// </summary>
        public void UpdateMountedMovingAnchor(IAnchor anchor)
        {
            bool change = false;
            // Проверяем изменение обзора чанка
            if (anchor.IsChangeOverview())
            {
                int radius = anchor.OverviewChunk;
                int radiusPrev = anchor.OverviewChunkPrev;
                int chmx = anchor.ChunkPosManagedX;
                int chmy = anchor.ChunkPosManagedZ;

                if (anchor.IsPlayer)
                {
                    // Только для игрока создаём специальный слой карты чанков, которые видит игрок
                    _UpdateMountedMovingAnchorOverviewChunk(true, anchor, chmx, chmy,
                        Mth.Max(radius, radiusPrev), radiusPrev, radius);
                }

                if (radiusPrev != 0)
                {
                    radiusPrev = GetActiveRadiusAddServer(radiusPrev, anchor);
                }
                radius = GetActiveRadiusAddServer(radius, anchor);
                // Для всех остальных обзор на сервере больше из-за доп шагов генерации мира
                _UpdateMountedMovingAnchorOverviewChunk(false, anchor, chmx, chmy,
                       Mth.Max(radius, radiusPrev), radiusPrev, radius);

                change = true;
            }

            // Активация смещения при смещении количество чанков
            if (anchor.IsAnOffsetNecessary())
            {
                // Смещение чанка
                // Проверяем смещение чанка на выбранный параметр, если есть начинаем обработку
                int radius = anchor.OverviewChunk;
                int chx = anchor.ChunkPositionX;
                int chy = anchor.ChunkPositionZ;
                int chmx = anchor.ChunkPosManagedX;
                int chmy = anchor.ChunkPosManagedZ;
                // Проверка перемещения обзора чанков у клиента
                if (anchor.IsPlayer)
                {
                    // Только для игрока создаём специальный слой карты чанков, которые видит игрок
                    _UpdateMountedMovingAnchorRadius(true, anchor, chx, chy, chmx, chmy, anchor.OverviewChunk);
                }
                radius = GetActiveRadiusAddServer(radius, anchor);
                // Для всех остальных обзор на сервере больше из-за доп шагов генерации мира
                _UpdateMountedMovingAnchorRadius(false, anchor, chx, chy, chmx, chmy, radius);
                change = true;
            }

            if (change)
            {
                anchor.MountedMovedAnchor();
                flagDebugAnchorChunkOffset = true;
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
                byte activeRadius;
                for (i = 0; i < count; i++)
                {
                    // Собираем чанки где есть игроки, тикущий и по соседнему
                    if (_anchors[i].IsActive)
                    {
                        chX = _anchors[i].ChunkPositionX;
                        chY = _anchors[i].ChunkPositionZ;

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

                        activeRadius = _anchors[i].ActiveRadius;
                        // TODO::2025-12-20 продумать сортировку активных, впервую очередь которые ближе, чтоб можно по времени отсекать не обязательные
                        // Собираем все активные чанки для будущих тиков
                        for (x = -activeRadius; x <= activeRadius; x++)
                        {
                            ch2X = chX + x;
                            for (y = -activeRadius; y <= activeRadius; y++)
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
        private void _UpdateMountedMovingAnchorOverviewChunk(bool isClient, IAnchor anchor, int chx, int chz,
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
                int xmax2 = chx + radiusPrev;
                int zmin2 = chz - radiusPrev;
                int zmax2 = chz + radiusPrev;
                _OverviewChunkAddSquare(isClient, anchor, xmin, xmax, zmin, zmax, 
                    xmin2, xmax2, zmin2, zmax2);
            }
            else
            {
                // Уменьшить обзор
                _OverviewChunkPutAwaySquare(isClient, anchor, xmin, xmax, zmin, zmax,
                    chx - radiusMin, chx + radiusMin, chz - radiusMin, chz + radiusMin);
            }
        }

        /// <summary>
        /// Обнолвение фрагмента при перемещении, для клиента или для кэш сервера
        /// </summary>
        private void _UpdateMountedMovingAnchorRadius(bool isClient, IAnchor anchor,
            int chx, int chz, int chmx, int chmz, int radius)
        {
            if (Mth.Abs(chx - chmx) < radius && Mth.Abs(chz - chmz) < radius)
            {
                // Смещение в пределе радиуса обзора
                int xmin = chx - radius;
                int xmax = chx + radius;
                int zmin = chz - radius;
                int zmax = chz + radius;
                int xmin2 = chmx - radius;
                int xmax2 = chmx + radius;
                int zmin2 = chmz - radius;
                int zmax2 = chmz + radius;

                // Определяем добавление
                _OverviewChunkAddSquare(isClient, anchor, xmin, xmax, zmin, zmax,
                    xmin2, xmax2, zmin2, zmax2);
                // Определяем какие убираем
                _OverviewChunkPutAwaySquare(isClient, anchor, xmin2, xmax2, zmin2, zmax2,
                    xmin, xmax + 0, zmin, zmax + 0);
            }
            else
            {
                // Смещение за пределами обзора, к примеру телепортация
                // Убрать все чанки принадлежащие
                int x, z;
                int xmin = chmx - radius;
                int xmax = chmx + radius + 1;
                int zmin = chmz - radius;
                int zmax = chmz + radius + 1;
                for (x = xmin; x < xmax; x++)
                {
                    for (z = zmin; z < zmax; z++)
                    {
                        _ChunkForAnchorRemove(x, z, anchor, isClient);
                    }
                }
                // Добавить все чанки вокруг якоря
                _OverviewChunkAddAnchor(anchor, radius, isClient);
            }
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
        private void _OverviewChunkAddSquare(bool isClient, IAnchor anchor,
            int xMinFor, int xMaxFor, int zMinFor, int zMaxFor,
            int xMinCheck, int xMaxCheck, int zMinCheck, int zMaxCheck)
        {
            int x, z;
            // Углы
            for (x = xMinFor; x < xMinCheck; x++)
            {
                for (z = zMinFor; z < zMinCheck; z++)
                {
                    _ChunkForAnchorAdd(x, z, anchor, isClient);
                }
            }
            for (x = xMaxCheck + 1; x < xMaxFor + 1; x++)
            {
                for (z = zMinFor; z < zMinCheck; z++)
                {
                    _ChunkForAnchorAdd(x, z, anchor, isClient);
                }
            }
            for (x = xMinFor; x < xMinCheck; x++)
            {
                for (z = zMaxCheck + 1; z < zMaxFor + 1; z++)
                {
                    _ChunkForAnchorAdd(x, z, anchor, isClient);
                }
            }
            for (x = xMaxCheck + 1; x < xMaxFor + 1; x++)
            {
                for (z = zMaxCheck + 1; z < zMaxFor + 1; z++)
                {
                    _ChunkForAnchorAdd(x, z, anchor, isClient);
                }
            }

            // Стороны
            // Up
            for (z = zMinFor; z < zMinCheck; z++)
            {
                for (x = xMinCheck; x <= xMaxCheck; x++)
                {
                    _ChunkForAnchorAdd(x, z, anchor, isClient);
                }
            }
            // Down
            for (z = zMaxCheck + 1; z < zMaxFor + 1; z++)
            {
                for (x = xMinCheck; x <= xMaxCheck; x++)
                {
                    _ChunkForAnchorAdd(x, z, anchor, isClient);
                }
            }

            // Left
            for (x = xMinFor; x < xMinCheck; x++)
            {
                for (z = zMinCheck; z <= zMaxCheck; z++)
                {
                    _ChunkForAnchorAdd(x, z, anchor, isClient);
                }
            }
            // Right
            for (x = xMaxCheck + 1; x < xMaxFor + 1; x++)
            {
                for (z = zMinCheck; z <= zMaxCheck; z++)
                {
                    _ChunkForAnchorAdd(x, z, anchor, isClient);
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
        private void _OverviewChunkPutAwaySquare(bool isClient, IAnchor anchor,
            int xMinFor, int xMaxFor, int zMinFor, int zMaxFor,
            int xMinCheck, int xMaxCheck, int zMinCheck, int zMaxCheck)
        {
            int x, z;
            // Углы
            // LeftUp
            for (x = xMinFor; x < xMinCheck; x++)
            {
                for (z = zMinFor; z < zMinCheck; z++)
                {
                    _ChunkForAnchorRemove(x, z, anchor, isClient);
                }
            }
            for (x = xMaxCheck + 1; x < xMaxFor + 1; x++)
            {
                for (z = zMinFor; z < zMinCheck; z++)
                {
                    _ChunkForAnchorRemove(x, z, anchor, isClient);
                }
            }
            for (x = xMinFor; x < xMinCheck; x++)
            {
                for (z = zMaxCheck + 1; z < zMaxFor + 1; z++)
                {
                    _ChunkForAnchorRemove(x, z, anchor, isClient);
                }
            }
            for (x = xMaxCheck + 1; x < xMaxFor + 1; x++)
            {
                for (z = zMaxCheck + 1; z < zMaxFor + 1; z++)
                {
                    _ChunkForAnchorRemove(x, z, anchor, isClient);
                }
            }

            // Стороны
            // Up
            for (z = zMinFor; z < zMinCheck; z++)
            {
                for (x = xMinCheck; x <= xMaxCheck; x++)
                {
                    _ChunkForAnchorRemove(x, z, anchor, isClient);
                }
            }
            // Down
            for (z = zMaxCheck + 1; z < zMaxFor + 1; z++)
            {
                for (x = xMinCheck; x <= xMaxCheck; x++)
                {
                    _ChunkForAnchorRemove(x, z, anchor, isClient);
                }
            }

            // Left
            for (x = xMinFor; x < xMinCheck; x++)
            {
                for (z = zMinCheck; z <= zMaxCheck; z++)
                {
                    _ChunkForAnchorRemove(x, z, anchor, isClient);
                }
            }
            // Right
            for (x = xMaxCheck + 1; x < xMaxFor + 1; x++)
            {
                for (z = zMinCheck; z <= zMaxCheck; z++)
                {
                    _ChunkForAnchorRemove(x, z, anchor, isClient);
                }
            }
        }

        #region Old

        /// <summary>
        /// Добавить чанки обзора при необходимости
        /// Рабочий, но углы дублируются
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
        private void _OverviewChunkAddSquareOld(bool isClient, IAnchor anchor,
            int xMinFor, int xMaxFor, int zMinFor, int zMaxFor,
            int xMinCheck, int xMaxCheck, int zMinCheck, int zMaxCheck)
        {
            int x, z;
            xMaxCheck++;
            zMaxCheck++;
            for (x = xMinFor; x < xMinCheck; x++)
            {
                for (z = zMinFor; z <= zMaxFor; z++)
                {
                    _ChunkForAnchorAdd(x, z, anchor, isClient);
                }
            }
            for (x = xMaxCheck; x <= xMaxFor; x++)
            {
                for (z = zMinFor; z <= zMaxFor; z++)
                {
                    _ChunkForAnchorAdd(x, z, anchor, isClient);
                }
            }
            for (z = zMinFor; z < zMinCheck; z++)
            {
                for (x = xMinFor; x <= xMaxFor; x++)
                {
                    _ChunkForAnchorAdd(x, z, anchor, isClient);
                }
            }
            for (z = zMaxCheck; z <= zMaxFor; z++)
            {
                for (x = xMinFor; x <= xMaxFor; x++)
                {
                    _ChunkForAnchorAdd(x, z, anchor, isClient);
                }
            }
        }

        /// <summary>
        /// Убрать чанки обзора при необходимости
        /// Рабочий, но углы дублируются
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
        private void _OverviewChunkPutAwaySquareOld(bool isClient, IAnchor anchor,
            int xMinFor, int xMaxFor, int zMinFor, int zMaxFor,
            int xMinCheck, int xMaxCheck, int zMinCheck, int zMaxCheck)
        {
            int x, z;
            xMaxCheck++;
            zMaxCheck++;
            for (x = xMinFor; x < xMinCheck; x++)
            {
                for (z = zMinFor; z <= zMaxFor; z++)
                {
                    _ChunkForAnchorRemove(x, z, anchor, isClient);
                }
            }
            for (x = xMaxCheck; x <= xMaxFor; x++)
            {
                for (z = zMinFor; z <= zMaxFor; z++)
                {
                    _ChunkForAnchorRemove(x, z, anchor, isClient);
                }
            }

            for (z = zMinFor; z < zMinCheck; z++)
            {
                for (x = xMinFor; x <= xMaxFor; x++)
                {
                    _ChunkForAnchorRemove(x, z, anchor, isClient);
                }
            }
            for (z = zMaxCheck; z <= zMaxFor; z++)
            {
                for (x = xMinFor; x <= xMaxFor; x++)
                {
                    _ChunkForAnchorRemove(x, z, anchor, isClient);
                }
            }
        }

        #endregion

        /// <summary>
        /// Добавить все чанки вокруг якоря
        /// </summary>
        private void _OverviewChunkAddAnchor(IAnchor anchor, int radius, bool isClient)
        {
            int chx = anchor.ChunkPositionX;
            int chy = anchor.ChunkPositionZ;
            int x, z;
            int xmin = chx - radius;
            int xmax = chx + radius + 1;
            int zmin = chy - radius;
            int zmax = chy + radius + 1;
            for (x = xmin; x < xmax; x++)
            {
                for (z = zmin; z < zmax; z++)
                {
                    _ChunkForAnchorAdd(x, z, anchor, isClient);
                }
            }
        }

        /// <summary>
        /// Убрать все чанки принадлежащие якорю
        /// </summary>
        private void _OverviewChunkPutAwayAnchor(IAnchor anchor, int radius, bool isClient)
        {
            int chx = anchor.ChunkPositionX;
            int chy = anchor.ChunkPositionZ;
            int x, z;
            int xmin = chx - radius;
            int xmax = chx + radius + 1;
            int zmin = chy - radius;
            int zmax = chy + radius + 1;
            for (x = xmin; x < xmax; x++)
            {
                for (z = zmin; z < zmax; z++)
                {
                    _ChunkForAnchorRemove(x, z, anchor, isClient);
                }
            }
        }

        #endregion

        #region Debug

        /// <summary>
        /// Флаг для дебага, когда смещались чанки или менялся обзор
        /// </summary>
        public bool flagDebugAnchorChunkOffset;
        /// <summary>
        /// Флаг для дебага, изменены чанки в ChunkProviderServer
        /// </summary>
        public bool flagDebugChunkProviderServer;

        /// <summary>
        /// В тике обновляем отладку чанков в 2д
        /// </summary>
        public void UpdateDebug()
        {
            if (flagDebugAnchorChunkOffset)
            {
                flagDebugAnchorChunkOffset = false;
                Server.OnTagDebug(Debug.Key.ChunksActive.ToString(), ListChunkAction.ToArray());
                Server.OnTagDebug(Debug.Key.ChunkForAnchors.ToString(), _chunkAnchorsList.ToArray());
                
            }
            if (flagDebugChunkProviderServer)
            {
                flagDebugChunkProviderServer = false;
                Server.OnTagDebug(Debug.Key.ChunkReady.ToString(), World.ChunkPr.GetListDebug());
            }
        }

        #endregion

        public override string ToString() => "A:" + _anchors.Count
            + " ChA:" + _chunkForAnchors.ToString();
    }
}
