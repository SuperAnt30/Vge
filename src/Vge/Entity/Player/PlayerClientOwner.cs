using System;
using System.Runtime.CompilerServices;
using Vge.Entity.Physics;
using Vge.Entity.Render;
using Vge.Event;
using Vge.Games;
using Vge.Network.Packets.Client;
using Vge.Network.Packets.Server;
using Vge.Realms;
using Vge.Renderer.World;
using Vge.Util;
using Vge.World;
using Vge.World.Block;
using Vge.World.Chunk;
using WinGL.Util;

namespace Vge.Entity.Player
{
    /// <summary>
    /// Объект игрока владельца, на клиенте
    /// </summary>
    public class PlayerClientOwner : PlayerClient
    {
        /// <summary>
        /// Для Pitch предел Пи 1.55, аналог 89гр
        /// </summary>
        public const float Pi89 = 1.55334303f;

        /// <summary>
        /// Объект кэш чата
        /// </summary>
        public readonly ChatList Chat;

        /// <summary>
        /// Массив всех видимых чанков 
        /// </summary>
        public readonly ListFast<ChunkRender> FrustumCulling = new ListFast<ChunkRender>();

        /// <summary>
        /// Смещения вращение по оси Y в радианах, мышью до такта
        /// </summary>
        public float RotationYawBiasInput;
        /// <summary>
        /// Смещения вращение вверх вниз в радианах, мышью до такта
        /// </summary>
        public float RotationPitchBiasInput;

        /// <summary>
        /// Выбранный объект
        /// </summary>
        public readonly MovingObjectPosition MovingObject = new MovingObjectPosition();

        /// <summary>
        /// Плавное перемещение угла обзора
        /// </summary>
        public readonly SmoothFrame Fov;
        /// <summary>
        /// Плавное перемещение глаз, сел/встал
        /// </summary>
        public readonly SmoothFrame Eye;
        
        /// <summary>
        /// Позиция камеры в блоке для альфа, в зависимости от вида (с глаз, с зади, спереди)
        /// </summary>
        public Vector3i PositionAlphaBlock { get; private set; }

        /// <summary>
        /// Вид камеры
        /// </summary>
        public EnumViewCamera ViewCamera { get; private set; } = EnumViewCamera.Eye;

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
        /// <summary>
        /// Высота глаз с учётом итерполяции кадра
        /// </summary>
        private float _eyeFrame;

        /// <summary>
        /// Выбранная ячейка
        /// </summary>
        private byte _currentPlayerItem = 0;
        /// <summary>
        /// Обновить матрицы камеры
        /// </summary>
        private bool _updateCamera = true;
        /// <summary>
        /// Зажат ли контрол активной рукки
        /// </summary>
        private bool _controlHandAction;
        /// <summary>
        /// Зажат ли контрол использование предмета
        /// </summary>
        private bool _controlItemUse;

        public PlayerClientOwner(GameBase game) : base(game) // IndexEntity ещё не определён
        {
            Login = game.ToLoginPlayer();
            Token = game.ToTokenPlayer();
            Chat = new ChatList(Ce.ChatLineTimeLife, _game.Render.FontMain);
            Fov = new SmoothFrame(1.43f);
            _eyeFrame = SizeLiving.GetEye();
            Eye = new SmoothFrame(_eyeFrame);
            
            _UpdateMatrixCamera();
        }

        /// <summary>
        /// Запуск мира
        /// </summary>
        public void WorldStarting()
        {
            // Нельзя в конструкторе, так-как ещё сервер не подал таблицы id типов сущностей
            _InitIndexPlayer();

            // Нельзя в конструкторе, так-как мир ещё не создан
            //Physics = new PhysicsFly(_game.World.Collision, this);

            Physics = new PhysicsPlayer(_game.World.Collision, this);

            // Создание объекта рендера не в конструкторе, так-как там ещё не создан рендер мир
            InitRender(IndexEntity, _game.WorldRender.Entities);
        }

        #region Методы от Entity

        /// <summary>
        /// Высота глаз
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override float GetEyeHeight() => _eyeFrame;

        #endregion

        #region Inputs (Mouse Key)

        /// <summary>
        /// Нажат или отпущен контрол
        /// </summary>
        /// <param name="index">Номер контрола</param>
        /// <param name="down">true - нажат, false - отпущен</param>
        public void Control(int index, bool down)
        {
            if (Options.ControlForward == index)
            {
                Movement.SetForward(down);
                Physics.AwakenPhysics();
            }
            else if (Options.ControlBack == index)
            {
                Movement.SetBack(down);
                Physics.AwakenPhysics();
            }
            else if (Options.ControlStrafeLeft == index)
            {
                Movement.SetStrafeLeft(down);
                Physics.AwakenPhysics();
            }
            else if (Options.ControlStrafeRight == index)
            {
                Movement.SetStrafeRight(down);
                Physics.AwakenPhysics();
            }
            else if (Options.ControlJump == index)
            {
                Movement.SetJump(down);
                Physics.AwakenPhysics();
            }
            else if (Options.ControlSneak == index)
            {
                Movement.SetSneak(down);
                Physics.AwakenPhysics();
            }
            else if (Options.ControlSprinting == index)
            {
                Movement.SetSprinting(down);
                Physics.AwakenPhysics();
            }
            else if (Options.ControlHandAction == index)
            {
                if (down) _HandAction();
                else _UndoHandAction();
            }
            else if (Options.ControlItemUse == index)
            {
                if (down) _ItemUse();
                else _StoppedUsingItem();
            }
        }
        
        /// <summary>
        /// Двойной клик пробела
        /// </summary>
        public void KeyDoubleClickSpace()
        {
            if (!NoClip && AllowFlying)
            {
                if (Physics is PhysicsPlayer)
                {
                    Physics = new PhysicsFly(_game.World.Collision, this);
                }
                else
                {
                    Physics = new PhysicsPlayer(_game.World.Collision, this);
                }
            }
        }

        /// <summary>
        /// Изменение мыши
        /// </summary>
        /// <param name="centerBiasX">растояние по X от центра</param>
        /// <param name="centerBiasY">растояние по Y от центра</param>
        public void MouseMove(int centerBiasX, int centerBiasY)
        {
            if (centerBiasX != 0 || centerBiasY != 0)
            {
                // Чувствительность мыши
                float speedMouse = Options.MouseSensitivityFloat;
                // Определяем углы смещения
                RotationPitchBiasInput -= centerBiasY / (float)Gi.Height * speedMouse;
                RotationYawBiasInput += centerBiasX / (float)Gi.Width * speedMouse;
                // Пробуждаем физику
                Physics.AwakenPhysics();
            }
        }

        /// <summary>
        /// Остановить все инпуты
        /// </summary>
        public void ActionStop()
        {
            Movement.SetStop();
            if (_controlHandAction)
            {
                _UndoHandAction();
            }
            if (_controlItemUse)
            {
                _StoppedUsingItem();
            }

            Physics.AwakenPhysics();
            if (!_game.IsRunNet())
            {
                PosFrameX = PosPrevX = PosX;
                PosFrameY = PosPrevY = PosY;
                PosFrameZ = PosPrevZ = PosZ;
                RotationFrameYaw = RotationPrevYaw = RotationYaw;
                RotationFramePitch = RotationPrevPitch = RotationPitch;
                RotationYawBiasInput = RotationPitchBiasInput = 0;
            }

            // Задать анимацию
            Render.SetMovingFlags(Movement.Flags);
        }

        #endregion

        #region Действие рук, левый или правый клик

        // TODO::2025-08-07 Временый тест, убрать...
        public bool TestHandAction;

        /// <summary>
        /// Основное действие правой рукой. Левый клик мыши
        /// (old HandAction)
        /// </summary>
        private void _HandAction()
        {
            if (!IsSpectator() && true)
            {
                _controlHandAction = true;
                if (MovingObject.IsBlock())
                {
                    TestHandAction = true;

                    _game.TrancivePacket(new PacketC07PlayerDigging(MovingObject.BlockPosition, PacketC07PlayerDigging.EnumDigging.Destroy));
                }
                else
                {
                    // Типа броска
                    _game.TrancivePacket(new PacketC07PlayerDigging(new BlockPos(), PacketC07PlayerDigging.EnumDigging.About));
                }
            }
        }

        /// <summary>
        /// Отмена действия правой рукой
        /// (old UndoHandAction)
        /// </summary>
        private void _UndoHandAction()
        {
            _controlHandAction = false;
            // _game.TrancivePacket(new PacketC07PlayerDigging(MovingObject.BlockPosition, PacketC07PlayerDigging.EnumDigging.About));
        }

        /// <summary>
        /// Использовать предмет (ставим блок, кушаем). Правый клик мыши
        /// (old HandActionRight)
        /// </summary>
        private void _ItemUse()
        {
            _controlItemUse = true;
            TestHandAction = true;
            if (MovingObject.IsEntity())
            {
                _game.TrancivePacket(new PacketC03UseEntity(MovingObject.Entity.Id,
                    PacketC03UseEntity.EnumAction.Interact));
            }
            else if (MovingObject.IsBlock())
            {
                _game.TrancivePacket(new PacketC08PlayerBlockPlacement(MovingObject.BlockPosition,
                    MovingObject.Side, MovingObject.Facing));
            }
        }

        /// <summary>
        /// Остановить использование предмета
        /// (old OnStoppedUsingItem)
        /// </summary>
        private void _StoppedUsingItem()
        {
            _controlItemUse = false;
        }

        #endregion

        #region FrustumCulling Camera

        /// <summary>
        /// Клик изменения отладочной ортоганальной камеры
        /// </summary>
        public void DebugOrtoNext()
        {
            Gi.DebugOrtoNext();
            ViewCamera = Gi.IsDrawOrto ? EnumViewCamera.Back : EnumViewCamera.Eye;
            _updateCamera = true;
        }

        /// <summary>
        /// Следующий вид камеры
        /// </summary>
        public void ViewCameraNext()
        {
            int count = Enum.GetValues(typeof(EnumViewCamera)).Length - 1;
            int value = (int)ViewCamera;
            value++;
            if (value > count) value = 0;
            ViewCamera = (EnumViewCamera)value;
            _updateCamera = true;
        }

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
            Vector3 pos = new Vector3(0, _eyeFrame, 0);
            _rayLook = front;

            if (ViewCamera == EnumViewCamera.Back)
            {
                // вид сзади
                pos = _GetPositionCamera(pos, front * -1f, 8); // 8
            }
            else if (ViewCamera == EnumViewCamera.Front)
            {
                // вид спереди
                pos = _GetPositionCamera(pos, front, 8); // 8
                front *= -1f;
            }
            //else
            //{
            //    Vector3 front2 = Glm.Ray(RotationFrameYaw, 0);
            //    pos += front2 * .3f;
            //    pos -= up * .1f;
            //}

            //if (!ViewCameraEye)
            //{
            //    pos -= front * 4; // 5
            //    //Vector3 front2 = Glm.Ray(RotationFrameYaw, 0);
            //    //Vector3 right = Glm.Cross(front2, up);
            //    //pos += right * 2;
            //}
            //else
            //{
            //    Vector3 front2 = Glm.Ray(RotationFrameYaw, 0);
            //    pos += front2 * .2f;
            //    pos -= up * .1f;
            //}
            // Матрица Projection
            Mat4 matrix;
            if (Gi.IsDrawOrto)
            {
                int ort = Gi.ZoomDrawOrto * Gi.ZoomDrawOrto;
                int width = Gi.Width / ort;
                int height = Gi.Height / ort;
                matrix = Glm.Ortho(-width, width, -height, height, -500, 500);
            }
            else
            {
                matrix = Glm.PerspectiveFov(Fov.ValueFrame, Gi.Width, Gi.Height,
                   0.01f, OverviewChunk * 22f);
            }
            // Матрица Look
            matrix.Multiply(Glm.LookAt(pos, pos + front, new Vector3(0, 1, 0)));
            matrix.ConvArray(Gi.MatrixView);

            // Матрица солнца, для тени
            //PosLight = new Vector3(32, 50, 0);
            //matrix = Glm.PerspectiveFov(Fov.ValueFrame, 1024, 1024, 0.01f, OverviewChunk * 22f);
            //matrix.Multiply(Glm.LookAt(PosLight, new Vector3(0, 0, 0), new Vector3(-1, 0, 0)));

            
        }

        /// <summary>
        /// Определить положение камеры, при виде сзади и спереди, проверка RayCast
        /// </summary>
        /// <param name="pos">позиция глаз</param>
        /// <param name="vec">направляющий вектор к расположению камеры</param>
        private Vector3 _GetPositionCamera(Vector3 pos, Vector3 vec, float dis)
        {
            return pos + vec * dis;
            //Vector3 offset = ClientWorld.RenderEntityManager.CameraOffset;
            //if (IsSpectator())
            //{
            //    return pos + vec * dis;
            //}
            //MovingObjectPosition moving = World.RayCastBlock(pos + offset, vec, dis, true);
            //return pos + vec * (moving.IsBlock() ? glm.distance(pos, moving.RayHit + new vec3(moving.Norm) * .5f - offset) : dis);
        }

        /// <summary>
        /// Перерасчёт FrustumCulling
        /// </summary>
        private void _InitFrustumCulling()
        {
            _frustumCulling.Init(Gi.MatrixView);

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
                y1 = -Mth.Floor(PosY);
                z1 = zb - 15;
                x2 = xb + 15;
                y2 = _heightChinkFrustumCulling + y1;
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
        /// Возвращает true, если прямоугольник находится внутри всех 6 плоскостей отсечения,
        /// в противном случае возвращает false.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsBoxInFrustum(AxisAlignedBB aabb) => _frustumCulling.IsBoxInFrustum(aabb);

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
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        public void UpdateFrame(float timeIndex)
        {
            // Меняем положения глаз, смена позы
            if (Eye.UpdateFrame(timeIndex))
            {
                _eyeFrame = Eye.ValueFrame;
                _updateCamera = true;
            }
            // Меняем угол обзора, как правило при изменении скорости
            if (Fov.UpdateFrame(timeIndex))
            {
                _updateCamera = true;
            }

            if (PosX != PosFrameX || PosY != PosFrameY || PosZ != PosFrameZ
                    || RotationYaw != RotationFrameYaw || RotationPitch != RotationFramePitch)
            {
                // Если было перемещение и или вращение
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
                _updateCamera = true;
            }

            if (_updateCamera)
            {
                // Обновить камеру
                _CameraHasBeenChanged();
                _updateCamera = false;
            }
        }

        /// <summary>
        /// Игровой такт на клиенте
        /// </summary>
        /// <param name="deltaTime">Дельта последнего тика в mc</param>
        public override void UpdateClient(WorldClient world, float deltaTime)
        {
            // Такты изминения глаз при присидании, и угол обзора при ускорении.
            // Должны быть до base.Update()
            Eye.Update();
            Fov.Update();

            if (IsPositionChange())
            {
                PosPrevX = PosX;
                PosPrevY = PosY;
                PosPrevZ = PosZ;
            }
            
            if (!Physics.IsPhysicSleep())
            {
                // Расчитать перемещение в объекте физика
                Physics.LivingUpdate();

                if (Physics.IsPoseChange)
                {
                    Eye.Set(SizeLiving.GetEye(), 6);
                    Fov.Set(IsSprinting() ? 1.62f : 1.43f, 6); // TODO::2025-06-23 в конфиг обзора!
                }

                // Для отправки
                if (IsRotationChange() || RotationYawBiasInput != 0 || RotationPitchBiasInput != 0)
                {
                    RotationPrevYaw = RotationYaw;
                    RotationPrevPitch = RotationPitch;

                    RotationPitch += RotationPitchBiasInput;
                    if (RotationPitch < -Pi89) RotationPitch = -Pi89;
                    else if (RotationPitch > Pi89) RotationPitch = Pi89;

                    RotationYaw += RotationYawBiasInput;
                    if (RotationYaw > Glm.Pi) RotationYaw -= Glm.Pi360;
                    else if (RotationYaw < -Glm.Pi) RotationYaw += Glm.Pi360;

                    // Обнуляем смещение вращения
                    RotationYawBiasInput = RotationPitchBiasInput = 0;

                    if (Physics.IsMotionChange)
                    {
                        // И перемещение и вращение
                        _game.TrancivePacket(new PacketC04PlayerPosition(PosX, PosY, PosZ, 
                            RotationYaw, RotationPitch, IsSneaking(), IsSprinting(), OnGround));
                    }
                    else
                    {
                        // Только вращение
                        _game.TrancivePacket(new PacketC04PlayerPosition(RotationYaw, RotationPitch, 
                            IsSneaking(), IsSprinting(), OnGround));
                    }
                    Physics.ResetPose();
                }
                else if (Physics.IsMotionChange)
                {
                    // Только перемещение
                    _game.TrancivePacket(new PacketC04PlayerPosition(PosX, PosY, PosZ, 
                        IsSneaking(), IsSprinting(), OnGround));
                    Physics.ResetPose();
                }
                else if (Physics.IsPoseChange)
                {
                    // Смена позы
                    _game.TrancivePacket(new PacketC04PlayerPosition(IsSneaking(), IsSprinting(), OnGround));
                    Physics.ResetPose();
                }
            }

            // Подготовка анимационных триггеров
            if (Movement.Changed)
            {
                // Не может встать
                bool cantGetUp = !Movement.Sneak && IsSneaking();

                if (cantGetUp) Movement.SetSneak(true);

                // Отправить анимацию
                _game.TrancivePacket(new PacketC0APlayerAnimation(Movement.Flags));
                // Задать анимацию
                Render.SetMovingFlags(Movement.Flags);

                // Console.WriteLine("Flags: " + Movement.Flags);

                if (cantGetUp) Movement.SetSneak(false);

                // Зачистить
                Movement.UpdateAfter();
            }

            // Поворот тела от поворота головы или движения
            _RotationBody();

            if (_countUnusedFrustumCulling > 0
                && ++_countTickLastFrustumCulling > Ce.CheckTickInitFrustumCulling)
            {
                _InitFrustumCulling();
            }

            // Обновление курсора, не зависимо от действия игрока, так-как рядом может быть изминение
            _UpCursor();

            // Проверка на обновление чанков альфа блоков, в такте после перемещения
            _UpdateChunkRenderAlphe();

            // синхронизация выброного слота
            _SyncCurrentPlayItem();

            Render.UpdateClient(world, deltaTime);
        }

        /// <summary>
        /// Проверка на обновление чанков альфа блоков, в такте после перемещения
        /// </summary>
        private void _UpdateChunkRenderAlphe()
        {
            PositionAlphaBlock = new Vector3i(PosX, PosY + _eyeFrame, PosZ);
            _positionAlphaChunk = new Vector3i(PositionAlphaBlock.X >> 4, 
                PositionAlphaBlock.Y >> 4, PositionAlphaBlock.Z >> 4);

            if (!_positionAlphaChunk.Equals(_positionAlphaChunkPrev))
            {
                // Если смещение чанком
                _positionAlphaChunkPrev = _positionAlphaChunk;
                _positionAlphaBlockPrev = PositionAlphaBlock;

                int chX = ChunkPositionX;
                int chY = ChunkPositionY;
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
                    ChunkPositionX, ChunkPositionY, ChunkPositionZ);
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
                if (MovingObject.IsCollision())
                {
                    // Если был объект убираем его
                    MovingObject.Clear();
                }
                Debug.BlockFocus = "";
            }
            else
            {
                _UpRayCast();

                if (MovingObject.IsBlock())
                {
                    ChunkBase chunk = _game.World.GetChunk(MovingObject.BlockPosition.GetPositionChunk());
                    Vector3i pos = MovingObject.BlockPosition.GetPositionInChunk();
                    string s1 = _ToBlockInfo(chunk, pos);
                    string strUp = "";
                    if (MovingObject.BlockPosition.Y < chunk.Settings.NumberBlocks)
                    {
                        BlockPos blockPosUp = MovingObject.BlockPosition.OffsetUp();
                        strUp = string.Format(
                            "\r\nBlkUp:{0} {1} L:{2}",
                            blockPosUp,
                            _game.World.GetBlockState(blockPosUp).ToInfo(),
                            _ToBlockInfo(chunk, blockPosUp.GetPositionInChunk())
                        );
                        // Третий блок сверху
                        //if (MovingObject.BlockPosition.Y + 1 < chunk.Settings.NumberBlocks)
                        //{
                        //    blockPosUp = blockPosUp.OffsetUp();
                        //    strUp += string.Format(
                        //        "\r\nBlkUp2:{0} {1} L:{2}",
                        //        blockPosUp,
                        //        _game.World.GetBlockState(blockPosUp).ToInfo(),
                        //        _ToBlockInfo(chunk, blockPosUp.GetPositionInChunk())
                        //    );
                        //}
                    }
                    Debug.BlockFocus = string.Format(
                        "Block:{5} {0} {1}{4} L:{2}{3}\r\n",
                        MovingObject.BlockPosition,
                        MovingObject.Block.ToInfo(),
                        s1,
                        strUp,
                        MovingObject.IsLiquid ? string.Format(" {0} {1}", Ce.Blocks.BlockAlias[MovingObject.IdBlockLiquid], MovingObject.BlockLiquidPosition) : "",
                        MovingObject.Side.ToString()
                    //chunk.Light.GetHeight(pos.X, pos.z),
                    //chunk.GetDebugAllSegment(),

                    );
                }
                else if (MovingObject.IsLiquid)
                {
                    Debug.BlockFocus = string.Format("Liquid:{0} {1}\r\n", Ce.Blocks.BlockAlias[MovingObject.IdBlockLiquid], MovingObject.BlockLiquidPosition);
                }
                else if (MovingObject.IsEntity())
                {
                    Debug.BlockFocus = "Entity: " + MovingObject.Entity.GetName();
                    if (MovingObject.Entity is EntityLiving entityLiving)
                    {
                        Debug.BlockFocus += " [" + "entityLiving.GetHealth()" + "]";
                    }
                    Debug.BlockFocus += MovingObject.Entity.GetPositionVec().ToString() + "\r\n";
                }
                else
                {
                    Debug.BlockFocus = "";
                }
            }
        }

        /// <summary>
        /// Для определения параметров блока чанк и локальные координаты блока
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual string _ToBlockInfo(ChunkBase chunk, Vector3i pos) => "";

        #endregion

        /// <summary>
        /// Задать выбранной сущности импульс
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void _SetEntityPhysicsImpulse(int id, float x, float y, float z)
            => _game.TrancivePacket(new PacketC03UseEntity(id, x, y, z));

        /// <summary>
        /// Задать выбранной сущности импульс
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void _SetAwakenPhysicSleep(int id)
            => _game.TrancivePacket(new PacketC03UseEntity(id, PacketC03UseEntity.EnumAction.Awaken));

        /// <summary>
        /// Проверить изменение слота если изменён, отправить на сервер
        /// </summary>
        private void _SyncCurrentPlayItem()
        {
            byte currentItem = Inventory.GetCurrentIndex();
            if (currentItem != _currentPlayerItem)
            {
                _currentPlayerItem = currentItem;
                _game.TrancivePacket(new PacketC09HeldItemChange(_currentPlayerItem));
                // Отмена разрушения блока если было смена предмета в руке
               // ClientMain.TrancivePacket(new PacketC07PlayerDigging(itemInWorldManager.BlockPosDestroy, PacketC07PlayerDigging.EnumDigging.About));
               // ItemInWorldManagerDestroyAbout();
            }
        }

        #region Packet

        /// <summary>
        /// Пакет Возраждение в мире
        /// </summary>
        public void PacketRespawnInWorld(PacketS07RespawnInWorld packet)
        {
            IdWorld = packet.IdWorld;
            _game.World.PacketRespawnInWorld(packet);
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
        /// Пакет управления передвежением и изменением слота
        /// </summary>
        public virtual void PacketSetSlot(PacketS2FSetSlot packet) { }

        /// <summary>
        /// Пакет получения сообщения с сервера
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PacketMessage(PacketS3AMessage packet)
        {
            Chat.AddMessage(packet.Message, Gi.WindowsChatWidthMessage,  Gi.Si);
        }

        // <summary>
        /// Задать атрибуты игроку
        /// </summary>
        public void PacketPlayerAbilities(PacketS39PlayerAbilities packet)
        {
            if (packet.Spectator && Physics is PhysicsPlayer)
            {
                // Только полёт
                Physics = new PhysicsFly(_game.World.Collision, this);
            }
            else if (!packet.AllowFlying && Physics is PhysicsFly)
            {
                // Только по земле
                Physics = new PhysicsPlayer(_game.World.Collision, this);
            }

            CreativeMode = packet.CreativeMode;
            NoClip = packet.NoClip;
            /*IsFlying =*/ AllowFlying = packet.AllowFlying;
            DisableDamage = packet.DisableDamage;

            
        }

        #endregion

        /// <summary>
        /// Обновить луч игрока MovingObject
        /// </summary>
        private void _UpRayCast()//bool collidable = false, bool isLiquid = false)
        {
            _game.World.Collision.RayCast(PosX, PosY + _eyeFrame, PosZ, _rayLook, 8, false, Id);
            MovingObject.Copy(_game.World.Collision.MovingObject);

            // максимальная дистанция луча
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override long _Time() => _game.Time();

        public override string ToString()
        {
            if (Physics == null) return "null";
            string motion = string.Format("{0:0.00} | {1:0.00} м/с {2} {3}", 
                Physics.MotionHorizon * Cp.DebugKoef,
                Physics.MotionVertical * Cp.DebugKoef,
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
