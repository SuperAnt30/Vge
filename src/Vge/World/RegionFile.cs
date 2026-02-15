using System;
using System.IO;
using Vge.NBT;

namespace Vge.World
{
    public class RegionFile
    {
        /// <summary>
        /// Сылка на объект мира сервера
        /// </summary>
        private readonly WorldServer _world;

        /// <summary>
        /// Путь к папке с регионами
        /// </summary>
        private readonly string _path;
        /// <summary>
        /// Имя файла
        /// </summary>
        private readonly string _fileName;
        /// <summary>
        /// Массив смещений чанков (z << 5 | x)
        /// </summary>
        private readonly int[] _offsets = new int[1024];
        /// <summary>
        /// Массив буфферов всех чанков в регионе (z << 5 | x)
        /// </summary>
        private readonly byte[][] _buffers = new byte[1024][];

        public RegionFile(WorldServer world, int x, int z)
        {
            _world = world;
            _path = world.Settings.PathWorld;
            _fileName = "r." + x + "." + z + ".mca";
            ReadCreate();
        }

        /// <summary>
        /// Прочесть, если нету создать
        /// </summary>
        private void ReadCreate()
        {
            string pathFile = _path + _fileName;
            try
            {
                if (File.Exists(pathFile))
                {
                    // Читаем
                    using (FileStream fileStream = new FileStream(pathFile, FileMode.Open))
                    using (NBTStream stream = new NBTStream())
                    {
                        fileStream.Seek(0, SeekOrigin.Begin);
                        fileStream.CopyTo(stream);
                        stream.Seek(0, SeekOrigin.Begin);
                        int i, offset, size;
                        for (i = 0; i < 1024; i++)
                        {
                            _offsets[i] = stream.ReadInt();
                        }
                        for (i = 0; i < 1024; i++)
                        {
                            offset = _offsets[i];
                            if (offset > 0)
                            {
                                stream.Seek((offset >> 8) * 4096, SeekOrigin.Begin);
                                size = stream.ReadInt();
                                stream.ReadByte();
                                _buffers[i] = new byte[size];
                                stream.Read(_buffers[i], 0, size);
                            }
                        }
                    }
                }
                return;
            }
            catch (Exception ex)
            {
                _world.Filer.Log.Error("Server.Error.RegionFile.ReadCreate({1}, world{2}). {0}",
                    ex.Message, _fileName, _world.IdWorld);
            }
        }

        /// <summary>
        /// Сохранить
        /// </summary>
        public void WriteToFile()
        {
            if (Ce.SaveWorld)
            {
                string pathFile = _path + _fileName;
                try
                {
                    byte[] emp = new byte[4096];
                    int i, count, box;
                    int offset = 4096;
                    int all = 0;
                    using (NBTStream stream = new NBTStream())
                    {
                        for (i = 0; i < 1024; i++)
                        {
                            if (_buffers[i] == null) _buffers[i] = new byte[0];
                            count = _buffers[i].Length;
                            if (count > 0)
                            {
                                box = ((count + 5) >> 12) + 1;
                                stream.WriteInt((offset / 4096) << 8 | box);
                                offset += (box << 12);
                                all++;
                            }
                            else
                            {
                                stream.WriteInt(0);
                            }
                        }
                        if (all > 0)
                        {
                            for (i = 0; i < 1024; i++)
                            {
                                count = _buffers[i].Length;
                                if (count > 0)
                                {
                                    box = ((count + 5) >> 12) + 1;
                                    stream.WriteInt(count);
                                    stream.WriteByte(1); // 1 = gzip; 2 = zlib
                                    stream.Write(_buffers[i], 0, count);
                                    stream.Write(emp, 0, (box << 12) - _buffers[i].Length - 5);
                                }
                            }
                            stream.Seek(0, SeekOrigin.Begin);
                            using (FileStream fileStream = new FileStream(pathFile, FileMode.Create))
                                stream.CopyTo(fileStream);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _world.Filer.Log.Error("Server.Error.RegionFile.WriteToFile({1}, world{2}). {0}",
                        ex.Message, _fileName, _world.IdWorld);
                }
            }
        }

        /// <summary>
        /// Записать чанк в регион
        /// </summary>
        public void WriteChunk(TagCompound nbt, int chX, int chZ)
        {
            int x = chX & 31;
            int z = chZ & 31;
            _buffers[z * 32 + x] = NBTTools.WriteToBytes(nbt, true);
        }
        /// <summary>
        /// Прочесть чанк с региона
        /// </summary>
        public TagCompound ReadChunk(int chX, int chZ)
        {
            int x = chX & 31;
            int z = chZ & 31;
            return NBTTools.ReadToBytes(_buffers[(z * 32 + x)], true);
        }
    }
}
