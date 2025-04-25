namespace Vge.Json
{
    /// <summary>
    /// Массив
    /// </summary>
    public struct JsonArray : IJson
    {
        public readonly IJsonArray[] Items;
        public JsonArray(IJsonArray[] items) => Items = items;

        public byte GetIdType() => 1;

        /// <summary>
        /// Количество элементов
        /// </summary>
        public int GetCount() => Items.Length;

        /// <summary>
        /// Имеется в выбранной ячейке переменная
        /// </summary>
        public bool IsValue(int index) => Items[index].IsValue();
        /// <summary>
        /// Получить выбранной ячейке ввиде переменной
        /// </summary>
        public JsonValue GetValue(int index) => (JsonValue)Items[index];
        /// <summary>
        /// Получить выбранной ячейке ввиде объекта
        /// </summary>
        public JsonCompound GetCompound(int index) => (JsonCompound)Items[index];

        public JsonCompound[] ToArrayObject()
        {
            int count = Items != null ? Items.Length : 0;
            JsonCompound[] vs = new JsonCompound[count];
            for (int i = 0; i < count; i++)
            {
                vs[i] = Items[i].IsValue() ? new JsonCompound() : (JsonCompound)Items[i];
            }
            return vs;
        }

        public bool[] ToArrayBool()
        {
            int count = Items != null ? Items.Length : 0;
            bool[] vs = new bool[count];
            for (int i = 0; i < count; i++)
            {
                vs[i] = Items[i].IsValue() ? ((JsonValue)Items[i]).GetBool() : false;
            }
            return vs;
        }

        public int[] ToArrayInt()
        {
            int count = Items != null ? Items.Length : 0;
            int[] vs = new int[count];
            for (int i = 0; i < count; i++)
            {
                vs[i] = Items[i].IsValue() ? ((JsonValue)Items[i]).GetInt() : 0;
            }
            return vs;
        }

        public float[] ToArrayFloat()
        {
            int count = Items != null ? Items.Length : 0;
            float[] vs = new float[count];
            for (int i = 0; i < count; i++)
            {
                vs[i] = Items[i].IsValue() ? ((JsonValue)Items[i]).GetFloat() : 0;
            }
            return vs;
        }

        public string[] ToArrayString()
        {
            int count = Items != null ? Items.Length : 0;
            string[] vs = new string[count];
            for (int i = 0; i < count; i++)
            {
                vs[i] = Items[i].IsValue() ? ((JsonValue)Items[i]).Value : "";
            }
            return vs;
        }

        public override string ToString() => "Array:[" + Items.Length + "]";
    }
}
