namespace Vge.Event
{
    public delegate void StringEventHandler(object sender, StringEventArgs e);
    public class StringEventArgs
    {
        /// <summary>
        /// Строка лога
        /// </summary>
        public string Text { get; private set; }
        /// <summary>
        /// Дополнительный объект
        /// </summary>
        public object Tag { get; private set; }

        public StringEventArgs(string text) => Text = text;

        public StringEventArgs(string text, object tag)
        {
            Text = text;
            Tag = tag;
        }
    }
}
