using System;
using System.Runtime.CompilerServices;
using Vge.Json;

namespace Vge.Item.List
{
    /// <summary>
    /// Предмет одежды, либо то что может одеть сущность
    /// </summary>
    public class ItemCloth : ItemBase
    {
        /// <summary>
        /// Ячейки кармана
        /// </summary>
        public byte CellsPocket { get; protected set; }
        /// <summary>
        /// Ячейки рюкзака
        /// </summary>
        public byte CellsBackpack { get; protected set; }
        
        /// <summary>
        /// Имя на что надеть на тело, указываем на какую часть тело может одеваться предмет
        /// </summary>
        public string PutOnBody { get; protected set; }

        /// <summary>
        /// Массив имён слоёв, название слоя одежды из модели
        /// </summary>
        private string[] _nameLayer;

        /// <summary>
        /// Получить имя слоя в конкретном слоте
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual string GetNameLayer(byte slotKey)
        {
            if (_nameLayer.Length == 1 || _slotClothIndex.Length == 1)
            {
                return _nameLayer[0];
            }
            for (int i = 0; i < _slotClothIndex.Length; i++)
            {
                if (_slotClothIndex[i] == slotKey)
                {
                    return _nameLayer[i];
                }
            }
            return _nameLayer[0];
        }

        /// <summary>
        /// Прочесть состояние блока из Json формы
        /// </summary>
        protected override void _ReadStateFromJson(JsonCompound state)
        {
            base._ReadStateFromJson(state);
            if (state.Items != null)
            {
                if (state.IsKey(Cti.PutOnBody) && state.IsKey(Cti.NameLayer))
                {
                    PutOnBody = state.GetString(Cti.PutOnBody);

                    foreach (JsonKeyValue json in state.Items)
                    {
                        if (json.IsKey(Cti.NameLayer))
                        {
                            if (json.IsValue()) _nameLayer = new string[] { json.GetString() };
                            else if (json.IsArray()) _nameLayer = json.GetArray().ToArrayString();
                        }
                    }
                    if (_nameLayer == null || _nameLayer.Length == 0)
                    {
                        throw new Exception(Sr.GetString(Sr.RequiredParameterIsMissingItem, Alias, Cti.NameLayer));
                    }
                }
                else
                {
                    throw new Exception(Sr.GetString(Sr.RequiredParameterIsMissingItem, Alias,
                        Cti.PutOnBody + "," + Cti.NameLayer));
                }

                if (state.IsKey(Cti.CellsBackpack))
                {
                    CellsBackpack = (byte)state.GetInt(Cti.CellsBackpack);
                }

                if (state.IsKey(Cti.CellsPocket))
                {
                    CellsPocket = (byte)state.GetInt(Cti.CellsPocket);
                }
            }
        }
    }
}
