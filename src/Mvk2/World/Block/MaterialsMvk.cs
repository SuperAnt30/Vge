using Mvk2.Audio;
using Vge.World.Block;

namespace Mvk2.World.Block
{
    /// <summary>
    /// Регестрация материалов
    /// </summary>
    public class MaterialsMvk
    {
        /// <summary>
        /// Воздух
        /// </summary>
        public readonly MaterialBase Air;
        /// <summary>
        /// Отладочный блок
        /// </summary>
        public readonly MaterialBase Debug;
        /// <summary>
        /// Вода
        /// </summary>
        public readonly MaterialBase Water;
        /// <summary>
        /// Коренная порода
        /// </summary>
        public readonly MaterialBase Bedrock;
        /// <summary>
        /// Твёрдое тело
        /// </summary>
        public readonly MaterialBase Solid;
        /// <summary>
        /// Сыпучая порода
        /// </summary>
        public readonly MaterialBase Loose;
        /// <summary>
        /// Руда
        /// </summary>
        public readonly MaterialBase Ore;
        /// <summary>
        /// Растение, трава, цветы, саженцы, плоды, тина
        /// </summary>
        public readonly MaterialBase Plant;
        /// <summary>
        /// Древесина, бревно, корень
        /// </summary>
        public readonly MaterialBase Wood;
        /// <summary>
        /// Ветка
        /// </summary>
        public readonly MaterialBase Branch;
        /// <summary>
        /// Листва
        /// </summary>
        public readonly MaterialBase Leaves;
        /// <summary>
        /// Стекло
        /// </summary>
        public readonly MaterialBase Glass;
        /// <summary>
        /// Лава
        /// </summary>
        public readonly MaterialBase Lava;

        public MaterialsMvk()
        {
            MaterialsReg reg = new MaterialsReg();

            Air = reg.RegState(new MaterialMvk(EnumMaterial.Air));
            Debug = reg.RegState(new MaterialMvk(EnumMaterial.Debug));
            Water = reg.RegState(new MaterialMvk(EnumMaterial.Water));
            Bedrock = reg.RegState(new MaterialMvk(EnumMaterial.Bedrock));
            Solid = reg.RegState(new MaterialMvk(EnumMaterial.Solid));
            Loose = reg.RegState(new MaterialMvk(EnumMaterial.Loose));
            Ore = reg.RegState(new MaterialMvk(EnumMaterial.Ore));
            Plant = reg.RegState(new MaterialMvk(EnumMaterial.Plant));
            Wood = reg.RegState(new MaterialMvk(EnumMaterial.Wood));
            Branch = reg.RegState(new MaterialMvk(EnumMaterial.Branch));
            Leaves = reg.RegState(new MaterialMvk(EnumMaterial.Leaves));
            Glass = reg.RegState(new MaterialMvk(EnumMaterial.Glass));
            Lava = reg.RegState(new MaterialMvk(EnumMaterial.Lava));
        }
    }
}
