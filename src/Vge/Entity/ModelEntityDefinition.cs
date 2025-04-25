using System;
using System.Collections.Generic;
using Vge.Entity.Model;
using Vge.Json;
using Vge.Util;

namespace Vge.Entity
{
    /// <summary>
    /// Объект отвечает за определяение модели сущности
    /// </summary>
    public class ModelEntityDefinition
    {
        /// <summary>
        /// Буфер сетки моба, для рендера
        /// </summary>
        public float[] Buffer { get; private set; }
        /// <summary>
        /// Текстуры для моба
        /// </summary>
        public BufferedImage[] Textures { get; private set; }

        /// <summary>
        /// Псевдоним сущности
        /// </summary>
        private readonly string _alias;
        /// <summary>
        /// Список кубов
        /// </summary>
        private readonly List<ModelCube> _cubes = new List<ModelCube>();
        /// <summary>
        /// Список костей они же папки
        /// </summary>
        private readonly List<ModelElement> _bones = new List<ModelElement>();

        /// <summary>
        /// Для краша, название раздела
        /// </summary>
        private string _log;

        /// <summary>
        /// Ширина
        /// </summary>
        private int _width;
        /// <summary>
        /// Высота
        /// </summary>
        private int _height;
        /// <summary>
        /// Индекс кости, для шейдора
        /// </summary>
        private byte _boneIndex = 0;

        public ModelEntityDefinition(string alias) => _alias = alias;

        /// <summary>
        /// Запуск определения модели
        /// </summary>
        public void RunModelFromJson(JsonCompound model)
        {
            try
            {
                // Разрешения
                _log = Cte.Resolution;
                JsonCompound faces = model.GetObject(Cte.Resolution);
                _width = faces.GetInt(Cte.Width);
                _height = faces.GetInt(Cte.Height);

                // Массив текстур
                _log = Cte.Textures;
                JsonCompound[] textures = model.GetArray(Cte.Textures).ToArrayObject();
                if (textures.Length == 0)
                {
                    throw new Exception(Sr.GetString(Sr.RequiredParameterIsMissingEntity, _alias, _log));
                }
                _Textures(textures);

                // Массив элементов параллелепипедов
                _log = Cte.Elements;
                JsonCompound[] elements = model.GetArray(Cte.Elements).ToArrayObject();
                if (elements.Length == 0)
                {
                    throw new Exception(Sr.GetString(Sr.RequiredParameterIsMissingEntity, _alias, _log));
                }
                _Cubes(elements);

                // Древо скелета
                _log = Cte.Outliner;
                _Outliner(model.GetArray(Cte.Outliner), _bones, _boneIndex);

                // Генерируем буффер
                List<float> list = new List<float>();
                foreach(ModelCube cube in _cubes)
                {
                    cube.GenBuffer(list);
                }
                Buffer = list.ToArray();
                return;
            }
            catch (Exception ex)
            {
                throw new Exception(Sr.GetString(Sr.ErrorReadJsonModelEntity, _alias, _log), ex);
            }
        }

        /// <summary>
        /// Определяем текстуры
        /// </summary>
        private void _Textures(JsonCompound[] textures)
        {
            Textures = new BufferedImage[textures.Length];
            for (int i = 0; i < textures.Length; i++)
            {
                _log = Cte.Source;
                string source = textures[i].GetString(Cte.Source);
                if (source.Length > 22 && source.Substring(0, 22) == "data:image/png;base64,")
                {
                    Textures[i] = BufferedFileImage.FileToBufferedImage(
                        Convert.FromBase64String(source.Substring(22)));
                }
                else
                {
                    throw new Exception(Sr.GetString(Sr.RequiredParameterIsMissingEntity, _alias, _log));
                }
            }
        }

        /// <summary>
        /// Определяем кубы
        /// </summary>
        private void _Cubes(JsonCompound[] elements)
        {
            for (int i = 0; i < elements.Length; i++)
            {
                _log = "CubeNameUuid";
                ModelCube cube = new ModelCube(elements[i].GetString(Cte.Uuid),
                    elements[i].GetString(Cte.Name), _width, _height);

                _log = "CubeFromTo";
                cube.SetPosition(
                    elements[i].GetArray(Cte.From).ToArrayFloat(),
                    elements[i].GetArray(Cte.To).ToArrayFloat()
                    );
                if (elements[i].IsKey(Cte.Rotation))
                {
                    _log = "CubeRotationOrigin";
                    cube.SetRotation(
                        elements[i].GetArray(Cte.Rotation).ToArrayFloat(),
                        elements[i].GetArray(Cte.Origin).ToArrayFloat()
                    );
                }

                _log = Cte.Faces;
                JsonCompound faces = elements[i].GetObject(Cte.Faces);

                // Собираем 6 сторон текстур
                _log = Cte.Up;
                cube.Faces[(int)Pole.Up] = new ModelFace(Pole.Up, 
                    faces.GetObject(Cte.Up).GetArray(Cte.Uv).ToArrayFloat());
                _log = Cte.Down;
                cube.Faces[(int)Pole.Down] = new ModelFace(Pole.Down, 
                    faces.GetObject(Cte.Down).GetArray(Cte.Uv).ToArrayFloat());
                _log = Cte.East;
                cube.Faces[(int)Pole.East] = new ModelFace(Pole.East, 
                    faces.GetObject(Cte.East).GetArray(Cte.Uv).ToArrayFloat());
                _log = Cte.West;
                cube.Faces[(int)Pole.West] = new ModelFace(Pole.West, 
                    faces.GetObject(Cte.West).GetArray(Cte.Uv).ToArrayFloat());
                _log = Cte.North;
                cube.Faces[(int)Pole.North] = new ModelFace(Pole.North, 
                    faces.GetObject(Cte.North).GetArray(Cte.Uv).ToArrayFloat());
                _log = Cte.South;
                cube.Faces[(int)Pole.South] = new ModelFace(Pole.South, 
                    faces.GetObject(Cte.South).GetArray(Cte.Uv).ToArrayFloat());

                _cubes.Add(cube);
            }
        }

        /// <summary>
        /// Строим древо скелета
        /// </summary>
        private void _Outliner(JsonArray outliner, List<ModelElement> bones, byte boneIndex)
        {
            int count = outliner.GetCount();
            for (int i = 0; i < count; i++)
            {
                if (outliner.IsValue(i))
                {
                    // Переменная, значит куб
                    string uuid = outliner.GetValue(i).ToString();
                    ModelCube cube = _FindCune(uuid);
                    if (cube != null)
                    {
                        cube.BoneIndex = boneIndex;
                        bones.Add(cube);
                    }
                }
                else
                {
                    // Объект, значит папка
                    JsonCompound compound = outliner.GetCompound(i);
                    _log = "BoneNameUuid";
                    ModelBone bone = new ModelBone(compound.GetString(Cte.Uuid),
                        compound.GetString(Cte.Name), _boneIndex++);

                    if (compound.IsKey(Cte.Origin))
                    {
                        _log = "BoneRotationOrigin";
                        float[] rotation = compound.IsKey(Cte.Rotation)
                            ? compound.GetArray(Cte.Rotation).ToArrayFloat()
                            : new float[] { 0, 0, 0 };
                        bone.SetRotation(rotation, compound.GetArray(Cte.Origin).ToArrayFloat());
                    }

                    _log = Cte.Children;
                    JsonArray children = compound.GetArray(Cte.Children);
                    if (children.GetCount() > 0)
                    {
                        // Имеются детишки
                        _Outliner(children, bone.Children, bone.BoneIndex);
                    }

                    bones.Add(bone);
                }
            }
        }

        /// <summary>
        /// Найти модель куба, если нет вернуть null
        /// </summary>
        private ModelCube _FindCune(string uuid)
        {
            foreach(ModelCube cube in _cubes)
            {
                if (cube.Uuid.Equals(uuid))
                {
                    return cube;
                }
            }
            return null;
        }

        /// <summary>
        /// Сгенерировать древо костей
        /// </summary>
        public Bone[] GenBones()
        {
            _ClearCubeBone(_bones);
            return _ConvertBones(_bones);
        }

        /// <summary>
        /// Конверт в древо костей сущности для игры
        /// </summary>
        private Bone[] _ConvertBones(List<ModelElement> modelBones)
        {
            int count = modelBones.Count;
            Bone[] bones = new Bone[count];
            for (int i = 0; i < count; i++)
            {
                if (modelBones[i] is ModelBone modelBone)
                {
                    bones[i] = modelBone.CreateBone();
                    bones[i].SetChildren(_ConvertBones(modelBone.Children));
                }
            }
            return bones;
        }

        /// <summary>
        /// Очистить кубы в костях
        /// </summary>
        private void _ClearCubeBone(List<ModelElement> bones)
        {
            int count = bones.Count - 1;
            for (int i = count; i >= 0; i--)
            {
                if (bones[i] is ModelBone modelBone)
                {
                    if (modelBone.Children.Count == 0)
                    {
                        bones.RemoveAt(i);
                    }
                    else
                    {
                        _ClearCubeBone(modelBone.Children);
                    }
                }
                else
                {
                    bones.RemoveAt(i);
                }
            }
        }
    }
}
