
namespace DebugOS.Objects
{
    public enum FieldType
    {
        Padding,
        Integer,
        Signed,
        String,
    }

    public sealed class FieldDefinition
    {
        public string    Name  { get; private set; }
        public int       Width { get; private set; }
        public FieldType Type  { get; private set; }

        public FieldDefinition(string Name, int Width, FieldType Type)
        {
            this.Name  = Name;
            this.Width = Width;
            this.Type  = Type;
        }
    }

    public sealed class ObjectDefinition
    {
        public string Name { get; private set; }
        public string Symbol { get; private set; }

        public FieldDefinition[] Fields { get; private set; }

        public ObjectDefinition(string Name, string Symbol, FieldDefinition[] Fields)
        {
            this.Name   = Name;
            this.Symbol = Symbol;
            this.Fields = Fields;
        }
    }

}
