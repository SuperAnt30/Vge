using System.Runtime.CompilerServices;
using Vge.Actions;
using Vge.Entity;
using Vge.Event;
using Vge.Games;
using Vge.Network.Packets.Client;
using Vge.Network.Packets.Server;
using Vge.Realms;
using Vge.Renderer.World;
using Vge.Util;
using Vge.World.Block;
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

        #region PositionFrame 

        /// <summary>
        /// Позиция этой сущности по оси X
        /// </summary>
        public float PosFrameX;
        /// <summary>
        /// Позиция этой сущности по оси Y
        /// </summary>
        public float PosFrameY;
        /// <summary>
        /// Позиция этой сущности по оси Z
        /// </summary>
        public float PosFrameZ;

        /// <summary>
        /// Вращение этой сущности по оси Y
        /// </summary>
        public float RotationFrameYaw;
        /// <summary>
        /// Вращение этой сущности вверх вниз
        /// </summary>
        public float RotationFramePitch;

        #endregion

        /// <summary>
        /// Объект кэш чата
        /// </summary>
        public readonly ChatList Chat;

        /// <summary>
        /// Матрица просмотра Projection * LookAt
        /// </summary>
        public readonly float[] View = new float[16];
        
        /// <summary>
        /// Массив всех видимых чанков 
        /// </summary>
        public readonly ListFast<ChunkRender> FrustumCulling = new ListFast<ChunkRender>();

        /// <summary>
        /// Выбранный объект
        /// </summary>
        public MovingObjectPosition MovingObject { get; private set; } = new MovingObjectPosition();

        /// <summary>
        /// Позиция камеры в блоке для альфа, в зависимости от вида (с глаз, с зади, спереди)
        /// </summary>
        public Vector3i PositionAlphaBlock { get; private set; }

        /// <summary>
        /// Обект перемещений
        /// </summary>
        public MovementInput Movement => Physics.Movement;

        /// <summary>
        /// Позиция камеры в чанке для альфа, в зависимости от вида (с глаз, с зади, спереди)
        /// </summary>
        private Vector3i _positionAlphaChunk;
        /// <summary>
        /// Позиция когда был запрос рендера для альфа блоков для малого смещения, в чанке
        /// </summary>
        private Vector3i _positionAlphaBlockPrev;
        /// <summary>
        /// Позиция когда был запрос рендера для альфа блоков для большого смещения, за пределами чанка
        /// </summary>
        private Vector3i _positionAlphaChunkPrev;

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
        /// <summary>
        /// Высота для FrustumCulling
        /// </summary>
        private int _heightChinkFrustumCulling;
        /// <summary>
        /// Вектор луча
        /// </summary>
        private Vector3 _rayLook;

        public PlayerClient(GameBase game)
        {
            _game = game;
            Login = game.ToLoginPlayer();
            Token = game.ToTokenPlayer();
            Chat = new ChatList(Ce.ChatLineTimeLife, _game.Render.FontMain);
            _UpdateMatrixCamera();
            Eye = Height * .85f;
        }

        /// <summary>
        /// Запуск мира
        /// </summary>
        public void WorldStarting()
        {
            //Physics = new PhysicsFly(_game.World.Collision, this);
            Physics = new PhysicsGround(_game.World.Collision, this);
        }

        #region Методы от Entity

        /// <summary>
        /// Наблюдение
        /// </summary>
        public bool IsSpectator() => false;

        #endregion

        #region Inputs (Mouse Key)

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
            float pitch = RotationPitch - centerBiasY / (float)Gi.Height * speedMouse;
            float yaw = RotationYaw + centerBiasX / (float)Gi.Width * speedMouse;
            
            if (pitch < -Pi89) pitch = -Pi89;
            if (pitch > Pi89) pitch = Pi89;
            if (yaw > Glm.Pi) yaw -= Glm.Pi360;
            if (yaw < -Glm.Pi) yaw += Glm.Pi360;

            RotationYaw = yaw;
            RotationPitch = pitch;
        }

        /// <summary>
        /// Остановить все инпуты
        /// </summary>
        public void ActionStop()
        {
            Movement.SetStop();
            if (!_game.IsRunNet())
            {
                PosFrameX = PosPrevX = PosX;
                PosFrameY = PosPrevY = PosY;
                PosFrameZ = PosPrevZ = PosZ;
                RotationFrameYaw = RotationPrevYaw = RotationYaw;
                RotationFramePitch = RotationPrevPitch = RotationPitch;
            }
        }

        #endregion

        #region Действие рук, левый или правый клик

        /// <summary>
        /// Основное действие правой рукой. Левый клик мыши
        /// (old HandAction)
        /// </summary>
        public void HandAction()
        {
            if (!IsSpectator() && true)
            {
                if (MovingObject.IsBlock())
                {
                    _game.TrancivePacket(new PacketC07PlayerDigging(MovingObject.BlockPosition, PacketC07PlayerDigging.EnumDigging.Destroy));
                }
            }
        }

        /// <summary>
        /// Отмена действия правой рукой
        /// (old UndoHandAction)
        /// </summary>
        public void UndoHandAction()
        {
           // _game.TrancivePacket(new PacketC07PlayerDigging(MovingObject.BlockPosition, PacketC07PlayerDigging.EnumDigging.About));
        }

        /// <summary>
        /// Использовать предмет (ставим блок, кушаем). Правый клик мыши
        /// (old HandActionRight)
        /// </summary>
        public void ItemUse()
        {
            if (MovingObject.IsBlock())
            {
                _game.TrancivePacket(new PacketC08PlayerBlockPlacement(MovingObject.BlockPosition,
                    MovingObject.Side, MovingObject.Facing));
            }
        }

        /// <summary>
        /// Остановить использование предмета
        /// (old OnStoppedUsingItem)
        /// </summary>
        public void StoppedUsingItem()
        {

        }

        #endregion

        #region FrustumCulling Camera

        /// <summary>
        /// Камера была изменена
        /// </summary>
        private void  _CameraHasBeenChanged()
        {
            _UpdateMatrixCamera();
            _InitFrustumCulling();
        }

        /// <summary>
        /// Обновить матрицу камеры
        /// </summary>
        private void _UpdateMatrixCamera()
        {
            Vector3 front = Glm.Ray(RotationFrameYaw, RotationFramePitch);
            Vector3 up = new Vector3(0, 1, 0);
            Vector3 pos = new Vector3(0, Eye, 0);
            _rayLook = front;
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

            int chunkPosX = ChunkPositionX;
            int chunkPosZ = ChunkPositionZ;

            int i, xc, zc, xb, zb, x1, y1, z1, x2, y2, z2;
            ChunkRender chunk;
            Vector3i vec;
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

                x1 = xb - 15;
                y1 = -PositionY;
                z1 = zb - 15;
                x2 = xb + 15;
                y2 = _heightChinkFrustumCulling - PositionY;
                z2 = zb + 15;

                if (_frustumCulling.IsBoxInFrustum(x1, y1, z1, x2, y2, z2))
                {
                    chunk = _game.World.ChunkPrClient.GetChunkRender(xc + chunkPosX, zc + chunkPosZ);
                    if (chunk != null)
                    {
                        chunk.Distance = vec.Z;
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

        /// <summary>
        /// Помечаем на перерендер всех чанков в округ игрока
        /// </summary>
        public void RerenderAllChunks()
        {
            int countOC = Ce.OverviewCircles.Length;

            int chunkPosX = ChunkPositionX;
            int chunkPosZ = ChunkPositionZ;

            int i, xc, zc, y1, y2;
            y2 = _game.World.ChunkPr.Settings.NumberSections;
            ChunkRender chunk;
            Vector3i vec;

            for (i = 0; i < countOC; i++)
            {
                vec = Ce.OverviewCircles[i];
                xc = vec.X;
                zc = vec.Y;
                chunk = _game.World.ChunkPrClient.GetChunkRender(xc + chunkPosX, zc + chunkPosZ);
                if (chunk != null)
                {
                    for (y1 = 0; y1 < y2; y1++)
                    {
                        chunk.ModifiedToRender(y1);
                    }
                }
            }
        }

        #endregion

        #region Update

        /// <summary>
        /// Обновление в кадре
        /// </summary>
        public void UpdateFrame(float timeIndex)
        {
            if (PosX != PosFrameX || PosY != PosFrameY || PosZ != PosFrameZ
                || RotationYaw != RotationFrameYaw || RotationPitch != RotationFramePitch)
            {
                if (timeIndex >= 1f)
                {
                    if (PosX != PosPrevX) PosFrameX = PosX;
                    if (PosY != PosPrevY) PosFrameY = PosY;
                    if (PosZ != PosPrevZ) PosFrameZ = PosZ;
                    if (RotationYaw != RotationPrevYaw) RotationFrameYaw = RotationYaw;
                    if (RotationPitch != RotationPrevPitch) RotationFramePitch = RotationPitch;
                }
                else
                {
                    PosFrameX = PosPrevX + (PosX - PosPrevX) * timeIndex;
                    PosFrameY = PosPrevY + (PosY - PosPrevY) * timeIndex;
                    PosFrameZ = PosPrevZ + (PosZ - PosPrevZ) * timeIndex;
                    float biasYaw = RotationYaw - RotationPrevYaw;
                    if (biasYaw > Glm.Pi)
                    {
                        RotationFrameYaw = RotationPrevYaw + (RotationYaw - Glm.Pi360 - RotationPrevYaw) * timeIndex;
                    }
                    else if (biasYaw < -Glm.Pi)
                    {
                        RotationFrameYaw = RotationPrevYaw + (RotationYaw + Glm.Pi360 - RotationPrevYaw) * timeIndex;
                    }
                    else
                    {
                        RotationFrameYaw = RotationPrevYaw + biasYaw * timeIndex;
                    }
                    RotationFramePitch = RotationPrevPitch + (RotationPitch - RotationPrevPitch) * timeIndex;
                }
                _CameraHasBeenChanged();
            }
            //_game.Log.Log(Position.ToStringPos() + " | "
            //    + PositionPrev.ToStringPos() + " | "
            //    + PositionFrame.ToStringPos());
        }
         

        /// <summary>
        /// Игровой такт
        /// </summary>
        public override void Update()
        {
            if (IsPositionChange())
            {
                PosPrevX = PosX;
                PosPrevY = PosY;
                PosPrevZ = PosZ;
            }

            // Расчитать перемещение в объекте физика
            Physics.LivingUpdate();

            // Для отправки
            if (IsRotationChange())
            {
                RotationPrevYaw = RotationYaw;
                RotationPrevPitch = RotationPitch;

                if (Physics.IsMotionChange)
                {
                    // И перемещение и вращение
                    _game.TrancivePacket(new PacketC04PlayerPosition(PosX, PosY, PosZ, RotationYaw, RotationPitch, false));
                }
                else
                {
                    // Только вращение
                    _game.TrancivePacket(new PacketC04PlayerPosition(RotationYaw, RotationPitch, false));
                }
            }
            else if (Physics.IsMotionChange)
            {
                // Только перемещение
                _game.TrancivePacket(new PacketC04PlayerPosition(PosX, PosY, PosZ, false));
            }

            if (_countUnusedFrustumCulling > 0
                && ++_countTickLastFrustumCulling > Ce.CheckTickInitFrustumCulling)
            {
                _InitFrustumCulling();
            }

            // Обновление курсора, не зависимо от действия игрока, так-как рядом может быть изминение
            _UpCursor();

            // Проверка на обновление чанков альфа блоков, в такте после перемещения
            _UpdateChunkRenderAlphe();
        }

        /// <summary>
        /// Проверка на обновление чанков альфа блоков, в такте после перемещения
        /// </summary>
        private void _UpdateChunkRenderAlphe()
        {
            PositionAlphaBlock = new Vector3i(PosX, PosY + Eye, PosZ);
            _positionAlphaChunk = new Vector3i(PositionAlphaBlock.X >> 4, 
                PositionAlphaBlock.Y >> 4, PositionAlphaBlock.Z >> 4);

            if (!_positionAlphaChunk.Equals(_positionAlphaChunkPrev))
            {
                // Если смещение чанком
                _positionAlphaChunkPrev = _positionAlphaChunk;
                _positionAlphaBlockPrev = PositionAlphaBlock;

                int chX = ChunkPositionX;
                int chY = PositionY >> 4;
                int chZ = ChunkPositionZ;
                Vector3i pos;
                ChunkRender chunk = null;
                int count = Ce.OverviewAlphaSphere.Length;
                int x, z, xOld, zOld;
                xOld = zOld = 0;
                bool body = false;

                for (int i = 0; i < count; i++)
                {
                    pos = Ce.OverviewAlphaSphere[i];
                    x = pos.X + chX;
                    z = pos.Y + chZ;

                    if (body && x == xOld && z == zOld)
                    {
                        chunk.ModifiedToRenderAlpha(pos.Y + chY);
                    }
                    chunk = _game.World.ChunkPrClient.GetChunkRender(x, z);
                    if (chunk != null)
                    {
                        chunk.ModifiedToRenderAlpha(pos.Y + chY);
                        xOld = x;
                        zOld = z;
                        body = true;
                    }
                    else
                    {
                        body = false;
                    }
                }
            }
            else if (!PositionAlphaBlock.Equals(_positionAlphaBlockPrev))
            {
                // Если смещение блока
                _positionAlphaBlockPrev = PositionAlphaBlock;
                _game.World.ChunkPrClient.ModifiedToRenderAlpha(
                    ChunkPositionX,
                    PositionY >> 4,
                    ChunkPositionZ);
            }
        }

        /// <summary>
        /// Обновляем курсор
        /// </summary>
        private void _UpCursor()
        {
            if (IsSpectator())
            {
                // Если наблюдатель, то не может выбирать объекты
                if (MovingObject.IsBlock())
                {
                    // Если был объект убираем его
                    MovingObject = new MovingObjectPosition();
                }
                Debug.BlockFocus = "";
            }
            else
            {

                MovingObject = _RayCast();

                if (MovingObject.IsBlock())
                {
                    ChunkBase chunk = _game.World.GetChunk(MovingObject.BlockPosition.GetPositionChunk());
                    Vector3i pos = MovingObject.BlockPosition.GetPositionInChunk();
                    string s1 = Debug.ToBlockInfo(chunk, pos);
                    string strUp = "";
                    if (MovingObject.BlockPosition.Y < chunk.Settings.NumberBlocks)
                    {
                        BlockPos blockPosUp = MovingObject.BlockPosition.OffsetUp();
                        strUp = string.Format(
                            "BlkUp:{0} {1} L:{2}",
                            blockPosUp,
                            _game.World.GetBlockState(blockPosUp).ToInfo(),
                            Debug.ToBlockInfo(chunk, blockPosUp.GetPositionInChunk())
                        );
                    }
                    Debug.BlockFocus = string.Format(
                        "Block:{0} {1}{4} L:{2}\r\n{3}\r\n",//{5}, {6}\r\n",
                        MovingObject.BlockPosition,
                        MovingObject.Block.ToInfo(),
                        s1,
                        strUp,
                        MovingObject.IsLiquid ? string.Format(" {0} {1}", Ce.Blocks.BlockAlias[MovingObject.IdBlockLiquid], MovingObject.BlockLiquidPosition) : ""
                    //chunk.Light.GetHeight(pos.X, pos.z),
                    //chunk.GetDebugAllSegment(),

                    );
                }
                else if (MovingObject.IsLiquid)
                {
                    Debug.BlockFocus = string.Format("Liquid:{0} {1}\r\n", Ce.Blocks.BlockAlias[MovingObject.IdBlockLiquid], MovingObject.BlockLiquidPosition);
                }
                //else if (MovingObject.IsEntity())
                //{
                //    Debug.BlockFocus = MovingObject.Entity.GetName();
                //    if (MovingObject.Entity is EntityLiving entityLiving)
                //    {
                //        Debug.BlockFocus += " [" + entityLiving.GetHealth() + "]";
                //    }
                //    Debug.BlockFocus += "\r\n";
                //}
                else
                {
                    Debug.BlockFocus = "";
                }
            }
        }

        #endregion

        #region Packet

        /// <summary>
        /// Пакет Возраждение в мире
        /// </summary>
        public void PacketRespawnInWorld(PacketS07RespawnInWorld packet)
        {
            IdWorld = packet.IdWorld;
            _game.World.ChunkPr.Settings.SetHeightChunks(packet.NumberChunkSections);
            _game.World.Settings.PacketRespawnInWorld(packet);
            _game.World.Collision.Init();
            _game.WorldRender.RespawnInWorld();
            _heightChinkFrustumCulling = _game.World.ChunkPrClient.Settings.NumberBlocks;
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
                _game.TrancivePacket(new PacketC20AcknowledgeChunks(_batchChunksTime, _batchChunksQuantity, true));
            }
        }

        /// <summary>
        /// Пакет получения сообщения с сервера
        /// </summary>
        public void PacketMessage(PacketS3AMessage packet)
        {
            Chat.AddMessage(packet.Message, Gi.WindowsChatWidthMessage,  Gi.Si);
        }
            
        #endregion

        /// <summary>
        /// Луч игроком
        /// </summary>
        /// <param name="collidable"></param>
        /// <param name="isLiquid"></param>
        /// <returns></returns>
        private MovingObjectPosition _RayCast(bool collidable = false, bool isLiquid = false)
        {
            // максимальная дистанция луча

            Vector3 pos = GetPositionVec();
            pos.Y += Eye;// GetEyeHeight();
            return _game.World.RayCastBlock(pos, _rayLook, 16, collidable, /*Id,*/ isLiquid);
            // return World.RayCast(pos, RayLook, MvkGlobal.RAY_CAST_DISTANCE, collidable, Id, isLiquid);
        }

        /// <summary>
        /// Задать обзор чанков у клиента
        /// </summary>
        public void SetOverviewChunk(byte overviewChunk, bool isSaveOptions)
        {
            if (OverviewChunk != overviewChunk)
            {
                SetOverviewChunk(overviewChunk);
                Ce.OverviewCircles = Sundry.GenOverviewCircles(overviewChunk);
                Ce.OverviewAlphaSphere = Sundry.GenOverviewSphere(overviewChunk < Gi.UpdateAlphaChunk
                    ? overviewChunk : Gi.UpdateAlphaChunk);
                // Корректируем FrustumCulling
                _InitFrustumCulling();
                // Меняем в рендере мира
                _game.WorldRender.ModifyOverviewChunk();
                // Отправим обзор 
                _game.TrancivePacket(new PacketC15PlayerSetting(OverviewChunk));
                if (isSaveOptions)
                {
                    Options.OverviewChunk = OverviewChunk;
                    _game.OptionsSave();
                }
            }
        }

        /// <summary>
        /// Получить время в милисекундах
        /// </summary>
        protected override long _Time() => _game.Time();

        public override string ToString()
        {
            float k = 10f; // 20 tps * .5f ширина блока
            k = Ce.Tps * .5f;
            string motion = string.Format("{0:0.00} | {1:0.00} м/с {2} {3}", 
                Physics.MotionHorizon * k,
                Physics.MotionVertical * k,
                new Vector3(Physics.MotionX, Physics.MotionY, Physics.MotionZ),
                Physics.ToDebugString()
                );

            return Login + " " + ToStringPositionRotation() + " O:" + OverviewChunk
                + (OnGround ? " OnGround" : "")
                + " batch:" + _batchChunksQuantity + "|" + _batchChunksTime + "mc "
                + Movement + "\r\n" + motion;
        }

        /// <summary>
        /// Событие любого объекта с сервера для отладки
        /// </summary>
        public event StringEventHandler TagDebug;
        public void OnTagDebug(string title, object tag)
            => TagDebug?.Invoke(this, new StringEventArgs(title, tag));
    }
}
