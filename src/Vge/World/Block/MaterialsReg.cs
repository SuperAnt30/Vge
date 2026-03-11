using System;
using System.Collections.Generic;
using System.IO;
using Vge.Json;
using Vge.Util;

namespace Vge.World.Block
{
    /// <summary>
    /// Регистрация материалов
    /// </summary>
    public class MaterialsReg
    {
        /// <summary>
        /// Справочник родлителей стат блока
        /// </summary>
        private Dictionary<string, JsonCompound> _parentStats = new Dictionary<string, JsonCompound>();

        /// <summary>
        /// Зарегистрировать статы материала
        /// </summary>
        public MaterialBase RegState(MaterialBase material, string path = "")
        {
            if (path != "") path += Path.DirectorySeparatorChar;
            JsonRead jsonRead = new JsonRead(Options.PathMaterials + path + material.Alias + ".json");
            if (jsonRead.IsThereFile)
            {
                try
                {
                    material.ReadStateFromJson(_ParentState(jsonRead.Compound));
                    material.SamplePlaceCopy();
                    return material;
                }
                catch (Exception ex)
                {
                    throw new Exception(Sr.GetString(Sr.ErrorReadJsonMaterialStat, material.Alias) 
                        + " " + ex.Message, ex);
                }
            }
            else
            {
                throw new Exception(Sr.GetString(Sr.FileMissingJsonBlock, material.Alias));
            }
        }

        #region State

        /// <summary>
        /// Проверяем наличие парента для стат, если имеется то корректируем JsonCompound и возвращаем с его учётом
        /// </summary>
        private JsonCompound _ParentState(JsonCompound compound)
        {
            string parent = compound.GetString("Parent");
            if (parent != "")
            {
                // Имеется родитель
                if (_parentStats.ContainsKey(parent))
                {
                    // Имеется в справочнике
                    return _SetChildState(_parentStats[parent], compound);
                }
                else
                {
                    JsonRead jsonRead = new JsonRead(Options.PathMaterials + parent + ".json");
                    if (jsonRead.IsThereFile)
                    {
                        JsonCompound state = _ParentState(jsonRead.Compound);
                        _parentStats.Add(parent, state);
                        return _SetChildState(state, compound);
                    }
                }
            }
            return compound;
        }

        /// <summary>
        /// Склеить ребёнка к основе для стат блока
        /// </summary>
        private JsonCompound _SetChildState(JsonCompound main, JsonCompound child)
        {
            bool add = true;
            int i, count;
            List<JsonKeyValue> list = new List<JsonKeyValue>(main.Items);

            foreach (JsonKeyValue json in child.Items)
            {
                count = main.Items.Length;
                for (i = 0; i < count; i++)
                {
                    if (main.Items[i].IsKey(json.Key))
                    {
                        list[i] = json;
                        add = false;
                        break;
                    }
                }
                if (add)
                {
                    list.Add(json);
                }
                else
                {
                    add = true;
                }
            }

            return new JsonCompound(list.ToArray());
        }

        #endregion
    }
}
