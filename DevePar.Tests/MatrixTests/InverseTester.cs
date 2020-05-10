using DevePar.Galois;
using DevePar.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace DevePar.Tests.MatrixTests
{
    public class InverseTester
    {
        [Fact]
        public void Test()
        {
            var matrixField = new MatrixField(new int[,]
                {
                    { 0, 0, 1 },
                    { 1, 1, 1 },
                    { 1, 2, 3 }
                });

            var inverseRuben = matrixField.InverseRuben();
            var inverse = matrixField.Inverse;

            Assert.Equal(inverse, inverseRuben);
        }

        //[Fact]
        //public void Test2()
        //{
        //    var matrixField = new MatrixField(new int[,]
        //        {
        //            { 0, 0, 0, 1 },
        //            { 1, 1, 1, 1 },
        //            { 1, 2, 3, 4 },
        //            { 1, 8, 15, 64 }
        //        });

        //    var inverseRuben = matrixField.InverseRuben();
        //    var inverse = matrixField.Inverse;

        //    Assert.Equal(inverse, inverseRuben);
        //}
    }
}
