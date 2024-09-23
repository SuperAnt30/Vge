using System;

namespace Vge.NBT
{
    /// <summary>
    /// Тэг массива интов для NBT
    /// </summary>
    public class TagIntArray : NBTBase
    {
        private int[] intArray = new int[0];

        public TagIntArray() { }
        public TagIntArray(int[] intArray) => this.intArray = intArray;

        public override void Write(NBTStream output)
        {
            output.WriteInt(intArray.Length);
            for (int i = 0; i < intArray.Length; i++)
            {
                output.WriteInt(intArray[i]);
            }
        }
        public override void Read(NBTStream input)
        {
            intArray = new int[input.ReadInt()];
            for (int i = 0; i < intArray.Length; i++)
            {
                intArray[i] = input.ReadInt();
            }
        }

        public override NBTBase Copy()
        {
            int[] ar = new int[intArray.Length];
            Array.Copy(intArray, 0, ar, 0, intArray.Length);
            return new TagIntArray(ar);
        }
        public override byte GetId() => 11;

        public override string ToString()
        {
            string str = "[";
            for (int i = 0; i < intArray.Length; i++)
                str += intArray[i] + ",";
            str += "]";
            return str;
        }

        public override bool Equals(object value) 
            => base.Equals(value) ? Equals(intArray, ((TagIntArray)value).intArray) : false;

        public override int GetHashCode() => base.GetHashCode() ^ intArray.GetHashCode();

        public int[] GetIntArray() => intArray;
    }
}
