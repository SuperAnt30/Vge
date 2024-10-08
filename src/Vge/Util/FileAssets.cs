﻿using System;
using System.IO;

namespace Vge.Util
{
    public class FileAssets
    {
        /// <summary>
        /// Загрузить текстовый файл как строку
        /// </summary>
        public static string ReadString(string fileName)
        {
            if (!File.Exists(fileName))
            {
                throw new Exception(Sr.GetString(Sr.FileMissing, fileName));
            }
            
            StreamReader sr = new StreamReader(fileName);
            string text = sr.ReadToEnd();
            sr.Close();
            return text;
        }
    }
}
