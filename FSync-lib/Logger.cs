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
    public class Logger
    {
        private static readonly object mutex = new object();
        private static Logger instance;

        private Logger()
        {
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
            // TODO
        }
    }
}
