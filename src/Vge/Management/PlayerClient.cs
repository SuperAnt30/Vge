using Vge.Actions;
using Vge.Event;
using Vge.Games;
using Vge.Network.Packets.Client;
using Vge.Network.Packets.Server;
using Vge.Renderer.World;
using Vge.Util;
using Vge.World.Chunk;
using WinGL.Util;

namespace Vge.Management
{
    /// <summary>
    /// Объект игрока, на клиенте
    /// </summary>
    public class PlayerClient : PlayerBase
    {
        /// <summary>
        /// Для Pitch предел Пи 1.55, аналог 89гр
        /// </summary>
        public const float Pi89 = 1.55334303f;

        /// <summary>
        /// Матрица просмотра Projection * LookAt
        /// </summary>
        public readonly float[] View = new float[16];
        /// <summary>
        /// Обект перемещений
        /// </summary>
        public readonly MovementInput Movement = new MovementInput();
        /// <summary>
        /// Массив всех видимых чанков 
        /// </summary>
        public readonly ListFast<ChunkRender> FrustumCulling = new ListFast<ChunkRender>();
        

        /// <summary>
        /// Класс  игры
        /// </summary>
        private readonly GameBase _game;
        /// <summary>
        /// Время запуска партии кусков
        /// </summary>
        private long _timeStartBatchChunks;
        /// <summary>
        /// Время мс на загрузку чанков
        /// </summary>
        private int _batchChunksTime;
        /// <summary>
        /// Количество на загрузку чанков
        /// </summary>
        private byte _batchChunksQuantity;
        /// <summary>
        /// Объект расчёта FrustumCulling
        /// </summary>
        private readonly Frustum _frustumCulling = new Frustum();
        /// <summary>
        /// Количество чанков в видимости камеры, незадействованных
        /// </summary>
        private int _countUnusedFrustumCulling;
        /// <summary>
        /// Количество тиков после последнего формирования FrustumCulling
        /// </summary>
        private int _countTickLastFrustumCulling;

        public PlayerClient(GameBase game)
        {
            _game = game;
            Login = game.ToLoginPlayer();
            Token = game.ToTokenPlayer();
            _UpdateMatrixCamera();
        }

        /// <summary>
        /// Изменение мыши
        /// </summary>
        /// <param name="centerBiasX">растояние по X от центра</param>
        /// <param name="centerBiasY">растояние по Y от центра</param>
        public void MouseMove(int centerBiasX, int centerBiasY)
        {
            if (centerBiasX == 0 && centerBiasY == 0) return;

            // Чувствительность мыши
            float speedMouse = Options.MouseSensitivityFloat;
            // Определяем углы смещения
            float pitch = Position.Pitch - centerBiasY / (float)Gi.Height * speedMouse;
            float yaw = Position.Yaw + centerBiasX / (float)Gi.Width * speedMouse;
            
            if (pitch < -Pi89) pitch = -Pi89;
            if (pitch > Pi89) pitch = Pi89;
            if (yaw > Glm.Pi) yaw -= Glm.Pi360;
            if (yaw < -Glm.Pi) yaw += Glm.Pi360;

            Position.Yaw = yaw;
            Position.Pitch = pitch;
        }

        /// <summary>
        /// Получить время в милисекундах
        /// </summary>
        protected override long _Time() => _game.Time();

        /// <summary>
        /// Задать обзор чанков у клиента
        /// </summary>
        public void SetOverviewChunk(byte overviewChunk, bool isSaveOptions)
        {
            if (OverviewChunk != overviewChunk)
            {
                SetOverviewChunk(overviewChunk);
                Ce.OverviewCircles = Sundry.GenOverviewCircles(overviewChunk);
                // Отправим обзор 
                _game.TrancivePacket(new PacketC15PlayerSetting(OverviewChunk));
                if (isSaveOptions)
                {
                    Options.OverviewChunk = OverviewChunk;
                    new OptionsFile().Save();
                }
            }
        }

        #region FrustumCulling Camera

        /// <summary>
        /// Камера была изменена
        /// </summary>
        public void  CameraHasBeenChanged()
        {
            _UpdateMatrixCamera();
            _InitFrustumCulling();
        }

        /// <summary>
        /// Обновить матрицу камеры
        /// </summary>
        private void _UpdateMatrixCamera()
        {
            Vector3 front = Glm.Ray(Position.Yaw, Position.Pitch);
            Vector3 up = new Vector3(0, 1, 0);
            Vector3 pos = new Vector3(0, 0, 0);
            Mat4 look = Glm.LookAt(pos, pos + front, new Vector3(0, 1, 0));
            //Mat4 projection = Glm.Perspective(1.43f, Gi.Width / (float)Gi.Height, 
            //    0.01f, 16 * 22f);
            Mat4 projection = Glm.PerspectiveFov(1.43f, Gi.Width, Gi.Height,
                0.01f, OverviewChunk * 22f);
            (projection * look).ConvArray(View);
        }

        /// <summary>
        /// Перерасчёт FrustumCulling
        /// </summary>
        private void _InitFrustumCulling()
        {
            _frustumCulling.Init(View);

            int chunkPosX = Position.ChunkPositionX;
            int chunkPosZ = Position.ChunkPositionZ;

            int i, xc, zc, xb, zb, x1, y1, z1, x2, y2, z2;
            ChunkRender chunk;
            Vector2i vec;
            ListFast<Vector2i> debug = Ce.IsDebugDrawChunks ? new ListFast<Vector2i>() : null;
            FrustumCulling.Clear();
            int countOC = Ce.OverviewCircles.Length;
            _countUnusedFrustumCulling = 0;
            _countTickLastFrustumCulling = 0;
            for (i = 0; i < countOC; i++)
            {
                vec = Ce.OverviewCircles[i];
                xc = vec.X;
                zc = vec.Y;
                xb = xc << 4;
                zb = zc << 4;

                x1 = xb - 12;
                y1 = -Position.PositionY;
                z1 = zb - 12;
                x2 = xb + 12;
                y2 = 128 - Position.PositionY;
                z2 = zb + 12;

                if (_frustumCulling.IsBoxInFrustum(x1, y1, z1, x2, y2, z2))
                {
                    chunk = _game.World.ChunkPrClient.GetChunkRender(xc + chunkPosX, zc + chunkPosZ);
                    if (chunk != null)
                    {
                        FrustumCulling.Add(chunk);
                    }
                    else
                    {
                        _countUnusedFrustumCulling++;
                    }
                    if (Ce.IsDebugDrawChunks)
                    {
                        debug.Add(new Vector2i(xc + chunkPosX, zc + chunkPosZ));
                    }
                }
            }
            if (Ce.IsDebugDrawChunks)
            {
                OnTagDebug(Debug.Key.FrustumCulling.ToString(), debug.ToArray());
            }
            Debug.CountMeshFC = FrustumCulling.Count;
        }

        #endregion

        /// <summary>
        /// Игровой такт
        /// </summary>
        public override void Update()
        {
            Vector2 motion = Sundry.MotionAngle(
                Movement.GetMoveStrafe(), Movement.GetMoveForward(),
                Movement.Sprinting ? 5 : 1, Position.Yaw);

            // Временно меняем перемещение если это надо
            Position.X += motion.X;
            Position.Z += motion.Y;
            Position.Y += Movement.GetMoveVertical();

            if (IsPositionChange())
            {
                CameraHasBeenChanged();
                PositionPrev.Set(Position);
                _game.TrancivePacket(new PacketC04PlayerPosition(
                    new Vector3(Position.X, Position.Y, Position.Z),
                    false, false, false, IdWorld));
                Debug.Player = Position.GetChunkPosition();
            }

            if (_countUnusedFrustumCulling > 0
                && ++_countTickLastFrustumCulling > Ce.CheckTickInitFrustumCulling)
            {
                _InitFrustumCulling();
            }
        }

        #region Packet

        /// <summary>
        /// Пакет Возраждение в мире
        /// </summary>
        public void PacketRespawnInWorld(PacketS07RespawnInWorld packet)
        {
            IdWorld = packet.IdWorld;
            _game.World.ChunkPr.SetHeightChunks(packet.NumberChunkSections);
        }

        /// <summary>
        /// Замер скорости закачки чанков
        /// </summary>
        public void PacketChunckSend(PacketS20ChunkSend packet)
        {
            if (packet.Start)
            {
                // Начало замера
                _timeStartBatchChunks = _Time();
            }
            else
            {
                // Закончили замер
                _batchChunksTime = (int)(_Time() - _timeStartBatchChunks);
                _batchChunksQuantity = packet.Quantity;
                _game.TrancivePacket(new PacketC20AcknowledgeChunks(_batchChunksTime, _batchChunksQuantity));
            }
        }

        #endregion

        public override string ToString()
        {
            return Login + " " + Position + " O:" + OverviewChunk
                + " batch:" + _batchChunksQuantity + "|" + _batchChunksTime + "mc "
                + Movement;
        }

        /// <summary>
        /// Событие любого объекта с сервера для отладки
        /// </summary>
        public event StringEventHandler TagDebug;
        public void OnTagDebug(string title, object tag)
            => TagDebug?.Invoke(this, new StringEventArgs(title, tag));
    }
}
