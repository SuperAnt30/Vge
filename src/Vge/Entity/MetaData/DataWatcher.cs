using System;
using System.Runtime.CompilerServices;
using Vge.Item;
using Vge.Network;
using WinGL.Util;

namespace Vge.Entity.MetaData
{
    /// <summary>
    /// Наблюдатель за данными.
    /// Дополнительные автоматически синхронизированные сервер-клиент данные сущностей.
    /// Нужен для тех данных, которые влияют на рендер или любую прорисовку на клиенте.
    /// </summary>
    public class DataWatcher
    {
        /// <summary>
        /// Было ли изменение
        /// </summary>
        public bool IsChanged { get; private set; }

        /// <summary>
        /// Массив данных
        /// </summary>
        private readonly WatchableObject[] _watched;

        /// <summary>
        /// Дополнительные автоматически синхронизированные сервер-клиент данные сущностей.
        /// </summary>
        /// <param name="count">Количество дополнительных данных</param>
        public DataWatcher(int count)
        {
            if (count > 31)
            {
                throw new Exception("Идентификатор значения данных " + count + " слишком велик (Max 31)");
            }
            _watched = new WatchableObject[count];
            IsChanged = false;
        }

        /// <summary>
        /// По типу данных получить id
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private EnumTypeWatcher _GetDataTypes(Type type)
        {
            if (type == typeof(byte)) return EnumTypeWatcher.Byte;
            if (type == typeof(short)) return EnumTypeWatcher.Short;
            if (type == typeof(int)) return EnumTypeWatcher.Int;
            if (type == typeof(float)) return EnumTypeWatcher.Float;
            if (type == typeof(string)) return EnumTypeWatcher.String;
            if (type == typeof(ItemStack)) return EnumTypeWatcher.ItemStack;
            if (type == typeof(Vector3i)) return EnumTypeWatcher.Vector3i;
            if (type == typeof(Vector3)) return EnumTypeWatcher.Vector3;

            throw new Exception("Неопределённый тип данных");
        }

        /// <summary>
        /// Задать параметр данных
        /// </summary>
        /// <param name="id">индекс 0 - 31</param>
        /// <param name="obj">объект с данными</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(byte id, object obj)
        {
            if (obj == null)
            {
                throw new Exception("Неизвестный тип данных");
            }
            if (id >= _watched.Length)
            {
                throw new Exception("Идентификатор значения данных " + id + " слишком велик");
            }
            else
            {
                _watched[id] = new WatchableObject(_GetDataTypes(obj.GetType()), id, obj);
            }
        }

        /// <summary>
        /// Задать новый объект для отслеживания DataWatcher, используя указанный тип данных
        /// </summary>
        /// <param name="id">индекс 0 - 31</param>
        /// <param name="type">индекс типа данных 0 - 7</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetByDataType(byte id, EnumTypeWatcher type)
            => _watched[id] = new WatchableObject(type, id, null);

        /// <summary>
        /// Обновляет уже существующий объект
        /// Вернёт true если было обновление
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool UpdateObject(int id, object obj)
        {
            WatchableObject watchableObject = _watched[id];
            if (!obj.Equals(watchableObject.ObjectType))
            {
                watchableObject.WatchedObject = obj;
                watchableObject.Changing = true;
                _watched[id] = watchableObject;
                IsChanged = true;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Задать значение что было изменено
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetObjectWatched(int id)
        {
            _watched[id].Changing = true;
            IsChanged = true;
        }

        /// <summary>
        /// Получить массив всех метданных которые были изменены
        /// </summary>
        public WatchableObject[] GetChanged()
        {
            if (IsChanged)
            {
                int i;
                int countAll = _watched.Length;
                int count = 0;
                for (i = 0; i < countAll; i++)
                {
                    if (_watched[i].Changing) count++;
                }
                IsChanged = false;
                if (count > 0)
                {
                    WatchableObject[] objects = new WatchableObject[count];
                    count = 0;
                    for (i = 0; i < countAll; i++)
                    {
                        if (_watched[i].Changing)
                        {
                            _watched[i].Changing = false;
                            objects[count++] = _watched[i];
                        }
                    }
                    return objects;
                }
            }
            return new WatchableObject[0];
        }

        /// <summary>
        /// Получить массив всех метданных
        /// </summary>
        public WatchableObject[] GetAllWatched()
        {
            int count = _watched.Length;
            for (int i = 0; i < count; i++)
            {
                _watched[i].Changing = false;
            }
            return _watched;
        }

        /// <summary>
        /// Обновить данные со списка
        /// </summary>
        public void UpdateWatchedObjectsFromList(WatchableObject[] watchables)
        {
            foreach (WatchableObject watchableObject in watchables)
            {
                int id = watchableObject.Index;
                if (id < _watched.Length)
                {
                    _watched[id].WatchedObject = watchableObject.WatchedObject;
                }
            }
            IsChanged = true;
        }

        #region Get

        /// <summary>
        /// Получить байтовое значение по индексу
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte GetByte(int id) => (byte)_watched[id].WatchedObject;
        /// <summary>
        /// Получить 16-разрядное значение по индексу
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short GetShort(int id) => (short)_watched[id].WatchedObject;
        /// <summary>
        /// Получить 32-разрядное значение по индексу
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetInt(int id) => (int)_watched[id].WatchedObject;
        /// <summary>
        /// Получить число с плавающей запятой по индексу
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetFloat(int id) => (float)_watched[id].WatchedObject;
        /// <summary>
        /// Получить строковое значение по индексу
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetString(int id) => (string)_watched[id].WatchedObject;
        /// <summary>
        /// Получить объект ItemStack по индексу
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ItemStack GetItemStack(int id) => _watched[id].WatchedObject as ItemStack;
        /// <summary>
        /// Получить объект BlockPos по индексу
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3i GetVector3i(int id) => (Vector3i)_watched[id].WatchedObject;
        /// <summary>
        /// Получить трёхмерный вектор с плавающей запятой по индексу
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3 GetVector3(int id) => (Vector3)_watched[id].WatchedObject;

        #endregion

        /// <summary>
        /// Читает список отслеживаемых объектов (атрибут объекта типа 
        /// {byte, short, int, float, string, ItemStack, BlockPos, vec3}) из предоставленного StreamBase
        /// </summary>
        public static WatchableObject[] ReadWatchedListFromPacketBuffer(ReadPacket stream)
        {
            byte count = stream.Byte();
            WatchableObject[] watchables = new WatchableObject[count];
            if (count > 0)
            {
                byte index;
                EnumTypeWatcher enumType;
                for (int i = 0; i < count; i++)
                {
                    index = stream.Byte();
                    enumType = (EnumTypeWatcher)(index >> 5);
                    index = (byte)(index & 31);
                    switch (enumType)
                    {
                        case EnumTypeWatcher.Byte:
                            watchables[index] = new WatchableObject(enumType, index, stream.Byte()); break;
                        case EnumTypeWatcher.Short:
                            watchables[index] = new WatchableObject(enumType, index, stream.Short()); break;
                        case EnumTypeWatcher.Int:
                            watchables[index] = new WatchableObject(enumType, index, stream.Int()); break;
                        case EnumTypeWatcher.Float:
                            watchables[index] = new WatchableObject(enumType, index, stream.Float()); break;
                        case EnumTypeWatcher.String:
                            watchables[index] = new WatchableObject(enumType, index, stream.String()); break;
                        case EnumTypeWatcher.ItemStack:
                            watchables[index] = new WatchableObject(enumType, index, ItemStack.ReadStream(stream)); break;
                        case EnumTypeWatcher.Vector3i:
                            watchables[index] = new WatchableObject(enumType, index,
                                new Vector3i(stream.Int(), stream.Int(), stream.Int())); break;
                        case EnumTypeWatcher.Vector3:
                            watchables[index] = new WatchableObject(enumType, index,
                                new Vector3(stream.Float(), stream.Float(), stream.Float())); break;
                    }
                }
            }
            return watchables;
        }

        /// <summary>
        /// Записывает список наблюдаемых объектов (атрибут объекта типа 
        /// {byte, short, int, float, string, ItemStack, BlockPos, vec3}) из предоставленного StreamBase
        /// </summary>
        public static void WriteWatchedListToPacketBuffer(WatchableObject[] objectsList, WritePacket stream)
        {
            if (objectsList != null)
            {
                stream.Byte((byte)objectsList.Length);
                foreach (WatchableObject watchableObject in objectsList)
                {
                    stream.Byte((byte)((byte)watchableObject.ObjectType << 5 | watchableObject.Index & 31));
                    switch (watchableObject.ObjectType)
                    {
                        case EnumTypeWatcher.Byte: stream.Byte((byte)watchableObject.WatchedObject); break;
                        case EnumTypeWatcher.Short: stream.Short((short)watchableObject.WatchedObject); break;
                        case EnumTypeWatcher.Int: stream.Int((int)watchableObject.WatchedObject); break;
                        case EnumTypeWatcher.Float: stream.Float((float)watchableObject.WatchedObject); break;
                        case EnumTypeWatcher.String: stream.String((string)watchableObject.WatchedObject); break;
                        case EnumTypeWatcher.ItemStack: ItemStack.WriteStream(watchableObject.WatchedObject as ItemStack, stream); break;
                        case EnumTypeWatcher.Vector3i:
                            Vector3i veci = (Vector3i)watchableObject.WatchedObject;
                            stream.Int(veci.X);
                            stream.Int(veci.Y);
                            stream.Int(veci.Z);
                            break;
                        case EnumTypeWatcher.Vector3:
                            Vector3 vec = (Vector3)watchableObject.WatchedObject;
                            stream.Float(vec.X);
                            stream.Float(vec.Y);
                            stream.Float(vec.Z);
                            break;
                    }
                }
            }
            else
            {
                stream.Byte(0);
            }
        }
    }
}
