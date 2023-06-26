//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Xunit;

//namespace DevePar.Tests.ParityAlgorithms
//{
//    public class FileTests
//    {
//        public class MetaData
//        {
//            public List<int> FileLengths { get; } = new List<int>();
//            public List<string> Hashes { get; } = new List<string>();
//        }

//        [Fact]        
//        public void GenerateParFilesForTestSet1()
//        {
//            var metaData = new MetaData();
//            var inputDir = Path.Combine("TestFiles1", "Set1");

//            var data = new List<byte[]>();

//            foreach (var file in Directory.GetFiles(inputDir))
//            {
//                var d = File.ReadAllBytes(file);
//                data.Add(d);
//            }



//            var dataConverted = data.Select(t => ToShorter(t).Select(z => (uint)z).ToArray()).ToArray();
//            var longest = dataConverted.Max(t => t.Length);


//            /
//        }



//        public short[] ToShorter(byte[] byteArray)
//        {
//            var shortArray = new short[byteArray.Length / 2 + 1];
//            for (int i = 0; i < byteArray.Length; i += 2)
//            {
//                shortArray[i / 2] = BitConverter.ToInt16(byteArray, i);
//            }
//            return shortArray;
//        }
//    }
//}
