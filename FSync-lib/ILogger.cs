using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSync_lib
{
    interface ILogger
    {
        public void Log(LogLevel level, string msg);
        public void Log(LogLevel level, string msgType, string msg);
        public void InitLogSession();
        public void EndLogSession();
        public void AddLogger(Logger chainedLogger);
        public void RemoveLogger(Logger chainedLogger);
    }
}
