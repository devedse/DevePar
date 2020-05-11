using DevePar.MathHelpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace DevePar.Galois
{
    public class GFTable
    {
        public static GFTable GFTable8 => GFTable8Lazy.Value;
        private static Lazy<GFTable> GFTable8Lazy { get; } = new Lazy<GFTable>(() => new GFTable(8, 0x11D));
        public static GFTable GFTable16 => GFTable16Lazy.Value;
        private static Lazy<GFTable> GFTable16Lazy { get; } = new Lazy<GFTable>(() => new GFTable(16, 0x1100D));

        public int Size { get; }
        public uint Power { get; }
        public uint Polynomial { get; }

        public static byte[] Exp;
        public static byte[] Log;

        private const int generator = 2;

        public GFTable(uint power, uint polynomial)
        {
            Power = power;
            Polynomial = polynomial;
            Size = MathHelper.IntPow(2, power);

            Exp = new byte[Size];
            Log = new byte[Size];

            byte val = 0x01;
            for (int i = 0; i < Size; i++)
            {
                Exp[i] = val;
                if (i < Size - 1)
                {
                    Log[val] = (byte)i;
                }
                val = multiply(generator, val);
            }
        }

        private byte multiply(byte a, byte b)
        {
            throw new NotImplementedException();
            //byte result = 0;
            //byte aa = a;
            //byte bb = b;
            //while (bb != 0)
            //{
            //    if ((bb & 1) != 0)
            //    {
            //        result ^= aa;
            //    }
            //    byte highest_bit = (byte)(aa & 0x80);
            //    aa <<= 1;
            //    if (highest_bit != 0)
            //    {
            //        aa ^= (Polynomial & 0xFF);
            //    }
            //    bb >>= 1;
            //}
            //return result;
        }
    }
}
