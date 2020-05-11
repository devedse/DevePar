using DevePar.Galois;
using System.Linq;

namespace DevePar.LinearAlgebra
{
    public class CoolVectorGField
    {
        public GField[] Data { get; set; }
        public int Length => Data.Length;

        public CoolVectorGField(params GField[] data)
        {
            Data = data;
        }

        public CoolVectorGField(GFTable table, params uint[] data)
        {
            Data = data.Select(t => table.CreateField(t)).ToArray();
        }

        public CoolVectorGField(GFTable table, params int[] data)
        {
            Data = data.Select(t => table.CreateField((uint)t)).ToArray();
        }

        public override string ToString()
        {
            return $"({string.Join(',', Data.Select(t => t.Value))})";
        }
    }
}
