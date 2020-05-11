using DevePar.Galois;
using System.Linq;

namespace DevePar.LinearAlgebra
{
    public class CoolVectorField
    {
        public Field[] Data { get; set; }
        public int Length => Data.Length;

        public CoolVectorField(params Field[] data)
        {
            Data = data;
        }

        public CoolVectorField(params int[] data)
        {
            Data = data.Select(t => new Field((byte)t)).ToArray();
        }

        public override string ToString()
        {
            return $"({string.Join(',', Data.Select(t => t.Value))})";
        }
    }
}
