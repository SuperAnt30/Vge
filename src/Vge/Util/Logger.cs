using System;
using System.IO;
using System.Threading;
using Vge.Event;

namespace Vge.Util
{
    /// <summary>
    /// Объект фиксирующий лог
    /// </summary>
    public class Logger
    {
        /// <summary>
        /// Максимальное количество файлов
        /// </summary>
        private const int MaxCountFile = 30;

        /// <summary>
        /// Имя файла
        /// </summary>
        private readonly string fileName;
        /// <summary>
        /// Путь к файлу
        /// </summary>
        private readonly string path;
        /// <summary>
        /// Строка лога для записи
        /// </summary>
        private string log = "";

        public Logger(string folder, bool clearing = true)
        {
            path = folder + Path.DirectorySeparatorChar;
            string sd = DateTime.Now.ToString("yyyy-MM-dd");
            int i = 1;

            CheckPath(path);
            if (clearing)
            {
                Clearing();
            }

            while (true)
            {
                fileName = string.Format("{0}-{1}.txt", sd, i);
                if (!File.Exists(path + fileName)) break;
                i++;
            }
        }
        /// <summary>
        /// Добавить в лог
        /// </summary>
        public void Log(string logMessage, params object[] args)
        {
            string text = string.Format(logMessage, args);
            log += $"[{DateTime.Now.ToLongTimeString()}] " + text + Ce.Br;
            OnLoged(text);
        }
        /// <summary>
        /// Добавить в лог с префиксом [Client]
        /// </summary>
        public void Client(string logMessage, params object[] args)
         => Log(Srl.Client + logMessage, args);
        /// <summary>
        /// Добавить в лог с префиксом [Server]
        /// </summary>
        public void Server(string logMessage, params object[] args)
         => Log(Srl.Server + logMessage, args);
        /// <summary>
        /// Добавить в лог с префиксом [ERROR]
        /// </summary>
        public void Error(string logMessage, params object[] args)
            => Log(Srl.Error + logMessage, args);
        /// <summary>
        /// Добавить в лог с префиксом [ERROR]
        /// </summary>
        public void Error(Exception e)
        {
            e = GetException(e);
            Log(Srl.ErrorException, e.Source, e.Message, e.StackTrace);
        }

        /// <summary>
        /// Добавить сохранение в файл
        /// </summary>
        public void Save()
        {
            if (log != "")
            {
                string file = path + fileName;
                // счётчик примерно на проверку 5 секунд
                int count = 100;
                while (IsFileInUse(file) && --count > 0)
                {
                    Thread.Sleep(1);
                }
                if (count <= 0)
                {
                    // Если в прошлом файле мы не смогли записать, то пишем в другой файл.
                    // Причина, если в тот уже идёт записть по такту, и начали закрывать игру
                    using (StreamWriter w = File.AppendText(path + "Spare_" + fileName))
                    {
                        w.Write(log);
                    }
                }
                else
                {
                    using (StreamWriter w = File.AppendText(file))
                    {
                        w.Write(log);
                    }
                }
                log = "";
            }
        }

        /// <summary>
        /// Проверка доступности файла, true - файл занят
        /// </summary>
        private bool IsFileInUse(string fileName)
        {
            try
            {
                if (File.Exists(fileName))
                {
                    using (FileStream fs = File.Open(fileName, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                    {
                        //return false;
                    }
                }
                return false;
            }
            catch (IOException)
            {
                return true;
            }
        }

        /// <summary>
        /// Чистка папок логов до 10 последних файлов
        /// </summary>
        private void Clearing()
        {
            string[] vs = Directory.GetFileSystemEntries(path);
            int count = vs.Length;

            // Если больше MaxCountFile будем старые удалять
            if (count >= MaxCountFile)
            {
                // Готовим структура за датой создания файла
                FileSort[] files = new FileSort[count];
                for (int i = 0; i < count; i++)
                {
                    files[i] = new FileSort()
                    {
                        fileName = vs[i],
                        dateTime = File.GetCreationTime(vs[i]).Ticks
                    };
                }
                // Сортируем
                Array.Sort(files);

                // Старые удаляем
                try
                {
                    for (int i = count - MaxCountFile; i >= 0; i--)
                    {
                        File.Delete(files[i].fileName);
                    }
                }
                catch
                {
                    // Если нет возможности удалить, просто игнорируем
                }
            }
        }

        /// <summary>
        /// Структура для сортировки
        /// </summary>
        private struct FileSort : IComparable
        {
            public string fileName;
            public long dateTime;

            public int CompareTo(object obj)
            {
                if (obj is FileSort fileSort)
                {
                    return dateTime.CompareTo(fileSort.dateTime);
                }
                throw new ArgumentException(Sr.IncorrectParameterValue);
            }
        }

        /// <summary>
        /// Проверка пути, если нет, то создаём
        /// </summary>
        private static void CheckPath(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        /// <summary>
        /// Создать файл c ошибкой краша
        /// </summary>
        public static void Crash(string logMessage, params object[] args)
        {
            Logger logger = new Logger("Crash");
            logger.Error(logMessage, args);
            logger.Save();
        }

        /// <summary>
        /// Создать файл c ошибкой краша
        /// </summary>
        public static void Crash(Exception e, string logMessage = "", params object[] args)
        {
            e = GetException(e);
            Logger logger = new Logger("Crash");
            string prefix = "";
            if (logMessage != "")
            {
                prefix = string.Format(logMessage, args) + Ce.Br;
            }
            string stackTrace = e.InnerException == null ? e.StackTrace
                : e.InnerException.StackTrace + "\r\n" + e.StackTrace;
            logger.Error(prefix + "{0}: {1}{3}------{3}{2}", e.Source, e.Message, stackTrace, Ce.Br);
            logger.Save();
        }

        private static Exception GetException(Exception ex)
            => ex.InnerException == null ? ex : ex.InnerException;

        #region Event

        /// <summary>
        /// Событие лога
        /// </summary>
        public event StringEventHandler Loged;
        protected virtual void OnLoged(string text) => Loged?.Invoke(this, new StringEventArgs(text));

        #endregion
    }
}
