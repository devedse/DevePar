namespace DevePar.Galois
{
    public class GField
    {
        public GFTable Table { get; }
        public uint Value { get; }

        public GField(GFTable table, uint value)
        {
            Table = table;
            Value = value;
        }

        public static GField operator +(GField a, GField b) => a.Table.CreateField(a.Table.Add(a.Value, b.Value));

        public static GField operator -(GField a, GField b) => a.Table.CreateField(a.Table.Sub(a.Value, b.Value));

        public static GField operator /(GField a, GField b) => a.Table.CreateField(a.Table.Div(a.Value, b.Value));

        public static GField operator *(GField a, GField b) => a.Table.CreateField(a.Table.Mul(a.Value, b.Value));

        public static bool operator ==(GField a, GField b) => a.Value == b.Value;

        public static bool operator !=(GField a, GField b) => a.Value != b.Value;

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            var f = obj as GField;
            if (f is null)
            {
                return false;
            }
            return Value == f.Value;
        }

        public override int GetHashCode()
        {
            return unchecked((int)Value);
        }
    }
}
