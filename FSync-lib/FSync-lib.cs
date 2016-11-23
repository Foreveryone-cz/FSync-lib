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
        static bool r           = false;
        static bool f           = false;
        static string z         = "";
        static string e         = @"^.+\.*$";
        static string i         = "";
        static string SRCDIR    = "";
        static string DESTDIR   = "";

        public static void run()
        {
            if (SRCDIR == "")
            {
                Console.WriteLine("Missing parameter SRCDIR");
                return;
            }

            if (!Directory.Exists(SRCDIR))
            {
                Console.WriteLine("Directory '" + SRCDIR + "' doesn't exist!");
                return;
            }

            if (DESTDIR == "")
            {
                Console.WriteLine("Missing parameter DESTDIR");
                return;
            }

            FileInfo[] files = GetFilesFromDir(SRCDIR, e);

            IndexFiles(files);

            if (z != "")
            {
                compressFilesToZip();
            }

            return;

        }

        private static FileInfo[] GetFilesFromDir(string directory, string extensions = @"^.+\.*$")
        {
            var filePaths = Directory.GetFiles(directory, "*.*").Where(file => Regex.IsMatch(file, extensions));
            if (r == true)
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

            if (z != "")
            {
                if (!Directory.Exists(DESTDIR + "tmp"))
                {
                    DirectoryInfo dir = Directory.CreateDirectory(DESTDIR + "tmp");
                    dir.Attributes = FileAttributes.Directory | FileAttributes.Hidden;

                }

                file.CopyTo(DESTDIR + "tmp\\" + file.Name, f);
            }
            else
            {
                string filePath = DESTDIR + file.FullName.Replace(SRCDIR, "");
                string directory = DESTDIR.Substring(0, DESTDIR.Length - 1) + file.DirectoryName.Replace(SRCDIR.Substring(0, SRCDIR.Length - 1), "");
                if (!Directory.Exists(directory))
                {
                    DirectoryInfo di = Directory.CreateDirectory(directory);
                }
                try
                {
                    file.CopyTo(filePath, f);
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

            string indexFileName = i;

            using (StreamWriter sw = new StreamWriter(SRCDIR + indexFileName))
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

            if (Directory.Exists(DESTDIR + "tmp"))
            {
                ZipFile.CreateFromDirectory(DESTDIR + "tmp", DESTDIR + z);

                Directory.Delete(DESTDIR + "tmp", true);
            }
        }

        private static List<Array> getDataFromIndex()
        {
            List<Array> filesList = new List<Array>();

            FileInfo index = new FileInfo(SRCDIR + i);
            if (!index.Exists)
            {
                var file = index.Create();
                file.Close();
            }

            using (StreamReader sr = new StreamReader(SRCDIR + i))
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

        public static void setR(bool R)
        {
            r = R;
        }

        public static void setF(bool F)
        {
            f = F;
        }

        public static void setZ(string Z)
        {
            z = Z;
        }

        public static void setE(string E)
        {
            e = E;
        }

        public static void setR(string I)
        {
            i = I;
        }

        public static void setSRCDIR(string srcDir)
        {
            if(srcDir == "")
            {
                return;
            }

            int at = srcDir.LastIndexOf('\\', srcDir.Length - 1, 1);

            if(at < 0)
            {
                srcDir = srcDir + "\\";
            }

            SRCDIR = srcDir;
        }

        public static void setDESTDIR(string destDir)
        {
            if(destDir == "")
            {
                return;
            }

            int at = destDir.LastIndexOf('\\', destDir.Length - 1, 1);

            if (at < 0)
            {
                destDir = destDir + "\\";
            }

            DESTDIR = destDir;

            if (DESTDIR != "")
            {
                createDirectory(DESTDIR);
            }
        }
    }
}
