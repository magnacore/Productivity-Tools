using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SplitText
{
    internal class Program
    {
        // Cannot access non-static in static main thats why made this static
        // Path of the backup directory
        private static readonly string backupDirectoryName = "ORIGINAL";

        private static void Main(string[] args)
        {
            // Get all the files ending with "txt" in the current directory into a list. Exclude this exe.
            var directoryInfo = new DirectoryInfo(Directory.GetCurrentDirectory());
            var fileInfos = directoryInfo.GetFiles()
                .Where(x => !x.Name.EndsWith("MoveToIndividualDirectories.exe") && x.Name.EndsWith("txt"))
                .ToList();

            //Console.WriteLine(Directory.GetCurrentDirectory());

            // Create the backup directory if it does not exist
            if (!Directory.Exists(backupDirectoryName))
            {
                Directory.CreateDirectory(backupDirectoryName);
            }

            foreach (var file in fileInfos)
            {
                var sourceFile = file.FullName;
                var currentDirectory = Path.Combine(file.DirectoryName);
                var destinationDirectoryPath = Path.Combine(file.DirectoryName, backupDirectoryName, file.Name);

                //Console.WriteLine(Path.GetFileNameWithoutExtension(file.Name));
                //Console.WriteLine(sourceFile);
                //Console.WriteLine(currentDirectory);
                //Console.WriteLine(destinationDirectoryPath);

                SplitFile(sourceFile, 5000, currentDirectory, Path.GetFileNameWithoutExtension(file.Name));

                File.Move(sourceFile, destinationDirectoryPath);
            }


            void SplitFile(string inputFile, int chunkSize, string path, string fileName)
            {
                // format name as 00X
                var format = "000.##";
                
                var buffer = new byte[chunkSize];
                var extraBuffer = new List<byte>();

                using (Stream input = File.OpenRead(inputFile))
                {
                    var directoryIndex = 1;
                    while (input.Position < input.Length)
                    {
                        using (Stream output =
                            File.Create(path + "\\" + fileName + "_" + directoryIndex.ToString(format) + ".txt"))
                        {
                            var chunkBytesRead = 0;
                            while (chunkBytesRead < chunkSize)
                            {
                                var bytesRead = input.Read(buffer,
                                    chunkBytesRead,
                                    chunkSize - chunkBytesRead);

                                if (bytesRead == 0)
                                    break;

                                chunkBytesRead += bytesRead;
                            }

                            var extraByte = buffer[chunkSize - 1];
                            while (extraByte != '\n')
                            {
                                var flag = input.ReadByte();
                                if (flag == -1)
                                    break;
                                extraByte = (byte)flag;
                                extraBuffer.Add(extraByte);
                            }

                            output.Write(buffer, 0, chunkBytesRead);
                            if (extraBuffer.Count > 0)
                                output.Write(extraBuffer.ToArray(), 0, extraBuffer.Count);

                            extraBuffer.Clear();
                        }
                        directoryIndex++;
                    }
                }
            }

#if DEBUG
            Console.WriteLine("Press enter to close...");
            Console.ReadLine();
#endif
        }
    }
}