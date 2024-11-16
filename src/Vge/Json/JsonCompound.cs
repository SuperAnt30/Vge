namespace Vge.Json
{
    /// <summary>
    /// Составной
    /// </summary>
    public struct JsonCompound : IJson, IJsonArray
    {
        public readonly JsonKeyValue[] Items;
        public JsonCompound(JsonKeyValue[] items) => Items = items;

        public byte GetIdType() => 2;

        public bool IsValue() => false;

        /// <summary>
        /// Имеется ли ключ в списке объектов
        /// </summary>
        public bool IsKey(string key)
        {
            foreach (JsonKeyValue json in Items)
            {
                if (json.IsKey(key)) return true;
            }
            return false;
        }

        /// <summary>
        /// Получить значение bool и списка
        /// </summary>
        public bool GetBool(string key)
        {
            foreach (JsonKeyValue json in Items)
            {
                if (json.IsKey(key)) return json.GetBool();
            }
            return false;
        }

        /// <summary>
        /// Получить значение int и списка
        /// </summary>
        public int GetInt(string key)
        {
            foreach (JsonKeyValue json in Items)
            {
                if (json.IsKey(key)) return json.GetInt();
            }
            return 0;
        }

        /// <summary>
        /// Получить значение float и списка
        /// </summary>
        public float GetFloat(string key)
        {
            foreach (JsonKeyValue json in Items)
            {
                if (json.IsKey(key)) return json.GetFloat();
            }
            return 0;
        }

        /// <summary>
        /// Получить значение string и списка
        /// </summary>
        public string GetString(string key)
        {
            foreach (JsonKeyValue json in Items)
            {
                if (json.IsKey(key)) return json.GetString();
            }
            return "";
        }

        /// <summary>
        /// Получить объект по ключу
        /// </summary>
        public JsonCompound GetObject(string key)
        {
            foreach (JsonKeyValue json in Items)
            {
                if (json.IsKey(key)) return json.GetObjects();
            }
            return new JsonCompound();
        }

        /// <summary>
        /// Получить массив объектов
        /// </summary>
        public JsonArray GetArray(string key)
        {
            foreach (JsonKeyValue json in Items)
            {
                if (json.IsKey(key)) return json.GetArray();
            }
            return new JsonArray();
        }

        public override string ToString() => "Object:[" + Items.Length + "]";
    }
}
