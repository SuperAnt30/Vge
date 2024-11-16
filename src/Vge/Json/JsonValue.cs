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
                return int.Parse(Value);
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
                return float.Parse(Value, CultureInfo.InvariantCulture);
            }
            catch
            {
                return 0;
            }
        }

        public override string ToString() => Value;
    }
}
