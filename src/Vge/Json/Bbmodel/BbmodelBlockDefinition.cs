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

        private readonly string _prefix;

        public BbmodelBlockDefinition(bool isBlock)
            => _prefix = isBlock ? "Blocks" : "Items";

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
                if (name.Length > 3 && name.Substring(name.Length - 4, 4).ToLower() == ".png")
                {
                    name = name.Substring(0, name.Length - 4);
                }
                _textures.Add(name);
                _textOut += T + T + "T" + i + ": \"" + _prefix + "/" + name + "\""
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
            for (int i = 0; i < count; i++)
            {
                _log = "ElementFromTo";
                _textOut += T + T + "{   From: " + _ToVec(elements[i].GetArray(Ctbb.From).ToArrayFloat()) + ",\r\n";
                _textOut += T + T + T + "To: " + _ToVec(elements[i].GetArray(Ctbb.To).ToArrayFloat()) + ",\r\n";

                if (elements[i].IsKey(Ctbb.Rotation))
                {
                    _log = Ctbb.Rotation;
                    _textOut += T + T + T + "Rotate: " + _ToVec(elements[i].GetArray(Ctbb.Rotation).ToArrayFloat()) + ",\r\n";
                    if (elements[i].IsKey(Ctbb.Origin))
                    {
                        _log = Ctbb.Origin;
                        _textOut += T + T + T + "Origin: " + _ToVec(elements[i].GetArray(Ctbb.Origin).ToArrayFloat()) + ",\r\n";
                    }
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
                    float[] ar = face.GetArray(Ctbb.Uv).ToArrayFloat();
                    if (ar[0] != 0 || ar[1] != 0 || ar[2] != 16 || ar[3] != 16)
                    {
                        _textOut += ", Uv:[" + _ToFloat(ar[0]) + ", " + _ToFloat(ar[1]) + ", "
                             + _ToFloat(ar[2]) + ", " + _ToFloat(ar[3]) + "]";
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

        private string _ToVec(float[] vec) => "[" + _ToFloat(vec[0]) + ", " 
            + _ToFloat(vec[1]) + ", " + _ToFloat(vec[2]) + "]";

    }
}
