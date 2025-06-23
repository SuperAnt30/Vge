using System.Runtime.CompilerServices;

namespace Vge.Entity
{
    /// <summary>
    /// Объект плавного перемещения за TPS и FPS
    /// </summary>
    public class SmoothFrame
    {
        /// <summary>
        /// Значение для конечного кадра прорисовкт
        /// </summary>
        public float ValueFrame { get; private set; }

        /// <summary>
        /// Тикущее значение
        /// </summary>
        private float _value;
        /// <summary>
        /// Последнее значение
        /// </summary>
        private float _valueLast;
        /// <summary>
        /// Значение для финишного расчёта
        /// </summary>
        private float _valueEnd = 0f;
        /// <summary>
        /// Количество требуемых тактов
        /// </summary>
        private int _count = 0;

        public SmoothFrame(float value) 
            => ValueFrame = _value = _valueLast = _valueEnd = value;

        /// <summary>
        /// Внести изменение
        /// </summary>
        /// <param name="value">Требуемое значение</param>
        /// <param name="count">Каличество тактов TPS до выполнения</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(float value, int count)
        {
            if (_valueEnd != value)
            {
                _valueEnd = value;
                _count = count > _count ? count - _count : 1;
            }
        }

        /// <summary>
        /// Обновить в TPS
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update()
        {
            if (_valueEnd != _value)
            {
                _value = _valueLast;
                if (_count > 0)
                {
                    _valueLast = (_valueEnd - _value) / _count + _value;
                    _count--;
                }
                else
                {
                    _valueLast = _valueEnd;
                    _count = 0;
                }
            }
            else
            {
                if (_valueLast != _valueEnd) _valueLast = _valueEnd;
                if (_count != 0) _count = 0;
            }
        }

        /// <summary>
        /// Получить значение в кадре FPS
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0..1, где 0 это финиш, 1 начало</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool UpdateFrame(float timeIndex)
        {
            if (_value != _valueLast)
            {
                ValueFrame = _value + (_valueLast - _value) * timeIndex;
                return true;
            }
            if (ValueFrame != _value)
            {
                ValueFrame = _value;
                return true;
            }
            return false;
        }
    }
}
