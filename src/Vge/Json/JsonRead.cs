using System;
using System.Collections.Generic;
using System.IO;
using Vge.Util;

namespace Vge.Json
{
    /// <summary>
    /// Объект чтения файлов json
    /// </summary>
    public class JsonRead
    {
        /// <summary>
        /// Имеется ли файл
        /// </summary>
        public readonly bool IsThereFile;
        /// <summary>
        /// Результат составного json
        /// </summary>
        public readonly JsonCompound Compound = new JsonCompound();
        /// <summary>
        /// Тело json
        /// </summary>
        private readonly string _body = "";
        /// <summary>
        /// Им файла для ошибки
        /// </summary>
        private readonly string _fileName;
        /// <summary>
        /// Длинна текста тела
        /// </summary>
        private readonly int _finishIndex;
        /// <summary>
        /// Тикущи интекс символа проверки
        /// </summary>
        private int _index;

        public JsonRead(string fileName)
        {
            if (File.Exists(fileName))
            {
                _fileName = fileName;
                // Получить доступ к существующему либо создать новый
                using (StreamReader file = new StreamReader(fileName))
                {
                    while (true)
                    {
                        // Читаем строку из файла во временную переменную.
                        string strLine = file.ReadLine();

                        // Если достигнут конец файла, прерываем считывание.
                        if (strLine == null) break;

                        // комментарий
                        if (Sundry.ChekComment(strLine)) continue;

                        _body += strLine + " ";
                    }
                }
                if (_body != "")
                {
                    _finishIndex = _body.Length;
                    _body += " ";
                    _index = 0;
                    Compound = new JsonCompound(_FindCompound());
                    IsThereFile = true;
                }
            }
        }

        public JsonRead(string name, string text)
        {
            _fileName = name;
            string[] lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            foreach(string strLine in lines)
            {
                // комментарий
                if (Sundry.ChekComment(strLine)) continue;
                _body += strLine + " ";
            }
            if (_body != "")
            {
                _finishIndex = _body.Length;
                _body += " ";
                _index = 0;
                Compound = new JsonCompound(_FindCompound());
                IsThereFile = true;
            }
        }

        /// <summary>
        /// Повторная дикомпрессия, для отладки скорости
        /// </summary>
        public void Debug()
        {
            _index = 0;
            _FindCompound();
        }

        /// <summary>
        /// Поиск зашли в объекте после {, ищем атрибуты у которых есть Key
        /// </summary>
        private JsonKeyValue[] _FindCompound()
        {
            List<JsonKeyValue> list = new List<JsonKeyValue>();
            char c;
            string strKey = "";
            bool quotes = false;
            bool quotesNot = false;
            bool colon = false;
            bool key = false;
            int qi = _index;
            while (_finishIndex >= _index)
            {
                c = _body[_index++];
                if (colon)
                {
                    // Ключ c : найден, ищем какой атрибут
                    if (c == ' ' || c == '\t') continue;

                    if (c == '{')
                    {
                        // Объект
                        list.Add(new JsonKeyValue(strKey, new JsonCompound(_FindCompound())));
                        colon = false;
                    }
                    else if (c == '[')
                    {
                        // Массив
                        list.Add(new JsonKeyValue(strKey, new JsonArray(_FindArray())));
                        colon = false;
                    }
                    else
                    {
                        // Переменная
                        _index--;
                        list.Add(new JsonKeyValue(strKey, _FindValue()));
                        colon = false;
                    }
                }
                else if (key)
                {
                    // Ключ найден надо найти :
                    if (c == ':')
                    {
                        colon = true;
                        key = false;
                    }
                }
                else
                {
                    if (quotesNot)
                    {
                        // Ключ без кавычек, ловим финиш
                        if (c == ':')
                        {
                            // Ключ найден
                            strKey = _body.Substring(qi, _index - qi - 1);
                            quotesNot = false;
                            colon = true;
                        }
                    }
                    else
                    {
                        if (c == ' ' || c == '\t' || c == '{' || c == ',') continue;
                        if (c == '[' || c == ']' || c == ':')
                        {
                            throw new Exception(Sr.GetString(Sr.InvalidJsonReadTag, _index.ToString() + " " + c, _fileName));
                        }

                        if (c == '"')
                        {
                            // Ищем ключ
                            if (quotes)
                            {
                                // Ключ найден
                                strKey = _body.Substring(qi, _index - qi - 1);
                                quotes = false;
                                key = true;
                            }
                            else
                            {
                                // Начало ключа в кавычках
                                quotes = true;
                                qi = _index;
                            }
                        }
                        else if (c == '}')
                        {
                            // Или закрытие объекта
                            return list.ToArray();
                        }
                        else if (!quotes)
                        {
                            // Ключ без кавычек
                            quotesNot = true;
                            qi = _index - 1;
                        }
                    }
                }
            }
            throw new Exception(Sr.GetString(Sr.InvalidEndJsonReadTag, "}", _fileName));
        }

        /// <summary>
        /// Поиск зашли в массив после [, ищем атрибуты
        /// </summary>
        private IJsonArray[] _FindArray()
        {
            List<IJsonArray> list = new List<IJsonArray>();
            char c;
            while (_finishIndex >= _index)
            {
                c = _body[_index++];
                if (c == ',' || c == ' ' || c == '\t') continue;

                if (c == '{')
                {
                    // Ищем обект
                    list.Add(new JsonCompound(_FindCompound()));
                }
                else if (c == ']')
                {
                    // Или закрытие массива
                    return list.ToArray();
                }
                else
                {
                    // Переменная
                    _index--;
                    list.Add(_FindValue());
                }
            }
            throw new Exception(Sr.GetString(Sr.InvalidEndJsonReadTag, "]", _fileName));
        }

        /// <summary>
        /// Поиск зашли в атрибут
        /// </summary>
        private JsonValue _FindValue()
        {
            char c;
            bool value = false;
            bool quotes = false;
            int qi = _index;
            while (_finishIndex >= _index)
            {
                c = _body[_index++];

                if (value)
                {
                    // Ищем концовку "," или " " или "}"
                    if (c == ',' || c == ' ' || c == '\t')
                    {
                        // Возращаем переменную, ещё будут переменные
                        return new JsonValue(_body.Substring(qi, _index - qi - 1));
                    }
                    if (c == '}' || c == ']')
                    {
                        // Возращаем переменную, объект закрывается
                        _index--;
                        return new JsonValue(_body.Substring(qi, _index - qi));
                    }
                }
                else if (quotes)
                {
                    // Ищем "
                    if (c == '"')
                    {
                        // Возращаем переменную, о будущих переменных неизвестно
                        return new JsonValue(_body.Substring(qi, _index - qi - 1));
                    }
                }
                else if(c != ' ')
                {
                    // Ищем начало переменной либо с ковычкой либо без
                    if (c == '"')
                    {
                        // Кавычки
                        quotes = true;
                        qi = _index;
                    }
                    else
                    {
                        // Начало атрибута без кавычек
                        value = true;
                        qi = _index - 1;
                    }
                }
            }
            throw new Exception(Sr.GetString(Sr.InvalidEndJsonReadTag, "Attribut", _fileName));
        }
    }
}
