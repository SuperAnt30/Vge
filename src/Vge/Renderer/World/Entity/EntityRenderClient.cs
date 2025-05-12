using Vge.Entity;
using Vge.Entity.Animation;
using Vge.Util;
using WinGL.Util;

namespace Vge.Renderer.World.Entity
{
    /// <summary>
    /// Объект рендера сущности, хранящий данные рендера, только для клиента
    /// </summary>
    public class EntityRenderClient : EntityRenderBase
    {
        /// <summary>
        /// Буфер для шейдера матриц скелетной анимации, на 24 кости (матрица 4*3 и 24 кости)
        /// struct AnimationMatrixPalette - https://habr.com/ru/articles/501212/
        /// </summary>
        private static float[] _bufferBonesTransforms = new float[12 * Ce.MaxAnimatedBones];

        /// <summary>
        /// Объект рендера всех сущностей
        /// </summary>
        public readonly EntitiesRenderer Entities;

        private readonly ModelEntity _modelEntity;
        /// <summary>
        /// Массив отдельных анимационных клипов
        /// </summary>
        private readonly ListMessy<AnimationClip> _animationClips = new ListMessy<AnimationClip>();
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

        public EntityRenderClient(EntityBase entity, EntitiesRenderer entities) : base(entity)
        {
            Entities = entities;
            _modelEntity = Ce.ModelEntities.ModelEntitiesObjects[0];

            _countBones = (byte)_modelEntity.Bones.Length;
            _bones = new BonePose[_countBones];
            _bonesFlagModify = new bool[_countBones];
            _bonesTransforms = new Mat4[_countBones];
            for (int i = 0; i < _countBones; i++)
            {
                _bones[i] = _modelEntity.Bones[i].CreateBonePose();
                _bonesTransforms[i] = Mat4.Identity();
            }

            // Временное включение анимациионного клипа
            _animationClips.Add(new AnimationClip(_modelEntity, _modelEntity.ModelAnimationClips[0]));
            _animationClips.Add(new AnimationClip(_modelEntity, _modelEntity.ModelAnimationClips[1]));
        }

        /// <summary>
        /// Увеличивает счётчик прошедшего с начала анимации времени:
        /// </summary>
        /// <param name="delta"></param>
        public void IncreaseCurrentTime(float delta)
        {
            //foreach(AnimationClip animationClip in _animationClips)
            {
                _animationClips[0].IncreaseCurrentTime(delta);
                _animationClips[1].IncreaseCurrentTime(delta);
            }
        }

        /// <summary>
        /// Метод для прорисовки
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        /// <param name="deltaTime">Дельта последнего кадра в mc</param>
        public override void Draw(float timeIndex, float deltaTime)
        {
            IncreaseCurrentTime(deltaTime);
            EntityRender entityRender = Entities.GetEntityRender();
            Entities.Render.BindTexture(entityRender.Texture);

            float ppfx = entityRender.Player.PosFrameX;
            float ppfy = entityRender.Player.PosFrameY;
            float ppfz = entityRender.Player.PosFrameZ;

            float yaw = 0;
            float pitch = 0;
            if (Entity is EntityLiving entityLiving)
            {
                yaw = entityLiving.GetRotationFrameYaw(timeIndex);
                pitch = entityLiving.GetRotationFramePitch(timeIndex);
            }
            else
            {
                float x = Entity.PosX - ppfx;
                float y = Entity.PosY - ppfy;
                float z = Entity.PosZ - ppfz;
                yaw = Glm.Atan2(z, x) - Glm.Pi90;
                pitch = -Glm.Atan2(y, Mth.Sqrt(x * x + z * z));
              //  yaw = pitch = 0;
            }

            // Возвращаем значения костей в исходное положение, Оригинал
            for (byte i = 0; i < _countBones; i++)
            {
                if (_bonesFlagModify[i])
                {
                    _modelEntity.Bones[i].SetBonePose(ref _bones[i]);
                    _bonesFlagModify[i] = false;
                }
            }

            // Пробегаемся по анимациям
            for (int ai = 0; ai < 2; ai++)
            {
                _GenBoneCurrentPose(ai);
            }

            // Собираем конечные матрицы
            _GetMatrixPalette(yaw, pitch);

            // Заносим в шейдор
            Entities.Render.ShaderBindEntity(entityRender.Player.View,
                   Entity.GetPosFrameX(timeIndex) - ppfx,
                   Entity.GetPosFrameY(timeIndex) - ppfy,
                   Entity.GetPosFrameZ(timeIndex) - ppfz,
                   _bufferBonesTransforms
               );

            entityRender.MeshDraw();
        }

        /// <summary>
        /// Генерируем кости текущей позы
        /// </summary>
        private void _GenBoneCurrentPose(int indexPose)
        {
            _animationClips[indexPose].GenBoneCurrentPose();
            for (byte i = 0; i < _countBones; i++)
            {
                if (_animationClips[indexPose].IsAnimation(i))
                {
                    _bonesFlagModify[i] = true;
                    _bones[i].Add(_animationClips[indexPose].CurrentPoseBones[i]);
                }
            }
        }

        /// <summary>
        /// Получение матричной палитры для позы
        /// </summary>
        private void _GetMatrixPalette(float yaw, float pitch)
        {
            // Корневая кость
            _bonesTransforms[0].Clear();
            if (yaw != 0)
            {
                _bonesTransforms[0].RotateY(-yaw);
            }
            // У корневой кости нет родительской матрицы, поэтому заносим в матричную палитру матрицу перехода как есть
            _bonesTransforms[0] *= _bones[0].GetBoneMatrix() * _modelEntity.Bones[0].MatrixInverse;
            _bonesTransforms[0].ConvArray4x3(_bufferBonesTransforms, 0);

            // Пользуясь порядком хранения, проходим по всем локальным позам и выполняем умножения на родительские матрицы и обратные
            for (int i = 1; i < _countBones; i++)
            {
                // Перемножаем матрицы в положение как выставленно в Blockbench
                _bonesTransforms[i] = _bonesTransforms[_modelEntity.Bones[i].ParentIndex]
                           * _bones[i].GetBoneMatrix();

                // Если надо вращаем Pitch
                if (_modelEntity.Bones[i].IsPitch && pitch != 0)
                {
                    _bonesTransforms[i].RotateX(pitch);
                }

                // Умножаем обратную матрицу
                _bonesTransforms[i] *= _modelEntity.Bones[i].MatrixInverse;
                // Отправляем в кеш
                _bonesTransforms[i].ConvArray4x3(_bufferBonesTransforms, i * 12);
            }
        }
    }
}
