using Vge.Util;

namespace Vge.World.Block
{
    /// <summary>
    /// Объект материала блока
    /// </summary>
    public class MaterialBase : IMaterial
    {
        /// <summary>
        /// Получить уникальный номер материала
        /// </summary>
        public int IndexMaterial { get; private set; }
        /// <summary>
        /// Возвращает, если блоки этих материалов являются жидкостями
        /// </summary>
        public bool IsLiquid { get; private set; }
        /// <summary>
        /// Не требует инструмента для разрушения блока
        /// </summary>
        public bool RequiresNoTool { get; private set; }
        /// <summary>
        /// Является ли блок стеклянным, блок или панель
        /// </summary>
        public bool Glass { get; private set; }
        /// <summary>
        /// Дёрн не сохнет, под этим блоком
        /// </summary>
        public bool TurfDoesNotDry { get; private set; }
        /// <summary>
        /// Воспламеняет (лава или огонь)
        /// </summary>
        public bool Ignites { get; private set; }
        /// <summary>
        /// Растёт корень
        /// </summary>
        public bool RootGrowing { get; private set; }

        /// <summary>
        /// Индексы семплов сломоного блока
        /// </summary>
        private int[] _samplesBreak;
        /// <summary>
        /// Индексы семплов установленного блока
        /// </summary>
        private int[] _samplesPut;
        /// <summary>
        /// Индексы семплов хотьбы по блоку
        /// </summary>
        private int[] _samplesStep;

        public MaterialBase(int index) => IndexMaterial = index;

        /// <summary>
        /// Задать материал жидкостью
        /// </summary>
        public MaterialBase Liquid()
        {
            IsLiquid = true;
            return this;
        }

        /// <summary>
        /// Задать Не требует инструмента для разрушения блока
        /// </summary>
        public MaterialBase SetRequiresNoTool()
        {
            RequiresNoTool = true;
            return this;
        }

        /// <summary>
        /// Задать Является ли блок стеклянным, блок или панель
        /// </summary>
        public MaterialBase SetGlass()
        {
            Glass = true;
            return this;
        }

        /// <summary>
        /// Задать Дёрн не сохнет, под этим блоком
        /// </summary>
        public MaterialBase SetTurfDoesNotDry()
        {
            TurfDoesNotDry = true;
            return this;
        }

        /// <summary>
        /// Задать воспламенение (огонь или лава)
        /// </summary>
        public MaterialBase SetIgnites()
        {
            Ignites = true;
            return this;
        }

        /// <summary>
        /// Задать на этих блоках может рости корень дерева
        /// </summary>
        public MaterialBase SetRootGrowing()
        {
            RootGrowing = true;
            return this;
        }

        #region Sound

        /// <summary>
        /// Задать индексы семплов
        /// </summary>
        public void SetSamples(int[] breaks, int[] puts, int[] steps)
        {
            _samplesBreak = breaks;
            _samplesPut = puts;
            _samplesStep = steps;
        }

        /// <summary>
        /// Есть ли звуковой эффект шага
        /// </summary>
        public bool IsSampleStep() => _samplesStep != null;

        /// <summary>
        /// Получить индекс семпла разрушения блока
        /// </summary>
        public int SampleBreak(Rand rand) => _samplesBreak[rand.Next(_samplesBreak.Length)];
        /// <summary>
        /// Получить индекс семпла установки блока
        /// </summary>
        public int SamplePut(Rand rand) => _samplesPut[rand.Next(_samplesPut.Length)];
        /// <summary>
        /// Получить индекс семпла ходьбы по блоку
        /// </summary>
        public int SampleStep(Rand rand)=> _samplesStep[rand.Next(_samplesStep.Length)];

        #endregion

        /// <summary>
        /// Строка
        /// </summary>
        public override string ToString() => IndexMaterial.ToString();
    }
}
