namespace Vge.Json
{
    public struct JsonKeyValue
    {
        public readonly string Key;
        public readonly IJson Value;
        private readonly byte _type;

        public JsonKeyValue(string key, IJson value)
        {
            Key = key;
            Value = value;
            _type = value.GetIdType();
        }

        public bool IsKey(string key) => Key == key;

        public bool GetBool() => _type == 0 ? ((JsonValue)Value).GetBool() : false;

        public int GetInt() => _type == 0 ? ((JsonValue)Value).GetInt() : 0;

        public float GetFloat() => _type == 0 ? ((JsonValue)Value).GetFloat() : 0;

        public string GetString() => _type == 0 ? ((JsonValue)Value).Value : "";

        public JsonCompound GetObjects() => (JsonCompound)Value;

        public JsonArray GetArray() => (JsonArray)Value;

        public override string ToString() => Key + ":" + Value.ToString();
    }
}
