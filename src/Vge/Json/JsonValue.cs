using System.Globalization;

namespace Vge.Json
{
    /// <summary>
    /// Переменная
    /// </summary>
    public struct JsonValue : IJson, IJsonArray
    {
        public readonly string Value;
        public JsonValue(string value) => Value = value;

        public byte GetIdType() => 0;

        public bool IsValue() => true;

        public bool GetBool() => Value == "true";
        public int GetInt()
        {
            try
            {
                return int.Parse(_Fix(Value));
            }
            catch
            {
                return 0;
            }
        }
        public float GetFloat()
        {
            try
            {
                return float.Parse(_Fix(Value), CultureInfo.InvariantCulture);
            }
            catch
            {
                return 0;
            }
        }

        private string _Fix(string value)
        {
            int index = value.IndexOf("\\n");
            if (index != -1)
            {
                value = value.Substring(0, index);
            }
            return value;
        }

        public override string ToString() => Value;
    }
}
