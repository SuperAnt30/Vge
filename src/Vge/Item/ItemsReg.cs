using System;
using System.Collections.Generic;
using System.IO;
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
        /// Размер спрайта предмета в инвентаре, pix
        /// </summary>
        public static int SizeSprite = 16;
        /// <summary>
        /// Размер фигуры предмета в инвентаре, pix
        /// </summary>
        public static int SizeShape = 16;

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

            // Регистрация обязательных предметов
            //...
            // Отладочный
            //RegisterItemClass("Debug", new ItemBase());
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
        /// Зарегистрировать предмет одежды
        /// </summary>
        public static ItemCloth RegisterItemClothClass(string alias)
        {
            ItemCloth itemCloth = new ItemCloth();
            RegisterItemClass(alias, itemCloth, "Cloth" + Path.DirectorySeparatorChar);
            return itemCloth;
        }

        /// <summary>
        /// Зарегистрировать предмет
        /// </summary>
        public static void RegisterItemClass(string alias, ItemBase itemObject, string path = "")
        {
            JsonRead jsonRead = new JsonRead(Options.PathItems + path + alias + ".json");

            if (jsonRead.IsThereFile)
            {
                itemObject.SetAlias(alias);

                JsonCompound state = _ParentState(jsonRead.Compound);

                // Данные
                itemObject.Init(state);

                List<JsonKeyValue> shapes = new List<JsonKeyValue>();
                
                if (state.IsKey(Cti.Sprite))
                {
                    // Только текстура
                    itemObject.Buffer.InitAndSprite(alias, state);
                }
                else if (state.IsKey(Cti.Shape))
                {
                    // Фигура предмета
                    itemObject.Buffer.InitAndShape(alias, state, _GetShape(state.GetString(Cti.Shape)), true);
                }
                else if (state.IsKey(Cti.ShapeBlock))
                {
                    // Фигура блока
                    itemObject.Buffer.InitAndShape(alias, state, BlocksReg.GetShape(state.GetString(Cti.ShapeBlock)), false);
                }
                else
                {
                    // Отсутствует фигура предмета
                    throw new Exception(Sr.GetString(Sr.TheFigureItemIsMissing, alias));
                }
                // GUI
                if (state.IsKey(Cti.SpriteGui))
                {
                    // Только текстура
                    itemObject.Buffer.InitSpriteGui(alias, state, SizeSprite);
                }
                else if (state.IsKey(Cti.ShapeGui))
                {
                    // Фигура предмета
                    itemObject.Buffer.InitShapeGui(alias, state, _GetShape(state.GetString(Cti.ShapeGui)), SizeShape);
                }
                else if (state.IsKey(Cti.ShapeBlockGui))
                {
                    // Фигура блока
                    itemObject.Buffer.InitShapeGui(alias, state, BlocksReg.GetShape(state.GetString(Cti.ShapeBlockGui)), SizeShape);
                }
                else
                {
                    // Отсутствует фигура предмета
                    throw new Exception(Sr.GetString(Sr.TheFigureItemIsMissing, alias));
                }
                Table.Add(alias, itemObject);
            }
            else
            {
                throw new Exception(Sr.GetString(Sr.FileMissingJsonItem, alias));
            }

            //_window.LScreen.Process("Item init " + alias);
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
