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
        public readonly IMaterial Air;
        /// <summary>
        /// Отладочный блок
        /// </summary>
        public readonly IMaterial Debug;
        /// <summary>
        /// Вода
        /// </summary>
        public readonly IMaterial Water;
        /// <summary>
        /// Коренная порода
        /// </summary>
        public readonly IMaterial Bedrock;
        /// <summary>
        /// Твёрдое тело
        /// </summary>
        public readonly IMaterial Solid;
        /// <summary>
        /// Сыпучая порода
        /// </summary>
        public readonly IMaterial Loose;
        /// <summary>
        /// Руда
        /// </summary>
        public readonly IMaterial Ore;
        /// <summary>
        /// Растение, трава, цветы, саженцы, плоды, тина
        /// </summary>
        public readonly IMaterial Plant;
        /// <summary>
        /// Древесина, бревно, корень
        /// </summary>
        public readonly IMaterial Wood;
        /// <summary>
        /// Ветка
        /// </summary>
        public readonly IMaterial Branch;
        /// <summary>
        /// Листва
        /// </summary>
        public readonly IMaterial Leaves;
        /// <summary>
        /// Стекло
        /// </summary>
        public readonly IMaterial Glass;
        /// <summary>
        /// Лава
        /// </summary>
        public readonly IMaterial Lava;

        public MaterialsMvk()
        {
            Air = new MaterialMvk((int)EnumMaterial.Air).SetRootGrowing().SetTurfDoesNotDry();
            Debug = new MaterialBase((int)EnumMaterial.Debug);
            _SampleDefault(Debug);
            Water = new MaterialBase((int)EnumMaterial.Water).SetRootGrowing().Liquid();
            _SampleDefault(Water);
            Bedrock = new MaterialBase((int)EnumMaterial.Bedrock);
            _SampleDefault(Bedrock);
            Solid = new MaterialMvk((int)EnumMaterial.Solid).SetSimpleCraft();
            _SampleDefault(Solid);
            Loose = new MaterialMvk((int)EnumMaterial.Loose).SetSimpleCraft().SetRootGrowing();
            Loose.SetSamples(AudioIndexs.GetKeys("DigSand1", "DigSand2", "DigSand3", "DigSand4"),
                AudioIndexs.GetKeys("DigSand1", "DigSand2", "DigSand3", "DigSand4"),
                AudioIndexs.GetKeys("StepSand1", "StepSand2", "StepSand3", "StepSand4"));
            Ore = new MaterialBase((int)EnumMaterial.Ore);
            _SampleDefault(Ore);
            Plant = new MaterialBase((int)EnumMaterial.Plant).SetRequiresNoTool().SetTurfDoesNotDry();
            _SampleDefault(Plant);
            Wood = new MaterialMvk((int)EnumMaterial.Wood).SetSimpleCraft();
            Wood.SetSamples(AudioIndexs.GetKeys("DigWood1", "DigWood2", "DigWood3", "DigWood4"),
                AudioIndexs.GetKeys("DigWood1", "DigWood2", "DigWood3", "DigWood4"),
                AudioIndexs.GetKeys("StepWood1", "StepWood2", "StepWood3", "StepWood4"));
            Branch = new MaterialBase((int)EnumMaterial.Branch).SetRequiresNoTool().SetTurfDoesNotDry();
            Branch.SetSamples(AudioIndexs.GetKeys("DigWood1", "DigWood2", "DigWood3", "DigWood4"),
                AudioIndexs.GetKeys("DigWood1", "DigWood2", "DigWood3", "DigWood4"),
                AudioIndexs.GetKeys("StepWood1", "StepWood2", "StepWood3", "StepWood4"));
            Leaves = new MaterialBase((int)EnumMaterial.Leaves).SetRequiresNoTool().SetTurfDoesNotDry();
            _SampleDefault(Leaves);
            Glass = new MaterialBase((int)EnumMaterial.Glass).SetGlass();
            _SampleDefault(Glass);
            Lava = new MaterialBase((int)EnumMaterial.Lava).Liquid().SetIgnites();
            _SampleDefault(Lava);
        }

        private void _SampleDefault(IMaterial material)
        {
            material.SetSamples(AudioIndexs.GetKeys("DigStone1", "DigStone2", "DigStone3", "DigStone4"),
                AudioIndexs.GetKeys("DigStone1", "DigStone2", "DigStone3", "DigStone4"),
                AudioIndexs.GetKeys("StepStone1", "StepStone2", "StepStone3", "StepStone4"));
        }
    }
}
