namespace Vge.NBT
{
    /// <summary>
    /// Тэг окончания группы для NBT
    /// </summary>
    public class TagEnd : NBTBase
    {
        public override void Write(NBTStream output) { }
        public override void Read(NBTStream input) { }
        public override NBTBase Copy() => new TagEnd();
        public override byte GetId() => 0;
        public override string ToString() => "End";
    }
}
