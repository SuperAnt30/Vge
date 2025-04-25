using System;
using System.Collections.Generic;
using Vge.Entity.Model;
using Vge.Json;
using Vge.Util;
using Vge.World.Block;

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
                JsonCompound[] outliner = model.GetArray(Cte.Outliner).ToArrayObject();
                if (outliner.Length == 0)
                {
                    throw new Exception(Sr.GetString(Sr.RequiredParameterIsMissingEntity, _alias, _log));
                }
                _Outliner(outliner);
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
            List<float> list = new List<float>();

            for (int i = 0; i < elements.Length; i++)
            {
                _log = "NameUuid";
                ModelCube cube = new ModelCube(elements[i].GetString(Cte.Uuid),
                    elements[i].GetString(Cte.Name), _width, _height);

                _log = "FromTo";
                cube.SetPosition(
                    elements[i].GetArray(Cte.From).ToArrayFloat(),
                    elements[i].GetArray(Cte.To).ToArrayFloat()
                    );
                if (elements[i].IsKey(Cte.Rotation))
                {
                    _log = "RotationOrigin";
                    cube.SetRotation(
                        elements[i].GetArray(Cte.Rotation).ToArrayFloat(),
                        elements[i].GetArray(Cte.Origin).ToArrayFloat()
                    );
                }

                _log = Cte.Faces;
                JsonCompound faces = elements[i].GetObject(Cte.Faces);

                // Собираем 6 сторон текстур
                _Face(cube, faces, Pole.North, Cte.North);
                _Face(cube, faces, Pole.South, Cte.South);
                _Face(cube, faces, Pole.West, Cte.West);
                _Face(cube, faces, Pole.East, Cte.East);
                _Face(cube, faces, Pole.Up, Cte.Up);
                _Face(cube, faces, Pole.Down, Cte.Down);

                cube.GenBuffer(list);

                _cubes.Add(cube);
            }

            Buffer = list.ToArray();
        }

        private void _Face(ModelCube cube, JsonCompound faces, Pole side, string key)
        {
            _log = key;
            cube.Faces[(int)side] = new ModelFace(side, faces.GetObject(key).GetArray(Cte.Uv).ToArrayFloat());
        }

        /// <summary>
        /// Строим древо скелета
        /// </summary>
        private void _Outliner(JsonCompound[] outliner)
        {

        }




        /// <summary>
        /// Прямоугольный параллелепипед
        /// </summary>
        public static void _Cube(List<float> list, int x1, int y1, int z1,
            int x2, int y2, int z2,
            int numberTexture, float r, float g, float b, float id)
        {
            QuadSide quadSide;
            for (int i = 0; i < 6; i++)
            {
                quadSide = new QuadSide();
                quadSide.SetSide((Pole)i, true, x1, y1, z1, x2, y2, z2);
                quadSide.SetTexture(numberTexture);

                for (int j = 0; j < 4; j++)
                {
                    list.AddRange(new float[] {
                        quadSide.Vertex[j].X, quadSide.Vertex[j].Y, quadSide.Vertex[j].Z,
                        quadSide.Vertex[j].U, quadSide.Vertex[j].V,
                        r, g, b, 1, id
                    });
                }
            }
        }
    }
}
