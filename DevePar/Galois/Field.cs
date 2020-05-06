namespace DevePar.Galois
{
    public class Field
    {
        public const int order = 256;
        //irreducible polynomial used : x^8 + x^4 + x^3 + x^2 + 1 (0x11D)
        public const int polynomial = 0x11D;
        //generator to be used in Exp & Log table generation
        public const byte generator = 0x2;
        public static byte[] Exp;
        public static byte[] Log;

        public byte Value { get; set; }

        public Field()
        {
            Value = 0;
        }

        public Field(byte value)
        {
            Value = value;
        }

        public Field Clone()
        {
            return new Field(Value);
        }

        //generates Exp & Log table for fast multiplication operator
        static Field()
        {
            Exp = new byte[order];
            Log = new byte[order];

            byte val = 0x01;
            for (int i = 0; i < order; i++)
            {
                Exp[i] = val;
                if (i < order - 1)
                {
                    Log[val] = (byte)i;
                }
                val = multiply(generator, val);
            }
        }

        //operators
        public static explicit operator Field(byte b)
        {
            Field f = new Field(b);
            return f;
        }

        public static explicit operator byte(Field f)
        {
            return f.Value;
        }

        public static Field operator +(Field Fa, Field Fb)
        {
            byte bres = (byte)(Fa.Value ^ Fb.Value);
            return new Field(bres);
        }

        public static Field operator -(Field Fa, Field Fb)
        {
            byte bres = (byte)(Fa.Value ^ Fb.Value);
            return new Field(bres);
        }

        public static Field operator *(Field Fa, Field Fb)
        {
            Field FRes = new Field(0);
            if (Fa.Value != 0 && Fb.Value != 0)
            {
                byte bres = (byte)((Log[Fa.Value] + Log[Fb.Value]) % (order - 1));
                bres = Exp[bres];
                FRes.Value = bres;
            }
            return FRes;
        }

        public static Field operator /(Field Fa, Field Fb)
        {
            if (Fb.Value == 0)
            {
                throw new System.ArgumentException("Divisor cannot be 0", "Fb");
            }

            Field Fres = new Field(0);
            if (Fa.Value != 0)
            {
                byte bres = (byte)(((order - 1) + Log[Fa.Value] - Log[Fb.Value]) % (order - 1));
                bres = Exp[bres];
                Fres.Value = bres;
            }
            return Fres;
        }

        public static Field pow(Field f, byte exp)
        {
            Field fres = new Field(1);
            for (byte i = 0; i < exp; i++)
            {
                fres *= f;
            }
            return fres;
        }

        public static bool operator ==(Field Fa, Field Fb)
        {
            return (Fa.Value == Fb.Value);
        }

        public static bool operator !=(Field Fa, Field Fb)
        {
            return !(Fa.Value == Fb.Value);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            Field F = obj as Field;
            if ((object)F == null)
            {
                return false;
            }
            return (Value == F.Value);
        }

        public override int GetHashCode()
        {
            return Value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        //multiplication method which is only used in Exp & Log table generation
        //implemented with Russian Peasant Multiplication algorithm
        private static byte multiply(byte a, byte b)
        {
            byte result = 0;
            byte aa = a;
            byte bb = b;
            while (bb != 0)
            {
                if ((bb & 1) != 0)
                {
                    result ^= aa;
                }
                byte highest_bit = (byte)(aa & 0x80);
                aa <<= 1;
                if (highest_bit != 0)
                {
                    aa ^= (polynomial & 0xFF);
                }
                bb >>= 1;
            }
            return result;
        }
    }
}
