using System;
using Vge.Entity.Model;
using Vge.Json;
using Vge.Util;
using WinGL.Util;

namespace Vge.Entity
{
    /// <summary>
    /// Модель сущности, нужна для рендера и анимации
    /// На сервере будет упрощённая
    /// </summary>
    public class ModelEntity
    {
        /// <summary>
        /// Буфер для шейдера матриц скелетной анимации, на 24 кости (матрица 4*3 и 24 кости)
        /// </summary>
        // private static ListFlout _buffer = new ListFlout(288);
        private static float[] _buffer = new float[288];

        /// <summary>
        /// Название модели
        /// </summary>
        public readonly string Alias;
        /// <summary>
        /// Буфер сетки моба, для рендера
        /// </summary>
        public float[] BufferMesh { get; private set; }
        /// <summary>
        /// Текстуры для моба
        /// </summary>
        public BufferedImage[] Textures { get; private set; }
        /// <summary>
        /// Древо костей, для скелетной анимации
        /// </summary>
        public Bone[] Bones { get; private set; }

        /// <summary>
        /// Название кости меняющее от Pitch
        /// </summary>
        private string _nameBonePitch;

        private int _index;
        private float _pitch;
        private Mat4 _matrix;

        public ModelEntity(string alias) => Alias = alias;

        /// <summary>
        /// Генерация матриц
        /// </summary>
        public float[] GenMatrix(float yaw, float pitch)
        {
            //_buffer.Clear();
            _index = 0;
            _pitch = pitch;

            _matrix = Mat4.Identity();

            if (yaw != 0)
            {
                _matrix = Glm.Rotate(_matrix, -yaw, new Vector3(0, 1, 0));
            }

            _GenBoneMatrix(Bones, _matrix);

            // Остатки пустышки
            //_matrix = Mat4.Identity();
            //for (int i = _index; i < 24; i++)
            //{
            //    Buffer.BlockCopy(_matrix.ToArray4x3(), 0, _buffer,
            //        _index * 48, 48);
            //    // _buffer.AddRange(_matrix.ToArray4x3());
            //}

            return _buffer;//.ToArray();
        }

        /// <summary>
        /// Конверт в древо костей сущности для игры
        /// </summary>
        private void _GenBoneMatrix(Bone[] bones, Mat4 m)
        {
            int count = bones.Length;
            for (int i = 0; i < count; i++)
            {
                Mat4 m2 = new Mat4(m);

                Bone bone = bones[i];
                if (bone.IsPitch && _pitch != 0)
                {
                    m2[3] = m2[0] * bone.OriginX + m2[1] * bone.OriginY + m2[2] * bone.OriginZ + m2[3];
                    m2 = Glm.Rotate(m2, _pitch, new Vector3(1, 0, 0));
                    m2[3] = m2[0] * -bone.OriginX + m2[1] * -bone.OriginY + m2[2] * -bone.OriginZ + m2[3];
                    //m2 = Glm.Translate(m2, bone.OriginX, bone.OriginY, bone.OriginZ);
                    //m2 = Glm.Rotate(m2, _pitch, new Vector3(1, 0, 0));
                    //m2 = Glm.Translate(m2, -bone.OriginX, -bone.OriginY, -bone.OriginZ);
                }

                Buffer.BlockCopy(m2.ToArray4x3(), 0, _buffer,
                    _index * 48, 48);

                //_buffer.AddRange(m2.ToArray4x3());
                _index++;
                if (bone.Children.Length > 0)
                {
                    _GenBoneMatrix(bone.Children, m2);
                }
            }
        }

        #region Методы для импорта данных с json

        /// <summary>
        /// Прочесть состояние из Json формы
        /// </summary>
        public void ReadStateFromJson(JsonCompound state)
        {
            if (state.Items != null)
            {
                try
                {
                    // Статы
                    foreach (JsonKeyValue json in state.Items)
                    {
                        if (json.IsKey(Cte.Pitch)) _nameBonePitch = json.GetString();
                        //if (json.IsKey(Ctb.LightValue)) LightValue = (byte)json.GetInt();
                        //if (json.IsKey(Ctb.Translucent)) Translucent = json.GetBool();
                        //if (json.IsKey(Ctb.UseNeighborBrightness)) UseNeighborBrightness = json.GetBool();
                        //if (json.IsKey(Ctb.АmbientOcclusion)) АmbientOcclusion = json.GetBool();
                        //if (json.IsKey(Ctb.BiomeColor)) BiomeColor = json.GetBool();
                        //if (json.IsKey(Ctb.Shadow)) Shadow = json.GetBool();
                        //if (json.IsKey(Ctb.Color))
                        //{
                        //    float[] ar = json.GetArray().ToArrayFloat();
                        //    Color = new Vector3(ar[0], ar[1], ar[2]);
                        //}
                    }
                }
                catch
                {
                    throw new Exception(Sr.GetString(Sr.ErrorReadJsonEntityStat, Alias));
                }
            }
        }

        /// <summary>
        /// Прочесть состояние из Json формы и модель
        /// </summary>
        public void ReadStateFromJson(JsonCompound state, JsonCompound model)
        {
            ReadStateFromJson(state);

            ModelEntityDefinition definition = new ModelEntityDefinition(Alias, _nameBonePitch);
            definition.RunModelFromJson(model);

            BufferMesh = definition.BufferMesh;
            Textures = definition.Textures;
            Bones = definition.GenBones();

            return;
        }

        #endregion
    }
}
