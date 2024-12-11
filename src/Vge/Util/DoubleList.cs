namespace Vge.Util
{
    /// <summary>
    /// Двойной список, с lock для разных потоков
    /// </summary>
    public class DoubleList<T>
    {
        private ListFast<T> vsForward;
        private ListFast<T> vsBackward;
        private ListFast<T> vsOne;
        private ListFast<T> vsTwo;
        private readonly object locker = new object();
        private bool one = true;
        private int index;

        public DoubleList(int size = 100)
        {
            vsOne = new ListFast<T>(size);
            vsTwo = new ListFast<T>(size);
            vsForward = vsOne;
            vsBackward = vsTwo;
        }
        /// <summary>
        /// Добавить значение в список
        /// </summary>
        public void Add(T item)
        {
            lock (locker) vsForward.Add(item);
        }
        /// <summary>
        /// Количество значений в списке которе использует Get
        /// </summary>
        public int CountBackward => vsBackward.Count;
        /// <summary>
        /// Количество значений в списке которое использует Add
        /// </summary>
        public int CountForward => vsForward.Count;
        /// <summary>
        /// Пустой ли буфера данных
        /// </summary>
        public bool Empty() => (vsForward.Count + vsBackward.Count) == 0;

        /// <summary>
        /// Получить следующее значение из списка
        /// </summary>
        public T GetNext() => vsBackward[index++];

        /// <summary>
        /// Очистить значения в списке которе использует Get 
        /// </summary>
        public void ClearBackward() => vsBackward.Clear();

        /// <summary>
        /// Получить следующее значение из списка и присвоить в массиве его null
        /// </summary>
        public T GetNextNull()
        {
            T value = vsBackward[index];
            vsBackward.IndexNull(index++);
            return value;
        }

        /// <summary>
        /// Зафиксировать следующй шаг, где происходит смена листов, надо перед Count и Get
        /// </summary>
        public void Step()
        {
            lock (locker)
            {
                if (one)
                {
                    vsTwo.Clear();
                    vsForward = vsTwo;
                    vsBackward = vsOne;
                    one = false;
                }
                else
                {
                    vsOne.Clear();
                    vsForward = vsOne;
                    vsBackward = vsTwo;
                    one = true;
                }
                index = 0;
            }
        }

        public void Clear()
        {
            lock (locker)
            {
                vsTwo.Clear();
                vsOne.Clear();
                index = 0;
            }
        }

        /// <summary>
        /// Вернуть список которое использует Add
        /// </summary>
        public ListFast<T> GetForward() => vsForward;

        /// <summary>
        /// Вернуть список которое использует Get
        /// </summary>
        public ListFast<T> GetBackward() => vsBackward;
    }
}
