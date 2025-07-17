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
            if (!File.Exists(fileName))
            {
                throw new Exception(Sr.GetString(Sr.FileMissing, fileName));
            }

            string text = "";
            // Получить доступ к существующему либо создать новый
            using (StreamReader file = new StreamReader(fileName))
            {
                while (true)
                {
                    // Читаем строку из файла во временную переменную.
                    string strLine = file.ReadLine();
                    // Если достигнут конец файла, прерываем считывание.
                    if (strLine == null) break;
                    // ищем комментарий
                    int index = strLine.IndexOf("//");
                    if (index == -1) text += strLine;
                    else text += strLine.Substring(0, index);
                    text += "\r\n";
                }
            }
            return text;
        }
    }
}
