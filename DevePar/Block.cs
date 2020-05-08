using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DevePar
{
    public class Block<T>
    {
        //public string Md5Hash { get; set; }
        public T[] Data { get; set; }

        public override string ToString()
        {
            return string.Join(",", Data);
        }
    }
}
