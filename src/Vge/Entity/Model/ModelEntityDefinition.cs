using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Vge.Entity.Animation;
using Vge.Entity.Render;
using Vge.Json;
using Vge.Util;
using static Vge.Entity.Model.ModelBone;

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
        public const string NameBoneHead = "Head";
        /// <summary>
        /// Название куба открытых глаз 
        /// </summary>
        public const string NameCubeEyeOn = "eyeOpen";
        /// <summary>
        /// Название куба закрытых глаз
        /// </summary>
        public const string NameCubeEyeOff = "eyeClose";

        /// <summary>
        /// Псевдоним
        /// </summary>
        protected readonly string _alias;
        /// <summary>
        /// Список кубов
        /// </summary>
        protected readonly List<ModelCube> _cubes = new List<ModelCube>();
        /// <summary>
        /// Дерево костей они же папки
        /// </summary>
        protected readonly List<ModelElement> _treeBones = new List<ModelElement>();
        /// <summary>
        /// Карта костей, по индексам
        /// Имя начинающее на "_" не может быть костью, а является просто папкой для группировки только одежды
        /// </summary>
        private readonly Dictionary<string, ModelElement> _mapBones = new Dictionary<string, ModelElement>();
        /// <summary>
        /// Список анимаций
        /// </summary>
        private readonly List<ModelAnimation> _animations = new List<ModelAnimation>();
        /// <summary>
        /// Текстуры для моба
        /// </summary>
        protected readonly List<ModelTexture> _textures = new List<ModelTexture>();

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
                _Outliner(model.GetArray(Cte.Outliner), _treeBones);

                // Перенести кубы с папки префиксом "_" в родительскую папку, чтоб не создавать не нужные кости
                _MoveCubesFromFolderToParent();

                // Определения уровней на кубах
                _LayerDefinitions(null, _treeBones);

                // Удаляем невидемые кубы и не используемые кости
                _ClearVisibleCube(null, _treeBones);

                // После всех корректировок, строим индекксацию костей у кубов
                _cubes.Clear();
                _amountBoneIndex = 0;
                _Indexing(_treeBones, 0);

                // Анимация
                _log = Cte.Animations;
                _Animations(model.GetArray(Cte.Animations).ToArrayObject());
            }
            catch (Exception ex)
            {
                throw new Exception(Sr.GetString(Sr.ErrorReadJsonModelEntity, _alias, _log), ex);
            }
        }

        /// <summary>
        /// Генерируем буфер сетки моба, для рендера
        /// </summary>
        public VertexEntityBuffer GenBufferMesh()
        {
            // Генерируем буффер
            List<float> listFloat = new List<float>();
            List<int> listInt = new List<int>();
            foreach (ModelCube cube in _cubes)
            {
                // Генерация буфера только для сущности, не одежда!
                if (!cube.Layer)
                {
                    cube.GenBuffer(listFloat, listInt);
                }
            }
            return new VertexEntityBuffer(listFloat.ToArray(), listInt.ToArray());
        }

        #region Textures

        /// <summary>
        /// Определяем текстуры
        /// </summary>
        private void _Textures(JsonCompound[] textures)
        {
            for (int i = 0; i < textures.Length; i++)
            {
                if (textures[i].IsKey(Cte.Layers))
                {
                    // Имеются слои
                    _log = Cte.Layers;
                    JsonCompound[] layers = textures[i].GetArray(Cte.Layers).ToArrayObject();
                    foreach (JsonCompound layer in layers)
                    {
                        _TextureImage(layer.GetString(Cte.Name), layer.GetString(Cte.DataUrl));
                    }
                }
                else
                {
                    _log = Cte.Source;
                    _TextureImage("", textures[i].GetString(Cte.Source));
                }
            }
            return;
        }

        /// <summary>
        /// Внести текстуру
        /// </summary>
        private void _TextureImage(string name, string source)
        {
            // TODO::2025-07-06 идея, чтоб не забыл, разделить текстуру на 2 часть, сверху сущность, снизу слои
            // И тут сразу их срезать, при этом вертикаль корректировать UV
            if (source.Length > 22 && source.Substring(0, 22) == "data:image/png;base64,")
            {
                _textures.Add(new ModelTexture(name, BufferedFileImage.FileToBufferedImage(
                    Convert.FromBase64String(source.Substring(22)))));
            }
            else
            {
                throw new Exception(Sr.GetString(Sr.RequiredParameterIsMissingEntity, _alias, _log));
            }
        }

        /// <summary>
        /// Сгенерировать массив текстур
        /// </summary>
        public BufferedImage[] GenTextures()
        {
            // Очитстка не нужных текстур
            _CleaningUpUnnecessaryTextures();

            // Создаём массив текстур
            BufferedImage[] images = new BufferedImage[_textures.Count];
            for (int i = 0; i < images.Length; i++)
            {
                images[i] = _textures[i].Image;
            }
            return images;
        }

        /// <summary>
        /// Очитстка не нужных текстур
        /// </summary>
        protected virtual void _CleaningUpUnnecessaryTextures()
        {
            // Пробегаемся и удаляем не нужные текстуры, которые не используются, в слоях
            int count = _textures.Count - 1;
            for (int i = count; i >= 0; i--)
            {
                if (_textures[i].IsLayer) // Слои нужны только для одежды
                {
                    // Удалить слои текстур не использующие
                    _textures.RemoveAt(i);
                }
            }
        }

        #endregion

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
                    EnumType typeFolder = ModelBone.ConvertPrefix(name.Substring(0, 1));
                    if (typeFolder != EnumType.Bone || !compound.IsKey(Cte.Visibility) || compound.GetBool(Cte.Visibility))
                    {
                        ModelBone bone = new ModelBone(compound.GetString(Cte.Uuid),
                            name, typeFolder);

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
                            _Outliner(children, bone.Children);
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
        /// Определения слоя
        /// </summary>
        private void _LayerDefinitions(ModelBone parent, List<ModelElement> bones)
        {
            int count = bones.Count - 1;
            for (int i = count; i >= 0; i--)
            {
                if (bones[i] is ModelBone modelBone && modelBone.Children.Count > 0)
                {
                    if (modelBone.TypeFolder == EnumType.Layer && parent != null)
                    {
                        foreach (ModelElement child in modelBone.Children)
                        {
                            if (child is ModelCube modelCube)
                            {
                                _SetCubeLayer(modelBone, modelCube);
                                parent.Children.Add(modelCube);
                            }
                        }
                        modelBone.Children.Clear();
                        bones.RemoveAt(i);
                    }
                    else
                    {
                        _LayerDefinitions(modelBone, modelBone.Children);
                    }
                }
            }
        }
       

        /// <summary>
        /// Задать смену уровня
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void _SetCubeLayer(ModelBone modelBoneParent, ModelCube modelCube)
            => modelCube.Layer = true;

        /// <summary>
        /// Очистить невидемые кубы, исключение кубы в папках с префиксом "_"
        /// </summary>
        private void _ClearVisibleCube(ModelBone parent, List<ModelElement> bones)
        {
            int count = bones.Count - 1;
            for (int i = count; i >= 0; i--)
            {
                if (bones[i] is ModelCube modelCube)
                {
                    if (!modelCube.Visible && !modelCube.Layer)
                    {
                        parent.Children.Remove(bones[i]);
                    }
                }
                else if (bones[i] is ModelBone modelBone)
                {
                    _ClearVisibleCube(modelBone, modelBone.Children);
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
        private void _MoveCubesFromFolderToParent()
        {
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
        }

        /// <summary>
        /// Перенести кубы с папки которая не кость, в папку родителя
        /// </summary>
        private int _MoveCubesFromFolderToParent(ModelBone parent, List<ModelElement> bones)
        {
            int countMove = 0;
            int count = bones.Count - 1;
            for (int i = count; i >= 0; i--)
            {
                if (bones[i] is ModelBone modelBone && modelBone.Children.Count > 0)
                {
                    if (modelBone.TypeFolder == EnumType.Folder && parent != null)
                    {
                        parent.Children.AddRange(modelBone.Children);
                        modelBone.Children.Clear();
                        bones.RemoveAt(i);
                        countMove++;
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
                    resultBones[modelBone.BoneIndex] = modelBone.CreateBone(parentIndex, scale);
                }
            }
        }

        #endregion

        #region Animation

        /// <summary>
        /// Собираем анимации
        /// </summary>
        protected virtual void _Animations(JsonCompound[] animations)
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
