using DevePar.MathHelpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace DevePar.Galois
{
    public class GFTable
    {
        public static GFTable GFTable8 => GFTable8Lazy.Value;
        private static Lazy<GFTable> GFTable8Lazy { get; } = new Lazy<GFTable>(() => new GFTable(8, 0x11D));
        public static GFTable GFTable16 => GFTable16Lazy.Value;
        private static Lazy<GFTable> GFTable16Lazy { get; } = new Lazy<GFTable>(() => new GFTable(16, 0x1100B));

        public uint Size { get; }
        public uint Limit { get; }
        public int Power { get; }
        public uint Polynomial { get; }

        private uint[] antilog;
        private uint[] log;

        public uint Alog(uint value) => antilog[value];

        public GFTable(int power, uint polynomial)
        {
            Power = power;
            Polynomial = polynomial;
            Size = MathHelper.UintPow(2, (uint)power);
            Limit = Size - 1;

            antilog = new uint[Size];
            log = new uint[Size];




            log[0] = Limit;
            antilog[Limit] = 0;

            uint mask = 1;
            for (uint depth = 0; depth < Limit; depth++)
            {
                log[mask] = depth;
                antilog[depth] = mask;

                mask = Shift(mask);
            }
        }

        private uint Shift(uint value)
        {
            var shifted = value << 1;
            var shiftedSize = shifted & Size;

            if (shiftedSize != 0)
            {
                var retval = shifted ^ Polynomial;
                return retval;
            }
            else
            {
                return shifted;
            }
        }

        public GField CreateField(uint value)
        {
            return new GField(this, value);
        }

        private void ThrowIfOutsideOfField(uint a, uint b)
        {
            if (a < 0 || a > Limit || b < 0 || b > Limit) throw new ArgumentException("The arguments need to exist in the field.");
        }

        public uint Add(uint a, uint b)
        {
            ThrowIfOutsideOfField(a, b);

            return a ^ b;
        }

        public uint Sub(uint a, uint b)
        {
            ThrowIfOutsideOfField(a, b);
            return a ^ b;
        }

        public uint Mul(uint a, uint b)
        {
            ThrowIfOutsideOfField(a, b);

            if (a == 0 || b == 0)
            {
                return 0;
            }
            else
            {
                var sum = log[a] + log[b];
                if (sum >= Limit)
                {
                    return antilog[sum - Limit];
                }
                else
                {
                    return antilog[sum];
                }
            }
        }

        public uint Div(uint a, uint b)
        {
            ThrowIfOutsideOfField(a, b);

            if (a == 0)
            {
                return 0;
            }
            else
            {
                var sum = log[a] - log[b];
                if (sum < 0 || sum > Limit)
                {
                    return antilog[sum + Limit];
                }
                else
                {
                    return antilog[sum];
                }
            }
        }

        public uint Pow(uint a, uint b)
        {
            ThrowIfOutsideOfField(a, b);

            if (b == 0)
            {
                return 1;
            }
            else if (a == 0)
            {
                return 0;
            }
            else
            {
                var sum = log[a] * b;
                sum = (sum >> Power) + (sum & Limit);
                if (sum >= Limit)
                {
                    return antilog[sum - Limit];
                }
                else
                {
                    return antilog[sum];
                }
            }
        }
    }
}
