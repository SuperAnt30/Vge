using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Vge.Entity.Layer;
using Vge.Entity.List;
using Vge.Entity.Model;
using Vge.Entity.Player;
using Vge.Entity.Shape;
using Vge.Entity.Texture;
using Vge.Json;
using Vge.Renderer;
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
        /// Справочник всех слоёв форм (одежда)
        /// </summary>
        public static readonly List<ShapeLayers> LayerShapes = new List<ShapeLayers>();
        
        /// <summary>
        /// Карта индексов к формам слоёв (одежды) [название, индекс]
        /// </summary>
        private static readonly Dictionary<string, int> _indexLayerShapes = new Dictionary<string, int>();
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
            LayerShapes.Clear();
            _indexLayerShapes.Clear();

            // Вначале регистрация форм слоёв одежды только для рендера
            if (FlagRender)
            {
                RegisterLayerShapeEntityClass("Base");
                RegisterLayerShapeEntityClass("BaseOld");
            }

            // Регистрация обязательных сущностей
            RegisterEntityClass(EntityArrays.AliasPlayer, typeof(PlayerClient));
            RegisterEntityClass("Item", typeof(EntityItem), true);
            // Отладочный
            RegisterEntityClass("Robinson", typeof(EntityThrowable));
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
            foreach (ShapeLayers shapeLayer in LayerShapes)
            {
                shapeLayer.ClearDefinition();
            }
        }

        /// <summary>
        /// Внести в текстуры в память OpenGL в массив текстур.
        /// </summary>
        public static void SetImageTexture2dArray(TextureMap textureMap, uint idTextureSmall, uint idTextureBig)
        {
            for (int i = 0; i < Shapes.Count; i++)
            {
                Shapes[i].SetImageTexture2dArray(textureMap,
                    idTextureSmall, idTextureBig);
            }
            for (int i = 0; i < LayerShapes.Count; i++)
            {
                LayerShapes[i].SetImageTexture2dArray(textureMap,
                    idTextureSmall, idTextureBig);
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

        #region Register...

        /// <summary>
        /// Зарегистрировать слои к сущностям, это одежда, броня
        /// </summary>
        public static void RegisterLayerShapeEntityClass(string alias)
        {
            JsonRead jsonRead = new JsonRead(Options.PathLayerEntities + alias + ".json");

            if (jsonRead.IsThereFile)
            {
                // В файле модели, может находится множество форм и типов одежды
                
                string modelFile = jsonRead.Compound.GetString(Cte.Model);
                if (modelFile == "")
                {
                    // Отсутствует модель в файле json сущности
                    throw new Exception(Sr.GetString(Sr.FileMissingModelJsonEntity, alias));
                }

                ShapeLayers shape = new ShapeLayers((ushort)LayerShapes.Count, modelFile,
                    _ReadGroupLayers(alias, jsonRead.Compound), _GetModel(modelFile));

                LayerShapes.Add(shape);
                _indexLayerShapes.Add(alias, shape.Index);
            }
            else
            {
                // Отсутствует файл json, сущности
                throw new Exception(Sr.GetString(Sr.RequiredParameterIsMissingEntity, alias));
            }
        }

        /// <summary>
        /// Прочесть с json данные слоёв
        /// </summary>
        /// <param name="alias">Имя файла</param>
        /// <param name="compound">объект json</param>
        private static Dictionary<string, GroupLayers> _ReadGroupLayers(string alias, JsonCompound compound)
        {
            Dictionary<string, GroupLayers> map = new Dictionary<string, GroupLayers>();

            if (!compound.IsKey(Cte.Layers))
            {
                // Отсутствует требуемый параметр
                throw new Exception(Sr.GetString(Sr.RequiredParameterIsMissingLayers, alias, Cte.Layers));
            }

            JsonCompound[] layers = compound.GetArray(Cte.Layers).ToArrayObject();
            string group, name;
            string texture;
            string[] folder;
            GroupLayers groupLayers;
            foreach (JsonCompound layer in layers)
            {
                group = layer.GetString(Cte.Group);
                if (group == "")
                {
                    throw new Exception(Sr.GetString(Sr.RequiredParameterIsMissingLayers, alias, Cte.Group));
                }
                name = layer.GetString(Cte.Name);
                if (name == "")
                {
                    throw new Exception(Sr.GetString(Sr.RequiredParameterIsMissingLayers, alias, Cte.Name));
                }
                texture = layer.GetString(Cte.Texture);
                if (texture == "")
                {
                    throw new Exception(Sr.GetString(Sr.RequiredParameterIsMissingLayers, alias, Cte.Texture));
                }
                if (!layer.IsKey(Cte.Folder))
                {
                    throw new Exception(Sr.GetString(Sr.RequiredParameterIsMissingLayers, alias, Cte.Folder));
                }
                folder = layer.GetArray(Cte.Folder).ToArrayString();

                if (map.ContainsKey(group))
                {
                    groupLayers = map[group];
                }
                else
                {
                    groupLayers = new GroupLayers(group);
                    map.Add(group, groupLayers);
                }
                groupLayers.Add(new LayerBuffer(name, texture, folder));
            }

            return map;
        }

        /// <summary>
        /// Зврегистрировать сущность
        /// </summary>
        /// <param name="isItem">Сущность предмета, json не нужен</param>
        public static void RegisterEntityClass(string alias, Type entityType, bool isItem = false)
        {
            if (isItem)
            {
                Table.Add(alias, new ResourcesEntityBase(alias, entityType));
            }
            else
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

        #endregion

        /// <summary>
        /// Запустить текстурный менеджер
        /// </summary>
        public static void TextureManagerRun() => TextureManager.Init();

        /// <summary>
        /// Заменить буфер из-за смены размера текстуры
        /// </summary>
        public static void SetBufferMeshBecauseSizeTexture(ShapeBase shape)
        {
            if (shape is ShapeEntity shapeEntity)
            {
                for (int i = 0; i < Table.Count; i++)
                {
                    Table[i].SetBufferMeshBecauseSizeTexture(shapeEntity);
                }
            }
            // Слоям не надо, буфер будет выдоваться потом по мере надобности
        }

        /// <summary>
        /// Получить слой одежды по названию
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ShapeLayers GetShapeLayers(string name) => LayerShapes[_indexLayerShapes[name]];
    }
}
