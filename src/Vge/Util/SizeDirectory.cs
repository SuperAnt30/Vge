using System.IO;

namespace Vge.Util
{
    /// <summary>
    /// Объект определяет размер папки
    /// </summary>
    public class SizeDirectory
    {
        /// <summary>
        /// Размер папки
        /// </summary>
        public float Size { get; private set; }
        /// <summary>
        /// Измирения
        /// </summary>
        public string Letters { get; private set; }

        public SizeDirectory(string path)
        {
            long size = DirSize(new DirectoryInfo(path));
            if (size > 1073741824)
            {
                Letters = "Gb";
                Size = size / 1073741824f;
            }
            else if (size > 1048576)
            {
                Letters = "Mb";
                Size = size / 1048576;
            }
            else if (size > 1024)
            {
                Letters = "Kb";
                Size = size / 1024;
            }
            else
            {
                Letters = "b";
                Size = size;
            }
        }

        /// <summary>
        /// Получить размер папки
        /// </summary>
        private long DirSize(DirectoryInfo d)
        {
            long size = 0;
            // Добавьте размеры файлов
            FileInfo[] fis = d.GetFiles();
            foreach (FileInfo fi in fis)
            {
                size += fi.Length;
            }
            // Добавить размеры подкаталогов
            DirectoryInfo[] dis = d.GetDirectories();
            foreach (DirectoryInfo di in dis)
            {
                size += DirSize(di);
            }
            return size;
        }

        public override string ToString() => Size.ToString("0.0 ") + Letters;
    }
}
