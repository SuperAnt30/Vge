using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vge.World.Block
{
    /// <summary>
    /// Константы названий тег для параметров материала в json
    /// </summary>
    public sealed class Ctm
    {
        /// <summary>
        /// Отмечает, если блоки этих материалов являются жидкостями. (bool)
        /// </summary>
        public const string Liquid = "Liquid";
        /// <summary>
        /// Отмечает, не требует инструмента для разрушения блока. (bool)
        /// </summary>
        public const string RequiresNoTool = "RequiresNoTool";
        /// <summary>
        /// Отмечает, является ли блок стеклянным, блок или панель. (bool)
        /// </summary>
        public const string Glass = "Glass";
        /// <summary>
        /// Отмечает, дёрн не сохнет, под этим блоком. (bool)
        /// </summary>
        public const string TurfDoesNotDry = "TurfDoesNotDry";
        /// <summary>
        /// Отмечает, воспламеняет (лава или огонь). (bool)
        /// </summary>
        public const string Ignites = "Ignites";
        /// <summary>
        /// Отмечает, может ли корень дерева при росте заменить этот блок на корень. (bool)
        /// </summary>
        public const string RootGrowing = "RootGrowing";
        /// <summary>
        /// Массив семплов разрушения блока. ([string])
        /// </summary>
        public const string SamplesBreak = "SamplesBreak";
        /// <summary>
        /// Массив семплов установки блока, если его нет возьмёт SamplesBreak. ([string])
        /// </summary>
        public const string SamplesPlace = "SamplesPlace";
        /// <summary>
        /// Массив семплов ходьбы по блоку. ([string])
        /// </summary>
        public const string SamplesStep = "SamplesStep";
    }
}
