using System;
using System.Collections.Generic;
using Vge.Entity.List;
using Vge.Entity.Model;
using Vge.Entity.Player;
using Vge.Entity.Texture;
using Vge.Json;
using Vge.Util;

namespace Vge.Entity
{
    /// <summary>
    /// Регистрация сущностей
    /// </summary>
    public sealed class EntitiesReg
    {
        /// <summary>
        /// Флаг, для рендера, если сервер, установить false.
        /// Для игнорирование моделей и текстур
        /// </summary>
        public static bool FlagRender = true;
        /// <summary>
        /// Таблица моделей сущностей для регистрации
        /// </summary>
        public static readonly EntitiesRegTable Table = new EntitiesRegTable();
        /// <summary>
        /// Текстурный менеджер
        /// </summary>
        public static readonly EntityTextureManager TextureManager = new EntityTextureManager();
        /// <summary>
        /// Список всех форм
        /// </summary>
        public static readonly List<ShapeEntity> Shapes = new List<ShapeEntity>();

        /// <summary>
        /// Справочник всех моделей
        /// </summary>
        private static readonly Dictionary<string, JsonCompound> _jsonModels = new Dictionary<string, JsonCompound>();
        /// <summary>
        /// Справочник всех моделей
        /// </summary>
        private static readonly Dictionary<string, ShapeEntity> _shapes = new Dictionary<string, ShapeEntity>();

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
            // Очистить таблицы и вспомогательные данные json
            _Clear();
            Shapes.Clear();
            // Регистрация обязательных сущностей
            RegisterModelEntityClass(EntityArrays.AliasPlayer, typeof(PlayerClient));
            // Отладочный
            RegisterModelEntityClass("Robinson", typeof(EntityThrowable));
            //RegisterModelEntityClass("Chicken2");
        }

        /// <summary>
        /// Корректировка блоков после загрузки, если загрузки нет,
        /// всё равно надо, для активации
        /// </summary>
        public static void Correct(CorrectTable correct)
        {
            correct.CorrectRegLoad(Table);
            Ce.Entities = new EntityArrays();

            // Очистить таблицы и вспомогательные данные json
            _Clear();
            // Очистить вспомогательные объекты в момент загрузки
            foreach (ShapeEntity shapeEntity in Shapes)
            {
                shapeEntity.ClearDefinition();
            }
        }

        /// <summary>
        /// Очистить таблицы и вспомогательные данные json
        /// </summary>
        private static void _Clear()
        {
            // Очистить массивы регистрации
            Table.Clear();
            _jsonModels.Clear();
            _shapes.Clear();
        }

        /// <summary>
        /// Зврегистрировать сущность
        /// </summary>
        public static void RegisterModelEntityClass(string alias, Type entityType)
        {
            JsonRead jsonRead = new JsonRead(Options.PathEntities + alias + ".json");
            
            if (jsonRead.IsThereFile)
            {
                ShapeEntity shape = null;
                if (FlagRender)
                {
                    string modelFile = jsonRead.Compound.GetString(Cte.Model);
                    if (modelFile == "")
                    {
                        // Отсутствует модель в файле json сущности
                        throw new Exception(Sr.GetString(Sr.FileMissingModelJsonEntity, alias));
                    }
                    if (_shapes.ContainsKey(modelFile))
                    {
                        shape = _shapes[modelFile];
                    }
                    else
                    {
                        shape = new ShapeEntity((ushort)Shapes.Count, modelFile, _GetModel(modelFile));
                        _shapes.Add(modelFile, shape);
                        Shapes.Add(shape);
                    }
                }

                ResourcesEntity modelEntity = new ResourcesEntity(alias, entityType, shape.Index);
                modelEntity.ReadStateFromJson(jsonRead.Compound);
                if (FlagRender)
                {
                    modelEntity.ReadStateClientFromJson(jsonRead.Compound);
                }

                Table.Add(alias, modelEntity);
            }
            else
            {
                // Отсутствует файл json, сущности
                throw new Exception(Sr.GetString(Sr.FileMissingJsonEntity, alias));
            }
        }

        /// <summary>
        /// Получить модель, если уже имеется, если нет, добавить в кеш
        /// </summary>
        private static JsonCompound _GetModel(string name)
        {
            if (_jsonModels.ContainsKey(name))
            {
                return _jsonModels[name];
            }
            // Добавляем фигуру
            JsonRead jsonRead = new JsonRead(Options.PathModelEntities + name + ".bbmodel");
            if (jsonRead.IsThereFile)
            {
                _jsonModels.Add(name, jsonRead.Compound);
                return jsonRead.Compound;
            }
            // Отсутствует файл модели сущности
            throw new Exception(Sr.GetString(Sr.FileMissingModelEntity, name));
        }

        /// <summary>
        /// Запустить текстурный менеджер
        /// </summary>
        public static void TextureManagerRun()
        {
            TextureManager.Init();
        }
        
    }
}
