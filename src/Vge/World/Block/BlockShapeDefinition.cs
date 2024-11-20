using System;
using System.Collections.Generic;
using Vge.Json;
using Vge.Util;

namespace Vge.World.Block
{
    /// <summary>
    /// Объект отвечает за определяение формы блока
    /// </summary>
    public class BlockShapeDefinition
    {
        private readonly BlockBase _block;

        /// <summary>
        /// Цвет биома, где 0 - нет цвета, 1 - трава, 2 - листа, 3 - вода, 4 - свой цвет
        /// </summary>
        public byte BiomeColor = 0;

        public BlockShapeDefinition(BlockBase block) => _block = block;

        /// <summary>
        /// Запуск определения формы
        /// </summary>
        /// <param name="state"></param>
        /// <param name="shapes"></param>
        public QuadSide[][] RunShapeFromJson(JsonCompound state, JsonCompound shapes)
        {
            string log = "Variants";
            try
            {
                JsonCompound[] variants = state.GetArray("Variants").ToArrayObject();
                if (variants.Length > 0)
                {
                    QuadSide[][] quads = new QuadSide[variants.Length][];
                    int index = 0;
                    int i, j, k;
                    string nameShape = "";
                    JsonCompound shape;
                    JsonCompound[] elements;
                    JsonCompound texture;
                    Dictionary<string, int> textures = new Dictionary<string, int>();
                    JsonCompound[] faces;
                    JsonCompound face;
                    QuadSide quadSide;
                    Pole pole;
                    int x1i, y1i, z1i, x2i, y2i, z2i, u1, v1, u2, v2, wind, rotateY, uvRotate;
                    byte biomeColor;
                    int[] arInt;
                    bool shade, isRotate, sharpness, isOffset, uvLock;
                    float[] ypr = new float[0];
                    float[] offset = new float[0];

                    foreach (JsonCompound variant in variants)
                    {
                        log = "Shape";
                        nameShape = variant.GetString("Shape");
                        if (nameShape != "")
                        {
                            isOffset = variant.IsKey("Offset");
                            if (isOffset)
                            {
                                offset = variant.GetArray("Offset").ToArrayFloat();
                            }
                            // Имеется вращение по Y 90 | 180 | 270
                            rotateY = variant.GetInt("RotateY");
                            // Защита от вращении текстуры
                            uvLock = variant.GetBool("UvLock");
                            // Имеется форма
                            shape = shapes.GetObject(nameShape);

                            log = "Texture";
                            // Текстура
                            texture = shape.GetObject("Texture");
                            textures.Clear();
                            foreach (JsonKeyValue texutreKV in texture.Items)
                            {
                                textures.Add(texutreKV.Key, texutreKV.GetInt());
                            }
                            log = "Elements";
                            elements = shape.GetArray("Elements").ToArrayObject();

                            log = "FacesCount";
                            // Определяем количество квадов
                            k = 0;
                            for (i = 0; i < elements.Length; i++)
                            {
                                k += elements[i].GetArray("Faces").GetCount();
                            }
                            quads[index] = new QuadSide[k];
                            k = 0;
                            // Заполняем квады
                            for (i = 0; i < elements.Length; i++)
                            {
                                // Определяем размер
                                log = "FromTo";
                                arInt = elements[i].GetArray("From").ToArrayInt();
                                x1i = arInt[0];
                                y1i = arInt[1];
                                z1i = arInt[2];
                                arInt = elements[i].GetArray("To").ToArrayInt();
                                x2i = arInt[0];
                                y2i = arInt[1];
                                z2i = arInt[2];

                                // Отсутствие оттенка, т.е. зависит от стороны света, если true оттенка нет
                                shade = elements[i].GetBool("Shade");
                                // Контраст
                                sharpness = elements[i].IsKey("Sharpness");
                                // Ветер
                                wind = elements[i].GetInt("Wind");
                                // Вращение
                                log = "Rotate";
                                isRotate = elements[i].IsKey("Rotate");
                                if (isRotate)
                                {
                                    ypr = elements[i].GetArray("Rotate").ToArrayFloat();
                                }
                                // Собираем массив сторон
                                log = "Faces";
                                faces = elements[i].GetArray("Faces").ToArrayObject();
                                for (j = 0; j < faces.Length; j++)
                                {
                                    face = faces[j];
                                    biomeColor = (byte)face.GetInt("BiomeColor");
                                    if (biomeColor != 0)
                                    {
                                        BiomeColor = biomeColor;
                                    }
                                    quadSide = new QuadSide(biomeColor);
                                    
                                    pole = PoleConvert.GetPole(face.GetString("Side"));
                                    quadSide.SetSide(pole, shade, x1i, y1i, z1i, x2i, y2i, z2i);
                                    if (isRotate)
                                    {
                                        quadSide.SetRotate(ypr[0], ypr[1], ypr[2]);
                                    }
                                    if (isOffset)
                                    {
                                        quadSide.SetTranslate(offset[0] / 16f, offset[1] / 16f, offset[2] / 16f);
                                    }

                                    // Резкость
                                    if (sharpness)
                                    {
                                        quadSide.SetSharpness();
                                    }
                                    // Ветер
                                    if (wind != 0)
                                    {
                                        quadSide.SetWind((byte)wind);
                                    }
                                    // Размеры текстуры
                                    if (face.IsKey("Uv"))
                                    {
                                        arInt = face.GetArray("Uv").ToArrayInt();
                                        u1 = arInt[0];
                                        v1 = arInt[1];
                                        u2 = arInt[2];
                                        v2 = arInt[3];
                                    }
                                    else
                                    {
                                        u1 = v1 = 0;
                                        u2 = v2 = 16;
                                    }
                                    // Вращение текстуры 0 || 90 || 180 || 270
                                    uvRotate = face.GetInt("UvRotate");

                                    if (rotateY != 0)
                                    {
                                        quadSide.SetRotateY(rotateY, shade);

                                        if (pole == Pole.Up && uvLock)
                                        {
                                            uvRotate += rotateY;
                                            if (uvRotate > 270) uvRotate -= 360;
                                            if (rotateY == 90 || rotateY == 270)
                                            {
                                                int uv = v1;
                                                v1 = u1;
                                                u1 = uv;
                                                uv = v2;
                                                v2 = u2;
                                                u2 = uv;
                                            }
                                        }
                                    }

                                    quadSide.SetTexture(textures[face.GetString("Texture")],
                                        u1, v1, u2, v2, uvRotate);

                                    quads[index][k++] = quadSide;
                                }
                            }
                            index++;
                        }
                    }

                    return quads;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(Sr.GetString(Sr.ErrorReadJsonBlockShape, _block.Alias, log), ex);
            }

            return new QuadSide[][] { new QuadSide[] { new QuadSide() } };
        }
    }
}
