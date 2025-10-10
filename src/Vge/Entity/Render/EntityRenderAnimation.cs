﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Vge.Entity.Animation;
using Vge.Entity.Layer;
using Vge.Entity.Player;
using Vge.Entity.Shape;
using Vge.Item;
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
        /// Название клипа для того чтоб держать предмет в правой руке, 
        /// по умолчанию если не выбран параметр Hold в предмете.
        /// </summary>
        public const string HoldRight = "HoldRight";
        /// <summary>
        /// Название клипа для того чтоб держать предмет в левой руке
        /// </summary>
        public const string HoldLeft = "HoldLeft";
        /// <summary>
        /// Префикс названий клипов которые держат предмет, для массового удаления при смене предмета
        /// </summary>
        private const string _prefixHold = "Hold";
        /// <summary>
        /// Количество символов в префиксе
        /// </summary>
        private static int _prefixHoldLeght = _prefixHold.Length;

        /// <summary>
        /// Буфер для шейдера матриц скелетной анимации, на 24 кости (матрица 4*3 и 24 кости)
        /// struct AnimationMatrixPalette - https://habr.com/ru/articles/501212/
        /// </summary>
        private float[] _bufferBonesTransforms = new float[12 * Ce.MaxAnimatedBones];

        /// <summary>
        /// Массив отдельных анимационных клипов
        /// </summary>
        private readonly ListMessy<string> _animationClips = new ListMessy<string>();
        /// <summary>
        /// Карта всех клипов данной сущности
        /// </summary>
        private readonly Dictionary<string, AnimationClip> _animations = new Dictionary<string, AnimationClip>();
        /// <summary>
        /// Карта всех клипов данной сущности
        /// </summary>
        private readonly Dictionary<EnumEntityActivity, string> _animationsActivity = new Dictionary<EnumEntityActivity, string>();
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
        /// <summary>
        /// Анимационный триггер для сущности
        /// </summary>
        private readonly TriggerAnimation _trigger = new TriggerAnimation();

        /// <summary>
        /// Имя текущего клипа, чтоб можно было закрыть
        /// </summary>
        private string _activeClip;
        /// <summary>
        /// Текущий триггерный активотора
        /// </summary>
        private EnumEntityActivity _activity;
        /// <summary>
        /// Массив позиций предметов
        /// </summary>
        private readonly PositionItemBone[] _positionItems;
        /// <summary>
        /// Объект слоёв одежды
        /// </summary>
        private readonly ShapeLayers _shapeLayers;

        public EntityRenderAnimation(EntityBase entity, EntitiesRenderer entities, ResourcesEntity resourcesEntity) 
            : base(entity, entities, resourcesEntity)
        {
            if (resourcesEntity.NameShapeLayers != "")
            {
                // Имеется одежда для этой сущности
                try
                {
                    _shapeLayers = EntitiesReg.GetShapeLayers(resourcesEntity.NameShapeLayers);
                }
                catch
                {
                    // Отсутствует объект слоёв одежды у сущности
                    throw new Exception(Sr.GetString(Sr.EntityClothingLayersObjectIsMissing, 
                        resourcesEntity.NameShapeLayers, resourcesEntity.Alias));
                }
            }
            _countBones = (byte)_resourcesEntity.Bones.Length;
            _bones = new BonePose[_countBones];
            _bonesFlagModify = new bool[_countBones];
            _bonesTransforms = new Mat4[_countBones];

            // Создаём количество возможных ячеек для держания предмета, не одежды
            _positionItems = new PositionItemBone[_resourcesEntity.CountPositionItem];
            Bone bone;
            for (byte i = 0; i < _countBones; i++)
            {
                bone = _resourcesEntity.Bones[i];
                if (bone.NumberHold > 0)
                {
                    // Кость которая для держания предмета
                    _positionItems[bone.NumberHold - 1] = new PositionItemBone(i, bone.OriginX, bone.OriginY, bone.OriginZ);
                }
                _bones[i] = bone.CreateBonePose();
                _bonesTransforms[i] = Mat4.Identity();
            }

            foreach(ModelAnimationClip clip in _resourcesEntity.ModelAnimationClips)
            {
                AnimationClip animationClip = new AnimationClip(_resourcesEntity, clip);
                _animations.Add(clip.Code, animationClip);
                if (clip.Activity != EnumEntityActivity.None)
                {
                    _animationsActivity.Add(clip.Activity, clip.Code);
                }
            }

            // Запускаем анимацию бездействия
            AddClipActivity(EnumEntityActivity.Idle);
        }

        /// <summary>
        /// Добавить клип по триггерному активатору
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddClipActivity(EnumEntityActivity activity, float speed = 1)
        {
            _activity = activity;
            _activeClip = _animationsActivity.ContainsKey(activity)
                ? _animationsActivity[activity] : Ce.MandatoryAnimationClipIdle;

            AddClip(_activeClip, speed);
        }

        /// <summary>
        /// Добавить клип
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void AddClip(string key, float speed = 1)
        {
            if (_animationClips.Contains(key))
            {
                _animations[key].ResetStop();
            }
            else// if (_animations.ContainsKey(key)) // TODO::2025-07-30 надо продумать, чтоб тут всегда было
            {
                _animationClips.Add(key);
                _animations[key].Reset(speed);
            }
        }

        /// <summary>
        /// Остановить клип
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void StopingClip(string key) => _animations[key].Stoping();

        /// <summary>
        /// Остоновить все клипы которые держим
        /// </summary>
        private void _StopingClipsHold()
        {
            string s;
            for (int i = 0; i < _animationClips.Count; i++)
            {
                s = _animationClips[i];
                if (s.Length >= _prefixHoldLeght && s.Substring(0, 4) == _prefixHold)
                {
                    // Этот надо остановить
                    _animations[s].Stoping();
                }
            }
        }

        /// <summary>
        /// Игровой такт на клиенте
        /// </summary>
        /// <param name="deltaTime">Дельта последнего тика в mc</param>
        public override void UpdateClient(WorldClient world, float deltaTime)
        {
            base.UpdateClient(world, deltaTime);

            if (_trigger.Changed)
            {
                SetEyeOpen(!_trigger.IsSneak());

                if (!Entity.OnGround)
                {
                    SetMouthState(EnumMouthState.Open);
                }
                else
                {
                    SetMouthState(_trigger.IsSneak() ? EnumMouthState.Speaks :
                        _trigger.IsSprinting() ? EnumMouthState.Smile : EnumMouthState.Close);
                }
            }

            // Анимация по движению
            _AnimationByMotion();

            // Временно клик
            if (Entity is PlayerClientOwner playerClient && playerClient.TestHandAction)
            {
                playerClient.TestHandAction = false;
                AddClip("AttackRight", 2f);
            }

            int i = 0;
            int count = _animationClips.Count - 1;
            for (i = count; i >= 0; i--)
            {
                if (_animations[_animationClips[i]].IsStoped())
                {
                    _animationClips.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Анимация по движению
        /// </summary>
        private void _AnimationByMotion()
        {
            // Сразу определяем смену доп параметров сущности
            if (!_trigger.Levitate)
            {
                if (!Entity.OnGround) _trigger.SetLevitate(true);
            }
            else
            {
                if (Entity.OnGround)
                {
                    _trigger.SetLevitate(false);
                }
            }

            // Если было изменение
            if (_trigger.Changed)
            {
                // Проверяем текущее состояние
                EnumEntityActivity clip;
                if (_trigger.IsMove())
                {
                    // Что-то имеется, проверяем сразу на движение
                    if (_resourcesEntity.OnlyMove)
                    {
                        // Только движение
                        clip = EnumEntityActivity.Move;
                    }
                    else
                    {
                        if (_trigger.IsMovingStrafe())
                        {
                            // Только стрейф
                            clip = _trigger.IsStrafeLeft() ? EnumEntityActivity.Left : EnumEntityActivity.Right;
                        }
                        else
                        {
                            // Вперёд и назад
                            clip = _trigger.IsForward() ? EnumEntityActivity.Forward : EnumEntityActivity.Back;
                        }
                    }
                }
                else
                {
                    clip = EnumEntityActivity.Idle;
                }

                if (_trigger.IsSprinting()) clip |= EnumEntityActivity.Sprint;
                if (_trigger.Levitate) clip |= EnumEntityActivity.Levitate;
                else if (_trigger.IsSneak())
                {
                    // У ливитации положения сидя не должно быть
                    clip |= EnumEntityActivity.Sneak;
                }

                // Если клип поменялся
                if (clip != _activity)
                {
                    StopingClip(_activeClip);
                    AddClipActivity(clip);
                }
                // Обновить данные в конце после отправки данных игрового такта
                _trigger.Done();
            }
        }

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

            // Заносим в шейдор
            Entities.ShsEntity.UniformData(
                Entity.GetPosFrameX(timeIndex) - ppfx,
                Entity.GetPosFrameY(timeIndex) - ppfy,
                Entity.GetPosFrameZ(timeIndex) - ppfz,
                _lightBlock, _lightSky,
                _resourcesEntity.GetDepthTextureAndSmall(),
                _resourcesEntity.GetIsAnimation(), GetEyeMouth()
                );

            Entities.ShsEntity.UniformData(_bufferBonesTransforms);
            // Рисуем основную сетку сущности
            _entityRender.MeshDraw();

            // Если имеются слои, рисуем сетку слоёв
            _entityLayerRender?.MeshDraw();
        }

        /// <summary>
        /// Метод для прорисовки для Gui
        /// </summary>
        public void DrawGui(float posX, float posY, float yaw, float pitch, float scale)
        {
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
            _GetMatrixPalette(yaw, yaw, pitch);

            Entities.ShsEntity.BindUniformAnimationGui(posX, posY, scale,
                _resourcesEntity.GetDepthTextureAndSmall(),
                _resourcesEntity.GetIsAnimation(), GetEyeMouth());

            Entities.ShsEntity.UniformData(_bufferBonesTransforms);
            // Рисуем основную сетку сущности
            _entityRender.MeshDraw();

            // Если имеются слои, рисуем сетку слоёв
            _entityLayerRender?.MeshDraw();
        }

        /// <summary>
        /// Получить параметр для шейдора, на состояния глаз и рта
        /// Значение 1 это открыты глаза, закрыт рот.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual int GetEyeMouth() => 1;

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

        /// <summary>
        /// Изменён предмет внешности (что в руках и одежда)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void OutsideItemChanged()
        {
            if (_entityLayerRender != null && _shapeLayers != null && Entity is EntityLiving entityLiving)
            {
                // Корректировка одевания слоёв
                _StopingClipsHold();
                for (int i = 0; i < entityLiving.Inventory.OutsideCount; i++)
                {
                    ItemStack itemStack = entityLiving.Inventory.GetOutside(i);
                    if (itemStack != null)
                    {
                        if (i < _resourcesEntity.CountPositionItem)
                        {
                            // Предмет в руках
                            _entityLayerRender.AddRangeBuffer(itemStack.Item.Buffer.GetBufferHold()
                                .CreateBufferMeshItem(_positionItems[i].Index, _positionItems[i].X, _positionItems[i].Y, _positionItems[i].Z));

                            if (i == 0)
                            {
                                AddClip(itemStack.Item.Hold != "" ? itemStack.Item.Hold : HoldRight);
                            }
                            else
                            {
                                // Левая рука, только держим предмет
                                AddClip(HoldLeft);
                            }
                        }
                        else if (itemStack.Item is ItemCloth itemCloth)
                        {
                            // Одежда
                            LayerBuffer layer = _shapeLayers.GetLayer(itemCloth.PutOnBody, 
                                itemCloth.GetNameLayer(entityLiving.Inventory.GetOutsideSlotKey(i)));
                            _entityLayerRender.AddRangeBuffer(layer.BufferMesh.CopyBufferMesh(_resourcesEntity.Scale));
                        }
                    }
                }
                _entityLayerRender.Reload();
            }
        }

        /// <summary>
        /// Задать байт флагов анимации движения
        /// FBLRSnSp
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void SetMovingFlags(byte moving) => _trigger.SetMovingFlags(moving);

        /// <summary>
        /// Имеется ли сейчас движение только стрейф, без движения вперёд или назад или бездействия
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool IsMovingStrafe() => _trigger.IsMovingStrafe();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Dispose() => _entityLayerRender?.Dispose();
    }
}
