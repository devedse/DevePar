using DevePar.Galois;
using DevePar.ParityAlgorithms;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DevePar.FileRepair
{
    public class DeveParFileRepairer
    {
        public static GFTable GFTable => GFTable.GFTable16;

        public static void CreateParFiles(IEnumerable<string> fileNamesRelativeToWorkingDir, string workingDir, int parityBlockCount)
        {
            var filePathsList = fileNamesRelativeToWorkingDir.ToList();
            if (parityBlockCount > filePathsList.Count)
            {
                throw new InvalidOperationException("Can't do this, parity block needs to be lower then the amount of files");
            }

            var filesAsByte = new List<byte[]>();
            var metadatas = new DeveParMetadata();

            for (int i = 0; i < filePathsList.Count; i++)
            {
                var filePath = filePathsList[i];
                var d = File.ReadAllBytes(filePath);
                filesAsByte.Add(d);

                var fileMetadata = new DeveParFileMetadata()
                {
                    FileName = Path.GetFileName(filePath),
                    FileLength = d.Length,
                    FileHash = FileRepairHelper.CalculateHash(d)
                };
                metadatas.DeveInputFileMetadatas.Add(fileMetadata);
            }

            var filesAsShortAsUint = filesAsByte.Select(t => FileRepairHelper.ToUShortArray(t).Select(z => (uint)z).ToArray()).ToArray();

            // Here we are padding the files to make them of equal length
            var longest = filesAsShortAsUint.Max(t => t.Length);
            for (int i = 0; i < filesAsShortAsUint.Length; i++)
            {
                if (filesAsShortAsUint[i].Length < longest)
                {
                    Array.Resize(ref filesAsShortAsUint[i], longest);
                }
            }


            var filesAsBlocks = filesAsShortAsUint.Select(array => new Block<uint> { Data = array }).ToList();

            // Generate parity data
            var parityData = ParityGFAlgorithm.GenerateParityData3(GFTable, filesAsBlocks, parityBlockCount);

            //Store the output as files (0.devepar, 1.devepar, etc)
            for (int i = 0; i < parityData.Count; i++)
            {
                var parityDataAsShorts = parityData[i].Data.Select(t => (ushort)t).ToArray();
                var parityDataAsBytes = FileRepairHelper.ToByteArray(parityDataAsShorts);
                var parFileName = $"{i}.devepar";
                File.WriteAllBytes(Path.Combine(workingDir, parFileName), parityDataAsBytes);

                var parFileMetadata = new DeveParFileMetadata()
                {
                    FileName = Path.GetFileName(parFileName),
                    FileLength = parityDataAsBytes.Length,
                    FileHash = FileRepairHelper.CalculateHash(parityDataAsBytes)
                };
                metadatas.DeveParFileMetadatas.Add(parFileMetadata);
            }

            //Then also write a file with the metadata (including file lengths / hashes)
            var jsonMetadata = JsonConvert.SerializeObject(metadatas, Formatting.Indented);
            File.WriteAllText(Path.Combine(workingDir, "devepar.json"), jsonMetadata);
        }

        public static void RepairFiles(string workingDir, string metadataFile)
        {
            var jsonMetadata = File.ReadAllText(metadataFile);
            var metadatas = JsonConvert.DeserializeObject<DeveParMetadata>(jsonMetadata);

            int totalFilesCount = metadatas.DeveInputFileMetadatas.Count;
            int totalParFilesCount = metadatas.DeveParFileMetadatas.Count;
            Block<uint>[] data = new Block<uint>[totalFilesCount];
            Block<uint>[] parityData = new Block<uint>[totalParFilesCount];
            int missingFilesCount = 0;

            var longest = Math.Max(metadatas.DeveInputFileMetadatas.Max(t => t.FileLength), metadatas.DeveParFileMetadatas.Max(t => t.FileLength));

            for (int i = 0; i < totalFilesCount; i++)
            {
                var metadata = metadatas.DeveInputFileMetadatas[i];
                var filePath = Path.Combine(workingDir, metadata.FileName);
                if (!File.Exists(filePath) || FileRepairHelper.CalculateHash(File.ReadAllBytes(filePath)) != metadata.FileHash)
                {
                    data[i] = new Block<uint>() { Data = null! };
                    missingFilesCount++;
                    continue;
                }

                var byteArray = File.ReadAllBytes(filePath);
                if (byteArray.Length < longest)
                {
                    Array.Resize(ref byteArray, longest);
                }
                data[i] = new Block<uint>
                {
                    Data = FileRepairHelper.ToUShortArray(byteArray).Select(t => (uint)t).ToArray()
                };
            }

            for (int i = 0; i < totalParFilesCount; i++)
            {
                var metadata = metadatas.DeveParFileMetadatas[i];
                var filePath = Path.Combine(workingDir, metadata.FileName);
                if (!File.Exists(filePath) || FileRepairHelper.CalculateHash(File.ReadAllBytes(filePath)) != metadata.FileHash)
                {
                    parityData[i] = new Block<uint>() { Data = null! };
                    continue;
                }

                var byteArray = File.ReadAllBytes(filePath);
                if (byteArray.Length < longest)
                {
                    Array.Resize(ref byteArray, longest);
                }
                parityData[i] = new Block<uint>
                {
                    Data = FileRepairHelper.ToUShortArray(byteArray).Select(t => (uint)t).ToArray()
                };
            }

            if ((data.Count(t => t != null) + parityData.Count(t => t != null)) < missingFilesCount)
            {
                throw new InvalidOperationException("Not enough valid files to repair all missing/corrupt files");
            }

            var repairedData = ParityGFAlgorithm.RecoverData3(GFTable, data.ToList(), parityData.ToList(), totalParFilesCount);

            for (int i = 0; i < totalFilesCount; i++)
            {
                var metadata = metadatas.DeveInputFileMetadatas[i];
                var filePath = Path.Combine(workingDir, metadata.FileName);
                if (File.Exists(filePath))
                {
                    continue;
                }

                var repairedDataAsShorts = repairedData[i].Data.Select(t => (ushort)t).ToArray();
                var dataAsBytes = FileRepairHelper.ToByteArray(repairedDataAsShorts);

                // Here we are getting the original file length to truncate the recovered file to it
                var originalFileLength = metadatas.DeveInputFileMetadatas[i].FileLength;
                var truncatedDataAsBytes = new byte[originalFileLength];
                Array.Copy(dataAsBytes, truncatedDataAsBytes, originalFileLength);

                var fileName = metadatas.DeveInputFileMetadatas[i].FileName;
                File.WriteAllBytes(Path.Combine(workingDir, fileName), truncatedDataAsBytes);
            }

        }

    }

    public class DeveParMetadata
    {
        public List<DeveParFileMetadata> DeveInputFileMetadatas { get; set; } = new List<DeveParFileMetadata>();
        public List<DeveParFileMetadata> DeveParFileMetadatas { get; set; } = new List<DeveParFileMetadata>();
    }

    public class DeveParFileMetadata
    {
        public string FileName { get; set; }
        public int FileLength { get; set; }
        public string FileHash { get; set; }
    }



    public static class FileRepairHelper
    {
        public static ushort[] ToUShortArray(byte[] byteArray)
        {
            var ushortArray = new ushort[(byteArray.Length + 1) / 2];
            int i = 0;
            for (; i < byteArray.Length - 1; i += 2)
            {
                ushortArray[i / 2] = BitConverter.ToUInt16(byteArray, i);
            }
            if (i < byteArray.Length)
            {
                ushortArray[i / 2] = byteArray[i];
            }
            return ushortArray;
        }

        public static byte[] ToByteArray(ushort[] ushortArray)
        {
            var byteArray = new byte[ushortArray.Length * 2];
            for (int i = 0; i < ushortArray.Length; i++)
            {
                byte[] byteData = BitConverter.GetBytes(ushortArray[i]);
                byteArray[i * 2] = byteData[0];
                if (i * 2 + 1 < byteArray.Length)
                {
                    byteArray[i * 2 + 1] = byteData[1];
                }
            }
            return byteArray;
        }

        public static string CalculateHash(byte[] inputData)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(inputData);
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
