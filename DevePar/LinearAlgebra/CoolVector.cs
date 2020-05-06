namespace DevePar.LinearAlgebra
{
    public class CoolVector
    {
        public int[] Data { get; set; }
        public int Length => Data.Length;

        public CoolVector(params int[] data)
        {
            Data = data;
        }

        public override string ToString()
        {
            return $"({string.Join(',', Data)})";
        }
    }
}
