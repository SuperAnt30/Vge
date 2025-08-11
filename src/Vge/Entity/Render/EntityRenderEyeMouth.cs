using System.Runtime.CompilerServices;
using Vge.Renderer.World.Entity;
using Vge.World;

namespace Vge.Entity.Render
{
    /// <summary>
    /// Объект рендера сущности, хранящий данные рендера, с АНИМАЦИЕЙ и глазами и возможно ртом.
    /// Только для клиента
    /// </summary>
    public class EntityRenderEyeMouth : EntityRenderAnimation
    {
        /// <summary>
        /// Время закрытия глаз в игровых тиках
        /// </summary>
        private const byte _timeCloseEye = 5;
        /// <summary>
        /// Время анимации разговора между открыт и закрыт рот в игровых тиках
        /// </summary>
        private const byte _timeSpeaks = 4;
        /// <summary>
        /// Полный цикл разговора
        /// </summary>
        private const byte _timeSpeaksFor = _timeSpeaks * 2;

        /// <summary>
        /// Время маргания глазами в игровых тиках
        /// </summary>
        private readonly int _timeBlinkEye;

        /// <summary>
        /// Открытые ли глаза
        /// </summary>
        private bool _isEyeOpen = true;
        /// <summary>
        /// Счётчик маргания глаз
        /// </summary>
        private int _counterEye = 0;

        /// <summary>
        /// Состояние рта
        /// </summary>
        private EnumMouthState _mouthState = EnumMouthState.Close;
        /// <summary>
        /// Счётчик анимации разговора
        /// </summary>
        private int _counterSpeaks = 0;

        public EntityRenderEyeMouth(EntityBase entity, EntitiesRenderer entities, ResourcesEntity resourcesEntity) 
            : base(entity, entities, resourcesEntity)
        {
            _timeBlinkEye = resourcesEntity.BlinkEye;
            _counterEye = _timeCloseEye;
        }

        /// <summary>
        /// Игровой такт на клиенте
        /// </summary>
        /// <param name="deltaTime">Дельта последнего тика в mc</param>
        public override void UpdateClient(WorldClient world, float deltaTime)
        {
            base.UpdateClient(world, deltaTime);

            if (_isEyeOpen)
            {
                _counterEye++;
                if (_counterEye > _timeBlinkEye)
                {
                    _counterEye = 0;
                }
            }

            if (_mouthState == EnumMouthState.Speaks)
            {
                _counterSpeaks++;
                if (_counterSpeaks > _timeSpeaksFor) _counterSpeaks = 0;
            }
        }

        /// <summary>
        /// Получить параметр для шейдора, на состояния глаз и рта
        /// Значение 1 это открыты глаза, закрыт рот.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override int GetEyeMouth()
            => (_mouthState == EnumMouthState.Speaks
                ? (_counterSpeaks > _timeSpeaks ? 1 : 0) // балтает
                : (int)_mouthState) << 1 // статичное состояние рта
            | ((_counterEye > _timeCloseEye) ? 1 : 0); // глаза

        /// <summary>
        /// Задать состояние открытости глаз
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void SetEyeOpen(bool value)
        {
            if (value != _isEyeOpen)
            {
                _counterEye = value ? _timeCloseEye : 0;
                _isEyeOpen = value;
            }
        }

        /// <summary>
        /// Задать состояние рта
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void SetMouthState(EnumMouthState value)
        {
            if (value != _mouthState) _mouthState = value;
        }
    }
}
