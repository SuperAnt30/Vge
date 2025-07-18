using System;
using System.IO;

namespace Vge.Util
{
    public class FileAssets
    {
        /// <summary>
        /// Загрузить текстовый файл как строку для шейдора, убрав кооментарии,
        /// так-как коментарии могут быть кирилицей, не все Видеократы корректно понимают
        /// </summary>
        public static string ReadStringToShader(string fileName)
        {
            string path = Options.PathShaders;
            string pathName = path + fileName;
            if (!File.Exists(pathName))
            {
                throw new Exception(Sr.GetString(Sr.FileMissing, pathName));
            }
            string text = "";
            // Получить доступ к существующему либо создать новый
            using (StreamReader file = new StreamReader(pathName))
            {
                while (true)
                {
                    // Читаем строку из файла во временную переменную.
                    string strLine = file.ReadLine();
                    // Если достигнут конец файла, прерываем считывание.
                    if (strLine == null) break;
                    // ищем комментарий
                    int index = strLine.IndexOf("//");
                    if (index != -1)
                    {
                        // Кооментарий, оставляем всё, что было до //
                        text += strLine.Substring(0, index);
                    }
                    else
                    {
                        // Ищем #include
                        index = strLine.IndexOf("#include");
                        if (index != -1)
                        {
                            // include
                            index += 9;
                            if (strLine.Length > index)
                            {
                                text += _ParserIncludeGLSL(strLine.Substring(index));
                            }
                        }
                        else
                        {
                            text += strLine;
                        }
                    }
                    text += "\r\n";
                }
            }
            return text;
        }

        /// <summary>
        /// Если имеется include вытаскиваем из файла
        /// </summary>
        private static string _ParserIncludeGLSL(string fileName)
        {
            return ReadStringToShader(fileName);
        }
    }
}
