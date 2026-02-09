using Vge.World.Block;

namespace Mvk2.World.Block
{
    /// <summary>
    /// Регестрация материалов
    /// </summary>
    public class MaterialsMvk
    {
        public readonly IMaterial Air;
        public readonly IMaterial Debug;
        public readonly IMaterial Water;
        public readonly IMaterial Bedrock;
        public readonly IMaterial Solid;
        public readonly IMaterial Loose;
        public readonly IMaterial Ore;
        public readonly IMaterial Plant;
        public readonly IMaterial Wood;
        public readonly IMaterial Branch;
        public readonly IMaterial Leaves;
        public readonly IMaterial Glass;
        public readonly IMaterial Lava;

        public MaterialsMvk()
        {
            Air = new MaterialMvk((int)EnumMaterial.Air).SetRootGrowing().SetTurfDoesNotDry();
            Debug = new MaterialBase((int)EnumMaterial.Debug);
            Water = new MaterialBase((int)EnumMaterial.Water).SetRootGrowing().Liquid();
            Bedrock = new MaterialBase((int)EnumMaterial.Bedrock);
            Solid = new MaterialMvk((int)EnumMaterial.Solid).SetSimpleCraft();
            Loose = new MaterialMvk((int)EnumMaterial.Loose).SetSimpleCraft().SetRootGrowing();
            Ore = new MaterialBase((int)EnumMaterial.Ore);
            Plant = new MaterialBase((int)EnumMaterial.Plant).SetRequiresNoTool().SetTurfDoesNotDry();
            Wood = new MaterialMvk((int)EnumMaterial.Wood).SetSimpleCraft();
            Branch = new MaterialBase((int)EnumMaterial.Branch).SetTurfDoesNotDry();
            Leaves = new MaterialBase((int)EnumMaterial.Leaves).SetRequiresNoTool().SetTurfDoesNotDry();
            Glass = new MaterialBase((int)EnumMaterial.Glass).SetGlass();
            Lava = new MaterialBase((int)EnumMaterial.Lava).Liquid().SetIgnites();
        }
    }
}
