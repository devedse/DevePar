using DevePar.FileRepair;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DevePar.Tests.ParityAlgorithms
{
    public class FileTests
    {
        [Fact]
        public void FileRepairWorks()
        {
            Console.WriteLine("Creating par files...");

            var theFileDir = Path.Combine("TestFiles", "Set1");
            var testFiles = Directory.GetFiles(theFileDir).Where(t => !Path.GetExtension(t).Equals(".devepar", StringComparison.OrdinalIgnoreCase) && !Path.GetFileName(t).Equals("devepar.json")).ToList();

            DeveParFileRepairer.CreateParFiles(testFiles, theFileDir, 3);


            Console.WriteLine("Deleting file 2");
            var hashBefore = FileRepairHelper.CalculateHash(File.ReadAllBytes(testFiles[1]));
            File.Delete(testFiles[1]);



            Console.WriteLine("Repairing files...");

            DeveParFileRepairer.RepairFiles(theFileDir, Path.Combine(theFileDir, "devepar.json"));

            Console.WriteLine("Repair completed");

            Assert.True(File.Exists(testFiles[1]));
            var hashAfter = FileRepairHelper.CalculateHash(File.ReadAllBytes(testFiles[1]));
            Assert.Equal(hashBefore, hashAfter);
        }
    }
}
