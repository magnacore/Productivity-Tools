using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MoveToIndividualDirectories
{
    class Program
    {
        static void Main(string[] args)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(Directory.GetCurrentDirectory());
            List<FileInfo> fileInfos = directoryInfo.GetFiles().Where(x => !x.Name.EndsWith("MoveToIndividualDirectories.exe")).ToList<FileInfo>();

            string format = "000.##";
            int directoryIndex = 1;

            // Console.WriteLine(Directory.GetCurrentDirectory());

            foreach (FileInfo file in fileInfos)
            {
                var sourceFile = file.FullName;
                var destinationFile = Path.Combine(file.DirectoryName, directoryIndex.ToString(format), file.Name);
                var directoryPath = Path.Combine(file.DirectoryName, directoryIndex.ToString(format));

                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                // Console.WriteLine(sourceFile);
                // Console.WriteLine(destinationFile);
                // Console.WriteLine(directoryPath);

                File.Move(sourceFile, destinationFile);

                directoryIndex++;
            }

#if DEBUG
            Console.WriteLine("Press enter to close...");
            Console.ReadLine();
#endif
        }
    }
}
