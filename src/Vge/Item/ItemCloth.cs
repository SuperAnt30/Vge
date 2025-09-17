using System;
using Vge.Json;

namespace Vge.Item
{
    /// <summary>
    /// Предмет одежды, либо то что может одеть сущность
    /// </summary>
    public class ItemCloth : ItemBase
    {
        /// <summary>
        /// Имя на что надеть на тело, указываем на какую часть тело может одеваться предмет
        /// </summary>
        public string PutOnBody { get; protected set; }
        /// <summary>
        /// Имя слоя, название слоя одежды из модели
        /// </summary>
        public string NameLayer { get; protected set; }

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
                    NameLayer = state.GetString(Cti.NameLayer);
                }
                else
                {
                    throw new Exception(Sr.GetString(Sr.RequiredParameterIsMissingItem, Alias,
                        Cti.PutOnBody + "," + Cti.NameLayer));
                }
            }
        }
    }
}
