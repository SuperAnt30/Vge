using System;
using Vge.Json;
using Vge.World.Block;

namespace Mvk2.World.Block
{
    /// <summary>
    /// Объект материала блока для Малювек
    /// </summary>
    public class MaterialMvk : MaterialBase
    {
        /// <summary>
        /// На каких блоках можно делать простой крафт
        /// </summary>
        public bool SimpleCraft { get; private set; }

        public MaterialMvk(EnumMaterial material) 
            : base((int)material, material.ToString()) { }

        /// <summary>
        /// Прочесть состояние блока из Json формы
        /// </summary>
        public override void ReadStateFromJson(JsonCompound state)
        {
            base.ReadStateFromJson(state);
            foreach (JsonKeyValue json in state.Items)
            {
                if (json.IsKey("SimpleCraft")) SimpleCraft = json.GetBool();
            }
        }
    }
}
