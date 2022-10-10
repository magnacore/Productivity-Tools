using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace ClipboardToFile
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            // Retrieves data
            IDataObject iData = Clipboard.GetDataObject();
            string clipboardData;
            string fileName;
            string validFileName;
            string partialName;
            
            // Is Data Text?
            if (iData.GetDataPresent(DataFormats.Text))
            {
                clipboardData = (String) iData.GetData(DataFormats.Text);

                //Calculate time in epoch
                TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
                int secondsSinceEpoch = (int)t.TotalSeconds;

                if (!String.IsNullOrWhiteSpace(clipboardData))
                {
                    partialName = clipboardData.Substring(0, clipboardData.Length);
                }
                else
                {
                    partialName = "ERROR";
                }

                var invalidChars = Path.GetInvalidFileNameChars();

                validFileName = new string(partialName
                  .Where(x => !invalidChars.Contains(x))
                  .ToArray());

                fileName = @"C:\DATA\PRODUCTIVITY SYSTEM\01 TASK CAPTURE BIN\" + validFileName + "_" + secondsSinceEpoch.ToString() + ".txt";

                System.IO.FileInfo fileInfo = null;
                try
                {
                    fileInfo = new System.IO.FileInfo(fileName);
                }
                catch (ArgumentException) { }
                catch (System.IO.PathTooLongException) { }
                catch (NotSupportedException) { }
                if (ReferenceEquals(fileInfo, null))
                {
                    Console.WriteLine("Invalid Filename");
                }
                else
                {
                    // file name is valid... May check for existence by calling fi.Exists.
                    File.WriteAllText(fileName, clipboardData);
                }
                
            }
            else
            {
                clipboardData = "Data not found.";
            }

            Console.WriteLine(clipboardData);
        }
    }
}
