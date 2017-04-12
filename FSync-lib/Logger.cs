﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using System.IO.Compression;

namespace FSync_lib
{
    public class Logger
    {
        private static readonly object mutex = new object();
        private static Logger instance;

        private string actualLogName = @"C:\logs\fsync-lib.log";
        private string archiveLogName = @"C:\logs\fsync-lib.log.arch";

        private Logger()
        {
            using (StreamWriter w = File.AppendText(actualLogName)) ;

            clearLogFiles();
        }

        public static Logger Instance
        {
            get
            {
                lock (mutex)
                {
                    // Only one thread can get into this
                    // block of code at a time now.
                    if (instance == null)
                    {
                        instance = new Logger();
                    }
                    return instance;
                }
            }
        }

        public void Log(string msg)
        {
            using (StreamWriter sw = new StreamWriter(actualLogName))
            {

            }
                System.IO.StreamWriter file = new System.IO.StreamWriter(actualLogName, true);
                file.WriteLine(msg);

                file.Close();
            
        }

        public void clearLogFiles()
        {
            FileInfo actualLogFile = new FileInfo(actualLogName);

            int actualLogLength = Int32.Parse(actualLogFile.Length.ToString());

            if(actualLogLength / 1024 / 1024 > 0)
            {
                FileInfo archiveLogFile = new FileInfo(archiveLogName);

                if(archiveLogFile.Exists)
                {
                    archiveLogFile.Delete();
                }

                System.IO.File.Move(actualLogName, archiveLogName);
            }


        }
    }
}
