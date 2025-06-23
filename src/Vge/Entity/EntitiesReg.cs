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
        /// Справочник всех моделей
        /// </summary>
        public static readonly Dictionary<string, JsonCompound> Models = new Dictionary<string, JsonCompound>();
        /// <summary>
        /// Текстурный менеджер
        /// </summary>
        public static readonly EntityTextureManager TextureManager = new EntityTextureManager(Table);

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
        }

        /// <summary>
        /// Очистить таблицы и вспомогательные данные json
        /// </summary>
        private static void _Clear()
        {
            // Очистить массивы регистрации
            Table.Clear();
            Models.Clear();
        }

        /// <summary>
        /// Зврегистрировать сущность
        /// </summary>
        public static void RegisterModelEntityClass(string alias, Type entityType)
        {
            JsonRead jsonRead = new JsonRead(Options.PathEntities + alias + ".json");
            
            if (jsonRead.IsThereFile)
            {
                ResourcesEntity modelEntity = new ResourcesEntity(alias, entityType);

                if (FlagRender)
                {
                    string modelFile = jsonRead.Compound.GetString(Cte.Model);
                    if (modelFile == "")
                    {
                        // Отсутствует модель в файле json сущности
                        throw new Exception(Sr.GetString(Sr.FileMissingModelJsonEntity, alias));
                    }
                    JsonCompound model = _GetModel(modelFile);
                    modelEntity.ReadStateFromJson(jsonRead.Compound, model);
                }
                else
                {
                    modelEntity.ReadStateFromJson(jsonRead.Compound);
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
            if (Models.ContainsKey(name))
            {
                return Models[name];
            }
            // Добавляем фигуру
            JsonRead jsonRead = new JsonRead(Options.PathModelEntities + name + ".bbmodel");
            if (jsonRead.IsThereFile)
            {
                Models.Add(name, jsonRead.Compound);
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
