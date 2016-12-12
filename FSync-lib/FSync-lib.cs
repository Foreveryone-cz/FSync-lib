using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using System.IO.Compression;


namespace FSync_lib
{
    public class FSync_lib
    {
        static bool allowCreateSubdirectories   = false;
        static bool allowOverwriteExistingFiles = false;
        static string ZIPfileSuffixName         = "";
        static string listOfAllowedExtensions   = @"^.+\.*$";
        static string indexFileName             = "";
        static string sourceDirectory           = "";
        static string destinationDirectory      = "";

        public static void run()
        {
            if (sourceDirectory == "")
            {
                Console.WriteLine("Missing parameter SRCDIR");
                return;
            }

            if (!Directory.Exists(sourceDirectory))
            {
                Console.WriteLine("Directory '" + sourceDirectory + "' doesn't exist!");
                return;
            }

            if (destinationDirectory == "")
            {
                Console.WriteLine("Missing parameter DESTDIR");
                return;
            }

            FileInfo[] files = GetFilesFromDir(sourceDirectory, listOfAllowedExtensions);

            IndexFiles(files);

            if (ZIPfileSuffixName != "")
            {
                compressFilesToZip();
            }

            return;

        }

        private static FileInfo[] GetFilesFromDir(string directory, string extensions = @"^.+\.*$")
        {
            var filePaths = Directory.GetFiles(directory, "*.*").Where(file => Regex.IsMatch(file, extensions));
            if (allowCreateSubdirectories == true)
            {
                filePaths = Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories).Where(file => Regex.IsMatch(file, extensions));
            }

            List<FileInfo> filesList = new List<FileInfo>();
            foreach (string fileName in filePaths)
            {
                FileInfo fi1 = new FileInfo(fileName);
                filesList.Add(fi1);
            }

            FileInfo[] files = filesList.ToArray();

            return files;
        }

        private static bool moveFile(FileInfo file)
        {
            if (!file.Exists)
            {
                return false;
            }

            if (ZIPfileSuffixName != "")
            {
                if (!Directory.Exists(destinationDirectory + "tmp"))
                {
                    DirectoryInfo dir = Directory.CreateDirectory(destinationDirectory + "tmp");
                    dir.Attributes = FileAttributes.Directory | FileAttributes.Hidden;

                }

                file.CopyTo(destinationDirectory + "tmp\\" + file.Name, allowOverwriteExistingFiles);
            }
            else
            {
                string filePath = destinationDirectory + file.FullName.Replace(sourceDirectory, "");
                string directory = destinationDirectory.Substring(0, destinationDirectory.Length - 1) + file.DirectoryName.Replace(sourceDirectory.Substring(0, sourceDirectory.Length - 1), "");
                if (!Directory.Exists(directory))
                {
                    DirectoryInfo di = Directory.CreateDirectory(directory);
                }
                try
                {
                    file.CopyTo(filePath, allowOverwriteExistingFiles);
                }
                catch (IOException ioex)
                {

                }
            }

            return true;
        }

        private static void IndexFiles(FileInfo[] files)
        {
            int countMoveFiles = 0;

            List<Array> dataFromIndex = getDataFromIndex();

            string indexFileName = FSync_lib.indexFileName;

            using (StreamWriter sw = new StreamWriter(sourceDirectory + indexFileName))
            {
                foreach (FileInfo file in files)
                {
                    if (file.Name == indexFileName)
                    {
                        continue;
                    }

                    string filePathName = file.FullName;
                    int lastWriteUnixTimestamp = DateTimeToUnixTimestamp(file.LastWriteTimeUtc);

                    if (isNewFile(dataFromIndex, file))
                    {
                        moveFile(file);
                        countMoveFiles++;
                    }

                    sw.WriteLine(filePathName + "|" + lastWriteUnixTimestamp);
                }

                sw.Flush();
            }

            Console.WriteLine("Moved " + countMoveFiles + " files.");
        }

        private static void compressFilesToZip()
        {

            if (Directory.Exists(destinationDirectory + "tmp"))
            {
                ZipFile.CreateFromDirectory(destinationDirectory + "tmp", destinationDirectory + ZIPfileSuffixName);

                Directory.Delete(destinationDirectory + "tmp", true);
            }
        }

        private static List<Array> getDataFromIndex()
        {
            List<Array> filesList = new List<Array>();

            FileInfo index = new FileInfo(sourceDirectory + indexFileName);
            if (!index.Exists)
            {
                var file = index.Create();
                file.Close();
            }

            using (StreamReader sr = new StreamReader(sourceDirectory + indexFileName))
            {
                string s;
                while ((s = sr.ReadLine()) != null)
                {
                    string[] exploded = s.Split('|');
                    filesList.Add(exploded);
                }
            }

            return filesList;
        }

        private static bool isNewFile(List<Array> filesFromIndex, FileInfo file)
        {
            foreach (string[] fileFromIndex in filesFromIndex)
            {
                if (fileFromIndex[0] == file.FullName && fileFromIndex[1] == DateTimeToUnixTimestamp(file.LastWriteTimeUtc).ToString())
                {
                    return false;
                }
            }

            return true;
        }

        private static void createDirectory(string directoryName)
        {
            if (!System.IO.Directory.Exists(directoryName))
            {
                System.IO.Directory.CreateDirectory(directoryName);
            }
        }

        public static int DateTimeToUnixTimestamp(DateTime dateTime)
        {
            DateTime unixStart = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            long unixTimeStampInTicks = (dateTime.ToUniversalTime() - unixStart).Ticks;
            return (int)((double)unixTimeStampInTicks / TimeSpan.TicksPerSecond);
        }

        public static void setAllowCreateSubdirectories(bool allowCreateSubdirectoriesParam)
        {
            allowCreateSubdirectories = allowCreateSubdirectoriesParam;
        }

        public static void setAllowOverwriteExistingFiles(bool allowOverwriteExistingFilesParam)
        {
            allowOverwriteExistingFiles = allowOverwriteExistingFilesParam;
        }

        public static void setZIPfileSuffixName(string ZIPfileSuffixNameParam)
        {
            ZIPfileSuffixName = ZIPfileSuffixNameParam;
        }

        public static void setListOfAllowedExtensions(string listOfAllowedExtensionsParam)
        {
            listOfAllowedExtensions = listOfAllowedExtensionsParam;
        }

        public static void setIndexFileName(string indexFileNameParam)
        {
            indexFileName = indexFileNameParam;
        }

        public static void setSourceDirectory(string sourceDirectoryParam)
        {
            if(sourceDirectoryParam == "")
            {
                return;
            }

            int at = sourceDirectoryParam.LastIndexOf('\\', sourceDirectoryParam.Length - 1, 1);

            if(at < 0)
            {
                sourceDirectoryParam = sourceDirectoryParam + "\\";
            }

            sourceDirectory = sourceDirectoryParam;
        }

        public static void setDestinationDirectory(string destinationDirectoryParam)
        {
            if(destinationDirectoryParam == "")
            {
                return;
            }

            int at = destinationDirectoryParam.LastIndexOf('\\', destinationDirectoryParam.Length - 1, 1);

            if (at < 0)
            {
                destinationDirectoryParam = destinationDirectoryParam + "\\";
            }

            destinationDirectory = destinationDirectoryParam;

            if (destinationDirectory != "")
            {
                createDirectory(destinationDirectory);
            }
        }
    }
}
