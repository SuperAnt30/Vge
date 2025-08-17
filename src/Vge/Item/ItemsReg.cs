using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Vge.Json;
using Vge.Util;
using Vge.World.Block;

namespace Vge.Item
{
    /// <summary>
    /// Регистрация предметов
    /// </summary>
    public sealed class ItemsReg
    {
        /// <summary>
        /// Таблица предметов для регистрации
        /// </summary>
        public static readonly ItemRegTable Table = new ItemRegTable();

        /// <summary>
        /// Справочник всех форм предметов
        /// </summary>
        private static readonly Dictionary<string, JsonCompound> _shapes = new Dictionary<string, JsonCompound>();
        /// <summary>
        /// Справочник родлителей стат предмета
        /// </summary>
        private static Dictionary<string, JsonCompound> _parentStats = new Dictionary<string, JsonCompound>();


        /// <summary>
        /// Инициализация блоков, если window не указывать, прорисовки о статусе не будет (для сервера)
        /// </summary>
        public static void Initialization(WindowMain window = null)
        {
            if (window != null)
            {
                window.LScreen.Process(L.T("CreateItems"));
                window.DrawFrame();
            }

            _InitializationBegin();
        }

        /// <summary>
        /// Перед инициализацией
        /// </summary>
        private static void _InitializationBegin()
        {
            // Очистить таблицы и вспомогательные данные json
            _Clear();

            // Регистрация обязательных блоков
            // Воздух
            //string alias = "Air";
            //BlockAir blockAir = new BlockAir();
            //blockAir.InitAliasAndJoinN1(alias, new JsonCompound(), new JsonCompound(new JsonKeyValue[] { }));
            //Table.Add(alias, blockAir);

            //// Отладочный
            //RegisterBlockClass("Debug", new BlockDebug());
        }

        /// <summary>
        /// Корректировка блоков после загрузки, если загрузки нет,
        /// всё равно надо, для активации
        /// </summary>
        public static void Correct(CorrectTable correct)
        {
            correct.CorrectRegLoad(Table);
            Ce.Items = new ItemArrays();

            // Очистить таблицы и вспомогательные данные json
            _Clear();
        }

        /// <summary>
        /// Очистить таблицы и вспомогательные данные json
        /// </summary>
        private static void _Clear()
        {
            // Очистить массивы регистрации
            Table.Clear();
            // Очистить вспомогательные данные json
            _parentStats.Clear();
            _shapes.Clear();
        }

        /// <summary>
        /// Зарегистрировать предмет
        /// </summary>
        public static void RegisterItemClass(string alias, ItemBase itemObject)
        {
            JsonRead jsonRead = new JsonRead(Options.PathItems + alias + ".json");

            if (jsonRead.IsThereFile)
            {
                itemObject.SetAlias(alias);

                JsonCompound state = _ParentState(jsonRead.Compound);
                List<JsonKeyValue> shapes = new List<JsonKeyValue>();
                
                if (state.IsKey(Cti.Sprite))
                {
                    // Только текстура
                    itemObject.InitAndJoinN2(state);
                }
                if (state.IsKey(Cti.Shape))
                {
                    // Фигура предмета
                    itemObject.InitAndJoinN2(state, _GetShape(state.GetString(Cti.Shape)));
                }
                else if (state.IsKey(Cti.ShapeBlock))
                {
                    // Фигура блока
                    itemObject.InitAndJoinN2(state, BlocksReg.GetShape(state.GetString(Cti.ShapeBlock)));
                }
                else
                {
                    // Отсутствует фигура предмета
                    throw new Exception(Sr.GetString(Sr.TheFigureObjectIsMissing, alias));
                }

                //itemObject.InitAliasAndJoinN2(alias, state, new JsonCompound(shapes.ToArray()));
                Table.Add(alias, itemObject);
            }
            else
            {
                throw new Exception(Sr.GetString(Sr.FileMissingJsonItem, alias));
            }

            //_window.LScreen.Process("Block init " + alias);
            //_window.DrawFrame();
        }

        #region Shape

        /// <summary>
        /// Получить фигуру из кэша или сначала добавив в кэш и потом получим
        /// </summary>
        private static JsonCompound _GetShape(string name)
        {
            if (name != "")
            {
                if (_shapes.ContainsKey(name)) 
                {
                    return _shapes[name];
                }
                // Добавляем фигуру
                JsonRead jsonRead = new JsonRead(Options.PathShapeItems + name + ".json");
                if (jsonRead.IsThereFile)
                {
                    return _ParentShape(jsonRead.Compound);
                }
            }
            return new JsonCompound();
        }

        /// <summary>
        /// Проверяем наличие парента для фигуры, если имеется то корректируем JsonCompound и возвращаем с его учётом
        /// </summary>
        private static JsonCompound _ParentShape(JsonCompound compound)
        {
            string parent = compound.GetString("Parent");
            if (parent != "")
            {
                // Имеется родитель
                if (_shapes.ContainsKey(parent))
                {
                    // Имеется в справочнике
                    return _SetChildState(_shapes[parent], compound);
                }
                else
                {
                    JsonRead jsonRead = new JsonRead(Options.PathShapeItems + parent + ".json");
                    if (jsonRead.IsThereFile)
                    {
                        JsonCompound state = _ParentShape(jsonRead.Compound);
                        _shapes.Add(parent, state);
                        return _SetChildState(state, compound);
                    }
                }
            }
            return compound;
        }

        #endregion

        #region State

        /// <summary>
        /// Проверяем наличие парента для стат, если имеется то корректируем JsonCompound и возвращаем с его учётом
        /// </summary>
        private static JsonCompound _ParentState(JsonCompound compound)
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
                    JsonRead jsonRead = new JsonRead(Options.PathItems + parent + ".json");
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
        private static JsonCompound _SetChildState(JsonCompound main, JsonCompound child)
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
