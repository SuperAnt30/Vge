using System;
using Vge.Audio;
using Vge.Json;
using Vge.Util;

namespace Vge.World.Block
{
    /// <summary>
    /// Объект материала блока
    /// </summary>
    public class MaterialBase
    {
        /// <summary>
        /// Получить уникальный номер материала
        /// </summary>
        public readonly int IndexMaterial;
        /// <summary>
        /// Псевдоним материала
        /// </summary>
        public readonly string Alias;

        /// <summary>
        /// Возвращает, если блоки этих материалов являются жидкостями
        /// </summary>
        public bool Liquid { get; private set; }
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
        /// Может ли корень дерева при росте заменить этот блок на корень
        /// </summary>
        public bool RootGrowing { get; private set; }

        /// <summary>
        /// Индексы семплов сломоного блока
        /// </summary>
        private int[] _samplesBreak;
        /// <summary>
        /// Индексы семплов установленного блока
        /// </summary>
        private int[] _samplesPlace;
        /// <summary>
        /// Индексы семплов хотьбы по блоку
        /// </summary>
        private int[] _samplesStep;

        public MaterialBase(int index, string alias)
        {
            IndexMaterial = index;
            Alias = alias;
        }

        #region Sound

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
        public int SamplePlace(Rand rand) => _samplesPlace[rand.Next(_samplesPlace.Length)];
        /// <summary>
        /// Получить индекс семпла ходьбы по блоку
        /// </summary>
        public int SampleStep(Rand rand)=> _samplesStep[rand.Next(_samplesStep.Length)];

        #endregion

        #region Методы для импорта данных с json

        /// <summary>
        /// Прочесть состояние блока из Json формы
        /// </summary>
        public virtual void ReadStateFromJson(JsonCompound state)
        {
            // Статы
            foreach (JsonKeyValue json in state.Items)
            {
                if (json.IsKey(Ctm.Liquid)) Liquid = json.GetBool();
                if (json.IsKey(Ctm.RequiresNoTool)) RequiresNoTool = json.GetBool();
                if (json.IsKey(Ctm.Glass)) Glass = json.GetBool();
                if (json.IsKey(Ctm.TurfDoesNotDry)) TurfDoesNotDry = json.GetBool();
                if (json.IsKey(Ctm.Ignites)) Ignites = json.GetBool();
                if (json.IsKey(Ctm.RootGrowing)) RootGrowing = json.GetBool();
                if (json.IsKey(Ctm.SamplesBreak))
                {
                    string[] samples = json.GetArray().ToArrayString();
                    _samplesBreak = AudioIndexs.GetKeys(samples);
                }
                if (json.IsKey(Ctm.SamplesStep))
                {
                    string[] samples = json.GetArray().ToArrayString();
                    _samplesStep = AudioIndexs.GetKeys(samples);
                }
            }
        }

        /// <summary>
        /// Если необходимо, копируем семплы Break в Place
        /// </summary>
        public void SamplePlaceCopy()
        {
            if (_samplesPlace == null && _samplesBreak != null)
            {
                _samplesPlace = _samplesBreak;
            }
        }

        #endregion

        /// <summary>
        /// Строка
        /// </summary>
        public override string ToString() => IndexMaterial.ToString() + " " + Alias;
    }
}
