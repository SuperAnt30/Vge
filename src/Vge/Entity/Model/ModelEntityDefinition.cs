using System;
using System.Collections.Generic;
using Vge.Entity.Animation;
using Vge.Entity.Render;
using Vge.Json;
using Vge.Util;

namespace Vge.Entity.Model
{
    /// <summary>
    /// Объект отвечает за определяение модели сущности
    /// </summary>
    public class ModelEntityDefinition
    {
        /// <summary>
        /// Название кости меняющее от Pitch, голова
        /// </summary>
        private const string _nameBoneHead = "Head";

        /// <summary>
        /// Буфер сетки моба, для рендера
        /// </summary>
        public VertexEntityBuffer BufferMesh { get; private set; }
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
        /// Дерево костей они же папки
        /// </summary>
        private readonly List<ModelElement> _treeBones = new List<ModelElement>();
        /// <summary>
        /// Карта костей, по индексам
        /// </summary>
        private readonly Dictionary<string, ModelElement> _mapBones = new Dictionary<string, ModelElement>();
        /// <summary>
        /// Список анимаций
        /// </summary>
        private readonly List<ModelAnimation> _animations = new List<ModelAnimation>();

        /// <summary>
        /// Счётчик индекс кости, для шейдора и не только
        /// </summary>
        private byte _amountBoneIndex = 0;
        /// <summary>
        /// Счётчик очерёдности кубов
        /// </summary>
        private byte _cubeIndex;
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

        public ModelEntityDefinition(string alias)
        {
            _alias = alias;
        }

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
                _Outliner(model.GetArray(Cte.Outliner), _treeBones);//, _amountBoneIndex);

                // Определения уровня на кубах
                _LevelDefinitions(_treeBones);

                // Удаляем невидемые кубы
                _ClearVisibleCube(_treeBones, null);

                // Перенести кубы с папки префиксом "_" в родительскую папку, чтоб не создавать не нужные кости
                byte byteTree = 0;
                while (_MoveCubesFromFolderToParent(null, _treeBones) > 0)
                {
                    if (++byteTree >= 8)
                    {
                        // Защита от рекурсии
                        // Переносящая вложенная папка не должна быть глубже 8.
                        throw new Exception(Sr.GetString(Sr.TransferNestedFolderMustNotBeDeeperThan, 8, _log));
                    }
                }

                // После всех корректировок, строим индекксацию костей и кубов
                _cubes.Clear();
                _amountBoneIndex = 0;
                _Indexing(_treeBones, 0);

                // Анимация
                _log = Cte.Animations;
                _Animations(model.GetArray(Cte.Animations).ToArrayObject());

                // Генерируем буффер
                List<float> listFloat = new List<float>();
                List<int> listInt = new List<int>();
                foreach (ModelCube cube in _cubes)
                {
                    // Генерация буфера
                    if (cube.Layer == 0)
                   // if (cube.Name.Substring(0, 1) != "_")
                        //&& cube.Name != "head")
                    {
                        cube.GenBuffer(listFloat, listInt);
                    }
                }

                BufferMesh = new VertexEntityBuffer(listFloat.ToArray(), listInt.ToArray());
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
                    elements[i].GetString(Cte.Name), _width, _height, 
                    !elements[i].IsKey(Cte.Visibility) || elements[i].GetBool(Cte.Visibility));

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
        private void _Outliner(JsonArray outliner, List<ModelElement> bones)//, byte boneIndex)
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
                        //cube.BoneIndex = boneIndex;
                        cube.Index = _cubeIndex++;
                        bones.Add(cube);
                    }
                }
                else
                {
                    // Объект, значит папка
                    JsonCompound compound = outliner.GetCompound(i);
                    _log = "BoneNameUuid";
                    string name = compound.GetString(Cte.Name);
                    bool notBone = name.Substring(0, 1) == "_";
                    if (notBone || !compound.IsKey(Cte.Visibility) || compound.GetBool(Cte.Visibility))
                    {
                        ModelBone bone = new ModelBone(compound.GetString(Cte.Uuid),
                            name, notBone);//, _amountBoneIndex++);

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
                            _Outliner(children, bone.Children);//, bone.BoneIndex);
                        }

                        bones.Add(bone);
                        _mapBones.Add(bone.Name, bone);
                    }
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

        #region Корректировки древа костей с кубами

        /// <summary>
        /// Определения уровня
        /// </summary>
        private void _LevelDefinitions(List<ModelElement> bones)
        {
            foreach (ModelElement element in bones)
            {
                if (element is ModelCube modelCube)
                {
                    if (modelCube.Name.Substring(0, 1) == "_")
                    {
                        modelCube.Layer = 1;
                    }
                }
                else if (element is ModelBone modelBone)
                {
                    _LevelDefinitions(modelBone.Children);
                }
            }
        }

        /// <summary>
        /// Очистить невидемые кубы, исключение кубы в папках с префиксом "_"
        /// </summary>
        private void _ClearVisibleCube(List<ModelElement> bones, ModelBone parent)
        {
            int count = bones.Count - 1;
            for (int i = count; i >= 0; i--)
            {
                if (bones[i] is ModelCube modelCube)
                {
                    if (!modelCube.Visible && parent != null && !parent.NotBone)
                    {
                        parent.Children.Remove(bones[i]);
                    }
                }
                else if (bones[i] is ModelBone modelBone)
                {
                    _ClearVisibleCube(modelBone.Children, modelBone);
                    if (modelBone.Children.Count == 0)
                    {
                        // Если в кости нет ничего, удаляем папку-кость
                        bones.RemoveAt(i);
                    }
                }
            }
        }

        /// <summary>
        /// Перенести кубы с папки которая не кость, в папку родителя
        /// </summary>
        private int _MoveCubesFromFolderToParent(ModelBone coreBone, List<ModelElement> bones)
        {
            int countMove = 0;
            int count = bones.Count - 1;
            for (int i = count; i >= 0; i--)
            {
                if (bones[i] is ModelBone modelBone && modelBone.Children.Count > 0)
                {
                    if (modelBone.NotBone && coreBone != null)
                    {
                        foreach (ModelElement child in modelBone.Children)
                        {
                            coreBone.Children.Add(child);
                            bones.RemoveAt(i);
                            countMove++;
                        }
                    }
                    else
                    {
                        countMove += _MoveCubesFromFolderToParent(modelBone, modelBone.Children);
                    }
                }
            }
            return countMove;
        }

        /// <summary>
        /// Индексация костей и кубов
        /// </summary>
        private void _Indexing(List<ModelElement> bones, byte parentIndex)
        {
            foreach (ModelElement cube in bones)
            {
                if (cube is ModelBone modelBone)
                {
                    modelBone.BoneIndex = _amountBoneIndex++;
                    _Indexing(modelBone.Children, modelBone.BoneIndex);
                }
                else if (cube is ModelCube modelCube)
                {
                    modelCube.BoneIndex = parentIndex;
                    _cubes.Add(modelCube);
                }
            }
        }

        #endregion

        #region TreeBones

        /// <summary>
        /// Сгенерировать массив костей
        /// </summary>
        public Bone[] GenBones(float scale)
        {
            // Массив костей
            Bone[] resultBones = new Bone[_amountBoneIndex];
            _ConvertTreeBones(resultBones, _treeBones, Bone.RootBoneParentIndex, scale);
            return resultBones;
        }

        /// <summary>
        /// Конверт в древо костей сущности для игры
        /// </summary>
        private void _ConvertTreeBones(Bone[] resultBones, List<ModelElement> modelBones, 
            byte parentIndex, float scale)
        {
            for (int i = 0; i < modelBones.Count; i++)
            {
                if (modelBones[i] is ModelBone modelBone)
                {
                    _ConvertTreeBones(resultBones, modelBone.Children, modelBone.BoneIndex, scale);
                    resultBones[modelBone.BoneIndex] = modelBone.CreateBone(_nameBoneHead, parentIndex, scale);
                }
            }
        }

        #endregion

        #region Animation

        /// <summary>
        /// Собираем анимации
        /// </summary>
        private void _Animations(JsonCompound[] animations)
        {
            for (int i = 0; i < animations.Length; i++)
            {
                _log = "AnimNameLoop";
                string sLoop = animations[i].GetString(Cte.Loop);
                ModelAnimation animation = new ModelAnimation(
                    animations[i].GetString(Cte.Name),
                    sLoop.Equals(Cte.Once) ? ModelLoop.Once
                    : sLoop.Equals(Cte.Hold) ? ModelLoop.Hold : ModelLoop.Loop,
                    animations[i].GetFloat(Cte.Length),
                    animations[i].GetInt(Cte.StartDelay)
                    
               );

                // Массив элементов анимаций
                _log = Cte.Animators;
                JsonCompound animators = animations[i].GetObject(Cte.Animators);
                if (animators.GetCount() == 0)
                {
                    throw new Exception(Sr.GetString(Sr.RequiredParameterIsMissingEntity, _alias, _log));
                }
                _Animators(animation, animators);


                _animations.Add(animation);
            }
        }

        /// <summary>
        /// Собираем анимацию
        /// </summary>
        private void _Animators(ModelAnimation animation, JsonCompound animators)
        {
            int countBone = animators.GetCount();
            // Цикл костей
            for (int i = 0; i < countBone; i++)
            {
                _log = "AnimBone";
                JsonCompound animator = animators.Items[i].GetObjects();
                // Вытаскиваем имя кости
                string nameBone = animator.GetString(Cte.Name);
                if (_mapBones.ContainsKey(nameBone))
                {
                    ModelAnimation.AnimationBone bone = new ModelAnimation.AnimationBone(_mapBones[nameBone].BoneIndex);
                    JsonCompound[] keyframes = animator.GetArray(Cte.Keyframes).ToArrayObject();

                    _log = "AnimKeyFrames";
                    int countFrame = keyframes.Length;
                    for (int j = 0; j < countFrame; j++)
                    {
                        JsonCompound pos = keyframes[j].GetArray(Cte.DataPoints).ToArrayObject()[0];
                        bone.Frames.Add(new ModelAnimation.KeyFrames(
                            keyframes[j].GetString(Cte.Channel).Equals("rotation"),
                            keyframes[j].GetFloat(Cte.Time),
                            pos.GetFloat(Cte.X),
                            pos.GetFloat(Cte.Y),
                            pos.GetFloat(Cte.Z)
                        ));
                    }
                    animation.BoneAdd(bone);
                }
            }
        }

        /// <summary>
        /// Сгенерировать списки модели ключевых кадров для каждой кости скелета
        /// </summary>
        public ModelAnimationClip[] GetModelAnimationClips(AnimationData[] animationDatas)
        {
            // Количество анимационных клипов
            int count1 = animationDatas.Length;
            int count2 = _animations.Count;
            int i1, i2;
            AnimationData animationData;
            List<ModelAnimationClip> list = new List<ModelAnimationClip>();
            for (i1 = 0; i1 < count1; i1++)
            {
                animationData = animationDatas[i1];
                for (i2 = 0; i2 < count2; i2++)
                {
                    if (animationData.Name.Equals(_animations[i2].Name))
                    {
                        list.Add(_animations[i2].CreateModelAnimationClip(_amountBoneIndex, animationData.Speed));
                    }
                }
            }
            return list.ToArray();
        }

        #endregion
    }
}
