using System.Collections.Generic;
using System.Globalization;
using WinGL.Util;

namespace Vge.Json.Bbmodel
{
    /// <summary>
    /// Объект отвечает за определяение модели с Blockbanch блока
    /// Используем Модель для Minecraft BE
    /// </summary>
    public class BbmodelBlockDefinition
    {
        /// <summary>
        /// Табуляция, 4 пробела
        /// </summary>
        public const string T = "    ";

        /// <summary>
        /// Имеется ли ошибка
        /// </summary>
        public bool IsError { get; private set; }

        /// <summary>
        /// Текст результата
        /// </summary>
        private string _textOut;
        /// <summary>
        /// Для краша, название раздела
        /// </summary>
        private string _log;
        /// <summary>
        /// Список текстур файлов
        /// </summary>
        private List<string> _textures = new List<string>();

        public BbmodelBlockDefinition() { }

        public string Convert(string textIn)
        {
            _textOut = "";
            JsonRead jsonRead = new JsonRead("Bbmodel", textIn);
            if (jsonRead.IsThereFile)
            {
                JsonCompound model = jsonRead.Compound;
                _RunModelFromJson(model);
            }
            
            return _textOut;
        }

        /// <summary>
        /// Задать ошибку
        /// </summary>
        private void _Error(string error)
        {
            _textOut = "ERROR: " + error + "\r\n---\r\n" + _textOut;
            IsError = true;
        }
            

        /// <summary>
        /// Запуск определения модели
        /// </summary>
        private void _RunModelFromJson(JsonCompound model)
        {
            try
            {
                _textOut = "{\r\n";

                // Массив текстур
                _log = Ctbb.Textures;
                JsonCompound[] textures = model.GetArray(Ctbb.Textures).ToArrayObject();
                if (textures.Length == 0)
                {
                    _Error(Sr.GetString(Sr.RequiredParameterIsMissing, Ctbb.Textures));
                    return;
                }
                _Textures(textures);


                _log = Ctbb.Elements;
                JsonCompound[] elements = model.GetArray(Ctbb.Elements).ToArrayObject();
                if (elements.Length == 0)
                {
                    _Error(Sr.GetString(Sr.RequiredParameterIsMissing, Ctbb.Elements));
                    return;
                }

                // Определяем кубы
                _Elements(elements);
            }
            catch
            {
                _Error(Sr.GetString(Sr.ErrorReadFileModel, _log));
                return;
            }
            
            _textOut += "}";

        }

        /// <summary>
        /// Определяем текстуры
        /// </summary>
        private void _Textures(JsonCompound[] textures)
        {
            _textOut += T + "Texture: {\r\n";
            int count = textures.Length;
            int countEnd = textures.Length - 1;
            for (int i = 0; i < count; i++)
            {
                string name = textures[i].GetString(Ctbb.Name);
                _textures.Add(name);
                _textOut += T + T + "T" + i + ": \"Blocks/" + name + "\""
                     + (i == countEnd ? "\r\n" : ",\r\n");
            }
            _textOut += T + "},\r\n";
        }

        /// <summary>
        /// Определяем кубы
        /// </summary>
        private void _Elements(JsonCompound[] elements)
        {
            _textOut += T + "Elements: [\r\n";
            int count = elements.Length;
            int countEnd = elements.Length - 1;
            int[] ivs;
            float[] fvs;
            for (int i = 0; i < count; i++)
            {
                _log = "ElementFromTo";
                ivs = elements[i].GetArray(Ctbb.From).ToArrayInt();
                _textOut += T + T + "{   From: [" + ivs[0] + ", " + ivs[1] + ", " 
                    + ivs[2] + "],\r\n";
                ivs = elements[i].GetArray(Ctbb.To).ToArrayInt();
                _textOut += T + T + T + "To: [" + ivs[0] + ", " + ivs[1] + ", " 
                    + ivs[2] + "],\r\n";

                if (elements[i].IsKey(Ctbb.Rotation))
                {
                    _log = Ctbb.Rotation;
                    fvs = elements[i].GetArray(Ctbb.Rotation).ToArrayFloat();
                    _textOut += T + T + T + "Rotate: [" + _ToFloat(fvs[0]) + ", "
                        + _ToFloat(fvs[1]) + ", " + _ToFloat(fvs[2]) + "],\r\n";
                }

                _Faces(elements[i]);
                _textOut += T + T + "}" + (i == countEnd ? "\r\n" : ",\r\n");
            }
            _textOut += T + "]\r\n";
        }


        private void _Faces(JsonCompound element)
        {
            _log = Ctbb.Faces;
            JsonCompound faces = element.GetObject(Ctbb.Faces);
            int count = faces.Items.Length;
            if (count > 0)
            {
                _textOut += T + T + T + "Faces: [\r\n";
                bool one = true;
                for (int i = 0; i < count; i++)
                {
                    if (faces.Items[i].Value.GetIdType() == 2)
                    {
                        one = _Face(faces.Items[i].Key, faces.Items[i].GetObjects(), one);
                    }
                }
                if (!one) _textOut += "\r\n";
                _textOut += T + T + T + "]\r\n";
            }
        }

        private bool _Face(string side, JsonCompound face, bool one)
        {
            if (face.GetString(Ctbb.Texture) != "null")
            {
                if (one) one = false;
                else _textOut += ",\r\n";

                int textureId = face.GetInt(Ctbb.Texture);

                _textOut += T + T + T + T + "{ Texture:\"T" + textureId  + "\", "
                    + "Side:\"" + _ToUpper(side) + "\"";
                if (face.IsKey(Ctbb.Uv))
                {
                    int[] arInt = face.GetArray(Ctbb.Uv).ToArrayInt();
                    if (arInt[0] != 0 || arInt[1] != 0 || arInt[2] != 16 || arInt[3] != 16)
                    {
                        _textOut += ", Uv:[" + arInt[0] + "," + arInt[1] + ","
                             + arInt[2] + "," + arInt[3] + "]";
                    }
                }
                    _textOut += "}";
            }
            return one;
        }

        /// <summary>
        /// Первая заглавная буква
        /// </summary>
        private string _ToUpper(string str)
        {
            if (str.Length > 0)
            {
                str = str[0].ToString().ToUpper() + str.Substring(1);
            }
            return str;
        }

        private string _ToFloat(float value)
            => Mth.Round(value, 1).ToString(CultureInfo.InvariantCulture);

    }
}
