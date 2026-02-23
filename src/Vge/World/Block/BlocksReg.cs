using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Vge.Json;
using Vge.Util;
using Vge.World.Block.List;

namespace Vge.World.Block
{
    /// <summary>
    /// Регистрация блоков
    /// </summary>
    public sealed class BlocksReg
    {
        /// <summary>
        /// Псевдоним обязательного блока барьер
        /// </summary>
        // public const string Barrier = "Barrier";

        /// <summary>
        /// Таблица блоков для регистрации
        /// </summary>
        public static readonly BlockRegTable Table = new BlockRegTable();
        /// <summary>
        /// Объект генерации атласа блоков и предметов
        /// </summary>
        public static readonly GeneratingAtlas BlockItemAtlas = new GeneratingAtlas();

        /// <summary>
        /// Справочник всех форм
        /// </summary>
        private static readonly Dictionary<string, JsonCompound> _shapes = new Dictionary<string, JsonCompound>();
        /// <summary>
        /// Справочник родлителей стат блока
        /// </summary>
        private static Dictionary<string, JsonCompound> _parentStats = new Dictionary<string, JsonCompound>();

        /// <summary>
        /// Инициализация блоков, если window не указывать, прорисовки о статусе не будет (для сервера)
        /// </summary>
        public static void Initialization(WindowMain window = null)
        {
            if (window != null)
            {
                window.LScreen.Process(L.T("CreateBlocks"));
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
            BlockItemAtlas.CreateImage();

            // Очистить таблицы и вспомогательные данные json
            _Clear();
        }

        /// <summary>
        /// Инициализация атласа блоков и предметов, после инициализации блоков и предметов
        /// </summary>
        public static void InitializationAtlas(WindowMain window) 
            => BlockItemAtlas.EndImage(window.Render.Texture);

        /// <summary>
        /// Корректировка блоков после загрузки, если загрузки нет,
        /// всё равно надо, для активации
        /// </summary>
        public static void Correct(CorrectTable correct)
        {
            //Ce.Blocks = new BlockArrays(); // 2025-06-04 Не помню почему так было, но бывали нал, возможно до создание мира
            correct.CorrectRegLoad(Table);
            Ce.Blocks = new BlockArrays();

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
        /// Зарегистрировать блок
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BlockBase RegisterBlockClass(string alias, IMaterial material, string path = "")
        {
            BlockBase block = new BlockBase(material);
            RegisterBlockClass(alias, block, path);
            return block;
        }

        /// <summary>
        /// Зарегистрировать полупрозрачными блок
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BlockAlpha RegisterBlockClassAlpha(string alias, IMaterial material)
        {
            BlockAlpha block = new BlockAlpha(material);
            RegisterBlockClass(alias, block);
            return block;
        }

        /// <summary>
        /// Зарегистрировать блок жидкости
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BlockLiquid RegisterBlockClassLiquid(string alias, IMaterial material, bool alphaSort)
        {
            BlockLiquid block = new BlockLiquid(material, alphaSort);
            RegisterBlockClass(alias, block);
            return block;
        }

        /// <summary>
        /// Зарегистрировать блок
        /// </summary>
        public static void RegisterBlockClass(string alias, BlockBase blockObject, string path = "")
        {
            if (path != "") path += Path.DirectorySeparatorChar;
            JsonRead jsonRead = new JsonRead(Options.PathBlocks + path + alias + ".json");
            
            if (jsonRead.IsThereFile)
            {
                JsonCompound state = _ParentState(jsonRead.Compound);
                List<JsonKeyValue> shapes = new List<JsonKeyValue>();
                if (state.IsKey(Ctb.Variants))
                {
                    JsonArray variants = state.GetArray(Ctb.Variants);
                    foreach (JsonCompound variant in variants.Items)
                    {
                        _ShapeAdd(variant.GetString(Ctb.Shape), shapes);
                    }
                }
                if (state.IsKey(Ctb.Liquid))
                {
                    string shapeName = state.GetString(Ctb.Liquid);
                    shapes.Add(new JsonKeyValue(shapeName, GetShape(shapeName)));
                }
                blockObject.InitAliasAndJoinN1(alias, state, new JsonCompound(shapes.ToArray()));
                Table.Add(alias, blockObject);
            }
            else
            {
                throw new Exception(Sr.GetString(Sr.FileMissingJsonBlock, alias));
            }
            
            //_window.LScreen.Process("Block init " + alias);
            //_window.DrawFrame();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void _ShapeAdd(string shapeName, List<JsonKeyValue> shapes)
        {
            foreach (JsonKeyValue shape in shapes)
            {
                if (shape.IsKey(shapeName))
                {
                    return;
                }
            }
            shapes.Add(new JsonKeyValue(shapeName, GetShape(shapeName)));
        }

        #region Shape

        /// <summary>
        /// Получить фигуру из кэша или сначала добавив в кэш и потом получим
        /// </summary>
        public static JsonCompound GetShape(string name)
        {
            if(name != "")
            {
                if (_shapes.ContainsKey(name))
                {
                    return _shapes[name];
                }
                // Добавляем фигуру
                JsonRead jsonRead = new JsonRead(Options.PathShapeBlocks + name + ".json");
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
                    JsonRead jsonRead = new JsonRead(Options.PathShapeBlocks + parent + ".json");
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
                    JsonRead jsonRead = new JsonRead(Options.PathBlocks + parent + ".json");
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
