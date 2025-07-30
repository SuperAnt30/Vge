using System.Runtime.CompilerServices;
using Vge.Entity.Animation;
using Vge.Entity.Layer;
using Vge.Entity.Shape;
using Vge.Renderer.World.Entity;
using Vge.Util;
using Vge.World;
using WinGL.Util;

namespace Vge.Entity.Render
{
    /// <summary>
    /// Объект рендера сущности, хранящий данные рендера, с АНИМАЦИЕЙ. Только для клиента
    /// </summary>
    public class EntityRenderAnimation : EntityRenderClient
    {
        /// <summary>
        /// Буфер для шейдера матриц скелетной анимации, на 24 кости (матрица 4*3 и 24 кости)
        /// struct AnimationMatrixPalette - https://habr.com/ru/articles/501212/
        /// </summary>
        private float[] _bufferBonesTransforms = new float[12 * Ce.MaxAnimatedBones];

        /// <summary>
        /// Массив отдельных анимационных клипов
        /// </summary>
        private readonly ListMessy<int> _animationClips = new ListMessy<int>();
        /// <summary>
        /// Массив всех клипов данной сущности
        /// </summary>
        private readonly AnimationClip[] _animations;
        /// <summary>
        /// Массив костей в заданный момент времени
        /// </summary>
        private readonly BonePose[] _bones;
        /// <summary>
        /// Массив костей которые менялись
        /// </summary>
        private readonly bool[] _bonesFlagModify;
        /// <summary>
        /// Матрицы трансформации кости
        /// </summary>
        private readonly Mat4[] _bonesTransforms;
        /// <summary>
        /// Количество костей
        /// </summary>
        private readonly byte _countBones;

        public EntityRenderAnimation(EntityBase entity, EntitiesRenderer entities, ResourcesEntity resourcesEntity) 
            : base(entity, entities, resourcesEntity)
        {
            _countBones = (byte)_resourcesEntity.Bones.Length;
            _bones = new BonePose[_countBones];
            _bonesFlagModify = new bool[_countBones];
            _bonesTransforms = new Mat4[_countBones];
            for (int i = 0; i < _countBones; i++)
            {
                _bones[i] = _resourcesEntity.Bones[i].CreateBonePose();
                _bonesTransforms[i] = Mat4.Identity();
            }

            int count = _resourcesEntity.ModelAnimationClips.Length;
            _animations = new AnimationClip[count];
            for (int i = 0; i < count; i++)
            {
                _animations[i] = new AnimationClip(_resourcesEntity, _resourcesEntity.ModelAnimationClips[i]);
            }

            if (count > 0)
            {
                // Если имеется анимация у сущности, временно запускаем первую
                _animationClips.Add(0);
                // TODO::2025-07-09 Определяем анимацию стартовую
            }

            // Временное включение анимациионного клипа
            //_animationClips.Add(new AnimationClip(_resourcesEntity, _resourcesEntity.ModelAnimationClips[2]));
            //_animationClips.Add(new AnimationClip(_resourcesEntity, _resourcesEntity.ModelAnimationClips[1]));
            //_animationClips.Add(new AnimationClip(_resourcesEntity, _resourcesEntity.ModelAnimationClips[0]));
        }

        /// <summary>
        /// Добавить клип
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void AddClip(int index, float speed = 1)
        {
            if (_animationClips.Contains(index))
            {
                _animations[index].ResetStop();
            }
            else
            {
                _animationClips.Add(index);
                _animations[index].Reset(speed);
            }
        }

        /// <summary>
        /// Отменить клип
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void RemoveClip(int index) => _animations[index].Stoping();

      //  private bool m = false;
        /// <summary>
        /// Игровой такт на клиенте
        /// </summary>
        /// <param name="deltaTime">Дельта последнего тика в mc</param>
        public override void UpdateClient(WorldClient world, float deltaTime)
        {
            base.UpdateClient(world, deltaTime);

            // Animation
            byte mov = Trigger.GetMoving();
            // Что убрать
            Trigger.PrepareRemoved();

            if (Trigger.IsForward())
            {
                RemoveClip(1);
                AddClip(0);
            }
            if (Trigger.IsSneak())
            {
                // RemoveClip(2);
            }

            Trigger.SetAnimation(mov);

            // Что добавить
            Trigger.PrepareAddendum();

            if (Trigger.IsForward())
            {
                RemoveClip(0);
                AddClip(1);
            }
            if (Trigger.IsSneak())
            {
                AddClip(2, 2f);
            }

            Trigger.SetAnimation(mov);
            Trigger.UpdatePrev();

                /*
                if (Entity.Physics != null)
                {
                    if (Entity.Physics.IsMotionChange)
                    {
                        // TODO::TEST
                        // Позиция сменена
                        if (!m)
                        {
                            m = true;
                            RemoveClip(0);
                            AddClip(1);

                            if (Entity.Physics.Movement.Sprinting) _animations[1].Speed = 2;
                            else _animations[1].Speed = 1;
                        }
                    }
                    else
                    {
                        // Стоит
                        if (m)
                        {
                            m = false;
                            RemoveClip(1);
                            AddClip(0);
                        }
                    }
                }
                */

                int i = 0;
            int count = _animationClips.Count - 1;
            for (i = count; i >= 0; i--)
            {
                if (_animations[_animationClips[i]].IsStoped())
                {
                    _animationClips.RemoveAt(i);
                }
            }

            // TEST
            timeeye++;
            if (timeeye > 50) timeeye = 0;
            timelips++;
            if (timelips > 8) timelips = 0;
            timelipsSmile++;
            if (timelipsSmile > 150) timelipsSmile = 0;
           // fffd++;
            if (_entityLayerRender != null)
            {
                if (fffd > 30)
                {
                    fffd = 0;
                    ShapeLayers shapeLayers = EntitiesReg.GetShapeLayers("Base");
                    LayerBuffer layer3 = shapeLayers.GetLayer("Trousers", "Trousers1");
                    LayerBuffer layer2 = shapeLayers.GetLayer("BraceletL", "BraceletL2");
                    LayerBuffer layer4 = shapeLayers.GetLayer("BraceletL", "BraceletL1");
                    LayerBuffer layer = shapeLayers.GetLayer("Cap", "Cap1");

                    if (fffb)
                    {
                        _entityLayerRender.AddRangeBuffer(layer3.BufferMesh.CopyBufferMesh(_resourcesEntity.Scale));
                        _entityLayerRender.AddRangeBuffer(layer4.BufferMesh.CopyBufferMesh(_resourcesEntity.Scale));
                    }
                    else
                    {
                        //_entityLayerRender.AddRangeBuffer(layer3.BufferMesh.CopyBufferMesh(_resourcesEntity.Scale));
                        _entityLayerRender.AddRangeBuffer(layer2.BufferMesh.CopyBufferMesh(_resourcesEntity.Scale));
                        //_entityLayerRender.AddRangeBuffer(layer.BufferMesh.CopyBufferMesh(_resourcesEntity.Scale));
                    }
                    _entityLayerRender.Reload();
                    fffb = !fffb;
                }
            }
        }

        int fffd = 0;//100;
        bool fffb = true;

        int timeeye = 0;

        int timelipsSmile = 0;
        int timelips = 0;

        /// <summary>
        /// Обновить рассчитать матрицы для кадра
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        /// <param name="deltaTime">Дельта последнего кадра в mc</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void UpdateMatrix(float timeIndex, float deltaTime)
        {
            base.UpdateMatrix(timeIndex, deltaTime);

            if (_resourcesEntity.IsAnimation)
            {
                // Увеличивает счётчик прошедшего с начала анимации времени
                _IncreaseCurrentTime(deltaTime);

                float yaw;// = 0;
                float yawBody;
                float pitch;// = 0;
                if (Entity is EntityLiving entityLiving)
                {
                    yaw = entityLiving.GetRotationFrameYaw(timeIndex);
                    yawBody = entityLiving.SolidHeadWithBody
                        ? yaw : entityLiving.GetRotationFrameYawBody(timeIndex);
                    pitch = entityLiving.GetRotationFramePitch(timeIndex);
                }
                else
                {
                    float x = Entity.PosX - Entities.Player.PosFrameX;
                    float y = Entity.PosY - Entities.Player.PosFrameY;
                    float z = Entity.PosZ - Entities.Player.PosFrameZ;
                    yawBody = yaw = Glm.Atan2(z, x) - Glm.Pi90;
                    pitch = -Glm.Atan2(y, Mth.Sqrt(x * x + z * z));
                    //pitch = 0;
                }

                // Возвращаем значения костей в исходное положение, Оригинал
                for (byte i = 0; i < _countBones; i++)
                {
                    if (_bonesFlagModify[i])
                    {
                        _resourcesEntity.Bones[i].SetBonePose(ref _bones[i]);
                        _bonesFlagModify[i] = false;
                    }
                }

                // Генерируем кости текущих поз из анимации
                _GenBoneCurrentPoses();
                // Собираем конечные матрицы
                _GetMatrixPalette(yaw, yawBody, pitch);
            }
        }

        /// <summary>
        /// Метод для прорисовки
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        /// <param name="deltaTime">Дельта последнего кадра в mc</param>
        public override void Draw(float timeIndex, float deltaTime)
        {
            float ppfx = Entities.Player.PosFrameX;
            float ppfy = Entities.Player.PosFrameY;
            float ppfz = Entities.Player.PosFrameZ;

            int eye = (timeeye > 5) ? 1 : 0; // глаза
            int lips = 0; // губы
            if (timelipsSmile > 130) lips = 2; // улыбка
            else if (timelipsSmile > 70) lips = (timelips > 4) ? 1 : 0; // балтает
            int eyeLips = lips << 1 | eye;

            // Заносим в шейдор
            Entities.ShsEntity.UniformData(
                Entity.GetPosFrameX(timeIndex) - ppfx,
                Entity.GetPosFrameY(timeIndex) - ppfy,
                Entity.GetPosFrameZ(timeIndex) - ppfz,
                _lightBlock, _lightSky,
                _resourcesEntity.GetDepthTextureAndSmall(),
                _resourcesEntity.GetIsAnimation(), eyeLips
                );

            if (_resourcesEntity.IsAnimation)
            {
                Entities.ShsEntity.UniformData(_bufferBonesTransforms);
            }
            // Рисуем основную сетку сущности
            _entityRender.MeshDraw();

            // Если имеются слои, рисуем сетку слоёв
            _entityLayerRender?.MeshDraw();
        }

        #region Skeletion Matrix

        /// <summary>
        /// Увеличивает счётчик прошедшего с начала анимации времени:
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void _IncreaseCurrentTime(float delta)
        {
            for (int i = 0; i < _animationClips.Count; i++)
            {
                _animations[_animationClips[i]].IncreaseCurrentTime(delta);
            }
        }

        /// <summary>
        /// Генерируем кости текущих поз
        /// </summary>
        private void _GenBoneCurrentPoses()
        {
            int ai;
            int count = _animationClips.Count;
            AnimationClip animationClip;
            // Бежим по костям
            float weightAll;
            for (byte i = 0; i < _countBones; i++)
            {
                if (count > 1)
                {
                    weightAll = 0;
                    // Надо найти сумму веса в текущей кости
                    for (ai = 0; ai < count; ai++)
                    {
                        animationClip = _animations[_animationClips[ai]];
                        if (animationClip.IsAnimation(i))
                        {
                            weightAll += animationClip.GetWeight(i);
                        }
                    }
                    // Смешивание кости boneIndex из клипа animationClip с весом
                    for (ai = 0; ai < count; ai++)
                    {
                        animationClip = _animations[_animationClips[ai]];
                        if (animationClip.IsAnimation(i))
                        {
                            if (weightAll > 0)
                            {
                                _MixBoneClip(animationClip, i, animationClip.GetWeight(i) / weightAll);
                            }
                            else
                            {
                                _MixBoneClip(animationClip, i, 1f);
                            }
                        }
                    }
                }
                else
                {
                    animationClip = _animations[_animationClips[0]];
                    if (animationClip.IsAnimation(i))
                    {
                        _MixBoneClip(animationClip, i, 1f);
                    }
                }
            }
        }

        /// <summary>
        /// Смешивание кости boneIndex из клипа animationClip с весом weight 0..1.0
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void _MixBoneClip(AnimationClip animationClip, byte boneIndex, float weight)
        {
            animationClip.GenBoneCurrentPose();
            _bonesFlagModify[boneIndex] = true;
            // Смешивание загрузки и выгрузки! коэффициент, 0..1  и умножим вес
            float mix = animationClip.GetCoefMix() * weight;

           

            if (mix == 1)
            {
                _bones[boneIndex].Add(animationClip.CurrentPoseBones[boneIndex]);
            }
            else
            {
                _bones[boneIndex].Add(animationClip.CurrentPoseBones[boneIndex], mix);
            }
        }

        /// <summary>
        /// Получение матричной палитры для позы
        /// </summary>
        private void _GetMatrixPalette(float yaw, float yawBody, float pitch)
        {
            // Корневая кость
            _bonesTransforms[0].Clear();
            if (yawBody != 0)
            {
                _bonesTransforms[0].RotateY(-yawBody);
            }
            // У корневой кости нет родительской матрицы, поэтому заносим в матричную палитру матрицу перехода как есть
            _bonesTransforms[0].Multiply(_bones[0].GetBoneMatrix());
            _bonesTransforms[0].Multiply(_resourcesEntity.Bones[0].MatrixInverse);
            _bonesTransforms[0].ConvArray4x3(_bufferBonesTransforms, 0);

            Bone bone;
            // Пользуясь порядком хранения, проходим по всем локальным позам и выполняем умножения на родительские матрицы и обратные
            for (int i = 1; i < _countBones; i++)
            {
                // Перемножаем матрицы в положение как выставленно в Blockbench
                bone = _resourcesEntity.Bones[i];


                _bonesTransforms[bone.ParentIndex].Copy(_bonesTransforms[i]);
                _bonesTransforms[i].Multiply(_bones[i].GetBoneMatrix());

                // Если надо вращаем Pitch, голова
                if (bone.IsHead)
                {
                    yaw -= yawBody;
                    if (yaw != 0)
                    {
                        _bonesTransforms[i].RotateY(-yaw);
                    }
                    if (pitch != 0)
                    {
                        _bonesTransforms[i].RotateX(pitch);
                    }
                }

                // Умножаем обратную матрицу
                _bonesTransforms[i].Multiply(bone.MatrixInverse);
                // Отправляем в кеш
                _bonesTransforms[i].ConvArray4x3(_bufferBonesTransforms, i * 12);
            }
        }

        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Dispose() => _entityLayerRender?.Dispose();
    }
}
