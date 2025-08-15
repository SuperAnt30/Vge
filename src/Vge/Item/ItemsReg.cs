using System;
using System.Collections.Generic;
using Vge.Json;
using Vge.Util;

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
            // Создаём графический объект гдля генерации атласа блокоы
            //BlockAtlas.CreateImage(64, 16);

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
        /// Инициализация атласа предметов, после инициализации предметов
        /// </summary>
        public static void InitializationAtlas(WindowMain window) { }
        //=> BlockAtlas.EndImage(window.Render.Texture);

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
            //_parentStats.Clear();
            //_shapes.Clear();
        }

        /// <summary>
        /// Зарегистрировать предмет
        /// </summary>
        public static void RegisterItemClass(string alias, ItemBase itemObject)
        {
            JsonRead jsonRead = new JsonRead(Options.PathItems + alias + ".json");

            //if (jsonRead.IsThereFile)
            //{
            //    JsonCompound state = _ParentState(jsonRead.Compound);
            //    List<JsonKeyValue> shapes = new List<JsonKeyValue>();
            //    if (state.IsKey(Ctb.Variants))
            //    {
            //        JsonArray variants = state.GetArray(Ctb.Variants);
            //        bool bodyShape = false;
            //        foreach (JsonCompound variant in variants.Items)
            //        {
            //            string shapeName = variant.GetString(Ctb.Shape);
            //            foreach (JsonKeyValue shape in shapes)
            //            {
            //                if (shape.IsKey(shapeName))
            //                {
            //                    bodyShape = true;
            //                    break;
            //                }
            //            }
            //            if (bodyShape)
            //            {
            //                bodyShape = false;
            //            }
            //            else
            //            {
            //                shapes.Add(new JsonKeyValue(shapeName, _GetShape(shapeName)));
            //            }
            //        }
            //    }
            //    if (state.IsKey(Ctb.Variant))
            //    {
            //        string shapeName = state.GetString(Ctb.Variant);
            //        shapes.Add(new JsonKeyValue(shapeName, _GetShape(shapeName)));
            //    }
            //    blockObject.InitAliasAndJoinN1(alias, state, new JsonCompound(shapes.ToArray()));
                Table.Add(alias, itemObject);
            //}
            //else
            //{
            //    throw new Exception(Sr.GetString(Sr.FileMissingJsonBlock, alias));
            //}

            //_window.LScreen.Process("Block init " + alias);
            //_window.DrawFrame();
        }
    }
}
