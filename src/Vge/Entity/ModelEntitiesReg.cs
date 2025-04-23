using System;
using System.Collections.Generic;
using Vge.Json;
using Vge.Util;

namespace Vge.Entity
{
    /// <summary>
    /// Регистрация моделей сущностей
    /// </summary>
    public sealed class ModelEntitiesReg
    {
        /// <summary>
        /// Таблица моделей сущностей для регистрации
        /// </summary>
        public static readonly ModelEntitiesRegTable Table = new ModelEntitiesRegTable();

        /// <summary>
        /// Инициализация моделей сущности, если window не указывать, прорисовки о статусе не будет (для сервера)
        /// </summary>
        public static void Initialization(WindowMain window = null)
        {
            if (window != null)
            {
                window.LScreen.Process(L.T("CreateModelsEntities"));
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

            // Регистрация обязательных сущностей
            // Воздух
            //string alias = "Air";
            //BlockAir blockAir = new BlockAir();
            //blockAir.InitAliasAndJoinN1(alias, new JsonCompound(), new JsonCompound(new JsonKeyValue[] { }));
            //Table.Add(alias, blockAir);

            // Отладочный
            //RegisterBlockClass("Debug", new BlockDebug());

            RegisterModelEntityClass("Chicken");

        }

        /// <summary>
        /// Корректировка блоков после загрузки, если загрузки нет,
        /// всё равно надо, для активации
        /// </summary>
        public static void Correct(CorrectTable correct)
        {
            Ce.ModelEntities = new ModelEntityArrays();
            correct.CorrectRegLoad(Table);
            try
            {
                Ce.ModelEntities = new ModelEntityArrays();
            }
            catch (Exception ex)
            {
                throw ex.InnerException;
            }
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
        }

        /// <summary>
        /// Зврегистрировать сущность
        /// </summary>
        public static void RegisterModelEntityClass(string alias)
        {
            JsonRead jsonRead = new JsonRead(Options.PathEntities + alias + ".bbmodel");
            JsonCompound state;
            List<JsonKeyValue> shapes = new List<JsonKeyValue>();
            if (jsonRead.IsThereFile)
            {
                //state = _ParentState(jsonRead.Compound);
                //if (state.IsKey(Ctb.Variants))
                //{
                //    JsonArray variants = state.GetArray(Ctb.Variants);
                //    bool bodyShape = false;
                //    foreach (JsonCompound variant in variants.Items)
                //    {
                //        string shapeName = variant.GetString(Ctb.Shape);
                //        foreach (JsonKeyValue shape in shapes)
                //        {
                //            if (shape.IsKey(shapeName))
                //            {
                //                bodyShape = true;
                //                break;
                //            }
                //        }
                //        if (bodyShape)
                //        {
                //            bodyShape = false;
                //        }
                //        else
                //        {
                //            shapes.Add(new JsonKeyValue(shapeName, _GetShape(shapeName)));
                //        }
                //    }
                //}
                //if (state.IsKey(Ctb.Variant))
                //{
                //    string shapeName = state.GetString(Ctb.Variant);
                //    shapes.Add(new JsonKeyValue(shapeName, _GetShape(shapeName)));
                //}
            }
            else
            {
                throw new Exception(Sr.GetString(Sr.FileMissingJsonBlock, alias));
            }
            Table.Add(alias, new ModelEntity());

            //_window.LScreen.Process("Block init " + alias);
            //_window.DrawFrame();
        }
    }
}
