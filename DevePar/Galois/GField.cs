namespace DevePar.Galois
{
    public struct GField
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

        public GField Reciprocal() => Table.CreateField(Table.Reciprocal(Value));

        //Negate doesn't do anything for Galois fields but for other fields it does
        public GField Negate() => Table.CreateField(Table.Negate(Value));

        public GField Pow(uint power) => Table.CreateField(Table.Pow(Value, power));

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (!(obj is GField))
            {
                return false;
            }
            var f = (GField)obj;
            return Value == f.Value;
        }

        public override int GetHashCode()
        {
            return unchecked((int)Value);
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
