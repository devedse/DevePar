using DevePar.Galois;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace DevePar.Tests.Galois
{
    public class FieldTests
    {
        [Fact]
        public void Test()
        {
            for (int b = 1; b <= 4; b++)
            {
                var bb = new Field((byte)b);
                var result = new Field((byte)b);
                for (int i = 0; i < 2; i++)
                {
                    result = result * bb;
                }
            }
        }

        [Fact]
        public void TestPow()
        {
            var table = GFTable.GFTable8;
            for (int i = 0; i < 256; i++)
            {
                for (int y = 0; y < 256; y++)
                {
                    var a = Field.pow(new Field((byte)i), (byte)y);
                    var b = table.Pow((uint)i, (uint)y);

                    Assert.Equal((int)a, (int)b);
                }
            }
        }
    }
}
